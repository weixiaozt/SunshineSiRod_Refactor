#!/usr/bin/env python3
"""用CMM同棒两组HOBJ离线验证倒角相机指纹模型。

本脚本只在已知验证数据集中显式使用物理调头关系；生产测量程序不导入本脚本，
运行模型也不会依据路径或文件名重排角点。
"""

from __future__ import annotations

import argparse
from concurrent.futures import ProcessPoolExecutor
import json
from pathlib import Path
import sys
from typing import Any

import numpy as np


MODULE_ROOT = Path(__file__).resolve().parent
sys.path.insert(0, str(MODULE_ROOT / "src"))

from chamfer_geometry import load_calibration, measure_hobj  # noqa: E402


DEVICE_CORNERS = ("AD", "AB", "CB", "CD")
TURNED_DEVICE_TO_PHYSICAL = {
    "AD": "AB",
    "AB": "AD",
    "CB": "CD",
    "CD": "CB",
}


def _worker(arguments: tuple[str, str, int]) -> dict[str, Any]:
    path_text, model_text, stations = arguments
    path = Path(path_text)
    result = measure_hobj(path, load_calibration(Path(model_text)), station_count=stations)
    return {
        "capture": path.name,
        "group": path.parent.name,
        "corners": {
            corner: {
                "chord_mm": result["corners"][corner]["metrics"]["chord_mm"]["value"],
                "corner_angle_deg": result["corners"][corner]["metrics"]["corner_angle_deg"]["value"],
            }
            for corner in DEVICE_CORNERS
        },
    }


def _stats(values: list[float]) -> dict[str, float | int]:
    array = np.asarray(values, dtype=float)
    return {
        "count": int(array.size),
        "mean": float(np.mean(array)),
        "std": float(np.std(array, ddof=1)) if array.size > 1 else 0.0,
        "min": float(np.min(array)),
        "max": float(np.max(array)),
        "range": float(np.ptp(array)),
    }


def main() -> int:
    parser = argparse.ArgumentParser(description="CMM 19图全切片倒角模型回归")
    parser.add_argument("--input", type=Path, default=MODULE_ROOT.parent / "CMM机构HOBJ")
    parser.add_argument("--model", type=Path, required=True)
    parser.add_argument("--truth", type=Path,
                        default=MODULE_ROOT / "calibration" / "cmm_chamfer_truth_reported.json")
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--stations", type=int, default=850)
    parser.add_argument("--workers", type=int, default=3)
    parser.add_argument("--exclude", nargs="*", default=["14_16.hobj"])
    args = parser.parse_args()

    model = load_calibration(args.model)
    excluded = set(args.exclude)
    paths = sorted(path for path in args.input.glob("*/*.hobj") if path.name not in excluded)
    if len(paths) != 19:
        raise RuntimeError(f"排除异常图后应为19张，实际为{len(paths)}张")
    jobs = [(str(path), str(args.model.resolve()), int(args.stations)) for path in paths]
    with ProcessPoolExecutor(max_workers=int(args.workers)) as executor:
        scans = list(executor.map(_worker, jobs))

    grouped: dict[str, dict[str, dict[str, list[float]]]] = {
        group: {
            corner: {"chord_mm": [], "corner_angle_deg": []}
            for corner in DEVICE_CORNERS
        }
        for group in ("head_to_tail", "tail_to_head")
    }
    for scan in scans:
        group = scan["group"]
        if group not in grouped:
            raise ValueError(f"未知CMM验证组：{group}")
        for device_corner, metrics in scan["corners"].items():
            physical_corner = (
                TURNED_DEVICE_TO_PHYSICAL[device_corner]
                if group == "tail_to_head"
                else device_corner
            )
            for field, value in metrics.items():
                grouped[group][physical_corner][field].append(float(value))

    group_statistics = {
        group: {
            corner: {field: _stats(values) for field, values in fields.items()}
            for corner, fields in corners.items()
        }
        for group, corners in grouped.items()
    }
    truth_payload = json.loads(args.truth.read_text(encoding="utf-8"))
    truth_candidate = model["camera_metric_calibration"]["selected_truth_candidate"]
    truth = {
        corner: truth_payload["report_as_written"][reported]
        for corner, reported in truth_payload["candidate_mappings"][truth_candidate].items()
    }
    comparisons: dict[str, Any] = {}
    normalized: list[float] = []
    for corner in DEVICE_CORNERS:
        first = group_statistics["head_to_tail"][corner]
        turned = group_statistics["tail_to_head"][corner]
        chord_difference = float(first["chord_mm"]["mean"] - turned["chord_mm"]["mean"])
        angle_difference = float(
            first["corner_angle_deg"]["mean"] - turned["corner_angle_deg"]["mean"]
        )
        combined_chord_mean = float(
            np.mean(grouped["head_to_tail"][corner]["chord_mm"]
                    + grouped["tail_to_head"][corner]["chord_mm"])
        )
        combined_angle_mean = float(
            np.mean(grouped["head_to_tail"][corner]["corner_angle_deg"]
                    + grouped["tail_to_head"][corner]["corner_angle_deg"])
        )
        chord_truth = float(truth[corner]["chord_mm"])
        angle_truth = float(truth[corner]["corner_angle_deg"])
        comparisons[corner] = {
            "head_to_tail_chord_mean_mm": first["chord_mm"]["mean"],
            "tail_to_head_chord_mean_mm": turned["chord_mm"]["mean"],
            "turnover_chord_difference_mm": chord_difference,
            "combined_chord_mean_mm": combined_chord_mean,
            "cmm_chord_mm": chord_truth,
            "combined_chord_error_mm": combined_chord_mean - chord_truth,
            "head_to_tail_angle_mean_deg": first["corner_angle_deg"]["mean"],
            "tail_to_head_angle_mean_deg": turned["corner_angle_deg"]["mean"],
            "turnover_angle_difference_deg": angle_difference,
            "combined_angle_mean_deg": combined_angle_mean,
            "cmm_angle_deg": angle_truth,
            "combined_angle_error_deg": combined_angle_mean - angle_truth,
        }
        normalized.extend([
            chord_difference / 0.01,
            angle_difference / 0.01,
            (combined_chord_mean - chord_truth) / 0.01,
            (combined_angle_mean - angle_truth) / 0.01,
        ])

    payload = {
        "model": str(args.model.resolve()),
        "algorithm": model["model"],
        "station_count": int(args.stations),
        "excluded": sorted(excluded),
        "capture_count": len(scans),
        "explicit_validation_mapping_only": True,
        "runtime_orientation_inference": False,
        "truth_candidate": truth_candidate,
        "summary_normalized_rms": float(np.sqrt(np.mean(np.asarray(normalized) ** 2))),
        "fixed_device_records": scans,
        "comparisons": comparisons,
        "group_statistics": group_statistics,
    }
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(payload, ensure_ascii=False, indent=2), encoding="utf-8")
    print(json.dumps({
        "output": str(args.output.resolve()),
        "summary_normalized_rms": payload["summary_normalized_rms"],
        "comparisons": comparisons,
    }, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
