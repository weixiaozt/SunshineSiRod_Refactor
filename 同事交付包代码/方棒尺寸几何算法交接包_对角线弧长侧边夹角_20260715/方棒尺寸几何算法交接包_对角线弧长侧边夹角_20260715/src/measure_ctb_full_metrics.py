import csv
import json
import math
from pathlib import Path

import numpy as np
import tifffile

import analyze_absolute_slice_calibration as slice_cal
import analyze_cmm_calibrated_sparse_slices as cmm_cal
import analyze_positive_repeatability as geom


SPECIMEN = "2606005B22-CTB"
CALIBRATION_SCAN = "11_56"
CALIBRATION_ROWS = np.rint(np.linspace(9000, 18000, 21)).astype(int)
EDGE_PAIRS = {
    "A": ("Top", "Right"),
    "B": ("Right", "Left"),
    "C": ("Left", "Down"),
    "D": ("Top", "Down"),
}
CORNER_CAMERA = {"AB": "Right", "BC": "Left", "CD": "Down", "AD": "Top"}
INVALID = geom.INVALID_THRESHOLD


def mean(values):
    data = np.asarray(values, dtype=float)
    data = data[np.isfinite(data)]
    if not data.size:
        return float("nan")
    data.sort()
    trim = int(data.size * 0.1) if data.size >= 10 else 0
    if trim:
        data = data[trim:-trim]
    return float(np.mean(data))


def line_fit(x, z, min_points=10):
    x = np.asarray(x, dtype=float)
    z = np.asarray(z, dtype=float)
    keep = np.isfinite(x) & np.isfinite(z)
    for _ in range(5):
        if keep.sum() < min_points:
            raise RuntimeError("too few points for chamfer fit")
        m, b = np.polyfit(x[keep], z[keep], 1)
        residual = z - (m * x + b)
        center = np.median(residual[keep])
        mad = np.median(np.abs(residual[keep] - center))
        limit = max(4.0 * 1.4826 * mad, 0.006)
        new_keep = keep & (np.abs(residual - center) <= limit)
        if np.array_equal(new_keep, keep):
            break
        keep = new_keep
    rms = math.sqrt(float(np.mean((z[keep] - (m * x[keep] + b)) ** 2)))
    return float(m), float(b), float(rms), int(keep.sum())


def intersection(line1, line2):
    m1, b1 = line1[:2]
    m2, b2 = line2[:2]
    if abs(m1 - m2) < 1e-10:
        raise RuntimeError("parallel fitted lines")
    x = (b2 - b1) / (m1 - m2)
    return np.array([x, m1 * x + b1], dtype=float)


def chamfer_feature(image, row):
    # This follows the verified HDEV contour convention: mirror columns, crop
    # 500:3200, fit two long faces and the 57-point central chamfer chord.
    profile = geom.section_profile(image, int(row))[::-1][500:3200]
    valid = np.isfinite(profile) & (profile > INVALID) & (profile < 9999)
    x = np.flatnonzero(valid).astype(float) * geom.X_SCALE_MM_PER_PX
    z = profile[valid]
    in_range = (z > -45.0) & (z < 45.0)
    x, z = x[in_range], z[in_range]
    if x.size < 1200:
        raise RuntimeError(f"too few contour points at row {row}")

    smooth = np.convolve(z, np.ones(21) / 21.0, mode="same")
    lo, hi = int(z.size * 0.03), int(z.size * 0.97)
    apex = lo + int(np.argmax(smooth[lo:hi]))
    left_a = int(z.size * 0.08)
    left_b = max(left_a + 50, int(apex - z.size * 0.08))
    right_a = min(z.size - 60, int(apex + z.size * 0.08))
    right_b = int(z.size * 0.92)
    chord_a, chord_b = max(0, apex - 28), min(z.size, apex + 29)

    left = line_fit(x[left_a:left_b], z[left_a:left_b], 50)
    chord = line_fit(x[chord_a:chord_b], z[chord_a:chord_b], 10)
    right = line_fit(x[right_a:right_b], z[right_a:right_b], 50)
    p1 = intersection(left, chord)
    p2 = intersection(chord, right)
    chord_length = float(np.linalg.norm(p2 - p1))
    angle_left = abs(math.degrees(math.atan(left[0])))
    angle_right = abs(math.degrees(math.atan(right[0])))
    return {
        "chord_mm": chord_length,
        "projection_45_mm": chord_length / math.sqrt(2.0),
        "side_angle_deg": 180.0 - angle_left - angle_right,
        "chord_fit_rms_um": chord[2] * 1000.0,
    }


