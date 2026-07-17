import csv
import json
import math
from pathlib import Path

import matplotlib.pyplot as plt
import numpy as np
import tifffile

import analyze_absolute_slice_calibration as slice_base
import analyze_positive_repeatability as geom


SLICE_COUNT = 21
FIRST_ROW = 9000
LAST_ROW = 18000
CAMERAS = geom.CAMERAS
CAMERA_ORDER = geom.CAMERA_ORDER
EDGE_PAIRS = {
    "A": ("Top", "Right"),
    "B": ("Right", "Left"),
    "C": ("Left", "Down"),
    "D": ("Top", "Down"),
}


def write_csv(path, rows):
    rows = list(rows)
    if not rows:
        return
    with path.open("w", newline="", encoding="utf-8-sig") as handle:
        writer = csv.DictWriter(handle, fieldnames=list(rows[0]))
        writer.writeheader()
        writer.writerows(rows)


def read_cmm_reference(root):
    path = root / ".codex_tmp" / "cmm_calibration_20260714" / "cmm_reference.json"
    payload = json.loads(path.read_text(encoding="utf-8"))
    values = {str(label): float(value) for label, value in payload["rows"]}
    return {
        "source_path": payload["sourcePath"],
        "source_range": payload["sourceRange"],
        "length_mm": values["长度"],
        "head": {edge: values[f"头-边长{edge}"] for edge in EDGE_PAIRS},
        "tail": {edge: values[f"尾-边长{edge}"] for edge in EDGE_PAIRS},
        "head_diagonals_mm": [values["头-对角线1"], values["头-对角线2"]],
        "tail_diagonals_mm": [values["尾-对角线1"], values["尾-对角线2"]],
    }


def describe(values):
    values = np.asarray(values, dtype=float)
    return {
        "count": int(values.size),
        "mean": float(np.mean(values)),
        "std": float(np.std(values, ddof=1)) if values.size > 1 else 0.0,
        "rms": float(math.sqrt(np.mean(values**2))),
        "p95_abs": float(np.percentile(np.abs(values), 95)),
        "max_abs": float(np.max(np.abs(values))),
    }


def standard_at_fraction(reference, fraction):
    return {
        edge: reference["head"][edge]
        + fraction * (reference["tail"][edge] - reference["head"][edge])
        for edge in EDGE_PAIRS
    }


def nominal_corners(standards):
    width = (standards["A"] + standards["C"]) / 2.0
    height = (standards["B"] + standards["D"]) / 2.0
    return {
        "Top": np.array([0.0, 0.0]),
        "Right": np.array([width, 0.0]),
        "Left": np.array([width, height]),
        "Down": np.array([0.0, height]),
    }


def summarize(scan_names, slice_rows):
    output = []
    for scan in [*scan_names, "ALL_2_TO_9"]:
        selected = slice_rows if scan == "ALL_2_TO_9" else [row for row in slice_rows if row["scan"] == scan]
        for metric in ["A", "B", "C", "D", "W", "H"]:
            errors = [row[f"{metric}_error_um"] for row in selected]
            stats = describe(errors)
            output.append({
                "scan": scan,
                "metric": metric,
                "slice_count": stats["count"],
                "standard_mean_mm": float(np.mean([row[f"{metric}_standard_mm"] for row in selected])),
                "measured_mean_mm": float(np.mean([row[f"{metric}_measured_mm"] for row in selected])),
                "mean_error_um": stats["mean"],
                "std_error_um": stats["std"],
                "rms_error_um": stats["rms"],
                "p95_abs_error_um": stats["p95_abs"],
                "max_abs_error_um": stats["max_abs"],
            })
    return output


