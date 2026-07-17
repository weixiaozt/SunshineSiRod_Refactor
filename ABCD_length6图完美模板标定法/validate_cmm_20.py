#!/usr/bin/env python3
"""Blindly compare two ABCD models with the explicitly grouped 20-image CMM bar."""

from __future__ import annotations

import argparse
import sys
from pathlib import Path
from typing import Any, Mapping

import numpy as np


ROOT = Path(__file__).resolve().parent
sys.path.insert(0, str(ROOT / "src"))

from abcd_length.common import EDGES, write_csv, write_json  # noqa: E402
from abcd_length.measurement import measure_hobj  # noqa: E402


CMM_TRUTH = {
    "head": {"A": 210.0473, "B": 105.0607, "C": 210.0473, "D": 105.0607},
    "tail": {"A": 210.0514, "B": 105.0507, "C": 210.0514, "D": 105.0507},
}


def physical_head_tail(
    result: Mapping[str, Any],
    calibration_pose: str,
) -> dict[str, dict[str, float]]:
    device_head = result["head_edges_mm"]
    device_tail = result["tail_edges_mm"]
    if calibration_pose == "normal":
        return {
            "head": {edge: float(device_head[edge]) for edge in EDGES},
            "tail": {edge: float(device_tail[edge]) for edge in EDGES},
        }
    if calibration_pose == "turned":
        return {
            "head": {
                "A": float(device_tail["A"]),
                "B": float(device_tail["D"]),
                "C": float(device_tail["C"]),
                "D": float(device_tail["B"]),
            },
            "tail": {
                "A": float(device_head["A"]),
                "B": float(device_head["D"]),
                "C": float(device_head["C"]),
                "D": float(device_head["B"]),
            },
        }
    raise ValueError(f"Unsupported calibration pose: {calibration_pose}")


def summarize(rows: list[dict[str, Any]], model_name: str) -> list[dict[str, Any]]:
    model_rows = [row for row in rows if row["model_name"] == model_name]
    output = []
    for end in ("head", "tail"):
        for edge in EDGES:
            key = f"physical_{end}_{edge}_mm"
            values = np.asarray([float(row[key]) for row in model_rows], dtype=float)
            errors = values - float(CMM_TRUTH[end][edge])
            output.append({
                "model_name": model_name,
                "physical_end": end,
                "edge": edge,
                "capture_count": len(values),
                "cmm_truth_mm": CMM_TRUTH[end][edge],
                "measured_mean_mm": float(np.mean(values)),
                "measured_std_mm": float(np.std(values)),
                "measured_range_mm": float(np.ptp(values)),
                "bias_mm": float(np.mean(errors)),
                "rms_error_mm": float(np.sqrt(np.mean(errors * errors))),
                "max_abs_error_mm": float(np.max(np.abs(errors))),
            })
    return output


def main() -> int:
    parser = argparse.ArgumentParser(
        description=(
            "CMM bar validation only. Pose grouping is supplied explicitly and is never "
            "used by production measurement."
        )
    )
    parser.add_argument(
        "--normal-dir",
        type=Path,
        default=ROOT.parent / "CMM机构HOBJ" / "head_to_tail",
    )
    parser.add_argument(
        "--turned-dir",
        type=Path,
        default=ROOT.parent / "CMM机构HOBJ" / "tail_to_head",
    )
    parser.add_argument(
        "--baseline-model",
        type=Path,
        default=ROOT / "models" / "abcd_length_six_capture_210_105.json",
    )
    parser.add_argument(
        "--candidate-model",
        type=Path,
        default=ROOT / "models" / "candidate_y_synchronized_210_105.json",
    )
    parser.add_argument(
        "--output-dir",
        type=Path,
        default=ROOT / "results" / "cmm_20_abcd_validation",
    )
    parser.add_argument(
        "--only-model",
        type=Path,
        default=None,
        help="Measure one explicitly supplied model instead of the baseline/candidate pair.",
    )
    parser.add_argument("--package-root", type=Path, default=None)
    args = parser.parse_args()

    groups = {
        "normal": sorted(args.normal_dir.glob("*.hobj"), key=lambda path: path.name),
        "turned": sorted(args.turned_dir.glob("*.hobj"), key=lambda path: path.name),
    }
    if any(len(paths) != 10 for paths in groups.values()):
        raise ValueError(
            f"Expected 10 explicitly grouped captures per pose, got "
            f"normal={len(groups['normal'])}, turned={len(groups['turned'])}"
        )
    models = (
        {"explicit_single_model": args.only_model}
        if args.only_model is not None
        else {
            "baseline_2d_same_row": args.baseline_model,
            "candidate_fixed_y_synchronized": args.candidate_model,
        }
    )
    args.output_dir.mkdir(parents=True, exist_ok=True)
    rows: list[dict[str, Any]] = []
    for model_name, model_path in models.items():
        model_output = args.output_dir / model_name
        model_output.mkdir(parents=True, exist_ok=True)
        for calibration_pose, captures in groups.items():
            for capture in captures:
                result = measure_hobj(capture, model_path, args.package_root)
                physical = physical_head_tail(result, calibration_pose)
                result_path = model_output / f"{calibration_pose}_{capture.stem}.json"
                write_json(result_path, result)
                row: dict[str, Any] = {
                    "model_name": model_name,
                    "calibration_pose": calibration_pose,
                    "capture_id": capture.stem,
                    "input_path": str(capture.resolve()),
                    "valid_station_count": result["diagnostics"]["valid_station_count"],
                    "path_or_filename_orientation_used": False,
                    "production_orientation_mapping_applied": False,
                    "validation_mapping_applied_after_measurement": True,
                    "camera_y_synchronization_applied": result[
                        "camera_y_synchronization_applied"
                    ],
                    "result_json": str(result_path.resolve()),
                }
                for end in ("head", "tail"):
                    for edge in EDGES:
                        value = physical[end][edge]
                        row[f"physical_{end}_{edge}_mm"] = value
                        row[f"physical_{end}_{edge}_error_mm"] = (
                            value - CMM_TRUTH[end][edge]
                        )
                rows.append(row)

    summary_rows = []
    for model_name in models:
        summary_rows.extend(summarize(rows, model_name))
    write_csv(args.output_dir / "per_capture_physical_abcd.csv", rows)
    write_csv(args.output_dir / "cmm_error_summary.csv", summary_rows)
    write_json(args.output_dir / "cmm_truth.json", CMM_TRUTH)
    print(args.output_dir / "per_capture_physical_abcd.csv")
    print(args.output_dir / "cmm_error_summary.csv")
    for row in summary_rows:
        print(
            f"{row['model_name']} {row['physical_end']} {row['edge']}: "
            f"mean={row['measured_mean_mm']:.6f} "
            f"truth={row['cmm_truth_mm']:.6f} "
            f"bias={row['bias_mm']:+.6f} "
            f"rms={row['rms_error_mm']:.6f} mm"
        )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
