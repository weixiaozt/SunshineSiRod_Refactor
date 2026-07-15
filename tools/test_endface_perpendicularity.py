import csv
import math
import tempfile
import unittest
from pathlib import Path
import numpy as np

from tools.measure_square_rod_edges import (
    EndFaceFit,
    HeightSource,
    INVALID_Z,
    calibration_hobjs_from_folder,
    classify_raw_endface_quality,
    is_turnover_capture,
    oriented_endface_truth,
    oriented_standard_pairs,
    face_to_endface_angles,
    endface_fit_record,
    fit_endfaces_from_source,
    load_standard_truth_csv,
    mean_face_endface_angle,
    longitudinal_side_plane_normals,
    read_manual_endface_angle_truth,
    read_manual_endface_truth,
    robust_plane_y_from_xz,
    same_face_line_pair_difference,
    source_valid_ranges,
    write_csv,
)


class SyntheticHeightSource(HeightSource):
    def __init__(self) -> None:
        self.width = 120
        self.height = 320
        self.images = {}
        translations = {1: (100.0, 100.0), 2: (0.0, 0.0), 3: (0.0, 100.0), 4: (100.0, 0.0)}
        for obj, (tx, tz) in translations.items():
            image = np.full((self.height, self.width), INVALID_Z, dtype=np.float32)
            for col in range(self.width):
                local_x = float(col)
                local_z = 0.2 * local_x + obj
                global_x = local_x + tx
                global_z = local_z + tz
                head_row = int(round((0.0015 * global_x - 0.0020 * global_z + 1.0) / 0.01))
                tail_row = int(round((-0.0010 * global_x + 0.0012 * global_z + 2.7) / 0.01))
                image[head_row : tail_row + 1, col] = local_z
            self.images[obj] = image

    def row(self, obj: int, row_index: int) -> np.ndarray:
        return self.images[obj][row_index]