def plot_outputs(out_dir, scan_names, slice_rows, summary_rows):
    fig, axes = plt.subplots(2, 2, figsize=(13, 8), sharex=True, sharey=True, constrained_layout=True)
    for axis, edge in zip(axes.flat, EDGE_PAIRS):
        for scan in scan_names:
            rows = [row for row in slice_rows if row["scan"] == scan]
            axis.plot(
                [row["distance_from_head_mm"] for row in rows],
                [row[f"{edge}_error_um"] for row in rows],
                marker="o",
                ms=2.8,
                lw=0.9,
                label=scan,
            )
        axis.axhline(0, color="0.4", lw=0.8)
        axis.set_title(f"Edge {edge}")
        axis.set_ylabel("Error from CMM-calibrated scan 1 (um)")
        axis.grid(True, alpha=0.2)
    for axis in axes[1]:
        axis.set_xlabel("Distance from head (mm)")
    axes[0, 1].legend(ncol=2, fontsize=8)
    fig.suptitle("Positive scans 2-9, 21 absolute-position slices")
    fig.savefig(out_dir / "cmm_sparse_slice_edge_errors.png", dpi=190)
    plt.close(fig)

    metrics = ["A", "B", "C", "D", "W", "H"]
    heat = np.zeros((len(scan_names), len(metrics)))
    for row_index, scan in enumerate(scan_names):
        by_metric = {row["metric"]: row for row in summary_rows if row["scan"] == scan}
        for column_index, metric in enumerate(metrics):
            heat[row_index, column_index] = by_metric[metric]["rms_error_um"]
    fig, axis = plt.subplots(figsize=(9.5, 5.8), constrained_layout=True)
    image = axis.imshow(heat, aspect="auto", cmap="viridis")
    axis.set_xticks(np.arange(len(metrics)), metrics)
    axis.set_yticks(np.arange(len(scan_names)), scan_names)
    for i in range(heat.shape[0]):
        for j in range(heat.shape[1]):
            color = "white" if heat[i, j] > (heat.min() + heat.max()) / 2 else "black"
            axis.text(j, i, f"{heat[i, j]:.1f}", ha="center", va="center", color=color, fontsize=8)
    axis.set_title("RMS error after scan-1 and CMM calibration (um)")
    fig.colorbar(image, ax=axis, label="RMS error (um)")
    fig.savefig(out_dir / "cmm_sparse_slice_rms_heatmap.png", dpi=190)
    plt.close(fig)


