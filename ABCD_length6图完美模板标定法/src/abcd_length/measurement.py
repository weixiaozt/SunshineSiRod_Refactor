"""Measure only A/B/C/D from one HOBJ using one orientation-agnostic model."""

from __future__ import annotations

import json
from pathlib import Path
from typing import Any, Mapping

import numpy as np

from .common import (
    CAMERAS,
    aggregate_edge_records,
    apply_bd_pair_relative_bias,
    edge_lengths,
    file_sha256,
    validate_model,
)
from .vendor import close_hobj, import_vendor_pipeline, open_hobj


def runtime_arrays(payload: Mapping[str, Any]) -> dict[str, Any]:
    runtime = payload["runtime_calibration"]
    return {
        "rows": np.asarray(runtime["rows"], dtype=int),
        "windows": {
            camera: {
                side: np.asarray(columns, dtype=int)
                for side, columns in runtime["windows"][camera].items()
            }
            for camera in CAMERAS
        },
        "matrices": {
            camera: np.asarray(runtime["matrices"][camera], dtype=float)
            for camera in CAMERAS
        },
        "origins": [
            {camera: np.asarray(row[camera], dtype=float) for camera in CAMERAS}
            for row in runtime["origins"]
        ],
        "end_margin_rows": int(runtime.get("end_margin_rows", 250)),
        "end_aggregation_station_count": int(
            runtime.get("end_aggregation_station_count", 100)
        ),
        "window_strategy": str(
            runtime.get("window_strategy", "shared_calibration")
        ),
        "y_synchronization": str(runtime.get("y_synchronization", "none")),
        "y_scale_mm_per_row": float(runtime.get("y_scale_mm_per_row", 0.05)),
        "camera_y_bias_mm_by_camera": {
            camera: float(value)
            for camera, value in runtime.get(
                "camera_y_bias_mm_by_camera", {}
            ).items()
        },
        "bd_pair_relative_bias_k_mm": float(
            runtime.get("bd_pair_relative_bias_k_mm", 0.0)
        ),
        "bd_primary_aggregation": str(
            runtime.get("bd_primary_aggregation", "head_tail_dense_mean")
        ),
    }


