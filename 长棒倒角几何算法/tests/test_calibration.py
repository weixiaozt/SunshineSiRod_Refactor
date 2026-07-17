from __future__ import annotations

from pathlib import Path
import sys
import unittest

import numpy as np


ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "src"))

from chamfer_geometry.measurement import (  # noqa: E402
    ALGORITHM_ID,
    ALGORITHM_NAME,
    ALGORITHM_VERSION,
    _metric_geometry,
    load_calibration,
)


class CalibrationMappingTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.model = load_calibration(ROOT / "models" / "chamfer_camera_metric_geometry_210_105.json")

    def test_fixed_corner_camera_mapping(self):
        expected = {"AD": "Top", "AB": "Right", "CB": "Left", "CD": "Down"}
        self.assertEqual(
            {corner: item["camera"] for corner, item in self.model["corners"].items()},
            expected,
        )

    def test_frozen_algorithm_identity(self):
        self.assertEqual(ALGORITHM_NAME, "长棒倒角几何·四相机指纹度量修正算法")
        self.assertEqual(ALGORITHM_ID, "long_rod_chamfer_four_camera_fingerprint_metric_correction")
        self.assertEqual(ALGORITHM_VERSION, 3)
        self.assertEqual(self.model["model"], ALGORITHM_ID)
        self.assertEqual(self.model["algorithm_name"], ALGORITHM_NAME)

    def test_frozen_camera_fingerprint_metric_matrices(self):
        expected = {
            "Left": [[0.9693549359760417, -0.00501683459129833], [0.0, 0.9690408289830409]],
            "Right": [[0.958095150430887, 0.011851165730768609], [0.0, 0.9570138962357867]],
            "Top": [[0.965367481776057, -0.0062411215988555585], [0.0, 0.9647216975075146]],
            "Down": [[0.9551382349077099, 0.0057133268045797304], [0.0, 0.9538751144074598]],
        }
        for camera, matrix in expected.items():
            np.testing.assert_allclose(
                self.model["hobj_channels"][camera]["metric_transform_2x2"],
                matrix,
                rtol=0.0,
                atol=1e-12,
            )

    def test_reversed_profile_face_mapping(self):
        expected = {
            "AD": ("A", "D"),
            "AB": ("B", "A"),
            "CB": ("C", "B"),
            "CD": ("D", "C"),
        }
        self.assertEqual(
            {
                corner: (
                    item["reversed_profile_left_face"],
                    item["reversed_profile_right_face"],
                )
                for corner, item in self.model["corners"].items()
            },
            expected,
        )

    def test_model_contains_no_final_value_compensation(self):
        text = (ROOT / "models" / "chamfer_camera_metric_geometry_210_105.json").read_text(
            encoding="utf-8"
        )
        for forbidden in (
            "angle_offsets_deg",
            "orientation_detector",
            "orientation_models",
            "projection_face_values_mm",
            "validation_records",
        ):
            self.assertNotIn(forbidden, text)

    def test_metric_matrix_recomputes_one_consistent_triangle(self):
        geometry = {
            "theoretical_corner_xz_mm": np.array([0.0, 0.0]),
            "left_endpoint_xz_mm": np.array([1.0, 0.0]),
            "right_endpoint_xz_mm": np.array([0.0, 1.0]),
        }
        result = _metric_geometry(
            geometry,
            {"metric_transform_2x2": [[2.0, 0.0], [0.0, 1.0]]},
        )
        self.assertAlmostEqual(result["left_projection_mm"], 2.0)
        self.assertAlmostEqual(result["right_projection_mm"], 1.0)
        self.assertAlmostEqual(result["chord_mm"], np.sqrt(5.0))
        self.assertAlmostEqual(result["corner_angle_deg"], 90.0)

    def test_local_scale_curve_is_clamped_to_calibrated_domain(self):
        geometry = {
            "theoretical_corner_xz_mm": np.array([0.0, 0.0]),
            "left_endpoint_xz_mm": np.array([10.0, 0.0]),
            "right_endpoint_xz_mm": np.array([0.0, 10.0]),
        }
        result = _metric_geometry(
            geometry,
            {
                "metric_transform_2x2": [[1.0, 0.0], [0.0, 1.0]],
                "metric_local_ray_scale_per_mm2": 0.1,
                "metric_local_ray_reference_mm2": 1.0,
                "metric_local_ray_delta_bounds_mm2": [-0.1, 0.2],
            },
        )
        # 未钳制会把10 mm射线放大到109 mm；现在只允许使用标定域上界0.2。
        self.assertAlmostEqual(result["left_projection_mm"], 10.2)
        self.assertAlmostEqual(result["right_projection_mm"], 10.2)
        self.assertAlmostEqual(result["chord_mm"], np.sqrt(2.0) * 10.2)

    def test_frozen_local_scale_fingerprint_parameters(self):
        expected = {
            "Left": (0.0288904476077354, 0.6661081176761773),
            "Right": (0.05419177987921032, 1.0059951100349385),
            "Top": (-0.035688349018195814, 1.0052080359657132),
            "Down": (0.09999999999621344, 0.6582119689125318),
        }
        for camera, (coefficient, reference) in expected.items():
            config = self.model["hobj_channels"][camera]
            self.assertAlmostEqual(config["metric_local_ray_scale_per_mm2"], coefficient)
            self.assertAlmostEqual(config["metric_local_ray_reference_mm2"], reference)
            low, high = config["metric_local_ray_delta_bounds_mm2"]
            self.assertLess(low, 0.0)
            self.assertGreater(high, 0.0)


if __name__ == "__main__":
    unittest.main()
