#!/usr/bin/env python3
"""Build the approved 长棒模板标定长度算法 model.

The production model never detects whether a bar was turned.  Explicitly supplied
calibration groups are used only here to estimate one shared B/D device-pair K.
"""

from __future__ import annotations

import csv
import json
import sys
from pathlib import Path
from typing import Any, Mapping, Sequence

import numpy as np


ROOT = Path(__file__).resolve().parent
PROJECT_ROOT = ROOT.parent
sys.path.insert(0, str(ROOT / "src"))

from abcd_length.common import (  # noqa: E402
    LONG_ROD_MODEL_VERSION,
    file_sha256,
    validate_model,
    write_csv,
    write_json,
)
from abcd_length.measurement import measure_with_runtime, runtime_arrays  # noqa: E402
from abcd_length.vendor import import_vendor_pipeline  # noqa: E402


ALGORITHM_ID = "long_rod_template_calibrated_length"
ALGORITHM_NAME_ZH = "长棒模板标定长度算法"
SOURCE_MODEL = ROOT / "models" / "candidate_y_synchronized_210_105.json"
OUTPUT_MODEL = ROOT / "models" / "long_rod_template_calibrated_length_210_105.json"
OUTPUT_DIAGNOSTICS = (
    ROOT / "results" / "long_rod_template_length_algorithm" / "k_derivation.csv"
)
CTB_MANIFEST = ROOT / "calibration_capture_manifest.csv"
CMM_NORMAL_DIR = PROJECT_ROOT / "CMM机构HOBJ" / "head_to_tail"
CMM_TURNED_DIR = PROJECT_ROOT / "CMM机构HOBJ" / "tail_to_head"
GTT_NORMAL_CAPTURE = (
    PROJECT_ROOT / "测试HOBJ" / "105-4" / "A23-GTT1_3" / "10_16.hobj"
)
GTT_TURNED_CAPTURE = (
    PROJECT_ROOT / "测试HOBJ" / "105-4" / "A23-GTT2_3" / "10_56.hobj"
)
EXCLUDED_DAMAGED_CAPTURE = "14_16.hobj"


def load_ctb_groups() -> tuple[list[Path], list[Path]]:
    with CTB_MANIFEST.open("r", encoding="utf-8-sig", newline="") as handle:
        entries = list(csv.DictReader(handle))
    normal = [ROOT / row["capture_file"] for row in entries if row["calibration_pose"] == "normal"]
    turned = [ROOT / row["capture_file"] for row in entries if row["calibration_pose"] == "turned"]
    if len(normal) != 3 or len(turned) != 3:
        raise RuntimeError("CTB calibration manifest must explicitly contain 3+3 captures")
    return normal, turned


def load_cmm_groups() -> tuple[list[Path], list[Path]]:
    normal = sorted(CMM_NORMAL_DIR.glob("*.hobj"), key=lambda path: path.name)
    turned = [
        path
        for path in sorted(CMM_TURNED_DIR.glob("*.hobj"), key=lambda path: path.name)
        if path.name != EXCLUDED_DAMAGED_CAPTURE
    ]
    if len(normal) != 10 or len(turned) != 9:
        raise RuntimeError(
            "CMM K audit requires 10 head_to_tail plus 9 valid tail_to_head captures "
            f"after excluding {EXCLUDED_DAMAGED_CAPTURE}"
        )
    return normal, turned


def raw_group_mean(
    paths: Sequence[Path],
    measured: Mapping[Path, Mapping[str, Any]],
) -> dict[str, float]:
    return {
        edge: float(np.mean([
            float(measured[path]["raw_reported_edges_mm"][edge]) for path in paths
        ]))
        for edge in "ABCD"
    }


def k_dataset_row(
    dataset: str,
    normal_paths: Sequence[Path],
    turned_paths: Sequence[Path],
    measured: Mapping[Path, Mapping[str, Any]],
) -> dict[str, Any]:
    normal = raw_group_mean(normal_paths, measured)
    turned = raw_group_mean(turned_paths, measured)
    k_b_to_d = float(turned["D"] - normal["B"])
    k_d_to_b = float(normal["D"] - turned["B"])
    return {
        "dataset": dataset,
        "normal_capture_count": len(normal_paths),
        "turned_capture_count": len(turned_paths),
        "normal_capture_ids": ";".join(path.stem for path in normal_paths),
        "turned_capture_ids": ";".join(path.stem for path in turned_paths),
        "normal_raw_A_mm": normal["A"],
        "normal_raw_B_mm": normal["B"],
        "normal_raw_C_mm": normal["C"],
        "normal_raw_D_mm": normal["D"],
        "turned_raw_A_mm": turned["A"],
        "turned_raw_B_mm": turned["B"],
        "turned_raw_C_mm": turned["C"],
        "turned_raw_D_mm": turned["D"],
        "K_from_normal_B_to_turned_D_mm": k_b_to_d,
        "K_from_normal_D_to_turned_B_mm": k_d_to_b,
        "dataset_equal_weight_K_mm": float((k_b_to_d + k_d_to_b) / 2.0),
    }


