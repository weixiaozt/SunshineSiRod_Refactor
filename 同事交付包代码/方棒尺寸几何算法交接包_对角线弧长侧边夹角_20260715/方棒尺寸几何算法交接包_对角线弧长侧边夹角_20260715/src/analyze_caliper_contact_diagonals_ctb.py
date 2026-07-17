import csv
import json
import math
from pathlib import Path

import numpy as np
import tifffile

import analyze_absolute_slice_calibration as slice_cal
import analyze_cmm_calibrated_sparse_slices as cmm_cal
import analyze_positive_repeatability as geom
import measure_ctb_full_metrics as metric


SPECIMEN = "2606005B22-CTB"
CALIBRATION_SCAN = "11_56"
ROWS = np.rint(np.linspace(9000, 18000, 21)).astype(int)
CORNER_CAMERA = {"AB": "Right", "BC": "Left", "CD": "Down", "AD": "Top"}
DIAGONALS = {"D1": ("AB", "CD"), "D2": ("AD", "BC")}
REVERSED_CROP_RAW_ORIGIN_PX = 2699


def robust_quadratic(x, z):
    keep = np.isfinite(x) & np.isfinite(z)
    for _ in range(6):
        coefficient = np.polyfit(x[keep], z[keep], 2)
        residual = z - np.polyval(coefficient, x)
        center = np.median(residual[keep])
        mad = np.median(np.abs(residual[keep] - center))
        limit = max(4.0 * 1.4826 * mad, 0.006)
        new_keep = keep & (np.abs(residual - center) <= limit)
        if np.array_equal(new_keep, keep):
            break
        keep = new_keep
    return coefficient, keep


def chamfer_curve_local(image, row, dense_count=401):
    """Return the measured chamfer as a denoised curve in raw camera coordinates."""
    profile = geom.section_profile(image, int(row))[::-1][500:3200]
    valid = np.isfinite(profile) & (profile > geom.INVALID_THRESHOLD) & (profile < 9999)
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

    left = metric.line_fit(x[left_a:left_b], z[left_a:left_b], 50)
    chord = metric.line_fit(x[chord_a:chord_b], z[chord_a:chord_b], 10)
    right = metric.line_fit(x[right_a:right_b], z[right_a:right_b], 50)
    endpoint1 = metric.intersection(left, chord)
    endpoint2 = metric.intersection(chord, right)
    curve_lo, curve_hi = sorted((float(endpoint1[0]), float(endpoint2[0])))

    # Include a small shoulder around the fitted tangency range. It prevents the
    # support point from being decided by the arbitrary 57-point chord window.
    shoulder = max(0.04, 0.05 * (curve_hi - curve_lo))
    selected = (x >= curve_lo - shoulder) & (x <= curve_hi + shoulder)
    coefficient, keep = robust_quadratic(x[selected], z[selected])
    fit_x = x[selected][keep]
    eval_x = np.linspace(max(curve_lo, fit_x.min()), min(curve_hi, fit_x.max()), dense_count)
    eval_z = np.polyval(coefficient, eval_x)

    k_mm = REVERSED_CROP_RAW_ORIGIN_PX * geom.X_SCALE_MM_PER_PX
    raw_curve = np.column_stack([k_mm - eval_x, eval_z])
    raw_points = np.column_stack([k_mm - x[selected][keep], z[selected][keep]])
    fit_rms = math.sqrt(float(np.mean((z[selected][keep] - np.polyval(coefficient, x[selected][keep])) ** 2)))
    return {
        "curve": raw_curve,
        "raw_points": raw_points,
        "fit_rms_um": fit_rms * 1000.0,
        "point_count": int(keep.sum()),
    }


