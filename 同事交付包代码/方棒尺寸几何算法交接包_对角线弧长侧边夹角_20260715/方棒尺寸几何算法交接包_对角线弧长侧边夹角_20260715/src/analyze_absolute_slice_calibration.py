import csv
import json
import math
from pathlib import Path

import matplotlib.pyplot as plt
import numpy as np
import tifffile

import analyze_positive_repeatability as geom


ROW_STEP = 10
# Exclude about 10 mm at each end where the two full face segments are not
# consistently visible. These rows diagnose end geometry, not rail repeatability.
ROW_MARGIN = 200
A_MM = 210.05
B_MM = 105.05
CAMERAS = geom.CAMERAS
CAMERA_ORDER = geom.CAMERA_ORDER
EDGE_PAIRS = {
    "A": ("Top", "Right", A_MM),
    "B": ("Right", "Left", B_MM),
    "C": ("Left", "Down", A_MM),
    "D": ("Top", "Down", B_MM),
}


def write_csv(path, rows):
    rows = list(rows)
    if not rows:
        return
    keys = []
    for row in rows:
        for key in row:
            if key not in keys:
                keys.append(key)
    with path.open("w", newline="", encoding="utf-8-sig") as f:
        writer = csv.DictWriter(f, fieldnames=keys)
        writer.writeheader()
        writer.writerows(rows)


def discover(root):
    manifests = list(root.rglob("_positive_unpacked/manifest.json"))
    if len(manifests) != 1:
        raise RuntimeError(f"expected one positive manifest, found {len(manifests)}")
    unpacked = manifests[0].parent
    scan_dirs = sorted(p for p in unpacked.iterdir() if p.is_dir())
    if len(scan_dirs) < 9:
        raise RuntimeError(f"expected at least 9 scans, found {len(scan_dirs)}")
    return unpacked, scan_dirs[:9], unpacked.parent / "_positive_analysis" / "absolute_slice_calibration"


def fixed_face_columns(arr, row):
    profile = geom.section_profile(arr, row)
    ok = np.isfinite(profile)
    xs = np.flatnonzero(ok)
    z = profile[ok]
    smooth = np.convolve(z, np.ones(21) / 21.0, mode="same")
    lo = int(z.size * 0.08)
    hi = int(z.size * 0.92)
    apex_i = lo + int(np.argmax(smooth[lo:hi]))
    margin = max(75, int(z.size * 0.055))
    left = xs[int(z.size * 0.08):apex_i - margin]
    right = xs[apex_i + margin:int(z.size * 0.92)]
    if left.size < 200 or right.size < 200:
        raise RuntimeError("could not define fixed face windows")
    return left, right


def band_profiles(arr, rows, cols):
    total = np.zeros((rows.size, cols.size), dtype=np.float64)
    count = np.zeros((rows.size, cols.size), dtype=np.int16)
    for delta in range(-2, 3):
        block = np.asarray(arr[np.ix_(rows + delta, cols)], dtype=np.float64)
        valid = np.isfinite(block) & (block > geom.INVALID_THRESHOLD)
        total += np.where(valid, block, 0.0)
        count += valid
    result = np.full_like(total, np.nan)
    np.divide(total, count, out=result, where=count > 0)
    return result


def vector_line_fit(x, z):
    x = np.asarray(x, dtype=np.float64)
    valid = np.isfinite(z)
    n = valid.sum(axis=1).astype(np.float64)
    xv = valid * x[None, :]
    zv = np.where(valid, z, 0.0)
    sx = xv.sum(axis=1)
    sy = zv.sum(axis=1)
    sxx = (xv * x[None, :]).sum(axis=1)
    sxy = (xv * zv).sum(axis=1)
    denominator = n * sxx - sx * sx
    slope = (n * sxy - sx * sy) / denominator
    intercept = (sy - slope * sx) / n
    predicted = slope[:, None] * x[None, :] + intercept[:, None]
    residual = np.where(valid, z - predicted, 0.0)
    rms = np.sqrt(np.sum(residual**2, axis=1) / n)
    return slope, intercept, rms, n.astype(int)


