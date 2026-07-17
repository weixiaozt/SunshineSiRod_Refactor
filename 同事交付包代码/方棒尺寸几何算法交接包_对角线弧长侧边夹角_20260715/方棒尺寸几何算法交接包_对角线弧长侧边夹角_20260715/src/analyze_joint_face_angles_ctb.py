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
ROWS = np.rint(np.linspace(9000, 18000, 21)).astype(int)
FACE_SOURCES = {
    "A": (("Top", "right"), ("Right", "left")),
    "B": (("Right", "right"), ("Left", "left")),
    "C": (("Left", "right"), ("Down", "left")),
    "D": (("Down", "right"), ("Top", "left")),
}
CORNER_FACES = {"AB": ("A", "B"), "BC": ("B", "C"), "CD": ("C", "D"), "AD": ("A", "D")}
CORNER_CAMERA = {"AB": "Right", "BC": "Left", "CD": "Down", "AD": "Top"}


def tls_direction(points):
    points = np.asarray(points, dtype=float)
    center = np.mean(points, axis=0)
    centered = points - center
    covariance = centered.T @ centered
    eigenvalues, eigenvectors = np.linalg.eigh(covariance)
    direction = eigenvectors[:, np.argmax(eigenvalues)]
    direction /= np.linalg.norm(direction)
    normal = np.array([-direction[1], direction[0]])
    rms = math.sqrt(float(np.mean((centered @ normal) ** 2)))
    return direction, center, rms


def shared_direction(groups):
    centers = [np.mean(group, axis=0) for group in groups]
    centered_groups = [group - center for group, center in zip(groups, centers)]
    centered = np.vstack(centered_groups)
    covariance = centered.T @ centered
    eigenvalues, eigenvectors = np.linalg.eigh(covariance)
    direction = eigenvectors[:, np.argmax(eigenvalues)]
    direction /= np.linalg.norm(direction)
    normal = np.array([-direction[1], direction[0]])
    residual = np.concatenate([group @ normal for group in centered_groups])
    rms = math.sqrt(float(np.mean(residual**2)))
    offset_gap = abs(float((centers[1] - centers[0]) @ normal))
    individual = [tls_direction(group)[0] for group in groups]
    direction_delta = acute_angle(individual[0], individual[1])
    return direction, rms, offset_gap, direction_delta


def acute_angle(vector1, vector2):
    cosine = float(np.clip(abs(np.dot(vector1, vector2)), 0.0, 1.0))
    return math.degrees(math.acos(cosine))


def describe(values):
    array = np.asarray(values, dtype=float)
    return {
        "mean_deg": float(np.mean(array)),
        "std_deg": float(np.std(array, ddof=1)) if array.size > 1 else 0.0,
        "min_deg": float(np.min(array)),
        "max_deg": float(np.max(array)),
        "range_deg": float(np.ptp(array)),
    }


def write_csv(path, rows):
    rows = list(rows)
    with path.open("w", newline="", encoding="utf-8-sig") as handle:
        writer = csv.DictWriter(handle, fieldnames=list(rows[0]))
        writer.writeheader()
        writer.writerows(rows)


