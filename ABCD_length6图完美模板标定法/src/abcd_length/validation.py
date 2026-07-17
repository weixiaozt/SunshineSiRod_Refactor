"""Post-measurement repeatability audit isolated from calibration and measurement."""

from __future__ import annotations

from pathlib import Path
from typing import Any, Mapping, Sequence

import numpy as np

from .common import EDGES, file_sha256, write_csv, write_json
from .measurement import measure_hobj


def _residual_summary(values: Sequence[float]) -> dict[str, float]:
    array = np.asarray(values, dtype=float)
    return {
        "mean_signed_mm": float(np.mean(array)),
        "mean_absolute_mm": float(np.mean(np.abs(array))),
        "rms_mm": float(np.sqrt(np.mean(array * array))),
        "max_absolute_mm": float(np.max(np.abs(array))),
    }


def summarize_pose_pair_consistency(
    manifest_entries: Sequence[Mapping[str, Any]],
    results_by_capture: Mapping[str, Mapping[str, Any]],
) -> dict[str, Any]:
    """Audit known calibration poses; this function is never called by runtime measurement."""
    pairs: dict[str, dict[str, Mapping[str, Any]]] = {}
    for entry in manifest_entries:
        capture_file = str(entry["capture_file"])
        pairs.setdefault(str(entry["pair_id"]), {})[
            str(entry["calibration_pose"])
        ] = results_by_capture[capture_file]
    residuals: dict[str, list[float]] = {
        "AC_global_A_same_face": [],
        "AC_global_C_same_face": [],
        "BD_global_B_to_D": [],
        "BD_global_D_to_B": [],
        "BD_endpoint_head_B_to_turned_tail_D": [],
        "BD_endpoint_tail_B_to_turned_head_D": [],
        "BD_endpoint_head_D_to_turned_tail_B": [],
        "BD_endpoint_tail_D_to_turned_head_B": [],
        "BD_head_tail_combined_B_to_D": [],
        "BD_head_tail_combined_D_to_B": [],
    }
    pair_rows = []
    for pair_id in sorted(pairs):
        pair = pairs[pair_id]
        if set(pair) != {"normal", "turned"}:
            raise ValueError(f"Incomplete calibration pose pair {pair_id}")
        normal = pair["normal"]
        turned = pair["turned"]
        values = {
            "AC_global_A_same_face": normal["global_edges_mm"]["A"] - turned["global_edges_mm"]["A"],
            "AC_global_C_same_face": normal["global_edges_mm"]["C"] - turned["global_edges_mm"]["C"],
            "BD_global_B_to_D": normal["global_edges_mm"]["B"] - turned["global_edges_mm"]["D"],
            "BD_global_D_to_B": normal["global_edges_mm"]["D"] - turned["global_edges_mm"]["B"],
            "BD_endpoint_head_B_to_turned_tail_D": normal["head_edges_mm"]["B"] - turned["tail_edges_mm"]["D"],
            "BD_endpoint_tail_B_to_turned_head_D": normal["tail_edges_mm"]["B"] - turned["head_edges_mm"]["D"],
            "BD_endpoint_head_D_to_turned_tail_B": normal["head_edges_mm"]["D"] - turned["tail_edges_mm"]["B"],
            "BD_endpoint_tail_D_to_turned_head_B": normal["tail_edges_mm"]["D"] - turned["head_edges_mm"]["B"],
            "BD_head_tail_combined_B_to_D": normal["head_tail_edges_mm"]["B"] - turned["head_tail_edges_mm"]["D"],
            "BD_head_tail_combined_D_to_B": normal["head_tail_edges_mm"]["D"] - turned["head_tail_edges_mm"]["B"],
        }
        for key, value in values.items():
            residuals[key].append(float(value))
        pair_rows.append({"pair_id": pair_id, **values})
    return {
        "purpose": "calibration_only_pose_equivariance_audit",
        "runtime_pose_detection_used": False,
        "runtime_face_mapping_applied": False,
        "known_physical_mapping": {
            "A": "A",
            "C": "C",
            "B": "D",
            "D": "B",
            "head": "tail",
            "tail": "head",
        },
        "pair_count": len(pair_rows),
        "pair_residuals_mm": pair_rows,
        "residual_summaries": {
            key: _residual_summary(values) for key, values in residuals.items()
        },
    }


