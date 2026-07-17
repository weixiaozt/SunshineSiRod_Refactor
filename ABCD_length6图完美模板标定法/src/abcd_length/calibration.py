"""Build one path-independent A/B/C/D camera model from six long-CTB HOBJs."""

from __future__ import annotations

import argparse
import csv
import json
from pathlib import Path
from typing import Any, Mapping, Sequence

import numpy as np
from scipy.optimize import least_squares

from .common import (
    CAMERAS,
    EDGES,
    MODEL_NAME,
    MODEL_VERSION,
    file_sha256,
    load_perfect_rectangle_truth,
    perfect_rectangle_corners,
    robust_vector_center,
    smooth_origin_curve,
    write_csv,
    write_json,
)
from .measurement import measure_with_runtime
from .validation import summarize_pose_pair_consistency
from .vendor import close_hobj, default_vendor_root, import_vendor_pipeline, open_hobj


def discover_six_captures(input_dir: Path) -> list[Path]:
    """Enumerate captures without parsing any orientation-like path tokens."""
    captures = sorted(Path(input_dir).glob("*.hobj"), key=lambda path: path.name.casefold())
    if len(captures) != 6:
        raise ValueError(f"Expected exactly six HOBJ captures, found {len(captures)} in {input_dir}")
    return captures


def load_capture_manifest(manifest_path: Path, input_dir: Path) -> list[dict[str, Any]]:
    """Load explicit calibration-only pose pairs without inferring path semantics."""
    with Path(manifest_path).open("r", newline="", encoding="utf-8-sig") as handle:
        rows = list(csv.DictReader(handle))
    required = {"capture_file", "calibration_pose", "pair_id"}
    if not rows or not required.issubset(rows[0]):
        raise ValueError(f"Calibration manifest requires columns {sorted(required)}")
    entries: list[dict[str, Any]] = []
    for row in rows:
        pose = str(row["calibration_pose"]).strip().lower()
        if pose not in {"normal", "turned"}:
            raise ValueError("calibration_pose must be normal or turned")
        capture = Path(input_dir) / str(row["capture_file"]).strip()
        if not capture.is_file():
            raise FileNotFoundError(capture)
        entries.append({
            "capture": capture,
            "capture_file": capture.name,
            "calibration_pose": pose,
            "pair_id": str(row["pair_id"]).strip(),
        })
    if len(entries) != 6 or len({entry["capture_file"] for entry in entries}) != 6:
        raise ValueError("Calibration manifest must explicitly list six unique HOBJs")
    pairs: dict[str, set[str]] = {}
    for entry in entries:
        pairs.setdefault(entry["pair_id"], set()).add(entry["calibration_pose"])
    if len(pairs) != 3 or any(poses != {"normal", "turned"} for poses in pairs.values()):
        raise ValueError("Calibration manifest must define three normal/turned pairs")
    return entries


def load_camera_y_calibration(
    calibration_path: Path,
) -> tuple[dict[str, float], dict[str, Any]]:
    payload = json.loads(Path(calibration_path).read_text(encoding="utf-8"))
    try:
        source = payload["camera_y_calibration"]
        biases = {
            camera: float(value)
            for camera, value in source["bias_total_mm_by_camera"].items()
        }
    except (KeyError, TypeError, ValueError) as exc:
        raise ValueError(f"Invalid camera Y calibration: {calibration_path}") from exc
    if set(biases) != set(CAMERAS):
        raise ValueError("Camera Y calibration must contain Top/Right/Left/Down")
    return biases, {
        "path": str(Path(calibration_path).resolve()),
        "sha256": file_sha256(Path(calibration_path)),
        "source": source.get("source"),
        "bias_total_mm_by_camera": biases,
    }


def common_bounds(pipeline: Any, captures: Sequence[Path]) -> tuple[int, int, dict[str, Any]]:
    starts: list[int] = []
    ends: list[int] = []
    audit: dict[str, Any] = {}
    for index, path in enumerate(captures, 1):
        images, mappings = open_hobj(path)
        try:
            bounds, _, _ = pipeline.image_bounds(images)
        finally:
            close_hobj(images, mappings)
        audit[f"capture_{index}"] = {
            "sha256": file_sha256(path),
            "camera_bounds_rows": {
                camera: [int(value[0]), int(value[1])]
                for camera, value in bounds.items()
            },
        }
        starts.extend(int(value[0]) for value in bounds.values())
        ends.extend(int(value[1]) for value in bounds.values())
    return max(starts), min(ends), audit