def main():
    root = Path.cwd()
    output_dir = root / "被测棒数据" / "_analysis" / "2606005B22-CTB_joint_face_angles"
    output_dir.mkdir(parents=True, exist_ok=True)
    baseline_dir = next(root.glob("*/_positive_unpacked/11_56"))
    specimen_dir = next(root.glob(f"*/_positive_unpacked/{SPECIMEN}"))

    reference = cmm_cal.read_cmm_reference(root)
    prior_path = next(root.glob("*/_positive_analysis/positive_repeatability_results.json"))
    prior = json.loads(prior_path.read_text(encoding="utf-8"))
    bounds = prior["bounds"]["11_56"]
    head_row = float(np.median([item["row0"] for item in bounds.values()]))
    tail_row = float(np.median([item["row1"] for item in bounds.values()]))

    windows = {}
    baseline = {}
    matrices = {}
    origins = []
    for camera, spec in geom.CAMERAS.items():
        image = tifffile.memmap(baseline_dir / spec["glob"])
        left, right = slice_cal.fixed_face_columns(image, int(ROWS[len(ROWS) // 2]))
        windows[camera] = {"left": left, "right": right}
        baseline[camera] = slice_cal.extract_corner_series(image, ROWS, left, right)
        matrices[camera] = slice_cal.orthogonal_map(camera, baseline[camera])

    for index, row in enumerate(ROWS):
        fraction = float(np.clip((row - head_row) / (tail_row - head_row), 0.0, 1.0))
        nominal = cmm_cal.nominal_corners(cmm_cal.standard_at_fraction(reference, fraction))
        item = {}
        for camera in geom.CAMERA_ORDER:
            local = np.array([baseline[camera]["u_mm"][index], baseline[camera]["z_mm"][index]])
            item[camera] = nominal[camera] - matrices[camera] @ local
        origins.append(item)

    all_rows = []
    scan_summaries = []
    for scan_dir in sorted(path for path in specimen_dir.iterdir() if path.is_dir()):
        images = {
            camera: tifffile.memmap(scan_dir / spec["glob"])
            for camera, spec in geom.CAMERAS.items()
        }
        face_points = {camera: {} for camera in geom.CAMERAS}
        current_sections = {}
        for camera in geom.CAMERAS:
            current_sections[camera] = slice_cal.extract_corner_series(
                images[camera], ROWS, windows[camera]["left"], windows[camera]["right"]
            )
            for side in ("left", "right"):
                columns = windows[camera][side]
                z = slice_cal.band_profiles(images[camera], ROWS, columns)
                x = columns.astype(float) * geom.X_SCALE_MM_PER_PX
                face_points[camera][side] = (x, z)

        scan_records = []
        for index, row in enumerate(ROWS):
            transformed = {}
            for camera in geom.CAMERAS:
                transformed[camera] = {}
                for side in ("left", "right"):
                    x, z_matrix = face_points[camera][side]
                    z = z_matrix[index]
                    valid = np.isfinite(z)
                    local = np.column_stack([x[valid], z[valid]])
                    transformed[camera][side] = (
                        matrices[camera] @ local.T
                    ).T + origins[index][camera]

            direct_direction = {}
            shared = {}
            face_quality = {}
            for face, sources in FACE_SOURCES.items():
                groups = [transformed[camera][side] for camera, side in sources]
                direct_direction[face], _, direct_rms = tls_direction(np.vstack(groups))
                shared_direction_value, shared_rms, offset_gap, direction_delta = shared_direction(groups)
                shared[face] = shared_direction_value
                face_quality[face] = {
                    "direct_rms_um": direct_rms * 1000.0,
                    "shared_rms_um": shared_rms * 1000.0,
                    "inter_camera_normal_gap_um": offset_gap * 1000.0,
                    "two_camera_direction_delta_deg": direction_delta,
                }

            direct_angles = {
                corner: acute_angle(direct_direction[face1], direct_direction[face2])
                for corner, (face1, face2) in CORNER_FACES.items()
            }
            shared_angles = {
                corner: acute_angle(shared[face1], shared[face2])
                for corner, (face1, face2) in CORNER_FACES.items()
            }
            single_angles = {}
            for corner, camera in CORNER_CAMERA.items():
                left_angle = math.degrees(math.atan(current_sections[camera]["left_slope"][index]))
                right_angle = math.degrees(math.atan(current_sections[camera]["right_slope"][index]))
                single_angles[corner] = 180.0 - abs(left_angle) - abs(right_angle)

            record = {"scan": scan_dir.name, "absolute_row": int(row)}
            for corner in CORNER_FACES:
                record[f"{corner}_single_camera_deg"] = single_angles[corner]
                record[f"{corner}_joint_common_line_deg"] = direct_angles[corner]
                record[f"{corner}_joint_shared_direction_deg"] = shared_angles[corner]
            for face in FACE_SOURCES:
                for metric_name, value in face_quality[face].items():
                    record[f"face_{face}_{metric_name}"] = value
            all_rows.append(record)
            scan_records.append(record)

        for corner in CORNER_FACES:
            summary = {"scan": scan_dir.name, "corner": corner, "slice_count": len(scan_records)}
            for method in ("single_camera", "joint_common_line", "joint_shared_direction"):
                stats = describe([item[f"{corner}_{method}_deg"] for item in scan_records])
                for name, value in stats.items():
                    summary[f"{method}_{name}"] = value
            scan_summaries.append(summary)

    aggregate = []
    for corner in CORNER_FACES:
        item = {"scan": "ALL_3_SCANS", "corner": corner, "slice_count": len(all_rows)}
        for method in ("single_camera", "joint_common_line", "joint_shared_direction"):
            stats = describe([row[f"{corner}_{method}_deg"] for row in all_rows])
            for name, value in stats.items():
                item[f"{method}_{name}"] = value
        aggregate.append(item)

    face_quality_summary = []
    for face in FACE_SOURCES:
        item = {"face": face, "sample_count": len(all_rows)}
        for metric_name in (
            "direct_rms_um",
            "shared_rms_um",
            "inter_camera_normal_gap_um",
            "two_camera_direction_delta_deg",
        ):
            values = [row[f"face_{face}_{metric_name}"] for row in all_rows]
            item[f"{metric_name}_mean"] = float(np.mean(values))
            item[f"{metric_name}_max"] = float(np.max(values))
        face_quality_summary.append(item)

    write_csv(output_dir / "joint_face_angle_per_slice.csv", all_rows)
    write_csv(output_dir / "joint_face_angle_per_scan_summary.csv", scan_summaries)
    write_csv(output_dir / "joint_face_angle_all_scans_summary.csv", aggregate)
    write_csv(output_dir / "joint_face_fit_quality.csv", face_quality_summary)
    payload = {
        "specimen": SPECIMEN,
        "scans": sorted({row["scan"] for row in all_rows}),
        "absolute_rows": ROWS.tolist(),
        "methods": {
            "single_camera": "two short face segments from the corner camera",
            "joint_common_line": "all points from both cameras fitted to one TLS line per physical face",
            "joint_shared_direction": "both cameras share one face direction but keep independent normal offsets",
        },
        "per_scan_summary": scan_summaries,
        "all_scans_summary": aggregate,
        "face_fit_quality": face_quality_summary,
    }
    (output_dir / "joint_face_angle_results.json").write_text(
        json.dumps(payload, ensure_ascii=False, indent=2), encoding="utf-8"
    )
    print(json.dumps({"output_dir": str(output_dir), "all_scans_summary": aggregate, "face_fit_quality": face_quality_summary}, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