def extract_corner_series(arr, rows, left_cols, right_cols):
    left_z = band_profiles(arr, rows, left_cols)
    right_z = band_profiles(arr, rows, right_cols)
    left_x = left_cols.astype(float) * geom.X_SCALE_MM_PER_PX
    right_x = right_cols.astype(float) * geom.X_SCALE_MM_PER_PX
    ml, bl, rms_l, count_l = vector_line_fit(left_x, left_z)
    mr, br, rms_r, count_r = vector_line_fit(right_x, right_z)
    u = (br - bl) / (ml - mr)
    z = ml * u + bl
    left_angle = np.degrees(np.arctan(ml))
    right_angle = np.degrees(np.arctan(mr))
    return {
        "u_mm": u,
        "z_mm": z,
        "left_slope": ml,
        "right_slope": mr,
        "roll_deg": (left_angle + right_angle) / 2.0,
        "included_angle_deg": left_angle - right_angle,
        "left_rms_um": rms_l * 1000.0,
        "right_rms_um": rms_r * 1000.0,
        "left_count": count_l,
        "right_count": count_r,
    }


def orthogonal_map(camera, baseline):
    ml = float(np.median(baseline["left_slope"]))
    mr = float(np.median(baseline["right_slope"]))
    left = np.array([-1.0, -ml])
    right = np.array([1.0, mr])
    left /= np.linalg.norm(left)
    right /= np.linalg.norm(right)
    local = np.column_stack([left, right])
    spec = CAMERAS[camera]
    target = np.column_stack([spec["left_dir"], spec["right_dir"]])
    u, _, vt = np.linalg.svd(target @ local.T)
    return u @ vt


def describe(values):
    a = np.asarray(values, dtype=float)
    a = a[np.isfinite(a)]
    return {
        "count": int(a.size),
        "mean": float(np.mean(a)),
        "std": float(np.std(a, ddof=1)) if a.size > 1 else 0.0,
        "rms": float(math.sqrt(np.mean(a**2))),
        "p95_abs": float(np.percentile(np.abs(a), 95)),
        "max_abs": float(np.max(np.abs(a))),
        "min": float(np.min(a)),
        "max": float(np.max(a)),
        "range": float(np.ptp(a)),
    }


def common_pose_fit(data, key):
    positions = np.asarray([row["absolute_position_mm"] for row in data], dtype=float)
    values_um = np.asarray([row[key] for row in data], dtype=float) * 1000.0
    center_position = float(np.mean(positions))
    centered = positions - center_position
    slope = float(np.dot(centered, values_um - np.mean(values_um)) / np.dot(centered, centered))
    center_value = float(np.mean(values_um))
    residual = values_um - (center_value + slope * centered)
    return {
        "center_position_mm": center_position,
        "center_value_um": center_value,
        "slope_mrad": slope,
        "residual_um": residual,
        "residual_rms_um": float(math.sqrt(np.mean(residual**2))),
    }


def summarize_common_pose(scan_names, detection_rows):
    rows = []
    for scan in scan_names:
        data = [row for row in detection_rows if row["scan"] == scan]
        x_fit = common_pose_fit(data, "common_x_mm")
        z_fit = common_pose_fit(data, "common_z_mm")
        rows.append({
            "scan": scan,
            "center_position_mm": x_fit["center_position_mm"],
            "center_x_um": x_fit["center_value_um"],
            "yaw_mrad": x_fit["slope_mrad"],
            "x_after_linear_rms_um": x_fit["residual_rms_um"],
            "center_z_um": z_fit["center_value_um"],
            "pitch_mrad": z_fit["slope_mrad"],
            "z_after_linear_rms_um": z_fit["residual_rms_um"],
            "roll_mean_mdeg": float(np.mean([row["common_roll_deg"] for row in data]) * 1000.0),
            "four_camera_rms_um": float(math.sqrt(np.mean([
                row["camera_relative_rms_um"] ** 2 for row in data
            ]))),
        })
    return rows