def main():
    root = Path.cwd()
    reference = read_cmm_reference(root)
    _, scan_dirs, _ = slice_base.discover(root)
    baseline_dir = scan_dirs[0]
    detection_dirs = scan_dirs[1:9]
    detection_names = [path.name for path in detection_dirs]
    out_dir = root / "标定棒数据" / "_positive_analysis" / "cmm_calibrated_sparse_slices"
    out_dir.mkdir(parents=True, exist_ok=True)

    prior = json.loads(
        (root / "标定棒数据" / "_positive_analysis" / "positive_repeatability_results.json").read_text(
            encoding="utf-8"
        )
    )
    baseline_bounds = prior["bounds"][baseline_dir.name]
    head_row = float(np.median([item["row0"] for item in baseline_bounds.values()]))
    tail_row = float(np.median([item["row1"] for item in baseline_bounds.values()]))
    calibrated_y_scale = reference["length_mm"] / (tail_row - head_row)
    rows = np.rint(np.linspace(FIRST_ROW, LAST_ROW, SLICE_COUNT)).astype(int)

    baseline = {}
    windows = {}
    for camera, spec in CAMERAS.items():
        image = tifffile.memmap(baseline_dir / spec["glob"])
        left_columns, right_columns = slice_base.fixed_face_columns(image, int(rows[len(rows) // 2]))
        windows[camera] = {"left": left_columns, "right": right_columns}
        baseline[camera] = slice_base.extract_corner_series(image, rows, left_columns, right_columns)
    matrices = {camera: slice_base.orthogonal_map(camera, baseline[camera]) for camera in CAMERAS}

    standards_by_index = []
    nominal_by_index = []
    calibration_rows = []
    for index, row in enumerate(rows):
        fraction = float(np.clip((row - head_row) / (tail_row - head_row), 0.0, 1.0))
        standards = standard_at_fraction(reference, fraction)
        standards_by_index.append(standards)
        nominal_by_index.append(nominal_corners(standards))
        calibration_row = {
            "slice_index": index + 1,
            "absolute_row": int(row),
            "machine_position_nominal_mm": float(row * geom.Y_SCALE_MM_PER_ROW),
            "distance_from_head_mm": float(fraction * reference["length_mm"]),
            "length_fraction": fraction,
        }
        for edge in EDGE_PAIRS:
            calibration_row[f"{edge}_standard_mm"] = standards[edge]
        for camera in CAMERA_ORDER:
            calibration_row[f"{camera}_u_mm"] = float(baseline[camera]["u_mm"][index])
            calibration_row[f"{camera}_z_mm"] = float(baseline[camera]["z_mm"][index])
        calibration_rows.append(calibration_row)
    write_csv(out_dir / "scan1_cmm_sparse_slice_calibration.csv", calibration_rows)

    measurement_rows = []
    for scan_dir in detection_dirs:
        current = {}
        for camera, spec in CAMERAS.items():
            image = tifffile.memmap(scan_dir / spec["glob"])
            current[camera] = slice_base.extract_corner_series(
                image,
                rows,
                windows[camera]["left"],
                windows[camera]["right"],
            )
        for index, row in enumerate(rows):
            standards = standards_by_index[index]
            nominal = nominal_by_index[index]
            points = {}
            for camera in CAMERA_ORDER:
                local_delta = np.array([
                    current[camera]["u_mm"][index] - baseline[camera]["u_mm"][index],
                    current[camera]["z_mm"][index] - baseline[camera]["z_mm"][index],
                ])
                points[camera] = nominal[camera] + matrices[camera] @ local_delta
            item = {
                "scan": scan_dir.name,
                "slice_index": index + 1,
                "absolute_row": int(row),
                "machine_position_nominal_mm": float(row * geom.Y_SCALE_MM_PER_ROW),
                "distance_from_head_mm": calibration_rows[index]["distance_from_head_mm"],
                "length_fraction": calibration_rows[index]["length_fraction"],
            }
            measured_edges = {}
            for edge, (camera1, camera2) in EDGE_PAIRS.items():
                nominal_distance = float(np.linalg.norm(nominal[camera1] - nominal[camera2]))
                current_distance = float(np.linalg.norm(points[camera1] - points[camera2]))
                measured = standards[edge] + current_distance - nominal_distance
                measured_edges[edge] = measured
                item[f"{edge}_standard_mm"] = standards[edge]
                item[f"{edge}_measured_mm"] = measured
                item[f"{edge}_error_um"] = (measured - standards[edge]) * 1000.0
            item["W_standard_mm"] = (standards["A"] + standards["C"]) / 2.0
            item["H_standard_mm"] = (standards["B"] + standards["D"]) / 2.0
            item["W_measured_mm"] = (measured_edges["A"] + measured_edges["C"]) / 2.0
            item["H_measured_mm"] = (measured_edges["B"] + measured_edges["D"]) / 2.0
            item["W_error_um"] = (item["W_measured_mm"] - item["W_standard_mm"]) * 1000.0
            item["H_error_um"] = (item["H_measured_mm"] - item["H_standard_mm"]) * 1000.0
            nominal_array = np.array([nominal[camera] for camera in CAMERA_ORDER])
            point_array = np.array([points[camera] for camera in CAMERA_ORDER])
            _, _, _, residual = geom.rigid_fit_2d(nominal_array, point_array)
            item["four_camera_relative_rms_um"] = float(
                math.sqrt(np.mean(np.sum(residual**2, axis=1))) * 1000.0
            )
            measurement_rows.append(item)
    write_csv(out_dir / "scan2_to_9_cmm_sparse_slice_measurements.csv", measurement_rows)

    summary_rows = summarize(detection_names, measurement_rows)
    write_csv(out_dir / "scan2_to_9_cmm_sparse_slice_summary.csv", summary_rows)
    metadata = {
        "reference_workbook": reference["source_path"],
        "reference_range": reference["source_range"],
        "cmm_length_mm": reference["length_mm"],
        "cmm_head_edges_mm": reference["head"],
        "cmm_tail_edges_mm": reference["tail"],
        "baseline_scan": baseline_dir.name,
        "detection_scans": detection_names,
        "slice_count": SLICE_COUNT,
        "absolute_rows": rows.tolist(),
        "scan1_head_row_median": head_row,
        "scan1_tail_row_median": tail_row,
        "calibrated_y_scale_mm_per_row": calibrated_y_scale,
        "head_tail_direction": "lower absolute row is head; higher absolute row is tail",
        "alignment": "same absolute machine rows; no head alignment and no length-fraction alignment",
    }
    (out_dir / "cmm_sparse_slice_metadata.json").write_text(
        json.dumps(metadata, ensure_ascii=False, indent=2), encoding="utf-8"
    )
    plot_outputs(out_dir, detection_names, measurement_rows, summary_rows)

    concise = [row for row in summary_rows if row["metric"] in {"W", "H"} and row["scan"] != "ALL_2_TO_9"]
    print(json.dumps({"output": str(out_dir), "metadata": metadata, "width_height_summary": concise}, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
