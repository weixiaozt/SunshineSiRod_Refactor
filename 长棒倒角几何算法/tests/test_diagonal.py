from __future__ import annotations

from pathlib import Path
import sys
import unittest

import numpy as np


ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "src"))

from chamfer_geometry.diagonal import (  # noqa: E402
    ALGORITHM_ID,
    ALGORITHM_VERSION,
    DIAGONALS,
    ENDPOINTS,
    _global_endpoint,
    _interpolate_points,
    _raw_reversed_point_to_vendor_local,
    load_global_coordinate_calibration,
)


class DiagonalDefinitionTests(unittest.TestCase):
    def test_frozen_endpoint_definition(self):
        self.assertEqual(ALGORITHM_VERSION, 1)
        self.assertEqual(ALGORITHM_ID, "long_rod_physical_chamfer_endpoint_global_diagonal")
        self.assertEqual(DIAGONALS["diagonal_1"], ("point2", "point6"))
        self.assertEqual(DIAGONALS["diagonal_2"], ("point1", "point5"))
        self.assertEqual(ENDPOINTS["point2"], {"corner": "AB", "face": "A", "camera": "Right"})
        self.assertEqual(ENDPOINTS["point6"], {"corner": "CD", "face": "C", "camera": "Down"})
        self.assertEqual(ENDPOINTS["point1"], {"corner": "AD", "face": "A", "camera": "Top"})
        self.assertEqual(ENDPOINTS["point5"], {"corner": "CB", "face": "C", "camera": "Left"})

    def test_reversed_crop_coordinate_returns_vendor_column_coordinate(self):
        result = _raw_reversed_point_to_vendor_local(
            np.array([0.0, 3.0]),
            {"local_x_scale_mm_per_pixel": 0.015},
            {"reversed_crop_start_pixel": 500, "reversed_crop_stop_pixel": 3200},
        )
        np.testing.assert_allclose(result, [2699 * 0.015, 3.0])

    def test_global_endpoint_anchors_at_raw_corner_and_uses_corrected_ray(self):
        geometry = {
            "theoretical_corner_xz_mm": np.array([10.0, 20.0]),
        }
        metric_geometry = {
            "theoretical_corner_xz_mm": np.array([100.0, 200.0]),
            "left_endpoint_xz_mm": np.array([102.0, 203.0]),
            "right_endpoint_xz_mm": np.array([99.0, 204.0]),
        }
        result = _global_endpoint(
            geometry,
            metric_geometry,
            face="A",
            corner_config={
                "reversed_profile_left_face": "A",
                "reversed_profile_right_face": "D",
            },
            camera_config={"local_x_scale_mm_per_pixel": 1.0},
            parameters={"reversed_crop_start_pixel": 0, "reversed_crop_stop_pixel": 101},
            global_matrix=np.eye(2),
            global_origin=np.array([5.0, 7.0]),
        )
        # raw corner vendor=(100-10,20); corrected ray reversed=(2,3), vendor=(-2,3)
        np.testing.assert_allclose(result, [93.0, 30.0])

    def test_interpolation_refuses_large_missing_gap(self):
        samples = [
            {"physical_y_mm": 0.0, "global_xz_mm": [0.0, 0.0]},
            {"physical_y_mm": 1.0, "global_xz_mm": [1.0, 2.0]},
            {"physical_y_mm": 10.0, "global_xz_mm": [10.0, 20.0]},
        ]
        result = _interpolate_points(samples, np.array([0.5, 5.0]), maximum_gap_mm=2.0)
        np.testing.assert_allclose(result[0], [0.5, 1.0])
        self.assertTrue(np.all(np.isnan(result[1])))

    def test_formal_length_model_supplies_fixed_y_and_four_camera_extrinsics(self):
        path = (
            ROOT.parent
            / "ABCD_length6图完美模板标定法"
            / "models"
            / "long_rod_template_calibrated_length_210_105.json"
        )
        payload = load_global_coordinate_calibration(path)
        runtime = payload["runtime_calibration"]
        self.assertEqual(runtime["y_synchronization"], "fixed_bias_interpolation")
        self.assertEqual(set(runtime["matrices"]), {"Top", "Right", "Left", "Down"})
        self.assertEqual(len(runtime["rows"]), 850)
        self.assertEqual(len(runtime["origins"]), 850)


if __name__ == "__main__":
    unittest.main()