def full_corner_profile_local(image, row):
    """Return the full measured corner profile, lightly median-smoothed."""
    profile = geom.section_profile(image, int(row))
    valid = np.isfinite(profile) & (profile > geom.INVALID_THRESHOLD) & (profile < 9999)
    columns = np.flatnonzero(valid).astype(float)
    z = profile[valid]
    if z.size < 1200:
        raise RuntimeError(f"too few contour points at row {row}")
    padded = np.pad(z, (2, 2), mode="edge")
    z_smooth = np.median(np.lib.stride_tricks.sliding_window_view(padded, 5), axis=1)
    return np.column_stack([columns * geom.X_SCALE_MM_PER_PX, z_smooth])


def rotate(vector, degrees):
    angle = math.radians(degrees)
    matrix = np.array([[math.cos(angle), -math.sin(angle)], [math.sin(angle), math.cos(angle)]])
    return matrix @ vector


def caliper_support(curve_positive, curve_negative, direction):
    direction = np.asarray(direction, dtype=float)
    direction /= np.linalg.norm(direction)
    positive_index = int(np.argmax(curve_positive @ direction))
    negative_index = int(np.argmin(curve_negative @ direction))
    positive = curve_positive[positive_index]
    negative = curve_negative[negative_index]
    separation = float((positive - negative) @ direction)
    euclidean = float(np.linalg.norm(positive - negative))
    lateral = math.sqrt(max(0.0, euclidean**2 - separation**2))
    return {
        "caliper_mm": separation,
        "contact_euclidean_mm": euclidean,
        "contact_lateral_offset_mm": lateral,
        "positive_contact": positive,
        "negative_contact": negative,
        "direction": direction,
    }


def optimized_caliper(curve_positive, curve_negative, nominal_direction, limit_deg=5.0):
    candidates = []
    for offset in np.linspace(-limit_deg, limit_deg, 1001):
        result = caliper_support(curve_positive, curve_negative, rotate(nominal_direction, offset))
        candidates.append((result["caliper_mm"], float(offset), result))
    _, offset, result = max(candidates, key=lambda item: item[0])
    result["offset_deg"] = offset
    return result


def describe(values):
    values = np.asarray(values, dtype=float)
    return {
        "mean_mm": float(np.mean(values)),
        "std_mm": float(np.std(values, ddof=1)) if values.size > 1 else 0.0,
        "min_mm": float(np.min(values)),
        "max_mm": float(np.max(values)),
        "range_mm": float(np.ptp(values)),
    }


def write_csv(path, rows):
    rows = list(rows)
    with path.open("w", newline="", encoding="utf-8-sig") as handle:
        writer = csv.DictWriter(handle, fieldnames=list(rows[0]))
        writer.writeheader()
        writer.writerows(rows)