def measure_with_runtime(
    input_path: Path,
    runtime: Mapping[str, Any],
    pipeline: Any,
    include_corner_records: bool = False,
) -> dict[str, Any]:
    images, mappings = open_hobj(input_path)
    try:
        bounds, common_start, common_end = pipeline.image_bounds(images)
        margin = int(runtime.get("end_margin_rows", 250))
        all_rows = np.asarray(runtime["rows"], dtype=int)
        valid_indices = np.flatnonzero(
            (all_rows >= common_start + margin) & (all_rows <= common_end - margin)
        )
        if len(valid_indices) < 10:
            raise RuntimeError(f"{input_path.name} has fewer than 10 overlapping calibration stations")
        rows = all_rows[valid_indices]
        window_strategy = str(runtime.get("window_strategy", "shared_calibration"))
        if window_strategy == "dynamic_per_capture":
            active_windows = {}
            middle_row = int(rows[len(rows) // 2])
            for camera in pipeline.geom.CAMERA_ORDER:
                left, right = pipeline.slice_cal.fixed_face_columns(
                    images[camera], middle_row
                )
                active_windows[camera] = {"left": left, "right": right}
        elif window_strategy == "shared_calibration":
            active_windows = runtime["windows"]
        else:
            raise ValueError(f"Unsupported window strategy: {window_strategy}")
        sections = {
            camera: pipeline.slice_cal.extract_corner_series(
                images[camera],
                rows,
                active_windows[camera]["left"],
                active_windows[camera]["right"],
            )
            for camera in pipeline.geom.CAMERA_ORDER
        }
        camera_points: dict[str, np.ndarray] = {}
        for camera in pipeline.geom.CAMERA_ORDER:
            local = np.column_stack([
                np.asarray(sections[camera]["u_mm"], dtype=float),
                np.asarray(sections[camera]["z_mm"], dtype=float),
            ])
            origins = np.vstack([
                runtime["origins"][int(model_index)][camera]
                for model_index in valid_indices
            ])
            camera_points[camera] = (
                origins + local @ np.asarray(runtime["matrices"][camera], dtype=float).T
            )

        y_synchronization = str(runtime.get("y_synchronization", "none"))
        y_scale = float(runtime.get("y_scale_mm_per_row", 0.05))
        camera_y_bias = {
            camera: float(value)
            for camera, value in runtime.get(
                "camera_y_bias_mm_by_camera", {}
            ).items()
        }
        pre_alignment_plane_rms_mm: list[float] = []
        if y_synchronization == "fixed_bias_interpolation":
            if set(camera_y_bias) != set(pipeline.geom.CAMERA_ORDER):
                raise ValueError("Y synchronization requires four camera Y biases")
            y_by_camera = {
                camera: rows.astype(float) * y_scale - camera_y_bias[camera]
                for camera in pipeline.geom.CAMERA_ORDER
            }
            for index in range(len(rows)):
                points_3d = np.asarray([
                    [
                        camera_points[camera][index, 0],
                        y_by_camera[camera][index],
                        camera_points[camera][index, 1],
                    ]
                    for camera in pipeline.geom.CAMERA_ORDER
                ], dtype=float)
                centered_3d = points_3d - np.mean(points_3d, axis=0)
                _, _, vt = np.linalg.svd(centered_3d, full_matrices=False)
                distances = centered_3d @ vt[-1]
                pre_alignment_plane_rms_mm.append(
                    float(np.sqrt(np.mean(distances * distances)))
                )
            common_y_start = max(values[0] for values in y_by_camera.values())
            common_y_end = min(values[-1] for values in y_by_camera.values())
            target_y = rows.astype(float) * y_scale
            target_y = target_y[
                (target_y >= common_y_start) & (target_y <= common_y_end)
            ]
            if len(target_y) < 10:
                raise RuntimeError("Fewer than 10 common physical-Y stations remain")
            aligned_points = {
                camera: np.column_stack([
                    np.interp(target_y, y_by_camera[camera], camera_points[camera][:, axis])
                    for axis in range(2)
                ])
                for camera in pipeline.geom.CAMERA_ORDER
            }
            output_rows = np.rint(target_y / y_scale).astype(int)
        elif y_synchronization == "none":
            target_y = rows.astype(float) * y_scale
            aligned_points = camera_points
            output_rows = rows
        else:
            raise ValueError(f"Unsupported Y synchronization: {y_synchronization}")

        records: list[dict[str, Any]] = []
        corner_records: list[dict[str, Any]] = []
        for local_index in range(len(target_y)):
            corners = {
                camera: aligned_points[camera][local_index]
                for camera in pipeline.geom.CAMERA_ORDER
            }
            edges = edge_lengths(corners)
            records.append({
                "slice_index": local_index + 1,
                "absolute_row": int(output_rows[local_index]),
                "common_physical_y_mm": float(target_y[local_index]),
                **edges,
            })
            if include_corner_records:
                corner_records.append({
                    "slice_index": local_index + 1,
                    "absolute_row": int(output_rows[local_index]),
                    "common_physical_y_mm": float(target_y[local_index]),
                    "corners_xz_mm": {
                        camera: {
                            "X": float(corners[camera][0]),
                            "Z": float(corners[camera][1]),
                        }
                        for camera in pipeline.geom.CAMERA_ORDER
                    },
                })
    finally:
        close_hobj(images, mappings)
    raw_aggregation = aggregate_edge_records(
        records,
        int(runtime.get("end_aggregation_station_count", 100)),
    )
    bd_pair_k_mm = float(runtime.get("bd_pair_relative_bias_k_mm", 0.0))
    corrected_records = apply_bd_pair_relative_bias(records, bd_pair_k_mm)
    aggregation = aggregate_edge_records(
        corrected_records,
        int(runtime.get("end_aggregation_station_count", 100)),
    )
    result = {
        "input_path": str(Path(input_path).resolve()),
        "capture_id": Path(input_path).stem,
        "method": "long_rod_template_calibrated_length",
        "path_or_filename_orientation_used": False,
        "orientation_mapping_applied": False,
        "final_edge_offsets_applied": False,
        "independent_final_edge_offsets_applied": False,
        "bd_pair_relative_bias_correction_applied": abs(bd_pair_k_mm) > 0.0,
        "bd_pair_relative_bias_k_mm": bd_pair_k_mm,
        "camera_y_synchronization_applied": y_synchronization
        == "fixed_bias_interpolation",
        "aggregation": {
            "A_C_final": "arithmetic_mean(all_valid_stations)",
            "B_D_primary": "mean(head_dense_mean,tail_dense_mean)",
            "B_D_secondary": "arithmetic_mean(all_valid_stations)",
            "requested_end_station_count_per_end": aggregation[
                "requested_end_station_count"
            ],
            "used_end_station_count_per_end": aggregation[
                "used_end_station_count_per_end"
            ],
        },
        "raw_slice_edges_mm": records,
        "corrected_slice_edges_mm": corrected_records,
        "raw_head_edges_mm": raw_aggregation["head"],
        "raw_tail_edges_mm": raw_aggregation["tail"],
        "raw_head_tail_edges_mm": raw_aggregation["head_tail"],
        "raw_global_edges_mm": raw_aggregation["global"],
        "raw_reported_edges_mm": raw_aggregation["reported"],
        "head_edges_mm": aggregation["head"],
        "tail_edges_mm": aggregation["tail"],
        "head_tail_edges_mm": aggregation["head_tail"],
        "global_edges_mm": aggregation["global"],
        "bd_candidate_edges_mm": {
            "head_tail_dense": {
                edge: aggregation["head_tail"][edge] for edge in ("B", "D")
            },
            "global_mean": {
                edge: aggregation["global"][edge] for edge in ("B", "D")
            },
        },
        "reported_edges_mm": aggregation["reported"],
        "diagnostics": {
            "valid_station_count": len(records),
            "y_synchronization": y_synchronization,
            "bd_pair_relative_bias_k_mm": bd_pair_k_mm,
            "bd_primary_aggregation": str(
                runtime.get("bd_primary_aggregation", "head_tail_dense_mean")
            ),
            "y_scale_mm_per_row": y_scale,
            "camera_y_bias_mm_by_camera": camera_y_bias,
            "pre_alignment_four_corner_plane_rms_mm": (
                {
                    "mean": float(np.mean(pre_alignment_plane_rms_mm)),
                    "p95": float(np.percentile(pre_alignment_plane_rms_mm, 95.0)),
                    "max": float(np.max(pre_alignment_plane_rms_mm)),
                }
                if pre_alignment_plane_rms_mm
                else None
            ),
            "pre_alignment_camera_y_span_mm": (
                float(max(camera_y_bias.values()) - min(camera_y_bias.values()))
                if camera_y_bias
                else 0.0
            ),
            "common_physical_y_range_mm": [
                float(target_y[0]),
                float(target_y[-1]),
            ],
            "used_absolute_rows": [int(row["absolute_row"]) for row in records],
            "camera_local_corner_mean_mm": {
                camera: {
                    "u": float(np.mean(sections[camera]["u_mm"])),
                    "z": float(np.mean(sections[camera]["z_mm"])),
                }
                for camera in pipeline.geom.CAMERA_ORDER
            },
            "active_face_column_windows": {
                camera: {
                    side: [int(columns[0]), int(columns[-1])]
                    for side, columns in active_windows[camera].items()
                }
                for camera in pipeline.geom.CAMERA_ORDER
            },
            "camera_bounds_rows": {
                camera: [int(value[0]), int(value[1])]
                for camera, value in bounds.items()
            },
        },
    }
    if include_corner_records:
        result["raw_slice_corners_xz_mm"] = corner_records
    return result


def measure_hobj(
    input_path: Path,
    model_path: Path,
    package_root: Path | None = None,
    allow_unvalidated: bool = False,
) -> dict[str, Any]:
    payload = json.loads(Path(model_path).read_text(encoding="utf-8"))
    validate_model(payload, require_valid=not allow_unvalidated)
    pipeline = import_vendor_pipeline(package_root)
    result = measure_with_runtime(Path(input_path), runtime_arrays(payload), pipeline)
    result["model_path"] = str(Path(model_path).resolve())
    result["model_sha256"] = file_sha256(Path(model_path))
    result["model_valid"] = payload.get("valid") is True
    result["algorithm_id"] = str(
        payload.get("algorithm_id", "long_rod_template_calibrated_length")
    )
    result["algorithm_name"] = str(
        payload.get("algorithm_name_zh", "长棒模板标定长度算法")
    )
    result["algorithm_version"] = int(payload.get("version", 0))
    return result
