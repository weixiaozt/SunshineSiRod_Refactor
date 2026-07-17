import csv
import json
import math
from dataclasses import dataclass
from pathlib import Path

import numpy as np
import tifffile

import analyze_absolute_slice_calibration as slice_cal
import analyze_caliper_contact_diagonals_ctb as caliper
import analyze_joint_face_angles_ctb as joint_angle
import analyze_positive_repeatability as geom
import measure_ctb_full_metrics as legacy_metric


CALIBRATION_SPECIMEN = "2606005B22-CTB"
CALIBRATION_SCAN = "13_08"
SLICE_COUNT = 75
SLICE_END_MARGIN_ROWS = 250
Y_SCALE_MM_PER_ROW = geom.Y_SCALE_MM_PER_ROW
EDGE_PAIRS = {
    "A": ("Top", "Right"),
    "B": ("Right", "Left"),
    "C": ("Left", "Down"),
    "D": ("Down", "Top"),
}
CORNER_CAMERA = {"AB": "Right", "BC": "Left", "CD": "Down", "AD": "Top"}
DIAGONALS = {"D1": ("AB", "CD"), "D2": ("AD", "BC")}
FACE_SOURCES = {
    "A": (("Top", "right"), ("Right", "left")),
    "B": (("Right", "right"), ("Left", "left")),
    "C": (("Left", "right"), ("Down", "left")),
    "D": (("Down", "right"), ("Top", "left")),
}
CORNER_FACES = {"AB": ("A", "B"), "BC": ("B", "C"), "CD": ("C", "D"), "AD": ("A", "D")}
OFFSET_POINTS = {
    "1": ("AB", "Right"),
    "2": ("CD", "Down"),
    "3": ("AD", "Top"),
    "4": ("BC", "Left"),
}
OFFSET_METRIC_NAMES = [
    f"{prefix}{number}"
    for prefix in ("起点偏移量", "结束点偏移量")
    for number in ("1", "2", "3", "4")
]


@dataclass
class ScanInput:
    specimen: str
    scan: str
    files: dict


def mean(values):
    values = np.asarray(list(values), dtype=float)
    values = values[np.isfinite(values)]
    if not values.size:
        return float("nan")
    return float(np.mean(values))


def quadrilateral_from_sides(sides):
    """Construct the least-nonorthogonal convex quadrilateral using only A-D."""
    a, b, c, d = [float(sides[key]) for key in "ABCD"]
    lower = max(abs(a - b), abs(c - d)) + 1e-5
    upper = min(a + b, c + d) - 1e-5
    diagonals = np.linspace(lower, upper, 40001)
    best = None
    for diagonal in diagonals:
        x_left = (a * a + diagonal * diagonal - b * b) / (2.0 * a)
        z2 = diagonal * diagonal - x_left * x_left
        if z2 <= 0:
            continue
        left = np.array([x_left, math.sqrt(z2)])
        unit = left / diagonal
        along = (d * d - c * c + diagonal * diagonal) / (2.0 * diagonal)
        h2 = d * d - along * along
        if h2 <= 0:
            continue
        perpendicular = np.array([-unit[1], unit[0]])
        candidates = [along * unit + math.sqrt(h2) * perpendicular,
                      along * unit - math.sqrt(h2) * perpendicular]
        down = min(candidates, key=lambda point: abs(point[0]) + abs(point[1] - d))
        top = np.array([0.0, 0.0])
        right = np.array([a, 0.0])
        points = [top, right, left, down]
        score = 0.0
        angles = []
        for index in range(4):
            previous = points[(index - 1) % 4] - points[index]
            following = points[(index + 1) % 4] - points[index]
            cosine = float(np.dot(previous, following) / (np.linalg.norm(previous) * np.linalg.norm(following)))
            score += cosine * cosine
            angles.append(math.degrees(math.acos(np.clip(cosine, -1.0, 1.0))))
        if best is None or score < best[0]:
            best = (score, points, angles, diagonal)
    if best is None:
        raise RuntimeError("ABCD cannot form a convex quadrilateral")
    _, points, angles, diagonal = best
    return {
        "Top": points[0],
        "Right": points[1],
        "Left": points[2],
        "Down": points[3],
    }, angles, diagonal