def main():
    root = Path.cwd()
    output_dir = root / "被测棒数据" / "_analysis" / f"{SPECIMEN}_caliper_diagonals"
    output_dir.mkdir(parents=True, exist_ok=True)
    baseline_dir = next(root.glob(f"*/_positive_unpacked/{CALIBRATION_SCAN}"))
    specimen_dir = next(root.glob(f"*/_positive_unpacked/{SPECIMEN}"))

    reference = cmm_cal.read_cmm_reference(root)
    prior_path = next(root.glob("*/_positive_analysis/positive_repeatability_results.json"))
    prior = json.loads(prior_path.read_text(encoding="utf-8"))
    bounds = prior["bounds"][CALIBRATION_SCAN]
    head_row = float(np.median([item["row0"] for item in bounds.values()]))
    tail_row = float(np.median([item["row1"] for item in bounds.values()]))

    baseline = {}
    matrices = {}
    windows = {}
    for camera, spec in geom.CAMERAS.items():
        image = tifffile.memmap(baseline_dir / spec["glob"])
        left, right = slice_cal.fixed_face_columns(image, int(ROWS[len(ROWS) // 2]))
        windows[camera] = (left, right)
        baseline[camera] = slice_cal.extract_corner_series(image, ROWS, left, right)
        matrices[camera] = slice_cal.orthogonal_map(camera, baseline[camera])

    origins = []
    for index, row in enumerate(ROWS):
        fraction = float(np.clip((row - head_row) / (tail_row - head_row), 0.0, 1.0))
        nominal = cmm_cal.nominal_corners(cmm_cal.standard_at_fraction(reference, fraction))
        row_origins = {}
        for camera in geom.CAMERA_ORDER:
            local = np.array([baseline[camera]["u_mm"][index], baseline[camera]["z_mm"][index]])
            row_origins[camera] = nominal[camera] - matrices[camera] @ local
        origins.append(row_origins)

    records = []
    for scan_dir in sorted(path for path in specimen_dir.iterdir() if path.is_dir()):
        images = {camera: tifffile.memmap(scan_dir / spec["glob"]) for camera, spec in geom.CAMERAS.items()}
        corner_series = {
            camera: slice_cal.extract_corner_series(images[camera], ROWS, *windows[camera])
            for camera in geom.CAMERAS
        }
        for index, row in enumerate(ROWS):
            corner_points = {}
            curves = {}
            full_profiles = {}
            neighborhoods = {}
            curve_quality = {}
            for corner, camera in CORNER_CAMERA.items():
                local_corner = np.array([
                    corner_series[camera]["u_mm"][index],
                    corner_series[camera]["z_mm"][index],
                ])
                corner_points[corner] = origins[index][camera] + matrices[camera] @ local_corner
                feature = chamfer_curve_local(images[camera], row)
                curves[corner] = (matrices[camera] @ feature["curve"].T).T + origins[index][camera]
                full_profile = full_corner_profile_local(images[camera], row)
                full_profile_global = (
                    matrices[camera] @ full_profile.T
                ).T + origins[index][camera]
                full_profiles[corner] = full_profile_global
                neighborhoods[corner] = full_profile_global[
                    np.linalg.norm(full_profile_global - corner_points[corner], axis=1) <= 2.5
                ]
                curve_quality[corner] = feature

            record = {"scan": scan_dir.name, "absolute_row": int(row)}
            for diagonal, (positive_corner, negative_corner) in DIAGONALS.items():
                nominal_direction = corner_points[positive_corner] - corner_points[negative_corner]
                nominal = caliper_support(curves[positive_corner], curves[negative_corner], nominal_direction)
                optimized = optimized_caliper(curves[positive_corner], curves[negative_corner], nominal_direction)
                full_profile = caliper_support(
                    neighborhoods[positive_corner], neighborhoods[negative_corner], nominal_direction
                )
                full_profile_optimized = optimized_caliper(
                    neighborhoods[positive_corner], neighborhoods[negative_corner], nominal_direction
                )
                positive_apex_index = int(
                    np.argmin(np.linalg.norm(curves[positive_corner] - corner_points[positive_corner], axis=1))
                )
                negative_apex_index = int(
                    np.argmin(np.linalg.norm(curves[negative_corner] - corner_points[negative_corner], axis=1))
                )
                positive_apex = curves[positive_corner][positive_apex_index]
                negative_apex = curves[negative_corner][negative_apex_index]
                apex_distance = float(np.linalg.norm(positive_apex - negative_apex))
                endpoint_distance = max(
                    float(np.linalg.norm(point1 - point2))
                    for point1 in curves[positive_corner][[0, -1]]
                    for point2 in curves[negative_corner][[0, -1]]
                )
                record[f"{diagonal}_arc_apex_distance_mm"] = apex_distance
                record[f"{diagonal}_endpoint_max_distance_mm"] = endpoint_distance
                record[f"{diagonal}_full_profile_support_mm"] = full_profile["caliper_mm"]
                record[f"{diagonal}_full_profile_contact_euclidean_mm"] = full_profile["contact_euclidean_mm"]
                record[f"{diagonal}_full_profile_optimized_mm"] = full_profile_optimized["caliper_mm"]
                record[f"{diagonal}_full_profile_optimized_angle_offset_deg"] = full_profile_optimized["offset_deg"]
                for radius in (1.5, 2.0, 2.5, 3.0, 4.0):
                    positive_near = full_profiles[positive_corner][
                        np.linalg.norm(
                            full_profiles[positive_corner] - corner_points[positive_corner], axis=1
                        ) <= radius
                    ]
                    negative_near = full_profiles[negative_corner][
                        np.linalg.norm(
                            full_profiles[negative_corner] - corner_points[negative_corner], axis=1
                        ) <= radius
                    ]
                    radius_support = caliper_support(
                        positive_near, negative_near, full_profile_optimized["direction"]
                    )
                    radius_key = str(radius).replace(".", "p")
                    record[f"{diagonal}_radius_{radius_key}_mm"] = radius_support["caliper_mm"]
                record[f"{diagonal}_positive_arc_apex_fraction"] = positive_apex_index / (len(curves[positive_corner]) - 1)
                record[f"{diagonal}_negative_arc_apex_fraction"] = negative_apex_index / (len(curves[negative_corner]) - 1)
                record[f"{diagonal}_nominal_caliper_mm"] = nominal["caliper_mm"]
                record[f"{diagonal}_nominal_contact_euclidean_mm"] = nominal["contact_euclidean_mm"]
                record[f"{diagonal}_nominal_lateral_offset_mm"] = nominal["contact_lateral_offset_mm"]
                record[f"{diagonal}_optimized_caliper_mm"] = optimized["caliper_mm"]
                record[f"{diagonal}_optimized_angle_offset_deg"] = optimized["offset_deg"]
                for prefix, point in (("positive", nominal["positive_contact"]), ("negative", nominal["negative_contact"])):
                    record[f"{diagonal}_{prefix}_contact_x_mm"] = float(point[0])
                    record[f"{diagonal}_{prefix}_contact_z_mm"] = float(point[1])
                for prefix, point in (("positive", positive_apex), ("negative", negative_apex)):
                    record[f"{diagonal}_{prefix}_arc_apex_x_mm"] = float(point[0])
                    record[f"{diagonal}_{prefix}_arc_apex_z_mm"] = float(point[1])
            for corner in CORNER_CAMERA:
                record[f"{corner}_curve_fit_rms_um"] = curve_quality[corner]["fit_rms_um"]
                record[f"{corner}_curve_point_count"] = curve_quality[corner]["point_count"]
            records.append(record)

    summaries = []
    for scan in sorted({record["scan"] for record in records}) + ["ALL_3_SCANS"]:
        selected = records if scan == "ALL_3_SCANS" else [record for record in records if record["scan"] == scan]
        item = {"scan": scan, "slice_count": len(selected)}
        for diagonal in DIAGONALS:
            for method in (
                "arc_apex_distance",
                "endpoint_max_distance",
                "nominal_caliper",
                "optimized_caliper",
                "full_profile_support",
                "full_profile_optimized",
            ):
                stats = describe([record[f"{diagonal}_{method}_mm"] for record in selected])
                for name, value in stats.items():
                    item[f"{diagonal}_{method}_{name}"] = value
            item[f"{diagonal}_optimized_angle_offset_mean_deg"] = float(
                np.mean([record[f"{diagonal}_optimized_angle_offset_deg"] for record in selected])
            )
        summaries.append(item)

    write_csv(output_dir / "caliper_diagonal_per_slice.csv", records)
    write_csv(output_dir / "caliper_diagonal_summary.csv", summaries)
    payload = {
        "specimen": SPECIMEN,
        "recommended_method": "full_profile_optimized",
        "definition": "parallel-jaw support distance on the complete measured corner profile, including chamfer-to-face shoulders; jaw direction may rotate within +/-5 degrees to simulate manual maximum-seeking",
        "rows": ROWS.tolist(),
        "summary": summaries,
    }
    (output_dir / "caliper_diagonal_results.json").write_text(
        json.dumps(payload, ensure_ascii=False, indent=2), encoding="utf-8"
    )
    print(json.dumps(payload, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
