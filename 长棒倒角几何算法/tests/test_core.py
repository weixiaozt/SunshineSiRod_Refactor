from __future__ import annotations

from pathlib import Path
import sys
import unittest

import numpy as np


ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "src"))

from chamfer_geometry.core import extract_corner_geometry, included_angle_deg  # noqa: E402


PARAMETERS = {
    "minimum_profile_points": 800,
    "apex_smoothing_points": 21,
    "apex_search_outer_fraction": 0.03,
    "main_face_outer_fraction": 0.08,
    "main_face_inner_fraction": 0.08,
    "minimum_main_face_points": 80,
    "chamfer_shoulder_distance_mm": 0.02,
    "chamfer_shoulder_face_rms_multiplier": 4.0,
    "minimum_chamfer_points": 15,
    "maximum_chamfer_points": 180,
    "maximum_apex_gap_points": 12,
}


def synthetic_profile(chamfer_slope: float, intercept: float = -0.5):
    x = np.linspace(-10.0, 10.0, 2001)
    left = x
    right = -x
    chamfer = chamfer_slope * x + intercept
    left_x = intercept / (1.0 - chamfer_slope)
    right_x = intercept / (-1.0 - chamfer_slope)
    z = np.where(x < left_x, left, np.where(x > right_x, right, chamfer))
    return x, z, left_x, right_x


class ChamferCoreTests(unittest.TestCase):
    def test_direct_ray_angle_is_not_acute_folded(self):
        first = np.array([1.0, 0.0])
        second = np.array([-0.5, np.sqrt(3.0) / 2.0])
        self.assertAlmostEqual(included_angle_deg(first, second), 120.0, places=10)

    def test_symmetric_45_degree_chamfer_has_equal_projections(self):
        x, z, _, _ = synthetic_profile(0.0)
        result = extract_corner_geometry(x, z, PARAMETERS)
        self.assertAlmostEqual(result["corner_angle_deg"], 90.0, places=6)
        self.assertAlmostEqual(result["left_projection_mm"], result["right_projection_mm"], places=6)
        self.assertAlmostEqual(result["chord_mm"], 1.0, places=6)

    def test_asymmetric_chamfer_keeps_two_independent_projections(self):
        x, z, left_x, right_x = synthetic_profile(0.20)
        result = extract_corner_geometry(x, z, PARAMETERS)
        expected_left = np.sqrt(2.0) * abs(left_x)
        expected_right = np.sqrt(2.0) * abs(right_x)
        self.assertAlmostEqual(result["left_projection_mm"], expected_left, places=6)
        self.assertAlmostEqual(result["right_projection_mm"], expected_right, places=6)
        self.assertGreater(abs(result["left_projection_mm"] - result["right_projection_mm"]), 0.1)

    def test_translation_does_not_change_geometry(self):
        x, z, _, _ = synthetic_profile(-0.15)
        first = extract_corner_geometry(x, z, PARAMETERS)
        second = extract_corner_geometry(x + 12.3, z - 4.7, PARAMETERS)
        for field in ("chord_mm", "left_projection_mm", "right_projection_mm", "corner_angle_deg"):
            self.assertAlmostEqual(first[field], second[field], places=8)


if __name__ == "__main__":
    unittest.main()