def discover_scans(root):
    unpacked = next(
        path for path in root.glob("*/_positive_unpacked")
        if (path / CALIBRATION_SPECIMEN).is_dir()
    )
    scans = []
    for specimen_dir in sorted(path for path in unpacked.iterdir() if path.is_dir()):
        for scan_dir in sorted(path for path in specimen_dir.iterdir() if path.is_dir()):
            files = {}
            for camera, spec in geom.CAMERAS.items():
                matches = list(scan_dir.glob(spec["glob"]))
                if matches:
                    files[camera] = matches[0]
            if len(files) == 4:
                scans.append(ScanInput(specimen_dir.name, scan_dir.name, files))

    custom_root = next(root.glob("*/104 NG-1"), None)
    if custom_root:
        for folder, scan_name in (("NG-1_3", "10_40"), ("NG-1diaotou_3", "10_44_reverse")):
            tiff_dir = custom_root / folder / "tiff"
            if not tiff_dir.is_dir():
                continue
            prefix = scan_name.replace("_reverse", "")
            files = {
                "Left": tiff_dir / f"{prefix}_1.tiff",
                "Right": tiff_dir / f"{prefix}_2.tiff",
                "Top": tiff_dir / f"{prefix}_3.tiff",
                "Down": tiff_dir / f"{prefix}_4.tiff",
            }
            if all(path.exists() for path in files.values()):
                scans.append(ScanInput("104 NG-1", scan_name, files))
    return scans


def image_bounds(images):
    bounds = {camera: legacy_metric.row_bounds(image) for camera, image in images.items()}
    common_start = max(value[0] for value in bounds.values())
    common_end = min(value[1] for value in bounds.values())
    return bounds, common_start, common_end


def optimized_support(points_positive, points_negative, nominal_direction):
    nominal_direction = np.asarray(nominal_direction, dtype=float)
    nominal_direction /= np.linalg.norm(nominal_direction)
    offsets = np.linspace(-5.0, 5.0, 1001)
    radians = np.deg2rad(offsets)
    directions = np.column_stack([
        nominal_direction[0] * np.cos(radians) - nominal_direction[1] * np.sin(radians),
        nominal_direction[0] * np.sin(radians) + nominal_direction[1] * np.cos(radians),
    ])
    widths = np.max(points_positive @ directions.T, axis=0) - np.min(points_negative @ directions.T, axis=0)
    index = int(np.argmax(widths))
    direction = directions[index]
    positive = points_positive[int(np.argmax(points_positive @ direction))]
    negative = points_negative[int(np.argmin(points_negative @ direction))]
    return float(widths[index]), float(offsets[index]), positive, negative


def end_offset_metrics(bounds, end_index, prefix, camera_y_bias_mm=None):
    camera_y_bias_mm = camera_y_bias_mm or {}
    end_rows = {
        camera: float(rows[end_index]) * Y_SCALE_MM_PER_ROW - float(camera_y_bias_mm.get(camera, 0.0))
        for camera, rows in bounds.items()
    }
    origin = float(np.mean(list(end_rows.values())))
    metrics = {}
    for number in ("1", "2", "3", "4"):
        _, camera = OFFSET_POINTS[number]
        metrics[f"{prefix}{number}"] = end_rows[camera] - origin
    return metrics


def endpoint_offset_metrics(bounds, camera_y_bias_mm=None):
    metrics = {}
    metrics.update(end_offset_metrics(bounds, 0, "起点偏移量", camera_y_bias_mm))
    metrics.update(end_offset_metrics(bounds, 1, "结束点偏移量", camera_y_bias_mm))
    return metrics