def row_bounds(image):
    sampled = image[:, ::4]
    count = (np.isfinite(sampled) & (sampled >= -50.0) & (sampled <= 50.0)).sum(axis=1)
    rows = np.flatnonzero(count > 125)
    if not rows.size:
        raise RuntimeError("no valid bar rows")
    return int(rows[0]), int(rows[-1])


def camera_images(scan_dir):
    return {
        camera: tifffile.memmap(scan_dir / spec["glob"])
        for camera, spec in geom.CAMERAS.items()
    }


def build_calibration(root, reference, calibration_head, calibration_tail):
    baseline_dir = next(root.glob(f"*/_positive_unpacked/{CALIBRATION_SCAN}"))
    baseline = {}
    matrices = {}
    for camera, spec in geom.CAMERAS.items():
        image = tifffile.memmap(baseline_dir / spec["glob"])
        left, right = slice_cal.fixed_face_columns(image, int(CALIBRATION_ROWS[len(CALIBRATION_ROWS) // 2]))
        baseline[camera] = slice_cal.extract_corner_series(image, CALIBRATION_ROWS, left, right)
        matrices[camera] = slice_cal.orthogonal_map(camera, baseline[camera])

    origins = {}
    for end, index in (("head", 0), ("tail", -1)):
        row = int(CALIBRATION_ROWS[index])
        fraction = float(np.clip((row - calibration_head) / (calibration_tail - calibration_head), 0, 1))
        standards = cmm_cal.standard_at_fraction(reference, fraction)
        nominal = cmm_cal.nominal_corners(standards)
        origins[end] = {}
        for camera in geom.CAMERA_ORDER:
            local = np.array([baseline[camera]["u_mm"][index], baseline[camera]["z_mm"][index]])
            origins[end][camera] = nominal[camera] - matrices[camera] @ local
    return matrices, origins


def section_edges(images, row, matrices, origins):
    points = {}
    for camera in geom.CAMERA_ORDER:
        section = geom.fit_section(images[camera], int(row))
        local = np.array([section["u_mm"], section["z_mm"]])
        points[camera] = origins[camera] + matrices[camera] @ local
    return {
        edge: float(np.linalg.norm(points[camera1] - points[camera2]))
        for edge, (camera1, camera2) in EDGE_PAIRS.items()
    }


def chamfer_segments(width, height, features):
    p = {corner: features[corner]["projection_45_mm"] for corner in CORNER_CAMERA}
    return {
        "AB": [np.array([p["AB"], 0.0]), np.array([0.0, p["AB"]])],
        "AD": [np.array([width - p["AD"], 0.0]), np.array([width, p["AD"]])],
        "CD": [np.array([width - p["CD"], height]), np.array([width, height - p["CD"]])],
        "BC": [np.array([0.0, height - p["BC"]]), np.array([p["BC"], height])],
    }


def max_segment_distance(segment1, segment2):
    return max(float(np.linalg.norm(p1 - p2)) for p1 in segment1 for p2 in segment2)


def section_measurement(images, row, matrices, origins):
    edges = section_edges(images, row, matrices, origins)
    features = {
        corner: chamfer_feature(images[camera], row)
        for corner, camera in CORNER_CAMERA.items()
    }
    width = (edges["A"] + edges["C"]) / 2.0
    height = (edges["B"] + edges["D"]) / 2.0
    segments = chamfer_segments(width, height, features)
    return {
        "row": int(row),
        "edges": edges,
        "features": features,
        "diag1": max_segment_distance(segments["AB"], segments["CD"]),
        "diag2": max_segment_distance(segments["AD"], segments["BC"]),
    }


def remove_ex(value):
    if value > 50:
        return value - 50
    if value < -50:
        return value + 50
    return value


def end_face_angles(starts, ends, head_edges, tail_edges):
    pairs = {
        "A": ("Top", "Right"),
        "B": ("Top", "Down"),
        "C": ("Down", "Left"),
        "D": ("Right", "Left"),
    }
    row_offsets = {
        "head": {"A": -12, "B": 368, "C": 10, "D": 386},
        "tail": {"A": -4, "B": 356, "C": 18, "D": 378},
    }
    mm_offsets = {
        "head": {"A": -0.10, "B": 0.26, "C": -0.15, "D": 0.23},
        "tail": {"A": -0.15, "B": 0.68, "C": -0.30, "D": 0.68},
    }
    output = {"hardcoded": {}, "no_offset": {}, "delta": {}, "axial_delta_mm": {}}
    for end, rows, edges in (("head", starts, head_edges), ("tail", ends, tail_edges)):
        for edge, (camera1, camera2) in pairs.items():
            raw_difference = rows[camera1] - rows[camera2]
            direct_mm = raw_difference * geom.Y_SCALE_MM_PER_ROW
            hard_rows = remove_ex(raw_difference + row_offsets[end][edge])
            hard_mm = hard_rows * 0.05 + mm_offsets[end][edge]
            direct_angle = 90.0 - math.degrees(math.atan2(abs(direct_mm), edges[edge]))
            hard_angle = 90.0 - math.degrees(math.atan2(abs(hard_mm), edges[edge]))
            key = f"{end}_{edge}"
            output["no_offset"][key] = direct_angle
            output["hardcoded"][key] = hard_angle
            output["delta"][key] = hard_angle - direct_angle
            output["axial_delta_mm"][key] = {"no_offset": direct_mm, "hardcoded": hard_mm}
    return output


def scan_result(scan_dir, matrices, origins, y_scale):
    images = camera_images(scan_dir)
    bounds = {camera: row_bounds(image) for camera, image in images.items()}
    starts = {camera: value[0] for camera, value in bounds.items()}
    ends = {camera: value[1] for camera, value in bounds.items()}
    common_start, common_end = max(starts.values()), min(ends.values())
    head_rows = np.rint(np.linspace(common_start + 250, common_start + 1250, 5)).astype(int)
    tail_rows = np.rint(np.linspace(common_end - 1250, common_end - 250, 5)).astype(int)
    body_rows = np.rint(np.linspace(common_start + 300, common_end - 300, 25)).astype(int)

    head_sections = [section_measurement(images, row, matrices, origins["head"]) for row in head_rows]
    tail_sections = [section_measurement(images, row, matrices, origins["tail"]) for row in tail_rows]
    body_features = []
    failures = []
    for row in body_rows:
        try:
            body_features.append({
                "row": int(row),
                "features": {
                    corner: chamfer_feature(images[camera], row)
                    for corner, camera in CORNER_CAMERA.items()
                },
            })
        except Exception as exc:
            failures.append({"row": int(row), "error": str(exc)})

    head_edges = {edge: mean([item["edges"][edge] for item in head_sections]) for edge in EDGE_PAIRS}
    tail_edges = {edge: mean([item["edges"][edge] for item in tail_sections]) for edge in EDGE_PAIRS}
    arcs = {
        corner: mean([item["features"][corner]["chord_mm"] for item in body_features])
        for corner in CORNER_CAMERA
    }
    side_angles = {
        corner: mean([item["features"][corner]["side_angle_deg"] for item in body_features])
        for corner in CORNER_CAMERA
    }
    lengths = {camera: (end - starts[camera]) * y_scale for camera, end in ends.items()}
    end_angles = end_face_angles(starts, ends, head_edges, tail_edges)

    requested = {
        "长度": max(lengths.values()),
        **{f"头-边长{edge}": head_edges[edge] for edge in EDGE_PAIRS},
        **{f"尾-边长{edge}": tail_edges[edge] for edge in EDGE_PAIRS},
        "头-对角线1": mean([item["diag1"] for item in head_sections]),
        "头-对角线2": mean([item["diag2"] for item in head_sections]),
        "尾-对角线1": mean([item["diag1"] for item in tail_sections]),
        "尾-对角线2": mean([item["diag2"] for item in tail_sections]),
    }
    for corner in ("AB", "BC", "CD", "AD"):
        projection = arcs[corner] / math.sqrt(2.0)
        requested[f"弧长投影{corner}-1"] = projection
        requested[f"弧长投影{corner}-2"] = projection
    requested["弧长AB"] = arcs["AB"]
    requested["弧长BD"] = arcs["BC"]
    requested["弧长CD"] = arcs["CD"]
    requested["弧长AD"] = arcs["AD"]
    for index, corner in enumerate(("AB", "BC", "CD", "AD"), 1):
        requested[f"侧面垂直度{index}"] = side_angles[corner]
    requested_no_offset = dict(requested)
    requested_hardcoded = dict(requested)
    for end_cn, end_en in (("头", "head"), ("尾", "tail")):
        for edge in EDGE_PAIRS:
            field = f"{end_cn}-端面垂直度-{edge}"
            requested_no_offset[field] = end_angles["no_offset"][f"{end_en}_{edge}"]
            requested_hardcoded[field] = end_angles["hardcoded"][f"{end_en}_{edge}"]

    return {
        "scan": scan_dir.name,
        "requested_no_offset_endface": requested_no_offset,
        "requested_hardcoded_endface": requested_hardcoded,
        "endface_comparison": end_angles,
        "diagnostics": {
            "row_bounds": bounds,
            "camera_lengths_mm": lengths,
            "head_rows": head_rows.tolist(),
            "tail_rows": tail_rows.tolist(),
            "body_rows": body_rows.tolist(),
            "body_failures": failures,
            "head_is_calibration_extrapolation": True,
            "tail_is_within_calibration_coverage": True,
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
    output_dir = root / "被测棒数据" / "_analysis" / "2606005B22-CTB_full_metrics"
    output_dir.mkdir(parents=True, exist_ok=True)
    reference = cmm_cal.read_cmm_reference(root)
    prior = json.loads((root / "标定棒数据" / "_positive_analysis" / "positive_repeatability_results.json").read_text(encoding="utf-8"))
    calibration_bounds = prior["bounds"][CALIBRATION_SCAN]
    calibration_head = float(np.median([item["row0"] for item in calibration_bounds.values()]))
    calibration_tail = float(np.median([item["row1"] for item in calibration_bounds.values()]))
    y_scale = reference["length_mm"] / (calibration_tail - calibration_head)
    matrices, origins = build_calibration(root, reference, calibration_head, calibration_tail)

    specimen_dir = next(root.glob(f"*/_positive_unpacked/{SPECIMEN}"))
    results = [scan_result(scan_dir, matrices, origins, y_scale) for scan_dir in sorted(specimen_dir.iterdir()) if scan_dir.is_dir()]
    metric_order = list(results[0]["requested_no_offset_endface"])
    summary_no_offset = {
        metric: mean([result["requested_no_offset_endface"][metric] for result in results])
        for metric in metric_order
    }
    summary_hardcoded = {
        metric: mean([result["requested_hardcoded_endface"][metric] for result in results])
        for metric in metric_order
    }
    spread_no_offset = {
        metric: float(np.ptp([result["requested_no_offset_endface"][metric] for result in results]))
        for metric in metric_order
    }
    payload = {
        "specimen": SPECIMEN,
        "method": {
            "calibration": "positive calibration-bar scan 11_56 plus CMM dimensions",
            "projection": "45 degree: chord/sqrt(2), projection 1 equals projection 2",
            "diagonal": "maximum endpoint distance between opposite finite chamfer segments",
            "endface_main": "analytic face angle using HDEV row/mm hardcoded offsets",
            "endface_comparison": "same analytic angle with all HDEV offsets removed",
            "calibrated_y_scale_mm_per_row": y_scale,
            "calibration_row_coverage": [calibration_head, calibration_tail],
        },
        "scans": results,
        "three_scan_mean_no_offset": summary_no_offset,
        "three_scan_mean_hardcoded": summary_hardcoded,
        "three_scan_range_no_offset": spread_no_offset,
    }
    (output_dir / "ctb_full_metrics.json").write_text(json.dumps(payload, ensure_ascii=False, indent=2), encoding="utf-8")

    rows = []
    for metric in metric_order:
        row = {"指标": metric}
        for result in results:
            row[result["scan"]] = result["requested_no_offset_endface"][metric]
        row["三次平均"] = summary_no_offset[metric]
        row["三次极差"] = spread_no_offset[metric]
        row["单位"] = "°" if "垂直度" in metric else "mm"
        rows.append(row)
    write_csv(output_dir / "CTB_37项测量结果.csv", rows)

    hardcoded_rows = []
    for metric in metric_order:
        row = {"指标": metric}
        for result in results:
            row[result["scan"]] = result["requested_hardcoded_endface"][metric]
        row["三次平均"] = summary_hardcoded[metric]
        row["单位"] = "°" if "垂直度" in metric else "mm"
        hardcoded_rows.append(row)
    write_csv(output_dir / "CTB_37项测量结果_旧硬编码端面.csv", hardcoded_rows)

    comparison_rows = []
    for result in results:
        for key in result["endface_comparison"]["hardcoded"]:
            comparison_rows.append({
                "扫描": result["scan"],
                "端面_边": key,
                "有硬编码偏移_度": result["endface_comparison"]["hardcoded"][key],
                "无偏移_度": result["endface_comparison"]["no_offset"][key],
                "有偏移减无偏移_度": result["endface_comparison"]["delta"][key],
                "有偏移轴向差_mm": result["endface_comparison"]["axial_delta_mm"][key]["hardcoded"],
                "无偏移轴向差_mm": result["endface_comparison"]["axial_delta_mm"][key]["no_offset"],
            })
    write_csv(output_dir / "CTB_端面垂直度_硬编码对比.csv", comparison_rows)
    print(json.dumps({"output_dir": str(output_dir), "three_scan_mean_no_offset": summary_no_offset}, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
