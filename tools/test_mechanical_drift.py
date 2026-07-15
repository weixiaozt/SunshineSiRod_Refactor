from __future__ import annotations

import csv
import unittest
import sqlite3
import sys
import tempfile
from pathlib import Path
from unittest.mock import patch

import numpy as np

from tools.mechanical_drift import build_model, classify, local_shift
from tools.measure_square_rod_edges import drift_fraction_for_row
from tools.measurement_dashboard import (
    FULL_STATISTICS_VALUE_FIELDS,
    _migrate_endface_summary_to_direct_average,
    append_compensation_change_log,
    apply_manual_offsets,
    build_measurement_command,
    compensation_log_summary,
    merge_delivery_abcd_summary,
    prepare_result_directories,
    save_config,
    summary_values_equal,
)


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
        self.assertEqual(partial["status"], "unmatched_unadjusted")
        self.assertTrue(partial["valid"])
        boundary = classify(self.model, self.normal_shape + self.drift * 0.80)
        self.assertEqual(boundary["status"], "abnormal_corrected")
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
        )
        self.assertEqual(source["stick_length_mm"], "826.900000")
        self.assertEqual(corrected["stick_length_mm"], "827.025000")

    def test_dimensional_manual_offsets_never_change_endface_results(self) -> None:
        source = {
            "head_endface_verticality_deg": "89.970000",
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
        )
        self.assertEqual(corrected["head_A_endface_angle_deg"], "89.900000")
        self.assertEqual(corrected["head_endface_verticality_deg"], "89.970000")

    def test_delivered_abcd_replaces_product_edges_without_native_fields(self) -> None:
        source = {f"{edge}_mm": str(index) for index, edge in enumerate("ABCD", 1)}
        payload = {
            "method": "delivery-test",
            "aggregation": "head-tail-test",
            "calibration_path": "delivery.json",
            "global_edges_mm": dict(zip("ABCD", (10.0, 20.0, 30.0, 40.0))),
            "head_edges_mm": dict(zip("ABCD", (9.0, 19.0, 29.0, 39.0))),
            "tail_edges_mm": dict(zip("ABCD", (11.0, 21.0, 31.0, 41.0))),
        }

        merged = merge_delivery_abcd_summary(source, payload)

        self.assertEqual(merged["A_mm"], "10.000000")
        self.assertNotIn("native_A_mm", merged)
        self.assertEqual(merged["delivery_head_D_mm"], "39.000000")
        self.assertEqual(merged["delivery_tail_D_mm"], "41.000000")
        self.assertEqual(source["A_mm"], "1")

    def test_delivered_geometry_replaces_diagonal_arc_projection_and_angle(self) -> None:
        source = {
            "diag1_M1_M2_mm": "230.0",
            "diag2_M3_M4_mm": "231.0",
            "stick_length_mm": "900.0",
            **{
                f"obj{obj}_{field}": str(obj)
                for obj in (1, 2, 3, 4)
                for field in (
                    "verticality_error_deg",
                    "chamfer_mm",
                    "projection_x_mm",
                    "projection_y_mm",
                )
            },
        }
        payload = {
            "method": "delivery-geometry-test",
            "aggregation": "delivery-aggregation-test",
            "calibration_path": "delivery.json",
            "delivered_length_mm": 901.0,
            "global_edges_mm": dict(zip("ABCD", (10.0, 20.0, 30.0, 40.0))),
            "head_edges_mm": dict(zip("ABCD", (9.0, 19.0, 29.0, 39.0))),
            "tail_edges_mm": dict(zip("ABCD", (11.0, 21.0, 31.0, 41.0))),
            "global_diagonals_mm": {"diag1": 240.0, "diag2": 241.0},
            "head_diagonals_mm": {"diag1": 239.0, "diag2": 240.0},
            "tail_diagonals_mm": {"diag1": 241.0, "diag2": 242.0},
            "corner_geometry": {
                str(obj): {
                    "main_face_angle_deg": 89.0 + obj / 10.0,
                    "arc_length_mm": 1.0 + obj / 10.0,
                    "projection_1_mm": 0.7 + obj / 100.0,
                    "projection_2_mm": 0.8 + obj / 100.0,
                }
                for obj in (1, 2, 3, 4)
            },
        }

        merged = merge_delivery_abcd_summary(source, payload)

        self.assertEqual(merged["diag1_M1_M2_mm"], "240.000000")
        self.assertNotIn("native_diag1_M1_M2_mm", merged)
        self.assertEqual(merged["delivery_head_diag2_mm"], "240.000000")
        self.assertEqual(merged["obj2_verticality_error_deg"], "89.200000")
        self.assertEqual(merged["obj2_chamfer_mm"], "1.200000")
        self.assertEqual(merged["obj2_projection_x_mm"], "0.720000")
        self.assertEqual(merged["obj2_projection_y_mm"], "0.820000")
        self.assertNotIn("native_obj2_chamfer_mm", merged)
        self.assertEqual(merged["stick_length_mm"], "901.000000")
        self.assertEqual(merged["delivery_length_mm"], "901.000000")

    def test_global_statistics_do_not_expose_endface_fields(self) -> None:
        self.assertFalse(any("endface" in field for field in FULL_STATISTICS_VALUE_FIELDS))
        self.assertIn("delivery_abcd_method", FULL_STATISTICS_VALUE_FIELDS)
        self.assertFalse(any(field.startswith("native_") for field in FULL_STATISTICS_VALUE_FIELDS))

    def test_result_directories_migrate_user_and_developer_files(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            output_root = Path(temporary)
            legacy_slice = output_root / "sample_measure.csv"
            legacy_json = output_root / "sample_delivery_geometry.json"
            legacy_csv = output_root / "measurement_statistics.csv"
            legacy_database = output_root / "measurement_statistics.sqlite"
            legacy_slice.write_text("record,A_mm\nmean,210\n", encoding="utf-8")
            legacy_json.write_text("{}", encoding="utf-8")
            legacy_csv.write_text("slice_csv_path\n" + str(legacy_slice) + "\n", encoding="utf-8")
            connection = sqlite3.connect(legacy_database)
            try:
                connection.execute(
                    "CREATE TABLE measurement_statistics (id INTEGER PRIMARY KEY, slice_csv_path TEXT)"
                )
                connection.execute(
                    "INSERT INTO measurement_statistics VALUES (1, ?)",
                    (str(legacy_slice),),
                )
                connection.commit()
            finally:
                connection.close()

            user_results, developer_details = prepare_result_directories(output_root)

            self.assertTrue((user_results / "measurement_statistics.csv").is_file())
            self.assertTrue((developer_details / "measurement_statistics.sqlite").is_file())
            self.assertTrue((developer_details / legacy_slice.name).is_file())
            self.assertTrue((developer_details / legacy_json.name).is_file())
            connection = sqlite3.connect(developer_details / "measurement_statistics.sqlite")
            try:
                saved_path = connection.execute(
                    "SELECT slice_csv_path FROM measurement_statistics WHERE id=1"
                ).fetchone()[0]
            finally:
                connection.close()
            self.assertEqual(saved_path, str(developer_details / legacy_slice.name))

    def test_compensation_log_records_offsets_and_display_values(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            original = {
                "edge_offsets_mm": {edge: 0.0 for edge in "ABCD"},
                "diagonal_offsets_mm": {"diag1": 0.0, "diag2": 0.0},
                "length_offset_mm": 0.0,
            }
            updated = {
                "edge_offsets_mm": {"A": 0.1, "B": 0.0, "C": 0.0, "D": 0.0},
                "diagonal_offsets_mm": {"diag1": -0.2, "diag2": 0.0},
                "length_offset_mm": 1.5,
            }
            latest = {
                "input_path": "sample.hobj",
                "raw_summary": {
                    "A_mm": "210.000000",
                    "diag1_M1_M2_mm": "233.000000",
                    "stick_length_mm": "900.000000",
                },
            }

            output = append_compensation_change_log(
                Path(temporary), original, updated, latest
            )
            with output.open("r", encoding="utf-8-sig", newline="") as handle:
                rows = list(csv.DictReader(handle))

            self.assertEqual([row["item"] for row in rows], ["A", "D1", "Length"])
            self.assertEqual(rows[0]["original_compensation"], "0.000000")
            self.assertEqual(rows[0]["new_compensation"], "0.100000")
            self.assertEqual(rows[0]["display_before_change"], "210.000000")
            self.assertEqual(rows[0]["display_after_change"], "210.100000")
            summary = compensation_log_summary({"output_dir": temporary})
            self.assertEqual(len(summary["rows"]), 3)

    def test_dashboard_rejects_legacy_final_endface_offsets(self) -> None:
        with self.assertRaisesRegex(ValueError, "permanently disabled"):
            save_config(
                {
                    "endface_angle_offsets_deg": {
                        "head": {face: 0.1 for face in "ABCD"},
                        "tail": {face: -0.1 for face in "ABCD"},
                    }
                }
            )

    def test_raw_audit_command_loads_no_drift_or_endface_model(self) -> None:
        config = {
            "script_path": str(Path("tools/measure_square_rod_edges.py")),
            "calibration_path": "camera.json",
            "drift_calibration_path": "drift.json",
            "endface_calibration_path": "endface.json",
            "endface_measurement_mode": "raw_audit",
            "step_mm": 10.0,
        }
        command, mode = build_measurement_command(
            config,
            Path("sample.hobj"),
            Path("output.csv"),
            endface_only=True,
        )
        self.assertEqual(mode, "raw_audit")
        self.assertEqual(command[0], sys.executable)
        self.assertEqual(command[command.index("--calibration") + 1], "camera.json")
        self.assertNotIn("--drift-calibration", command)
        self.assertNotIn("--endface-calibration", command)
        self.assertIn("--endface-only", command)

    def test_raw_audit_numeric_guard_ignores_only_csv_formatting_noise(self) -> None:
        self.assertTrue(summary_values_equal("90.204399", "90.2044"))
        self.assertFalse(summary_values_equal("90.204390", "90.2044"))

    def test_raw_trial_package_rejects_release_mode_switch(self) -> None:
        with patch("tools.measurement_dashboard.END_FACE_RAW_ONLY", True):
            with self.assertRaisesRegex(ValueError, "locked to M0 Raw"):
                save_config({"endface_measurement_mode": "release_corrected"})

    def test_release_command_keeps_each_model_argument_with_its_value(self) -> None:
        config = {
            "script_path": str(Path("tools/measure_square_rod_edges.py")),
            "calibration_path": "camera.json",
            "drift_calibration_path": "drift.json",
            "endface_calibration_path": "endface.json",
            "endface_measurement_mode": "release_corrected",
            "step_mm": 10.0,
        }
        command, mode = build_measurement_command(
            config,
            Path("sample.hobj"),
            Path("output.csv"),
            endface_only=True,
        )
        self.assertEqual(mode, "release_corrected")
        self.assertEqual(command[command.index("--calibration") + 1], "camera.json")
        self.assertEqual(command[command.index("--drift-calibration") + 1], "drift.json")
        self.assertEqual(command[command.index("--endface-calibration") + 1], "endface.json")

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

if __name__ == "__main__":
    unittest.main()
