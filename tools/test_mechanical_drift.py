from __future__ import annotations

import unittest

import numpy as np

from tools.mechanical_drift import build_model, classify, local_shift
from tools.measurement_dashboard import apply_manual_offsets


class MechanicalDriftTests(unittest.TestCase):
    def setUp(self) -> None:
        self.fractions = [0.05, 0.50, 0.95]
        self.normal_shape = np.zeros((4, 3, 2), dtype=float)
        self.drift = np.array(
            [
                [[0.0, 0.0], [-0.3, 0.3], [-0.6, 0.6]],
                [[0.0, 0.0], [0.3, 0.3], [0.6, 0.6]],
                [[0.0, 0.0], [0.3, -0.3], [0.6, -0.6]],
                [[0.0, 0.0], [-0.3, -0.3], [-0.6, -0.6]],
            ],
            dtype=float,
        )
        normal = {
            "n1": self.normal_shape - self.drift * 0.03,
            "n2": self.normal_shape,
            "n3": self.normal_shape + self.drift * 0.02,
        }
        abnormal = {
            "a1": self.normal_shape + self.drift * 0.99,
            "a2": self.normal_shape + self.drift * 1.01,
        }
        self.model = build_model(
            normal,
            abnormal,
            self.fractions,
            bar_id="standard-bar",
            specification="210_105",
        )

    def test_classifies_normal_abnormal_and_unknown_dead_band(self) -> None:
        self.assertEqual(classify(self.model, self.normal_shape)["status"], "normal")
        abnormal = classify(self.model, self.normal_shape + self.drift)
        self.assertEqual(abnormal["status"], "abnormal_corrected")
        self.assertAlmostEqual(abnormal["amplitude"], 1.0, places=2)
        self.assertEqual(classify(self.model, self.normal_shape + self.drift * 0.45)["status"], "unknown_invalid")
        unrelated = self.normal_shape.copy()
        unrelated[:, 1:, 0] = np.array([[0.5, -0.5], [-0.5, 0.5], [0.5, -0.5], [-0.5, 0.5]])
        self.assertEqual(classify(self.model, unrelated)["status"], "unknown_invalid")

    def test_interpolates_per_camera_local_correction(self) -> None:
        shift_x, shift_z = local_shift(self.model, 1, 0.50, 1.0)
        self.assertAlmostEqual(shift_x, -0.3, places=6)
        self.assertAlmostEqual(shift_z, 0.3, places=6)

    def test_manual_length_compensation_preserves_source(self) -> None:
        source = {"stick_length_mm": "826.900000", "A_mm": "210.000000"}
        corrected = apply_manual_offsets(
            source,
            {"A": 0.0, "B": 0.0, "C": 0.0, "D": 0.0},
            {"diag1": 0.0, "diag2": 0.0},
            0.125,
            {"head": {}, "tail": {}},
        )
        self.assertEqual(source["stick_length_mm"], "826.900000")
        self.assertEqual(corrected["stick_length_mm"], "827.025000")


if __name__ == "__main__":
    unittest.main()