def plot_outputs(out_dir, rows_mm, scan_names, detection_rows, baseline, summary_rows):
    fig, axes = plt.subplots(2, 2, figsize=(14, 8), sharex=True, sharey=True, constrained_layout=True)
    for ax, edge in zip(axes.flat, EDGE_PAIRS):
        for scan in scan_names:
            data = [r for r in detection_rows if r["scan"] == scan]
            ax.plot(rows_mm, [r[f"{edge}_deviation_um"] for r in data], lw=0.8, alpha=0.8, label=scan)
        ax.axhline(0, color="0.45", lw=0.8)
        ax.set_title(f"Edge {edge}")
        ax.set_ylabel("Deviation from scan-1 slice calibration (um)")
        ax.grid(True, alpha=0.2)
    for ax in axes[1]:
        ax.set_xlabel("Absolute machine scan position (mm)")
    axes[0, 1].legend(ncol=2, fontsize=8)
    fig.suptitle("Scans 2-9 measured at the same absolute machine-row slices")
    fig.savefig(out_dir / "slice_edge_deviation_curves.png", dpi=190)
    plt.close(fig)

    metrics = ["A", "B", "C", "D", "W", "H"]
    heat = np.zeros((len(scan_names), len(metrics)))
    for i, scan in enumerate(scan_names):
        scan_summary = {r["metric"]: r for r in summary_rows if r["scan"] == scan}
        for j, metric in enumerate(metrics):
            heat[i, j] = scan_summary[metric]["rms_um"]
    fig, ax = plt.subplots(figsize=(10, 6), constrained_layout=True)
    image = ax.imshow(heat, aspect="auto", cmap="viridis")
    ax.set_xticks(np.arange(len(metrics)), metrics)
    ax.set_yticks(np.arange(len(scan_names)), scan_names)
    for i in range(heat.shape[0]):
        for j in range(heat.shape[1]):
            color = "white" if heat[i, j] > (heat.min() + heat.max()) / 2 else "black"
            ax.text(j, i, f"{heat[i, j]:.1f}", ha="center", va="center", color=color, fontsize=8)
    ax.set_title("Per-scan RMS after scan-1 absolute-slice calibration (um)")
    fig.colorbar(image, ax=ax, label="RMS (um)")
    fig.savefig(out_dir / "slice_deviation_rms_heatmap.png", dpi=190)
    plt.close(fig)

    fig, axes = plt.subplots(2, 2, figsize=(14, 8), sharex=True, constrained_layout=True)
    for ax, camera in zip(axes.flat, CAMERA_ORDER):
        u = baseline[camera]["u_mm"]
        z = baseline[camera]["z_mm"]
        ax.plot(rows_mm, (u - np.median(u)) * 1000.0, lw=1.0, label="u template")
        ax.plot(rows_mm, (z - np.median(z)) * 1000.0, lw=1.0, label="z template")
        ax.set_title(camera)
        ax.set_ylabel("Scan-1 slice value minus median (um)")
        ax.grid(True, alpha=0.2)
        ax.legend()
    for ax in axes[1]:
        ax.set_xlabel("Absolute machine scan position (mm)")
    fig.suptitle("Absolute-row slice template captured from positive scan 1")
    fig.savefig(out_dir / "scan1_absolute_slice_template.png", dpi=190)
    plt.close(fig)

    fig, ax = plt.subplots(figsize=(12, 5), constrained_layout=True)
    for scan in scan_names:
        data = [r for r in detection_rows if r["scan"] == scan]
        ax.plot(rows_mm, [r["camera_relative_rms_um"] for r in data], lw=0.8, label=scan)
    ax.set_xlabel("Absolute machine scan position (mm)")
    ax.set_ylabel("Four-camera residual after common pose removal (um)")
    ax.set_title("Relative four-camera consistency by absolute slice")
    ax.grid(True, alpha=0.2)
    ax.legend(ncol=4, fontsize=8)
    fig.savefig(out_dir / "slice_four_camera_residual.png", dpi=190)
    plt.close(fig)

    fig, axes = plt.subplots(2, 1, figsize=(12, 8), sharex=True, constrained_layout=True)
    for scan in scan_names:
        data = [row for row in detection_rows if row["scan"] == scan]
        x_fit = common_pose_fit(data, "common_x_mm")
        z_fit = common_pose_fit(data, "common_z_mm")
        axes[0].plot(rows_mm, x_fit["residual_um"], lw=0.8, label=scan)
        axes[1].plot(rows_mm, z_fit["residual_um"], lw=0.8, label=scan)
    axes[0].set_ylabel("X residual (um)")
    axes[1].set_ylabel("Z residual (um)")
    axes[1].set_xlabel("Absolute machine scan position (mm)")
    for ax in axes:
        ax.axhline(0, color="0.45", lw=0.8)
        ax.grid(True, alpha=0.2)
    axes[0].legend(ncol=4, fontsize=8)
    fig.suptitle("Common pose residual after removing each scan's rigid bar line")
    fig.savefig(out_dir / "slice_common_pose_linear_residual.png", dpi=190)
    plt.close(fig)