def build_calibration(root, sides):
    scans = discover_scans(root)
    calibration_input = next(
        item for item in scans
        if item.specimen == CALIBRATION_SPECIMEN and item.scan == CALIBRATION_SCAN
    )
    images = {camera: tifffile.memmap(path) for camera, path in calibration_input.files.items()}
    bounds, common_start, common_end = image_bounds(images)
    rows = np.rint(np.linspace(
        common_start + SLICE_END_MARGIN_ROWS,
        common_end - SLICE_END_MARGIN_ROWS,
        SLICE_COUNT,
    )).astype(int)
    nominal, nominal_angles, nominal_diagonal = quadrilateral_from_sides(sides)

    windows = {}
    baseline = {}
    matrices = {}
    for camera in geom.CAMERA_ORDER:
        left, right = slice_cal.fixed_face_columns(images[camera], int(rows[len(rows) // 2]))
        windows[camera] = {"left": left, "right": right}
        baseline[camera] = slice_cal.extract_corner_series(images[camera], rows, left, right)
        matrices[camera] = slice_cal.orthogonal_map(camera, baseline[camera])

    origins = []
    for index in range(len(rows)):
        row_origins = {}
        for camera in geom.CAMERA_ORDER:
            local = np.array([baseline[camera]["u_mm"][index], baseline[camera]["z_mm"][index]])
            row_origins[camera] = nominal[camera] - matrices[camera] @ local
        origins.append(row_origins)
    return {
        "rows": rows,
        "windows": windows,
        "baseline": baseline,
        "matrices": matrices,
        "origins": origins,
        "nominal": nominal,
        "nominal_angles_deg": nominal_angles,
        "nominal_diagonal_mm": nominal_diagonal,
        "bounds": bounds,
        "scan_input": calibration_input,
        "all_scans": scans,
    }


def transform_face_points(images, rows, windows, matrices, origins):
    output = {camera: {} for camera in geom.CAMERAS}
    for camera in geom.CAMERA_ORDER:
        for side in ("left", "right"):
            columns = windows[camera][side]
            z_matrix = slice_cal.band_profiles(images[camera], rows, columns)
            x = columns.astype(float) * geom.X_SCALE_MM_PER_PX
            per_row = []
            for index in range(len(rows)):
                z = z_matrix[index]
                valid = np.isfinite(z)
                local = np.column_stack([x[valid], z[valid]])
                per_row.append((matrices[camera] @ local.T).T + origins[index][camera])
            output[camera][side] = per_row
    return output


def measure_scan(scan_input, calibration):
    images = {camera: tifffile.memmap(path) for camera, path in scan_input.files.items()}
    bounds, common_start, common_end = image_bounds(images)
    all_rows = calibration["rows"]
    valid_indices = np.flatnonzero(
        (all_rows >= common_start + SLICE_END_MARGIN_ROWS)
        & (all_rows <= common_end - SLICE_END_MARGIN_ROWS)
    )
    if valid_indices.size < 10:
        raise RuntimeError(
            f"{scan_input.specimen}/{scan_input.scan} has fewer than 10 overlapping calibration rows"
        )
    rows = all_rows[valid_indices]
    origins = [calibration["origins"][int(index)] for index in valid_indices]

    sections = {
        camera: slice_cal.extract_corner_series(
            images[camera], rows,
            calibration["windows"][camera]["left"],
            calibration["windows"][camera]["right"],
        )
        for camera in geom.CAMERA_ORDER
    }
    face_points = transform_face_points(
        images, rows, calibration["windows"], calibration["matrices"], origins
    )

    slice_records = []
    chamfers = {corner: [] for corner in CORNER_CAMERA}
    joint_angles = {corner: [] for corner in CORNER_CAMERA}
    for index, row in enumerate(rows):
        corners = {}
        neighborhoods = {}
        features = {}
        for corner, camera in CORNER_CAMERA.items():
            local_corner = np.array([
                sections[camera]["u_mm"][index], sections[camera]["z_mm"][index]
            ])
            corner_global = origins[index][camera] + calibration["matrices"][camera] @ local_corner
            corners[corner] = corner_global
            local_profile = caliper.full_corner_profile_local(images[camera], int(row))
            global_profile = (
                calibration["matrices"][camera] @ local_profile.T
            ).T + origins[index][camera]
            neighborhoods[corner] = global_profile[
                np.linalg.norm(global_profile - corner_global, axis=1) <= 2.5
            ]
            features[corner] = legacy_metric.chamfer_feature(images[camera], int(row))
            chamfers[corner].append(features[corner])

        camera_corner = {camera: corners[corner] for corner, camera in CORNER_CAMERA.items()}
        edges = {
            edge: float(np.linalg.norm(camera_corner[camera1] - camera_corner[camera2]))
            for edge, (camera1, camera2) in EDGE_PAIRS.items()
        }
        diagonals = {}
        diagonal_offsets = {}
        for diagonal, (positive_corner, negative_corner) in DIAGONALS.items():
            value, offset, _, _ = optimized_support(
                neighborhoods[positive_corner], neighborhoods[negative_corner],
                corners[positive_corner] - corners[negative_corner],
            )
            diagonals[diagonal] = value
            diagonal_offsets[diagonal] = offset

        shared_directions = {}
        for face, sources in FACE_SOURCES.items():
            groups = [face_points[camera][side][index] for camera, side in sources]
            direction, _, _, _ = joint_angle.shared_direction(groups)
            shared_directions[face] = direction
        for corner, (face1, face2) in CORNER_FACES.items():
            joint_angles[corner].append(
                joint_angle.acute_angle(shared_directions[face1], shared_directions[face2])
            )

        slice_records.append({
            "specimen": scan_input.specimen,
            "scan": scan_input.scan,
            "slice_index": index + 1,
            "absolute_row": int(row),
            "position_mm": float((row - rows[0]) * Y_SCALE_MM_PER_ROW),
            **{edge: edges[edge] for edge in "ABCD"},
            "W": (edges["A"] + edges["C"]) / 2.0,
            "H": (edges["B"] + edges["D"]) / 2.0,
            "diagonal1": diagonals["D1"],
            "diagonal2": diagonals["D2"],
            "diagonal1_angle_offset_deg": diagonal_offsets["D1"],
            "diagonal2_angle_offset_deg": diagonal_offsets["D2"],
        })

    head = slice_records[:5]
    tail = slice_records[-5:]
    head_edges = {edge: mean([record[edge] for record in head]) for edge in "ABCD"}
    tail_edges = {edge: mean([record[edge] for record in tail]) for edge in "ABCD"}
    camera_lengths = {
        camera: (value[1] - value[0]) * Y_SCALE_MM_PER_ROW
        for camera, value in bounds.items()
    }
    length = mean(camera_lengths.values())

    metric = {"长度": length}
    for edge in "ABCD":
        metric[f"头-边长{edge}"] = head_edges[edge]
    for edge in "ABCD":
        metric[f"尾-边长{edge}"] = tail_edges[edge]
    metric["头-对角线1"] = mean([record["diagonal1"] for record in head])
    metric["头-对角线2"] = mean([record["diagonal2"] for record in head])
    metric["尾-对角线1"] = mean([record["diagonal1"] for record in tail])
    metric["尾-对角线2"] = mean([record["diagonal2"] for record in tail])

    for corner in ("AB", "BC", "CD", "AD"):
        chord = mean([feature["chord_mm"] for feature in chamfers[corner]])
        projection = chord / math.sqrt(2.0)
        metric[f"弧长投影{corner}-1"] = projection
        metric[f"弧长投影{corner}-2"] = projection
    metric["弧长AB"] = mean([feature["chord_mm"] for feature in chamfers["AB"]])
    metric["弧长BD"] = mean([feature["chord_mm"] for feature in chamfers["BC"]])
    metric["弧长CD"] = mean([feature["chord_mm"] for feature in chamfers["CD"]])
    metric["弧长AD"] = mean([feature["chord_mm"] for feature in chamfers["AD"]])
    for number, corner in enumerate(("AB", "BC", "CD", "AD"), 1):
        metric[f"侧面垂直度{number}"] = mean(joint_angles[corner])

    starts = {camera: value[0] for camera, value in bounds.items()}
    ends = {camera: value[1] for camera, value in bounds.items()}
    metric.update(endpoint_offset_metrics(bounds))
    for end_cn, end_rows, edge_values in (
        ("头", starts, head_edges), ("尾", ends, tail_edges)
    ):
        for edge, (camera1, camera2) in EDGE_PAIRS.items():
            axial_delta = abs(end_rows[camera2] - end_rows[camera1]) * Y_SCALE_MM_PER_ROW
            metric[f"{end_cn}-端面垂直度-{edge}"] = 90.0 - math.degrees(
                math.atan2(axial_delta, edge_values[edge])
            )

    return {
        "specimen": scan_input.specimen,
        "scan": scan_input.scan,
        "metrics": metric,
        "slice_records": slice_records,
        "diagnostics": {
            "bounds": bounds,
            "camera_lengths_mm": camera_lengths,
            "calibration_rows": rows.tolist(),
            "joint_angle_method": "two cameras share one face direction with independent normal offsets",
            "diagonal_method": "complete-profile parallel-jaw support, optimized within +/-5 degrees",
        },
    }


def write_csv(path, rows):
    rows = list(rows)
    keys = []
    for row in rows:
        for key in row:
            if key not in keys:
                keys.append(key)
    with path.open("w", newline="", encoding="utf-8-sig") as handle:
        writer = csv.DictWriter(handle, fieldnames=keys)
        writer.writeheader()
        writer.writerows(rows)


def main():
    root = Path.cwd()
    standard_path = root / ".codex_tmp" / "ctb_standard" / "standard_abcd.json"
    standard_payload = json.loads(standard_path.read_text(encoding="utf-8"))
    sides = standard_payload["calibrationFields"]
    calibration = build_calibration(root, sides)
    output_dir = root / "被测棒数据" / "_analysis" / "ctb_abcd_slice_calibrated_all_bars"
    output_dir.mkdir(parents=True, exist_ok=True)

    results = []
    for scan_input in calibration["all_scans"]:
        print(f"Measuring {scan_input.specimen}/{scan_input.scan}...", flush=True)
        results.append(measure_scan(scan_input, calibration))

    metric_order = list(results[0]["metrics"])
    scan_rows = []
    for result in results:
        scan_rows.append({
            "被测棒": result["specimen"],
            "扫描": result["scan"],
            **result["metrics"],
        })
    slice_rows = [record for result in results for record in result["slice_records"]]
    write_csv(output_dir / "all_scan_metrics.csv", scan_rows)
    write_csv(output_dir / "all_slice_measurements.csv", slice_rows)

    repeatability_rows = []
    for specimen in sorted({result["specimen"] for result in results}):
        selected = [result for result in results if result["specimen"] == specimen]
        for metric_name in metric_order:
            values = [result["metrics"][metric_name] for result in selected]
            repeatability_rows.append({
                "被测棒": specimen,
                "指标": metric_name,
                "扫描次数": len(values),
                "平均值": mean(values),
                "极差": float(np.ptp(values)) if len(values) > 1 else 0.0,
                "单位": "°" if "垂直度" in metric_name else "mm",
            })
    write_csv(output_dir / "specimen_repeatability_summary.csv", repeatability_rows)

    payload = {
        "calibration": {
            "source_workbook": standard_payload["source"],
            "used_fields_only": sides,
            "ignored_workbook_field_count": standard_payload["ignoredFieldCount"],
            "specimen": CALIBRATION_SPECIMEN,
            "scan": CALIBRATION_SCAN,
            "slice_count": SLICE_COUNT,
            "rows": calibration["rows"].tolist(),
            "nominal_corner_angles_deg_from_abcd_only": calibration["nominal_angles_deg"],
            "y_scale_mm_per_row_not_from_workbook": Y_SCALE_MM_PER_ROW,
        },
        "scan_count": len(results),
        "results": results,
    }
    (output_dir / "all_results.json").write_text(
        json.dumps(payload, ensure_ascii=False, indent=2), encoding="utf-8"
    )
    print(json.dumps({
        "output_dir": str(output_dir),
        "scan_count": len(results),
        "specimens": sorted({result["specimen"] for result in results}),
    }, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
