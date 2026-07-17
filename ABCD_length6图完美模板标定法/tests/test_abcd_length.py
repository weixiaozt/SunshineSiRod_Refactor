from __future__ import annotations

import csv
import json
import sys
import tempfile
import unittest
from pathlib import Path

import numpy as np


ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "src"))

from abcd_length.calibration import discover_six_captures
from abcd_length.common import (
    LONG_ROD_MODEL_VERSION,
    aggregate_edge_records,
    apply_bd_pair_relative_bias,
    edge_lengths,
    load_perfect_rectangle_truth,
    perfect_rectangle_corners,
    robust_vector_center,
    smooth_origin_curve,
    validate_model,
)
from abcd_length.validation import summarize_pose_pair_consistency, summarize_results


class AbcdLengthTests(unittest.TestCase):
    def test_bd_pair_k_is_symmetric_and_preserves_sum(self) -> None:
        source = [{"A": 210.0, "B": 105.0, "C": 210.0, "D": 105.2}]
        corrected = apply_bd_pair_relative_bias(source, 0.010)[0]
        self.assertAlmostEqual(corrected["A"], 210.0)
        self.assertAlmostEqual(corrected["B"], 105.005)
        self.assertAlmostEqual(corrected["C"], 210.0)
        self.assertAlmostEqual(corrected["D"], 105.195)
        self.assertAlmostEqual(corrected["B"] + corrected["D"], 210.2)

    def test_perfect_rectangle_edges(self) -> None:
        truth = {"A": 210.01, "B": 105.11, "C": 210.01, "D": 105.11}
        measured = edge_lengths(perfect_rectangle_corners(truth))
        for edge, value in truth.items():
            self.assertAlmostEqual(measured[edge], value, places=9)

    def test_truth_requires_equal_opposites(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / "truth.csv"
            with path.open("w", newline="", encoding="utf-8") as handle:
                writer = csv.DictWriter(handle, fieldnames=["record_type", "A_mm", "B_mm", "C_mm", "D_mm"])
                writer.writeheader()
                writer.writerow({"record_type": "cross_section", "A_mm": 210, "B_mm": 105, "C_mm": 211, "D_mm": 105})
            with self.assertRaisesRegex(ValueError, "A == C"):
                load_perfect_rectangle_truth(path)

    def test_capture_discovery_does_not_parse_orientation_words(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            names = ["正.hobj", "反.hobj", "调头.hobj", "diaotou.hobj", "turnover.hobj", "plain.hobj"]
            for name in names:
                (root / name).touch()
            discovered = discover_six_captures(root)
            self.assertEqual(len(discovered), 6)
            self.assertEqual({path.name for path in discovered}, set(names))

    def test_robust_center_rejects_one_large_outlier(self) -> None:
        values = [[1.0, 2.0], [1.01, 1.99], [0.99, 2.01], [1.0, 2.0], [1.02, 2.01], [9.0, -5.0]]
        center, used, spread = robust_vector_center(values, 0.1)
        np.testing.assert_allclose(center, [1.0, 2.0], atol=0.02)
        self.assertEqual(used, 5)
        self.assertLess(spread, 0.05)

    def test_smoothing_preserves_constant_curve(self) -> None:
        curve = np.tile([[3.0, -2.0]], (75, 1))
        np.testing.assert_allclose(smooth_origin_curve(curve, 9), curve)

    def test_head_tail_aggregation(self) -> None:
        records = [
            {edge: float(index) for edge in "ABCD"}
            for index in range(12)
        ]
        result = aggregate_edge_records(records, 5)
        self.assertEqual(result["head"]["A"], 2.0)
        self.assertEqual(result["tail"]["A"], 9.0)
        self.assertEqual(result["global"]["A"], 5.5)
        self.assertEqual(result["reported"]["A"], 5.5)
        self.assertEqual(result["reported"]["B"], 5.5)

    def test_ac_global_and_bd_two_candidates(self) -> None:
        records = [
            {"A": float(index), "B": float(index * 2), "C": float(index + 10), "D": float(index * 3)}
            for index in range(20)
        ]
        result = aggregate_edge_records(records, 4)
        self.assertEqual(result["used_end_station_count_per_end"], 4)
        self.assertEqual(result["reported"]["A"], result["global"]["A"])
        self.assertEqual(result["reported"]["C"], result["global"]["C"])
        self.assertEqual(result["reported"]["B"], result["head_tail"]["B"])
        self.assertEqual(result["reported"]["D"], result["head_tail"]["D"])

    def test_pose_pair_consistency_crosses_bd_and_head_tail(self) -> None:
        def result(a: float, b: float, c: float, d: float) -> dict:
            global_edges = {"A": a, "B": b, "C": c, "D": d}
            return {
                "global_edges_mm": global_edges,
                "head_edges_mm": global_edges,
                "tail_edges_mm": global_edges,
                "head_tail_edges_mm": global_edges,
            }
        entries = [
            {"capture_file": "n.hobj", "calibration_pose": "normal", "pair_id": "1"},
            {"capture_file": "t.hobj", "calibration_pose": "turned", "pair_id": "1"},
        ]
        summary = summarize_pose_pair_consistency(
            entries,
            {
                "n.hobj": result(210.0, 105.1, 210.0, 105.2),
                "t.hobj": result(210.0, 105.2, 210.0, 105.1),
            },
        )
        self.assertEqual(summary["residual_summaries"]["BD_global_B_to_D"]["max_absolute_mm"], 0.0)
        self.assertFalse(summary["runtime_pose_detection_used"])

    def test_model_rejects_orientation_runtime_key(self) -> None:
        model = {
            "model": "abcd_length_six_capture_perfect_rectangle",
            "version": 2,
            "valid": True,
            "contains_final_edge_offsets": False,
            "path_based_orientation_detection": False,
            "orientation_detector": {"diaotou": "turnover"},
            "runtime_calibration": {
                "rows": list(range(10)),
                "origins": [{camera: [0, 0] for camera in ("Left", "Right", "Top", "Down")} for _ in range(10)],
                "windows": {camera: {"left": [1], "right": [2]} for camera in ("Left", "Right", "Top", "Down")},
                "matrices": {camera: [[1, 0], [0, 1]] for camera in ("Left", "Right", "Top", "Down")},
            },
        }
        with self.assertRaisesRegex(ValueError, "forbidden runtime orientation"):
            validate_model(model)

    def test_final_model_requires_fixed_y_and_one_pair_k(self) -> None:
        runtime = {
            "rows": list(range(10)),
            "origins": [
                {camera: [0, 0] for camera in ("Left", "Right", "Top", "Down")}
                for _ in range(10)
            ],
            "windows": {
                camera: {"left": [1], "right": [2]}
                for camera in ("Left", "Right", "Top", "Down")
            },
            "matrices": {
                camera: [[1, 0], [0, 1]]
                for camera in ("Left", "Right", "Top", "Down")
            },
            "y_synchronization": "fixed_bias_interpolation",
            "bd_pair_relative_bias_k_mm": 0.00015,
            "bd_primary_aggregation": "head_tail_dense_mean",
        }
        model = {
            "model": "abcd_length_six_capture_perfect_rectangle",
            "version": 3,
            "valid": True,
            "contains_final_edge_offsets": False,
            "contains_independent_final_edge_offsets": False,
            "path_based_orientation_detection": False,
            "runtime_calibration": runtime,
        }
        validate_model(model)
        runtime["y_synchronization"] = "none"
        with self.assertRaisesRegex(ValueError, "fixed camera-Y"):
            validate_model(model)

    def test_long_rod_model_locks_public_algorithm_name(self) -> None:
        runtime = {
            "rows": list(range(10)),
            "origins": [
                {camera: [0, 0] for camera in ("Left", "Right", "Top", "Down")}
                for _ in range(10)
            ],
            "windows": {
                camera: {"left": [1], "right": [2]}
                for camera in ("Left", "Right", "Top", "Down")
            },
            "matrices": {
                camera: [[1, 0], [0, 1]]
                for camera in ("Left", "Right", "Top", "Down")
            },
            "y_synchronization": "fixed_bias_interpolation",
            "bd_pair_relative_bias_k_mm": -0.0070223014168249165,
            "bd_primary_aggregation": "head_tail_dense_mean",
        }
        model = {
            "model": "abcd_length_six_capture_perfect_rectangle",
            "version": LONG_ROD_MODEL_VERSION,
            "algorithm_id": "long_rod_template_calibrated_length",
            "algorithm_name_zh": "长棒模板标定长度算法",
            "valid": True,
            "contains_final_edge_offsets": False,
            "contains_independent_final_edge_offsets": False,
            "path_based_orientation_detection": False,
            "runtime_calibration": runtime,
        }
        validate_model(model)
        model["algorithm_name_zh"] = "错误名称"
        with self.assertRaisesRegex(ValueError, "public name"):
            validate_model(model)

    def test_repeatability_summary_does_not_group_or_map_orientation(self) -> None:
        results = []
        for offset in (0.0, 0.01):
            results.append({
                "reported_edges_mm": {
                    "A": 210.0 + offset,
                    "B": 105.0 + offset,
                    "C": 210.0 - offset,
                    "D": 105.0 - offset,
                },
                "path_or_filename_orientation_used": False,
                "orientation_mapping_applied": False,
            })
        summary = summarize_results(results)
        self.assertFalse(summary["path_tokens_used"])
        self.assertFalse(summary["orientation_groups_used"])
        self.assertFalse(summary["orientation_mapping_applied"])
        self.assertAlmostEqual(summary["edge_statistics"]["A"]["range_mm"], 0.01)


if __name__ == "__main__":
    unittest.main()