def main():
    root = Path.cwd()
    _, scan_dirs, out_dir = discover(root)
    out_dir.mkdir(parents=True, exist_ok=True)
    baseline_scan = scan_dirs[0].name
    detection_dirs = scan_dirs[1:9]
    detection_names = [p.name for p in detection_dirs]

    bounds = {}
    for scan_dir in scan_dirs:
        bounds[scan_dir.name] = {}
        for camera, spec in CAMERAS.items():
            arr = tifffile.memmap(scan_dir / spec["glob"])
            r0, r1, _ = geom.find_row_bounds(arr)
            bounds[scan_dir.name][camera] = {"row0": r0, "row1": r1}
    common_start = max(v["row0"] for scan in bounds.values() for v in scan.values()) + ROW_MARGIN
    common_end = min(v["row1"] for scan in bounds.values() for v in scan.values()) - ROW_MARGIN
    common_start = int(math.ceil(common_start / ROW_STEP) * ROW_STEP)
    common_end = int(math.floor(common_end / ROW_STEP) * ROW_STEP)
    rows = np.arange(common_start, common_end + 1, ROW_STEP, dtype=int)
    rows_mm = rows * geom.Y_SCALE_MM_PER_ROW

    baseline = {}
    windows = {}
    baseline_dir = scan_dirs[0]
    for camera, spec in CAMERAS.items():
        arr = tifffile.memmap(baseline_dir / spec["glob"])
        left_cols, right_cols = fixed_face_columns(arr, int(rows[len(rows) // 2]))
        windows[camera] = {"left": left_cols, "right": right_cols}
        baseline[camera] = extract_corner_series(arr, rows, left_cols, right_cols)
    matrices = {camera: orthogonal_map(camera, baseline[camera]) for camera in CAMERAS}

    calibration_rows = []
    for index, row in enumerate(rows):
        item = {"absolute_row": int(row), "absolute_position_mm": float(rows_mm[index])}
        for camera in CAMERA_ORDER:
            values = baseline[camera]
            item[f"{camera}_u_mm"] = float(values["u_mm"][index])
            item[f"{camera}_z_mm"] = float(values["z_mm"][index])
            item[f"{camera}_roll_deg"] = float(values["roll_deg"][index])
            item[f"{camera}_included_angle_deg"] = float(values["included_angle_deg"][index])
        calibration_rows.append(item)
    write_csv(out_dir / "scan1_absolute_slice_calibration.csv", calibration_rows)

    nominal = {camera: np.asarray(CAMERAS[camera]["corner"], dtype=float) for camera in CAMERAS}
    nominal_array = np.array([nominal[c] for c in CAMERA_ORDER])
    detection_rows = []
    residual_rows = []
    for scan_dir in detection_dirs:
        scan = scan_dir.name
        current = {}
        for camera, spec in CAMERAS.items():
            arr = tifffile.memmap(scan_dir / spec["glob"])
            current[camera] = extract_corner_series(
                arr,
                rows,
                windows[camera]["left"],
                windows[camera]["right"],
            )
        for index, row in enumerate(rows):
            points = {}
            for camera in CAMERA_ORDER:
                local_delta = np.array([
                    current[camera]["u_mm"][index] - baseline[camera]["u_mm"][index],
                    current[camera]["z_mm"][index] - baseline[camera]["z_mm"][index],
                ])
                points[camera] = nominal[camera] + matrices[camera] @ local_delta
            point_array = np.array([points[c] for c in CAMERA_ORDER])
            _, translation, rotation_deg, residual = geom.rigid_fit_2d(nominal_array, point_array)
            item = {
                "scan": scan,
                "absolute_row": int(row),
                "absolute_position_mm": float(rows_mm[index]),
                "common_x_mm": float(translation[0]),
                "common_z_mm": float(translation[1]),
                "common_roll_deg": float(rotation_deg),
                "camera_relative_rms_um": float(math.sqrt(np.mean(np.sum(residual**2, axis=1))) * 1000.0),
            }
            edge_values = {}
            for edge, (camera1, camera2, standard) in EDGE_PAIRS.items():
                value = float(np.linalg.norm(points[camera1] - points[camera2]))
                edge_values[edge] = value
                item[f"{edge}_mm"] = value
                item[f"{edge}_deviation_um"] = (value - standard) * 1000.0
            item["W_mm"] = (edge_values["A"] + edge_values["C"]) / 2.0
            item["H_mm"] = (edge_values["B"] + edge_values["D"]) / 2.0
            item["W_deviation_um"] = (item["W_mm"] - A_MM) * 1000.0
            item["H_deviation_um"] = (item["H_mm"] - B_MM) * 1000.0
            item["AC_difference_um"] = (edge_values["A"] - edge_values["C"]) * 1000.0
            item["BD_difference_um"] = (edge_values["B"] - edge_values["D"]) * 1000.0
            detection_rows.append(item)
            for camera_index, camera in enumerate(CAMERA_ORDER):
                residual_rows.append({
                    "scan": scan,
                    "absolute_row": int(row),
                    "absolute_position_mm": float(rows_mm[index]),
                    "camera": camera,
                    "residual_x_um": float(residual[camera_index, 0] * 1000.0),
                    "residual_z_um": float(residual[camera_index, 1] * 1000.0),
                    "residual_norm_um": float(np.linalg.norm(residual[camera_index]) * 1000.0),
                    "roll_change_mdeg": float(
                        (current[camera]["roll_deg"][index] - baseline[camera]["roll_deg"][index]) * 1000.0
                    ),
                })
    write_csv(out_dir / "scan2_to_9_absolute_slice_measurements.csv", detection_rows)
    write_csv(out_dir / "scan2_to_9_camera_slice_residuals.csv", residual_rows)
    pose_summary_rows = summarize_common_pose(detection_names, detection_rows)
    write_csv(out_dir / "scan2_to_9_common_pose_summary.csv", pose_summary_rows)

    summary_rows = []
    for scan in detection_names:
        scan_rows = [r for r in detection_rows if r["scan"] == scan]
        for metric in ["A", "B", "C", "D", "W", "H", "AC_difference", "BD_difference"]:
            key = f"{metric}_deviation_um" if metric in {"A", "B", "C", "D", "W", "H"} else f"{metric}_um"
            stats = describe([r[key] for r in scan_rows])
            summary_rows.append({"scan": scan, "metric": metric, **{f"{k}_um": v for k, v in stats.items()}})
        residual_stats = describe([r["camera_relative_rms_um"] for r in scan_rows])
        summary_rows.append({"scan": scan, "metric": "four_camera_residual", **{f"{k}_um": v for k, v in residual_stats.items()}})
    for metric in ["A", "B", "C", "D", "W", "H", "AC_difference", "BD_difference"]:
        key = f"{metric}_deviation_um" if metric in {"A", "B", "C", "D", "W", "H"} else f"{metric}_um"
        stats = describe([r[key] for r in detection_rows])
        summary_rows.append({"scan": "ALL_2_TO_9", "metric": metric, **{f"{k}_um": v for k, v in stats.items()}})
    residual_stats = describe([r["camera_relative_rms_um"] for r in detection_rows])
    summary_rows.append({
        "scan": "ALL_2_TO_9",
        "metric": "four_camera_residual",
        **{f"{k}_um": v for k, v in residual_stats.items()},
    })
    write_csv(out_dir / "scan2_to_9_slice_summary.csv", summary_rows)

    metadata = {
        "baseline_scan": baseline_scan,
        "detection_scans": detection_names,
        "alignment": "absolute machine row; no head alignment and no length-fraction alignment",
        "absolute_row_start": int(rows[0]),
        "absolute_row_end": int(rows[-1]),
        "absolute_position_start_mm": float(rows_mm[0]),
        "absolute_position_end_mm": float(rows_mm[-1]),
        "row_step": ROW_STEP,
        "slice_spacing_mm": ROW_STEP * geom.Y_SCALE_MM_PER_ROW,
        "slice_count": int(rows.size),
        "edge_A_mm": A_MM,
        "edge_B_mm": B_MM,
        "camera_matrices": {camera: matrices[camera].tolist() for camera in CAMERAS},
        "face_windows": {
            camera: {
                "left_col0": int(windows[camera]["left"][0]),
                "left_col1": int(windows[camera]["left"][-1]),
                "right_col0": int(windows[camera]["right"][0]),
                "right_col1": int(windows[camera]["right"][-1]),
            }
            for camera in CAMERAS
        },
    }
    (out_dir / "absolute_slice_calibration_metadata.json").write_text(
        json.dumps(metadata, indent=2, ensure_ascii=False), encoding="utf-8"
    )
    plot_outputs(out_dir, rows_mm, detection_names, detection_rows, baseline, summary_rows)

    overall = [r for r in summary_rows if r["scan"] == "ALL_2_TO_9"]
    per_scan_rms = [r for r in summary_rows if r["scan"] != "ALL_2_TO_9" and r["metric"] in {"A", "B", "C", "D", "W", "H"}]
    print(json.dumps({"metadata": metadata, "overall_summary": overall, "per_scan_rms": per_scan_rms}, indent=2))


if __name__ == "__main__":
    main()