class EndFacePerpendicularityTests(unittest.TestCase):
    def test_raw_quality_gate_warns_without_changing_or_hiding_values(self) -> None:
        endfaces = {
            "head": EndFaceFit("head", True, rmse_mm=0.12),
            "tail": EndFaceFit("tail", True, rmse_mm=0.18),
        }
        passed = classify_raw_endface_quality(
            endfaces,
            {"maximum_seam_span_p05_p95_mm": 0.50},
        )
        self.assertEqual(passed["status"], "pass")
        self.assertTrue(passed["accepted"])

        warning = classify_raw_endface_quality(
            endfaces,
            {"maximum_seam_span_p05_p95_mm": 0.90},
        )
        self.assertEqual(warning["status"], "warning")
        self.assertTrue(warning["accepted"])
        self.assertFalse(warning["uses_professional_truth"])
        self.assertFalse(warning["correction_applied"])

        endfaces["tail"] = EndFaceFit("tail", True, rmse_mm=0.51)
        rejected = classify_raw_endface_quality(
            endfaces,
            {"maximum_seam_span_p05_p95_mm": 0.50},
        )
        self.assertEqual(rejected["status"], "rejected")
        self.assertFalse(rejected["accepted"])

    def test_same_face_camera_seam_is_diagnostic_geometry_not_a_correction(self) -> None:
        separation, angle = same_face_line_pair_difference(
            (np.array([0.0, 1.0]), np.array([1.0, 0.0])),
            (np.array([10.0, 1.2]), np.array([1.0, 0.0])),
            "A",
        )
        self.assertAlmostEqual(separation, 0.2, places=9)
        self.assertAlmostEqual(angle, 0.0, places=9)

    def test_independent_longitudinal_planes_preserve_directed_opposite_face_angles(self) -> None:
        rows = []
        slopes = {"left": 0.001, "right": 0.003, "top": 0.002, "bottom": 0.004}
        for y in np.linspace(0.0, 1000.0, 9):
            left = -50.0 + slopes["left"] * y
            right = 50.0 + slopes["right"] * y
            top = 100.0 + slopes["top"] * y
            bottom = -100.0 + slopes["bottom"] * y
            rows.append({
                "record": "slice", "valid": True, "y_mm": y,
                "P1_x": right, "P1_z": top,
                "P2_x": left, "P2_z": bottom,
                "P3_x": left, "P3_z": top,
                "P4_x": right, "P4_z": bottom,
            })
        normals = longitudinal_side_plane_normals(rows)
        section = {
            "P1": (50.0, 100.0), "P2": (-50.0, -100.0),
            "P3": (-50.0, 100.0), "P4": (50.0, -100.0),
        }
        fit = EndFaceFit(
            end="head", valid=True, normal_x=0.0, normal_y=1.0, normal_z=0.0
        )
        angles = face_to_endface_angles(section, fit, np.array([0.0, 1.0, 0.0]), normals)
        self.assertGreater(angles["A"], 90.0)
        self.assertGreater(angles["B"], 90.0)
        self.assertLess(angles["C"], 90.0)
        self.assertLess(angles["D"], 90.0)
        self.assertNotAlmostEqual(angles["A"], 180.0 - angles["C"], places=5)
        self.assertNotAlmostEqual(angles["B"], 180.0 - angles["D"], places=5)

    def test_camera_y_offsets_are_applied_to_longitudinal_side_planes(self) -> None:
        rows = []
        for y in np.linspace(0.0, 100.0, 9):
            top = 100.0 + 0.01 * y
            bottom = -100.0 + 0.01 * y
            rows.append({
                "record": "slice", "valid": True, "y_mm": y,
                "P1_x": 50.0, "P1_z": top,
                "P2_x": -50.0, "P2_z": bottom,
                "P3_x": -50.0, "P3_z": top,
                "P4_x": 50.0, "P4_z": bottom,
            })
        raw = longitudinal_side_plane_normals(rows)
        corrected = longitudinal_side_plane_normals(
            rows,
            camera_y_offsets_mm={1: 1.0, 2: 0.0, 3: -1.0, 4: 0.0},
        )
        self.assertGreater(abs(float(raw["A"][1])), 1e-4)
        self.assertFalse(np.allclose(raw["A"], corrected["A"]))

    def test_endface_only_csv_exposes_legacy_and_sixteen_local_product_angles(self) -> None:
        with tempfile.TemporaryDirectory() as folder:
            path = Path(folder) / "endface.csv"
            row = {
                "record": "mean",
                "valid": True,
                "measurement_valid": True,
                "A_mm": 210.0,
                **{
                    f"{end}_{face}_endface_angle_deg": 90.0
                    for end in ("head", "tail")
                    for face in "ABCD"
                },
                "head_endface_verticality_deg": 90.0,
                "tail_endface_verticality_deg": 90.0,
                **{
                    f"{end}_{channel}_endface_angle_deg": 90.0
                    for end in ("head", "tail")
                    for channel in (
                        "A_left", "A_right", "B_top", "B_bottom",
                        "C_top", "C_bottom", "D_left", "D_right",
                    )
                },
                "head_endface_representative_angle_deg": 89.9,
                "tail_endface_representative_angle_deg": 90.1,
            }
            write_csv(str(path), [row], endface_only=True)
            with path.open("r", encoding="utf-8-sig", newline="") as handle:
                reader = csv.DictReader(handle)
                saved = next(reader)
                fields = reader.fieldnames
        self.assertNotIn("A_mm", fields)
        self.assertNotIn("stick_length_mm", fields)
        self.assertEqual(
            [field for field in fields if field.endswith("_endface_angle_deg")],
            [
                *[f"{end}_{face}_endface_angle_deg" for end in ("head", "tail") for face in "ABCD"],
                *[
                    f"{end}_{channel}_endface_angle_deg"
                    for end in ("head", "tail")
                    for channel in (
                        "A_left", "A_right", "B_top", "B_bottom",
                        "C_top", "C_bottom", "D_left", "D_right",
                    )
                ],
            ],
        )
        self.assertEqual(saved["head_endface_verticality_deg"], "90.0")
        self.assertEqual(saved["head_endface_representative_angle_deg"], "89.9")

    def test_discovers_repeat_hobjs_by_parent_bar_folder(self) -> None:
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            for bar_id, captures in {"BAR_A": ["one", "two", "three"], "BAR_B _3": ["one", "two"]}.items():
                bar_folder = root / bar_id
                bar_folder.mkdir()
                for capture in captures:
                    (bar_folder / f"{capture}.hobj").touch()
            discovered = calibration_hobjs_from_folder(str(root))
        self.assertEqual([item[1] for item in discovered].count("BAR_A"), 3)
        self.assertEqual([item[1] for item in discovered].count("BAR_B_3"), 2)
        self.assertIn("BAR_A/one", [item[2] for item in discovered])
        self.assertIn("BAR_B_3/one", [item[2] for item in discovered])

    def test_turnover_folder_keeps_physical_bar_id_and_is_detected(self) -> None:
        with tempfile.TemporaryDirectory() as folder:
            path = Path(folder) / "BAR_A diaotou_3" / "one.hobj"
            path.parent.mkdir()
            path.touch()
            discovered = calibration_hobjs_from_folder(str(Path(folder)))
        self.assertEqual(discovered[0][1], "BAR_A_3")
        self.assertEqual(discovered[0][2], "BAR_A_3/turnover/one")
        self.assertTrue(is_turnover_capture(discovered[0][0]))

    def test_turnover_pairs_reverse_longitudinal_truth_windows(self) -> None:
        standard = {
            "range15_25": {"percent": 0.20, "A": 1.0, "B": 2.0, "C": 3.0, "D": 4.0},
            "range45_55": {"percent": 0.50, "A": 5.0, "B": 6.0, "C": 7.0, "D": 8.0},
            "range70_80": {"percent": 0.75, "A": 9.0, "B": 10.0, "C": 11.0, "D": 12.0},
        }
        pairs = oriented_standard_pairs(standard, "turnover")
        self.assertEqual([(source, truth) for source, _, truth, _ in pairs], [
            ("range15_25", "range70_80"),
            ("range45_55", "range45_55"),
            ("range70_80", "range15_25"),
        ])

    def test_turnover_endface_truth_swaps_ends_and_bd_only(self) -> None:
        truth = {
            "head": {"A": 1.0, "B": 2.0, "C": 3.0, "D": 4.0},
            "tail": {"A": 5.0, "B": 6.0, "C": 7.0, "D": 8.0},
        }
        mapped = oriented_endface_truth(truth, "turnover")
        self.assertEqual(mapped["head"], {"A": 5.0, "B": 8.0, "C": 7.0, "D": 6.0})
        self.assertEqual(mapped["tail"], {"A": 1.0, "B": 4.0, "C": 3.0, "D": 2.0})

    def test_reads_cross_section_rows_from_unified_calibration_csv(self) -> None:
        with tempfile.TemporaryDirectory() as folder:
            path = Path(folder) / "calibration.csv"
            with path.open("w", newline="", encoding="utf-8-sig") as handle:
                writer = csv.DictWriter(
                    handle,
                    fieldnames=["record_type", "bar_id", "position_percent", "end", "face", "position", "A_mm", "B_mm", "C_mm", "D_mm", "angle_deg"],
                )
                writer.writeheader()
                for bar_id, offset in [("BAR001", 0.0), ("BAR002", 1.0)]:
                    for percent, a in [("15-25", 210.01), ("45-55", 210.02), ("70-80", 210.03)]:
                        writer.writerow({"record_type": "cross_section", "bar_id": bar_id, "position_percent": percent, "A_mm": a + offset, "B_mm": 104.0, "C_mm": 210.0, "D_mm": 104.0})
                writer.writerow({"record_type": "endface_angle", "bar_id": "BAR001", "end": "head", "face": "A", "position": 1, "angle_deg": 90.0})
            standard = load_standard_truth_csv(str(path))
        self.assertEqual(list(standard), ["bar001", "bar002"])
        self.assertEqual(list(standard["bar001"]), ["range15_25", "range45_55", "range70_80"])
        self.assertEqual(standard["bar001"]["range45_55"], {"percent": 0.5, "start_percent": 0.45, "end_percent": 0.55, "A": 210.02, "B": 104.0, "C": 210.0, "D": 104.0})
        self.assertEqual(standard["bar002"]["range45_55"]["A"], 211.02)

    def test_face_to_end_angles_are_90_for_square_end(self) -> None:
        section = {"P1": (210.0, 104.0), "P2": (0.0, 0.0), "P3": (0.0, 104.0), "P4": (210.0, 0.0)}
        fit = EndFaceFit(end="head", valid=True, normal_x=0.0, normal_y=1.0, normal_z=0.0)
        angles = face_to_endface_angles(section, fit, np.array([0.0, 1.0, 0.0]))
        self.assertEqual(set(angles), {"A", "B", "C", "D"})
        for angle in angles.values():
            self.assertAlmostEqual(angle, 90.0, places=9)

    def test_endface_summary_is_direct_four_face_angle_average(self) -> None:
        angles = {"A": 89.90, "B": 90.02, "C": 89.95, "D": 90.01}
        self.assertAlmostEqual(mean_face_endface_angle(angles), 89.97, places=9)

    def test_endface_fit_record_keeps_plane_metric_in_separate_column(self) -> None:
        fit = EndFaceFit(end="head", valid=True, verticality_deg=0.125, point_count=20, inlier_count=18)
        record = endface_fit_record(fit, "raw_visual")
        self.assertEqual(record["head_endface_plane_verticality_deg"], 0.125)
        self.assertNotIn("head_endface_verticality_deg", record)

    def test_reads_24_direct_truth_angles_without_building_output_offsets(self) -> None:
        with tempfile.TemporaryDirectory() as folder:
            path = Path(folder) / "angles.csv"
            with path.open("w", newline="", encoding="utf-8-sig") as handle:
                writer = csv.DictWriter(handle, fieldnames=["bar_id", "end", "face", "position", "angle_deg"])
                writer.writeheader()
                for end in ["head", "tail"]:
                    for face_index, face in enumerate(["A", "B", "C", "D"]):
                        for position, delta in enumerate([-0.01, 0.0, 0.01], start=1):
                            writer.writerow({"bar_id": "BAR001", "end": end, "face": face, "position": position, "angle_deg": 89.9 + face_index * 0.02 + delta})
            truth = read_manual_endface_angle_truth(str(path), "BAR001")
        self.assertAlmostEqual(truth["head"]["A"], 89.9, places=9)
        self.assertAlmostEqual(truth["tail"]["D"], 89.96, places=9)

    def test_extracts_head_and_tail_planes_from_height_boundaries(self) -> None:
        source = SyntheticHeightSource()
        transforms = {
            1: {"matrix": [[1.0, 0.0], [0.0, 1.0]], "translation": [100.0, 100.0]},
            2: {"matrix": [[1.0, 0.0], [0.0, 1.0]], "translation": [0.0, 0.0]},
            3: {"matrix": [[1.0, 0.0], [0.0, 1.0]], "translation": [0.0, 100.0]},
            4: {"matrix": [[1.0, 0.0], [0.0, 1.0]], "translation": [100.0, 0.0]},
        }
        ranges = source_valid_ranges(source)
        common_start = max(value[0] for value in ranges.values())
        fits = fit_endfaces_from_source(
            source,
            ranges,
            transforms,
            {"corner_biases": {}},
            common_start,
            np.array([0.0, 1.0, 0.0]),
            1.0,
            0.01,
            1.2,
        )
        self.assertTrue(fits["head"].valid, fits["head"].reason)
        self.assertTrue(fits["tail"].valid, fits["tail"].reason)
        self.assertAlmostEqual(fits["head"].slope_x, 0.0015, delta=0.00035)
        self.assertAlmostEqual(fits["head"].slope_z, -0.0020, delta=0.0002)
        self.assertAlmostEqual(fits["tail"].slope_x, -0.0010, delta=0.00025)
        self.assertAlmostEqual(fits["tail"].slope_z, 0.0012, delta=0.0002)

    def test_robust_plane_rejects_outliers(self) -> None:
        xs, zs = np.meshgrid(np.linspace(0.0, 210.0, 15), np.linspace(0.0, 104.0, 9))
        ys = 0.0015 * xs - 0.0020 * zs + 3.0
        points = np.column_stack([xs.ravel(), ys.ravel(), zs.ravel()])
        points[:5, 1] += 2.0
        fit = robust_plane_y_from_xz(points, "head")
        self.assertTrue(fit.valid, fit.reason)
        self.assertAlmostEqual(fit.slope_x, 0.0015, places=7)
        self.assertAlmostEqual(fit.slope_z, -0.0020, places=7)
        self.assertLess(fit.inlier_count, fit.point_count)

    def test_reads_24_signed_manual_values(self) -> None:
        section = {"P1": (210.0, 104.0), "P2": (0.0, 0.0), "P3": (0.0, 104.0), "P4": (210.0, 0.0)}
        face_ends = {"A": ("P3", "P1"), "B": ("P1", "P4"), "C": ("P2", "P4"), "D": ("P3", "P2")}
        with tempfile.TemporaryDirectory() as folder:
            path = Path(folder) / "truth.csv"
            with path.open("w", newline="", encoding="utf-8-sig") as handle:
                writer = csv.DictWriter(handle, fieldnames=["bar_id", "end", "face", "position", "deviation_mm"])
                writer.writeheader()
                for end, sign in [("head", 1.0), ("tail", -1.0)]:
                    for face, (start_name, finish_name) in face_ends.items():
                        start, finish = section[start_name], section[finish_name]
                        for position, fraction in enumerate([0.25, 0.50, 0.75], start=1):
                            x = start[0] + fraction * (finish[0] - start[0])
                            z = start[1] + fraction * (finish[1] - start[1])
                            value = sign * (0.001 * x + 0.002 * z + 0.4)
                            writer.writerow({"bar_id": "BAR001", "end": end, "face": face, "position": position, "deviation_mm": value})
            fits = read_manual_endface_truth(str(path), section, "BAR001")
        self.assertTrue(fits["head"].valid, fits["head"].reason)
        self.assertTrue(fits["tail"].valid, fits["tail"].reason)
        self.assertAlmostEqual(fits["head"].slope_x, 0.001, places=7)
        self.assertAlmostEqual(fits["head"].slope_z, 0.002, places=7)
        self.assertAlmostEqual(fits["tail"].slope_x, -0.001, places=7)
        self.assertAlmostEqual(fits["tail"].slope_z, -0.002, places=7)

if __name__ == "__main__":
    unittest.main()