def main() -> int:
    payload: dict[str, Any] = json.loads(SOURCE_MODEL.read_text(encoding="utf-8"))
    validate_model(payload)
    runtime = runtime_arrays(payload)
    pipeline = import_vendor_pipeline(None)

    ctb_normal, ctb_turned = load_ctb_groups()
    cmm_normal, cmm_turned = load_cmm_groups()
    datasets = {
        "CTB_six_capture_template": (ctb_normal, ctb_turned),
        "CMM_long_bar_14_16_excluded": (cmm_normal, cmm_turned),
        "GTT_10_16_10_56": ([GTT_NORMAL_CAPTURE], [GTT_TURNED_CAPTURE]),
    }

    all_paths = sorted(
        {path.resolve() for groups in datasets.values() for group in groups for path in group},
        key=lambda path: str(path),
    )
    missing = [path for path in all_paths if not path.is_file()]
    if missing:
        raise FileNotFoundError(f"Missing K calibration captures: {missing}")

    measured: dict[Path, Mapping[str, Any]] = {}
    for path in all_paths:
        print(f"measure fixed-Y K source {path.name}", flush=True)
        measured[path] = measure_with_runtime(path, runtime, pipeline)

    diagnostics = [
        k_dataset_row(name, normal, turned, measured)
        for name, (normal, turned) in datasets.items()
    ]
    k_mm = float(np.mean([
        float(row["dataset_equal_weight_K_mm"]) for row in diagnostics
    ]))

    residuals: list[float] = []
    for row in diagnostics:
        normal_b = float(row["normal_raw_B_mm"])
        normal_d = float(row["normal_raw_D_mm"])
        turned_b = float(row["turned_raw_B_mm"])
        turned_d = float(row["turned_raw_D_mm"])
        residual_b_to_d = (normal_b + k_mm / 2.0) - (turned_d - k_mm / 2.0)
        residual_d_to_b = (normal_d - k_mm / 2.0) - (turned_b + k_mm / 2.0)
        row["approved_shared_K_mm"] = k_mm
        row["corrected_normal_B_minus_turned_D_mm"] = residual_b_to_d
        row["corrected_normal_D_minus_turned_B_mm"] = residual_d_to_b
        row["corrected_BD_cross_rms_mm"] = float(np.sqrt(
            (residual_b_to_d ** 2 + residual_d_to_b ** 2) / 2.0
        ))
        residuals.extend((residual_b_to_d, residual_d_to_b))

    payload["version"] = LONG_ROD_MODEL_VERSION
    payload["algorithm_id"] = ALGORITHM_ID
    payload["algorithm_name_zh"] = ALGORITHM_NAME_ZH
    payload["valid"] = True
    payload["release_ready"] = True
    payload["validation_scope"] = (
        "CTB_3_plus_3; CMM_10_plus_9_excluding_14_16; GTT_10_16_plus_10_56"
    )
    payload["contains_final_edge_offsets"] = False
    payload["contains_independent_final_edge_offsets"] = False
    payload["calibration_strategy"] = (
        "long_rod_template_fixed_y_synchronization_plus_equal_weight_"
        "three_bar_bd_pair_k_plus_head_tail_dense_mean"
    )
    payload["calibration_note"] = (
        "Runtime applies one geometry chain to every HOBJ and never detects pose. "
        "A/C use all valid stations. B/D use 100 head plus 100 tail stations. "
        "The shared K is the equal-weight mean of the CTB, CMM and GTT B/D crossed-pose "
        "self-consistency estimates; CMM capture 14_16 is excluded as known corner damage."
    )
    payload["parameters"]["bd_pair_relative_bias_k_mm"] = k_mm
    payload["parameters"]["bd_primary_aggregation"] = "head_tail_dense_mean"
    payload["runtime_calibration"]["bd_pair_relative_bias_k_mm"] = k_mm
    payload["runtime_calibration"]["bd_primary_aggregation"] = "head_tail_dense_mean"
    payload["bd_pair_relative_bias_calibration"] = {
        "correction_definition": {
            "A": "unchanged",
            "B": "raw_B + K/2",
            "C": "unchanged",
            "D": "raw_D - K/2",
        },
        "applies_before_aggregation": True,
        "preserves_B_plus_D": True,
        "uses_runtime_pose_detection": False,
        "uses_path_or_filename_tokens": False,
        "fit_policy": (
            "First average each explicit capture group; derive B-to-D and D-to-B K; "
            "average the two constraints per bar; then average CTB, CMM and GTT equally."
        ),
        "excluded_capture": EXCLUDED_DAMAGED_CAPTURE,
        "source_fixed_y_model": str(SOURCE_MODEL.resolve()),
        "source_fixed_y_model_sha256": file_sha256(SOURCE_MODEL),
        "ctb_manifest_path": str(CTB_MANIFEST.resolve()),
        "ctb_manifest_sha256": file_sha256(CTB_MANIFEST),
        "dataset_count": len(diagnostics),
        "K_mm": k_mm,
        "dataset_diagnostics": diagnostics,
    }
    payload.setdefault("validation", {})["long_rod_template_bd_pair_k"] = {
        "dataset_count": len(diagnostics),
        "constraint_count": len(residuals),
        "corrected_cross_dataset_rms_mm": float(np.sqrt(np.mean(
            np.asarray(residuals, dtype=float) ** 2
        ))),
        "production_release_claimed": True,
    }

    validate_model(payload)
    write_json(OUTPUT_MODEL, payload)
    write_csv(OUTPUT_DIAGNOSTICS, diagnostics)
    print(f"algorithm={ALGORITHM_NAME_ZH}")
    print(f"K={k_mm:.12f} mm")
    print(OUTPUT_MODEL)
    print(OUTPUT_DIAGNOSTICS)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
