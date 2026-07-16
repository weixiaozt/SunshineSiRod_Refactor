import importlib.util
import math
import sys
import unittest
from pathlib import Path

import numpy as np


RUNTIME_SOURCE = (
    Path(__file__).resolve().parents[1]
    / "release"
    / "SunshineSiRod_Global_DeliveryGeometry_20260715"
    / "coworker_endface_delivery"
    / "src"
    / "measure_single_scan.py"
)


def load_runtime_module():
    source_dir = RUNTIME_SOURCE.parent
    sys.path.insert(0, str(source_dir))
    try:
        spec = importlib.util.spec_from_file_location("coworker_halfplane_runtime", RUNTIME_SOURCE)
        module = importlib.util.module_from_spec(spec)
        assert spec.loader is not None
        spec.loader.exec_module(module)
        return module
    finally:
        sys.path.remove(str(source_dir))


runtime = load_runtime_module() if RUNTIME_SOURCE.is_file() else None


def rotation_matrix(axis, angle_deg):
    axis = np.asarray(axis, dtype=float)
    axis = axis / np.linalg.norm(axis)
    x, y, z = axis
    angle = math.radians(angle_deg)
    c = math.cos(angle)
    s = math.sin(angle)
    one_minus_c = 1.0 - c
    return np.array(
        [
            [c + x * x * one_minus_c, x * y * one_minus_c - z * s, x * z * one_minus_c + y * s],
            [y * x * one_minus_c + z * s, c + y * y * one_minus_c, y * z * one_minus_c - x * s],
            [z * x * one_minus_c - y * s, z * y * one_minus_c + x * s, c + z * z * one_minus_c],
        ],
        dtype=float,
    )


def synthetic_outward_normals(interior_angle_deg):
    angle = math.radians(interior_angle_deg)
    end_normal = np.array([0.0, 1.0, 0.0])
    side_normal = np.array([math.sin(angle), -math.cos(angle), 0.0])
    return side_normal, end_normal


@unittest.skipUnless(
    RUNTIME_SOURCE.is_file(),
    "coworker runtime source is available only in the local release workspace",
)
class CoworkerHalfplaneAngleTests(unittest.TestCase):
    def measured(self, target_deg):
        side_normal, end_normal = synthetic_outward_normals(target_deg)
        return runtime.material_interior_dihedral_deg(side_normal, end_normal)

    def test_preserves_acute_and_obtuse_material_angles(self):
        self.assertAlmostEqual(self.measured(60.0), 60.0, places=10)
        self.assertAlmostEqual(self.measured(120.0), 120.0, places=10)

    def test_is_independent_of_plane_normal_signs(self):
        side_normal, end_normal = synthetic_outward_normals(123.0)
        expected = runtime.material_interior_dihedral_deg(side_normal, end_normal)
        actual = runtime.material_interior_dihedral_deg(-side_normal, -end_normal)
        self.assertAlmostEqual(actual, expected, places=10)

    def test_is_invariant_under_rigid_rotation_and_turnover(self):
        side_normal, end_normal = synthetic_outward_normals(117.0)
        for axis, angle in (([1, 2, 3], 37.0), ([1, 0, 0], 180.0)):
            rotation = rotation_matrix(axis, angle)
            actual = runtime.material_interior_dihedral_deg(
                rotation @ side_normal,
                rotation @ end_normal,
            )
            self.assertAlmostEqual(actual, 117.0, places=10)

    def test_does_not_fold_or_compress_toward_90(self):
        self.assertAlmostEqual(self.measured(150.0), 150.0, places=10)
        self.assertGreater(self.measured(150.0), 90.0)


if __name__ == "__main__":
    unittest.main()
