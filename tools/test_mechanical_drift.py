from __future__ import annotations

import unittest
import sqlite3

import numpy as np

from tools.mechanical_drift import build_model, classify, local_shift
from tools.measure_square_rod_edges import drift_fraction_for_row
from tools.measurement_dashboard import apply_manual_offsets, _migrate_endface_summary_to_direct_average


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
            reference_common_start_row=100,
            reference_common_end_row=1100,
        )

    def test_classifies_continuous_known_drift_without_invalid_dead_band(self) -> None:
        self.assertEqual(classify(self.model, self.normal_shape)["status"], "normal")
        abnormal = classify(self.model, self.normal_shape + self.drift)
        self.assertEqual(abnormal["status"], "abnormal_corrected")
        self.assertAlmostEqual(abnormal["amplitude"], 1.0, places=2)
        partial = classify(self.model, self.normal_shape + self.drift * 0.45)
        self.assertEqual(partial["status"], "abnormal_corrected")
        self.assertTrue(partial["valid"])
        unrelated = self.normal_shape + self.drift * 0.70
        unrelated[:, 1:, 0] += np.array([[0.8, -0.8], [-0.8, 0.8], [0.8, -0.8], [-0.8, 0.8]])
        unmatched = classify(self.model, unrelated)
        self.assertEqual(unmatched["status"], "unmatched_unadjusted")
        self.assertTrue(unmatched["valid"])
        self.assertTrue(unmatched["warning"])

    def test_rigid_camera_placement_offsets_do_not_trigger_drift(self) -> None:
        offsets = np.array([[[1.2, -0.8]], [[-0.6, 0.4]], [[0.3, 1.1]], [[-1.0, -0.2]]])
        result = classify(self.model, self.normal_shape + offsets)
        self.assertEqual(result["status"], "normal")
        self.assertLess(abs(result["amplitude"]), 0.05)

    def test_large_unknown_mode_is_warned_even_with_zero_drift_projection(self) -> None:
        unrelated = self.normal_shape.copy()
        unrelated[:, 1:, 0] = np.array([[1.0, -1.0], [-1.0, 1.0], [1.0, -1.0], [-1.0, 1.0]])
        result = classify(self.model, unrelated)
        self.assertEqual(result["status"], "unmatched_unadjusted")
        self.assertTrue(result["valid"])
        self.assertIn("normal_reference_residual_too_high", result["reason"])

    def test_interpolates_per_camera_local_correction(self) -> None:
        shift_x, shift_z = local_shift(self.model, 1, 0.50, 1.0)
        self.assertAlmostEqual(shift_x, -0.3, places=6)
        self.assertAlmostEqual(shift_z, 0.3, places=6)

    def test_absolute_scan_row_mapping_does_not_stretch_a_shorter_bar(self) -> None:
        self.assertAlmostEqual(drift_fraction_for_row(self.model, 600, 400, 1100), 0.5)
        partial = self.normal_shape[:, 1:, :] + self.drift[:, 1:, :] * 0.8
        result = classify(self.model, partial, [0.50, 0.95])
        self.assertEqual(result["status"], "abnormal_corrected")
        self.assertAlmostEqual(result["amplitude"], 0.8, places=2)

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

    def test_manual_endface_offsets_recompute_direct_angle_average(self) -> None:
        source = {
            "head_A_endface_angle_deg": "89.900000",
            "head_B_endface_angle_deg": "90.020000",
            "head_C_endface_angle_deg": "89.950000",
            "head_D_endface_angle_deg": "90.010000",
        }
        corrected = apply_manual_offsets(
            source,
            {"A": 0.0, "B": 0.0, "C": 0.0, "D": 0.0},
            {"diag1": 0.0, "diag2": 0.0},
            0.0,
            {"head": {"A": 0.0, "B": 0.0, "C": 0.0, "D": 0.0}, "tail": {}},
        )
        self.assertEqual(corrected["head_endface_verticality_deg"], "89.970000")

    def test_historical_statistics_are_migrated_to_direct_angle_average(self) -> None:
        connection = sqlite3.connect(":memory:")
        fields = ["head_endface_verticality_deg"] + [
            f"head_{face}_endface_angle_deg" for face in ("A", "B", "C", "D")
        ]
        connection.execute(
            "CREATE TABLE measurement_statistics (id INTEGER PRIMARY KEY, "
            + ", ".join(f'"{field}" TEXT' for field in fields)
            + ")"
        )
        connection.execute(
            "INSERT INTO measurement_statistics VALUES (1, ?, ?, ?, ?, ?)",
            ("0.045000", "89.900000", "90.020000", "89.950000", "90.010000"),
        )
        connection.execute(
            "INSERT INTO measurement_statistics VALUES (2, ?, ?, ?, ?, ?)",
            ("0.055000", "89.900000", "90.020000", "89.950000", ""),
        )
        _migrate_endface_summary_to_direct_average(connection)
        value = connection.execute(
            "SELECT head_endface_verticality_deg FROM measurement_statistics WHERE id=1"
        ).fetchone()[0]
        incomplete = connection.execute(
            "SELECT head_endface_verticality_deg FROM measurement_statistics WHERE id=2"
        ).fetchone()[0]
        connection.close()
        self.assertEqual(value, "89.970000")
        self.assertEqual(incomplete, "")

    def test_incomplete_endface_angles_do_not_leave_a_stale_summary(self) -> None:
        source = {
            "head_endface_verticality_deg": "0.050000",
            "head_A_endface_angle_deg": "89.900000",
            "head_B_endface_angle_deg": "90.020000",
            "head_C_endface_angle_deg": "89.950000",
        }
        corrected = apply_manual_offsets(
            source,
            {"A": 0.0, "B": 0.0, "C": 0.0, "D": 0.0},
            {"diag1": 0.0, "diag2": 0.0},
            0.0,
            {"head": {"A": 0.0, "B": 0.0, "C": 0.0, "D": 0.0}, "tail": {}},
        )
        self.assertEqual(corrected["head_endface_verticality_deg"], "")


if __name__ == "__main__":
    unittest.main()