def summarize_results(results: Sequence[Mapping[str, Any]]) -> dict[str, Any]:
    if len(results) < 2:
        raise ValueError("Repeatability validation requires at least two measurements")
    for result in results:
        if result.get("path_or_filename_orientation_used") is not False:
            raise ValueError("Validation refuses a result that used path-based orientation")
        if result.get("orientation_mapping_applied") is not False:
            raise ValueError("Validation refuses a result that applied orientation mapping")
    edge_statistics: dict[str, Any] = {}
    for edge in EDGES:
        values = np.asarray([float(result["reported_edges_mm"][edge]) for result in results])
        edge_statistics[edge] = {
            "mean_mm": float(np.mean(values)),
            "std_mm": float(np.std(values)),
            "range_mm": float(np.ptp(values)),
            "min_mm": float(np.min(values)),
            "max_mm": float(np.max(values)),
        }
    opposite_differences = {
        "A_minus_C_mm": [
            float(result["reported_edges_mm"]["A"] - result["reported_edges_mm"]["C"])
            for result in results
        ],
        "B_minus_D_mm": [
            float(result["reported_edges_mm"]["B"] - result["reported_edges_mm"]["D"])
            for result in results
        ],
    }
    return {
        "measurement_count": len(results),
        "path_tokens_used": False,
        "orientation_groups_used": False,
        "orientation_mapping_applied": False,
        "interpretation": (
            "All captures are treated as anonymous repeated measurements. Any pose-related "
            "pattern remains visible in the spread and is never used to alter a measurement."
        ),
        "edge_statistics": edge_statistics,
        "opposite_edge_differences": {
            key: {
                "mean_mm": float(np.mean(values)),
                "std_mm": float(np.std(values)),
                "range_mm": float(np.ptp(values)),
                "values_mm": values,
            }
            for key, values in opposite_differences.items()
        },
    }


def validate_capture_set(
    captures: Sequence[Path],
    model_path: Path,
    output_dir: Path,
    package_root: Path | None = None,
    manifest_entries: Sequence[Mapping[str, Any]] | None = None,
) -> dict[str, Any]:
    output_dir.mkdir(parents=True, exist_ok=True)
    results = []
    index_rows = []
    for index, capture in enumerate(captures, 1):
        result = measure_hobj(capture, model_path, package_root)
        result_path = output_dir / f"capture_{index:02d}.json"
        write_json(result_path, result)
        results.append(result)
        index_rows.append({
            "capture_index": index,
            "input_sha256": file_sha256(capture),
            "result_json": str(result_path.resolve()),
            **{f"{edge}_mm": result["reported_edges_mm"][edge] for edge in EDGES},
            **{
                f"{edge}_global_mm": result["global_edges_mm"][edge]
                for edge in EDGES
            },
            **{
                f"{edge}_head_tail_mm": result["head_tail_edges_mm"][edge]
                for edge in EDGES
            },
            "A_minus_C_mm": result["reported_edges_mm"]["A"] - result["reported_edges_mm"]["C"],
            "B_minus_D_mm": result["reported_edges_mm"]["B"] - result["reported_edges_mm"]["D"],
            "path_tokens_used": False,
            "orientation_mapping_applied": False,
        })
    summary = summarize_results(results)
    if manifest_entries is not None:
        pose_pair_summary = summarize_pose_pair_consistency(
            manifest_entries,
            {
                Path(result["input_path"]).name: result
                for result in results
            },
        )
        summary["explicit_calibration_pose_pair_audit"] = pose_pair_summary
        write_json(output_dir / "pose_pair_consistency.json", pose_pair_summary)
        write_csv(
            output_dir / "pose_pair_residuals.csv",
            pose_pair_summary["pair_residuals_mm"],
        )
    summary["model_path"] = str(model_path.resolve())
    summary["model_sha256"] = file_sha256(model_path)
    write_csv(output_dir / "capture_results.csv", index_rows)
    write_json(output_dir / "repeatability_summary.json", summary)
    return summary