def shared_face_windows(
    pipeline: Any,
    captures: Sequence[Path],
    row: int,
    minimum_columns: int,
) -> dict[str, dict[str, np.ndarray]]:
    candidates = {
        camera: {"left": [], "right": []}
        for camera in pipeline.geom.CAMERA_ORDER
    }
    for path in captures:
        images, mappings = open_hobj(path)
        try:
            for camera in pipeline.geom.CAMERA_ORDER:
                left, right = pipeline.slice_cal.fixed_face_columns(images[camera], int(row))
                candidates[camera]["left"].append(np.asarray(left, dtype=int))
                candidates[camera]["right"].append(np.asarray(right, dtype=int))
        finally:
            close_hobj(images, mappings)
    windows: dict[str, dict[str, np.ndarray]] = {}
    for camera, sides in candidates.items():
        windows[camera] = {}
        for side, arrays in sides.items():
            start = max(int(values[0]) for values in arrays)
            end = min(int(values[-1]) for values in arrays)
            if end - start + 1 < int(minimum_columns):
                raise RuntimeError(f"No stable shared face window for {camera}/{side}")
            windows[camera][side] = np.arange(start, end + 1, dtype=int)
    return windows


def extract_capture_series(
    pipeline: Any,
    path: Path,
    rows: np.ndarray,
    windows: Mapping[str, Mapping[str, np.ndarray]],
    window_strategy: str = "shared_calibration",
) -> dict[str, Any]:
    images, mappings = open_hobj(path)
    try:
        if window_strategy == "dynamic_per_capture":
            active_windows = {}
            middle_row = int(rows[len(rows) // 2])
            for camera in pipeline.geom.CAMERA_ORDER:
                left, right = pipeline.slice_cal.fixed_face_columns(
                    images[camera], middle_row
                )
                active_windows[camera] = {"left": left, "right": right}
        elif window_strategy == "shared_calibration":
            active_windows = windows
        else:
            raise ValueError(f"Unsupported window strategy: {window_strategy}")
        series = {
            camera: pipeline.slice_cal.extract_corner_series(
                images[camera],
                rows,
                active_windows[camera]["left"],
                active_windows[camera]["right"],
            )
            for camera in pipeline.geom.CAMERA_ORDER
        }
    finally:
        close_hobj(images, mappings)
    return {"path": path, "series": series}


def pooled_matrices(pipeline: Any, captures: Sequence[Mapping[str, Any]]) -> dict[str, np.ndarray]:
    matrices: dict[str, np.ndarray] = {}
    for camera in pipeline.geom.CAMERA_ORDER:
        pooled = {
            key: np.concatenate([
                np.asarray(capture["series"][camera][key], dtype=float)
                for capture in captures
            ])
            for key in ("left_slope", "right_slope")
        }
        matrices[camera] = pipeline.slice_cal.orthogonal_map(camera, pooled)
    return matrices


def axis_scaled_matrices(
    pipeline: Any,
    captures: Sequence[Mapping[str, Any]],
    base_matrices: Mapping[str, np.ndarray],
    nominal: Mapping[str, np.ndarray],
    truth: Mapping[str, float],
) -> tuple[dict[str, np.ndarray], dict[str, Any]]:
    """Fit small, physical local-axis scale terms from repeated CTB geometry."""
    camera_order = tuple(pipeline.geom.CAMERA_ORDER)
    local_by_camera = {
        camera: np.stack([
            np.column_stack([
                np.asarray(capture["series"][camera]["u_mm"], dtype=float),
                np.asarray(capture["series"][camera]["z_mm"], dtype=float),
            ])
            for capture in captures
        ])
        for camera in camera_order
    }
    centered = {
        camera: values - np.mean(values, axis=0, keepdims=True)
        for camera, values in local_by_camera.items()
    }

    def matrices_from_scales(scales: np.ndarray) -> dict[str, np.ndarray]:
        return {
            camera: np.asarray(base_matrices[camera], dtype=float)
            @ np.diag(scales[2 * index:2 * index + 2])
            for index, camera in enumerate(camera_order)
        }

    edge_pairs = {
        "A": ("Top", "Right"),
        "B": ("Right", "Left"),
        "C": ("Left", "Down"),
        "D": ("Down", "Top"),
    }

    def residuals(scales: np.ndarray) -> np.ndarray:
        matrices = matrices_from_scales(scales)
        points = {
            camera: centered[camera] @ matrices[camera].T
            + np.asarray(nominal[camera], dtype=float)
            for camera in camera_order
        }
        edge_residuals = []
        for edge, (camera1, camera2) in edge_pairs.items():
            values = np.linalg.norm(points[camera1] - points[camera2], axis=2)
            edge_residuals.append((values - float(truth[edge])).ravel())
        # A 1% scale move contributes 0.005 mm to the prior residual.
        prior = (scales - 1.0) * 0.5
        return np.concatenate([*edge_residuals, prior])

    initial = np.ones(2 * len(camera_order), dtype=float)
    fit = least_squares(
        residuals,
        initial,
        bounds=(np.full_like(initial, 0.97), np.full_like(initial, 1.03)),
        loss="soft_l1",
        f_scale=0.005,
        max_nfev=200,
    )
    return matrices_from_scales(fit.x), {
        "strategy": "camera_local_axis_scales_from_repeated_perfect_bar_geometry",
        "success": bool(fit.success),
        "message": str(fit.message),
        "cost": float(fit.cost),
        "optimality": float(fit.optimality),
        "scales_by_camera": {
            camera: {
                "local_u": float(fit.x[2 * index]),
                "local_z": float(fit.x[2 * index + 1]),
            }
            for index, camera in enumerate(camera_order)
        },
    }


def build_matrices(
    pipeline: Any,
    captures: Sequence[Mapping[str, Any]],
    nominal: Mapping[str, np.ndarray],
    truth: Mapping[str, float],
    matrix_strategy: str,
) -> tuple[dict[str, np.ndarray], dict[str, Any]]:
    base = pooled_matrices(pipeline, captures)
    if matrix_strategy == "orthogonal":
        return base, {"strategy": "delivered_orthogonal_map"}
    if matrix_strategy == "axis_scaled":
        return axis_scaled_matrices(
            pipeline, captures, base, nominal, truth
        )
    raise ValueError(f"Unsupported matrix strategy: {matrix_strategy}")


def origin_curves(
    pipeline: Any,
    captures: Sequence[Mapping[str, Any]],
    matrices: Mapping[str, np.ndarray],
    nominal: Mapping[str, np.ndarray],
    outlier_limit_mm: float,
    smoothing_window: int,
    origin_estimator: str,
) -> tuple[list[dict[str, np.ndarray]], dict[str, Any]]:
    station_count = len(next(iter(captures))["series"][pipeline.geom.CAMERA_ORDER[0]]["u_mm"])
    curves: dict[str, np.ndarray] = {}
    diagnostics: dict[str, Any] = {}
    for camera in pipeline.geom.CAMERA_ORDER:
        raw_rows = []
        used_counts = []
        spreads = []
        for station in range(station_count):
            observations = []
            for capture in captures:
                local = np.array([
                    capture["series"][camera]["u_mm"][station],
                    capture["series"][camera]["z_mm"][station],
                ], dtype=float)
                observations.append(nominal[camera] - matrices[camera] @ local)
            robust_center, used_count, spread = robust_vector_center(
                observations, outlier_limit_mm
            )
            if origin_estimator == "robust_median":
                center = robust_center
            elif origin_estimator == "arithmetic_mean":
                center = np.mean(np.asarray(observations, dtype=float), axis=0)
                used_count = len(observations)
            else:
                raise ValueError(f"Unsupported origin estimator: {origin_estimator}")
            raw_rows.append(center)
            used_counts.append(used_count)
            spreads.append(spread)
        raw = np.asarray(raw_rows, dtype=float)
        smoothed = smooth_origin_curve(raw, smoothing_window)
        residual = np.linalg.norm(raw - smoothed, axis=1)
        curves[camera] = smoothed
        diagnostics[camera] = {
            "origin_estimator": origin_estimator,
            "capture_observations_per_station": len(captures),
            "minimum_used_observations": int(min(used_counts)),
            "origin_spread_mm": {
                "mean": float(np.mean(spreads)),
                "p95": float(np.percentile(spreads, 95.0)),
                "max": float(np.max(spreads)),
            },
            "smoothing_residual_mm": {
                "rms": float(np.sqrt(np.mean(residual * residual))),
                "p95": float(np.percentile(residual, 95.0)),
                "max": float(np.max(residual)),
            },
        }
    origins = [
        {camera: curves[camera][station] for camera in pipeline.geom.CAMERA_ORDER}
        for station in range(station_count)
    ]
    return origins, diagnostics


def make_runtime(
    rows: np.ndarray,
    windows: Mapping[str, Mapping[str, np.ndarray]],
    matrices: Mapping[str, np.ndarray],
    origins: Sequence[Mapping[str, np.ndarray]],
    end_margin_rows: int,
    end_aggregation_station_count: int,
    window_strategy: str,
    y_synchronization: str,
    camera_y_bias_mm_by_camera: Mapping[str, float],
    y_scale_mm_per_row: float,
) -> dict[str, Any]:
    return {
        "rows": rows,
        "windows": windows,
        "matrices": matrices,
        "origins": list(origins),
        "end_margin_rows": int(end_margin_rows),
        "end_aggregation_station_count": int(end_aggregation_station_count),
        "window_strategy": window_strategy,
        "y_synchronization": y_synchronization,
        "camera_y_bias_mm_by_camera": dict(camera_y_bias_mm_by_camera),
        "y_scale_mm_per_row": float(y_scale_mm_per_row),
    }


def leave_one_out(
    pipeline: Any,
    captures: Sequence[Path],
    rows: np.ndarray,
    nominal: Mapping[str, np.ndarray],
    truth: Mapping[str, float],
    outlier_limit_mm: float,
    smoothing_window: int,
    end_margin_rows: int,
    minimum_window_columns: int,
    end_aggregation_station_count: int,
    origin_estimator: str,
    matrix_strategy: str,
    window_strategy: str,
    y_synchronization: str,
    camera_y_bias_mm_by_camera: Mapping[str, float],
    y_scale_mm_per_row: float,
) -> list[dict[str, Any]]:
    output: list[dict[str, Any]] = []
    for held_out_index, held_out in enumerate(captures):
        training_paths = [path for index, path in enumerate(captures) if index != held_out_index]
        fold_windows = shared_face_windows(
            pipeline,
            training_paths,
            int(rows[len(rows) // 2]),
            minimum_window_columns,
        )
        training = [
            extract_capture_series(
                pipeline, path, rows, fold_windows, window_strategy
            )
            for path in training_paths
        ]
        matrices, _ = build_matrices(
            pipeline,
            training,
            nominal,
            truth,
            matrix_strategy,
        )
        origins, _ = origin_curves(
            pipeline,
            training,
            matrices,
            nominal,
            outlier_limit_mm,
            smoothing_window,
            origin_estimator,
        )
        runtime = make_runtime(
            rows,
            fold_windows,
            matrices,
            origins,
            end_margin_rows,
            end_aggregation_station_count,
            window_strategy,
            y_synchronization,
            camera_y_bias_mm_by_camera,
            y_scale_mm_per_row,
        )
        result = measure_with_runtime(held_out, runtime, pipeline)
        records = result["raw_slice_edges_mm"]
        row: dict[str, Any] = {
            "held_out_index": held_out_index + 1,
            "held_out_sha256": file_sha256(held_out),
            "training_capture_count": len(training),
            "valid_station_count": len(records),
            "path_tokens_used": False,
        }
        for edge in EDGES:
            errors = np.asarray([float(record[edge]) - float(truth[edge]) for record in records])
            final = float(result["reported_edges_mm"][edge])
            row[f"{edge}_reported_mm"] = final
            row[f"{edge}_reported_error_mm"] = final - float(truth[edge])
            row[f"{edge}_slice_bias_mm"] = float(np.mean(errors))
            row[f"{edge}_slice_rms_mm"] = float(np.sqrt(np.mean(errors * errors)))
        output.append(row)
    return output


def build_model(
    input_dir: Path,
    truth_csv: Path,
    package_root: Path | None,
    station_count: int,
    end_margin_rows: int,
    minimum_window_columns: int,
    outlier_limit_mm: float,
    smoothing_window: int,
    max_loo_slice_rms_mm: float,
    end_aggregation_station_count: int,
    capture_manifest: Path,
    origin_estimator: str,
    matrix_strategy: str,
    window_strategy: str,
    y_synchronization: str,
    camera_y_calibration: Path | None,
) -> tuple[dict[str, Any], list[dict[str, Any]]]:
    captures = discover_six_captures(input_dir)
    manifest_entries = load_capture_manifest(capture_manifest, input_dir)
    if {path.name for path in captures} != {
        entry["capture_file"] for entry in manifest_entries
    }:
        raise ValueError("Calibration manifest and input directory list different HOBJs")
    truth = load_perfect_rectangle_truth(truth_csv)
    nominal = perfect_rectangle_corners(truth)
    pipeline = import_vendor_pipeline(package_root)
    y_scale_mm_per_row = float(pipeline.Y_SCALE_MM_PER_ROW)
    camera_y_bias_mm_by_camera: dict[str, float] = {}
    camera_y_calibration_audit: dict[str, Any] | None = None
    if y_synchronization == "fixed_bias_interpolation":
        calibration_path = (
            Path(camera_y_calibration)
            if camera_y_calibration is not None
            else (Path(package_root) if package_root else default_vendor_root())
            / "calibration"
            / "current_calibration.json"
        )
        camera_y_bias_mm_by_camera, camera_y_calibration_audit = (
            load_camera_y_calibration(calibration_path)
        )
    elif y_synchronization != "none":
        raise ValueError(f"Unsupported Y synchronization: {y_synchronization}")
    common_start, common_end, capture_audit = common_bounds(pipeline, captures)
    first = common_start + int(end_margin_rows)
    last = common_end - int(end_margin_rows)
    if last <= first:
        raise RuntimeError("Six captures have no shared row range after end margins")
    rows = np.unique(np.rint(np.linspace(first, last, max(10, int(station_count)))).astype(int))
    windows = shared_face_windows(
        pipeline, captures, int(rows[len(rows) // 2]), minimum_window_columns
    )
    extracted = [
        extract_capture_series(
            pipeline, path, rows, windows, window_strategy
        )
        for path in captures
    ]
    matrices, matrix_diagnostics = build_matrices(
        pipeline,
        extracted,
        nominal,
        truth,
        matrix_strategy,
    )
    origins, origin_diagnostics = origin_curves(
        pipeline,
        extracted,
        matrices,
        nominal,
        outlier_limit_mm,
        smoothing_window,
        origin_estimator,
    )
    runtime = make_runtime(
        rows,
        windows,
        matrices,
        origins,
        end_margin_rows,
        end_aggregation_station_count,
        window_strategy,
        y_synchronization,
        camera_y_bias_mm_by_camera,
        y_scale_mm_per_row,
    )
    loo_rows = leave_one_out(
        pipeline,
        captures,
        rows,
        nominal,
        truth,
        outlier_limit_mm,
        smoothing_window,
        end_margin_rows,
        minimum_window_columns,
        end_aggregation_station_count,
        origin_estimator,
        matrix_strategy,
        window_strategy,
        y_synchronization,
        camera_y_bias_mm_by_camera,
        y_scale_mm_per_row,
    )
    full_results = {
        path.name: measure_with_runtime(path, runtime, pipeline)
        for path in captures
    }
    pose_pair_validation = summarize_pose_pair_consistency(
        manifest_entries,
        full_results,
    )
    max_rms = max(float(row[f"{edge}_slice_rms_mm"]) for row in loo_rows for edge in EDGES)
    payload: dict[str, Any] = {
        "model": MODEL_NAME,
        "version": MODEL_VERSION,
        "valid": max_rms <= float(max_loo_slice_rms_mm),
        "release_ready": False,
        "validation_scope": "single_standard_bar_internal_leave_one_capture_out",
        "contains_final_edge_offsets": False,
        "path_based_orientation_detection": False,
        "orientation_mapping_applied": False,
        "capture_names_used_as_model_features": False,
        "capture_count": len(captures),
        "capture_audit_by_anonymous_index": capture_audit,
        "perfect_rectangle_truth_mm": truth,
        "truth_csv": str(truth_csv.resolve()),
        "truth_csv_sha256": file_sha256(truth_csv),
        "calibration_strategy": "six_capture_dense_absolute_row_camera_spatial_template_with_explicit_pose_pair_audit",
        "calibration_note": (
            "All six HOBJs contribute equally to one camera model. The separate, human-authored "
            "manifest is used only to audit A/C same-face and B/D crossed-face consistency. "
            "Runtime never parses file or directory names and never switches or maps a result."
        ),
        "parameters": {
            "station_count": len(rows),
            "end_margin_rows": int(end_margin_rows),
            "minimum_window_columns": int(minimum_window_columns),
            "outlier_limit_mm": float(outlier_limit_mm),
            "smoothing_window_stations": int(smoothing_window),
            "end_aggregation_station_count_per_end": int(
                end_aggregation_station_count
            ),
            "origin_estimator": origin_estimator,
            "matrix_strategy": matrix_strategy,
            "window_strategy": window_strategy,
            "y_synchronization": y_synchronization,
            "y_scale_mm_per_row": y_scale_mm_per_row,
            "max_loo_slice_rms_mm": float(max_loo_slice_rms_mm),
        },
        "camera_y_calibration_audit": camera_y_calibration_audit,
        "origin_curve_diagnostics": origin_diagnostics,
        "camera_matrix_diagnostics": matrix_diagnostics,
        "validation": {
            "method": "six_fold_leave_one_capture_out_including_training_only_roi_without_orientation_labels",
            "max_slice_rms_mm": max_rms,
            "threshold_mm": float(max_loo_slice_rms_mm),
            "passed": max_rms <= float(max_loo_slice_rms_mm),
            "production_release_claimed": False,
            "explicit_calibration_pose_pair_audit": pose_pair_validation,
        },
        "runtime_calibration": runtime,
    }
    return payload, loo_rows


def main(argv: Sequence[str] | None = None) -> int:
    block_root = Path(__file__).resolve().parents[2]
    parser = argparse.ArgumentParser(description="Build the path-independent six-HOBJ ABCD calibration")
    parser.add_argument("--input-dir", type=Path, default=block_root)
    parser.add_argument("--truth-csv", type=Path, default=block_root / "CTB-长度数据.csv")
    parser.add_argument("--package-root", type=Path, default=None)
    parser.add_argument("--output-model", type=Path, default=block_root / "models" / "abcd_length_six_capture_210_105.json")
    parser.add_argument("--diagnostics-csv", type=Path, default=block_root / "results" / "leave_one_out_diagnostics.csv")
    parser.add_argument("--station-count", type=int, default=850)
    parser.add_argument("--end-margin-rows", type=int, default=250)
    parser.add_argument("--minimum-window-columns", type=int, default=200)
    parser.add_argument("--outlier-limit-mm", type=float, default=0.50)
    parser.add_argument(
        "--origin-estimator",
        choices=("robust_median", "arithmetic_mean"),
        default="robust_median",
    )
    parser.add_argument(
        "--matrix-strategy",
        choices=("orthogonal", "axis_scaled"),
        default="orthogonal",
    )
    parser.add_argument(
        "--window-strategy",
        choices=("shared_calibration", "dynamic_per_capture"),
        default="shared_calibration",
    )
    parser.add_argument(
        "--y-synchronization",
        choices=("none", "fixed_bias_interpolation"),
        default="none",
    )
    parser.add_argument("--camera-y-calibration", type=Path, default=None)
    parser.add_argument("--smoothing-window", type=int, default=21)
    parser.add_argument("--end-aggregation-station-count", type=int, default=100)
    parser.add_argument(
        "--capture-manifest",
        type=Path,
        default=block_root / "calibration_capture_manifest.csv",
    )
    parser.add_argument("--max-loo-slice-rms-mm", type=float, default=0.05)
    args = parser.parse_args(argv)
    payload, diagnostics = build_model(
        args.input_dir,
        args.truth_csv,
        args.package_root,
        args.station_count,
        args.end_margin_rows,
        args.minimum_window_columns,
        args.outlier_limit_mm,
        args.smoothing_window,
        args.max_loo_slice_rms_mm,
        args.end_aggregation_station_count,
        args.capture_manifest,
        args.origin_estimator,
        args.matrix_strategy,
        args.window_strategy,
        args.y_synchronization,
        args.camera_y_calibration,
    )
    write_json(args.output_model, payload)
    write_csv(args.diagnostics_csv, diagnostics)
    print(f"Model: {args.output_model}")
    print(f"Diagnostics: {args.diagnostics_csv}")
    print(f"Valid: {payload['valid']}")
    print(f"LOO max slice RMS: {payload['validation']['max_slice_rms_mm']:.6f} mm")
    return 0 if payload["valid"] else 4


if __name__ == "__main__":
    raise SystemExit(main())
