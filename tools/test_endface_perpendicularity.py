import csv
import math
import tempfile
import unittest
from pathlib import Path
from unittest.mock import patch

import numpy as np

from tools.measure_square_rod_edges import (
    EndFaceFit,
    HeightSource,
    INVALID_Z,
    CalibrationCapture,
    apply_endface_calibration_model,
    build_endface_calibration_model,
    build_endface_face_angle_calibration_model,
    build_balanced_endface_angle_calibration_model,
    calibration_hobjs_from_folder,
    face_to_endface_angles,
    fit_endfaces_from_source,
    load_standard_truth_csv,
    apply_endface_face_angle_calibration_model,
    read_manual_endface_angle_truth,
    read_manual_endface_truth,
    robust_plane_y_from_xz,
    source_valid_ranges,
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

    def test_endface_offsets_weight_each_bar_equally(self) -> None:
        source = SyntheticHeightSource()
        captures = [
            CalibrationCapture(source, "BAR_A/one", "BAR_A"),
            CalibrationCapture(source, "BAR_A/two", "BAR_A"),
            CalibrationCapture(source, "BAR_A/three", "BAR_A"),
            CalibrationCapture(source, "BAR_B/one", "BAR_B"),
            CalibrationCapture(source, "BAR_B/two", "BAR_B"),
        ]
        raw_values = {"BAR_A/one": 90.0, "BAR_A/two": 91.0, "BAR_A/three": 92.0, "BAR_B/one": 80.0, "BAR_B/two": 82.0}

        def fake_measure(capture, *_args):
            value = raw_values[capture.capture_id]
            return {end: {face: value for face in ["A", "B", "C", "D"]} for end in ["head", "tail"]}

        with tempfile.TemporaryDirectory() as folder:
            truth_path = Path(folder) / "truth.csv"
            with truth_path.open("w", newline="", encoding="utf-8-sig") as handle:
                writer = csv.DictWriter(handle, fieldnames=["record_type", "bar_id", "end", "face", "position", "angle_deg"])
                writer.writeheader()
                for bar_id in ["BAR_A", "BAR_B"]:
                    for end in ["head", "tail"]:
                        for face in ["A", "B", "C", "D"]:
                            for position in [1, 2, 3]:
                                writer.writerow({"record_type": "endface_angle", "bar_id": bar_id, "end": end, "face": face, "position": position, "angle_deg": 90.0})
                writer.writerow({"record_type": "endface_angle", "bar_id": "BAR_C", "end": "head", "face": "A", "position": 1, "angle_deg": 90.0})
            with patch("tools.measure_square_rod_edges.measure_endface_angles_for_capture", side_effect=fake_measure):
                model = build_balanced_endface_angle_calibration_model(captures, {}, str(truth_path), 1.0, 1.0, 1.0)
        # BAR_A offset = -1, BAR_B offset = +9; equal bar weighting gives +4.
        self.assertAlmostEqual(model["angle_offsets_deg"]["head"]["A"], 4.0, places=9)
        self.assertEqual(model["captured_bar_ids"], ["BAR_A", "BAR_B"])
        self.assertEqual(model["unused_truth_bar_ids"], ["BAR_C"])

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

    def test_reads_and_calibrates_24_direct_angles(self) -> None:
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
        raw = {end: {face: 90.0 for face in ["A", "B", "C", "D"]} for end in ["head", "tail"]}
        model = build_endface_face_angle_calibration_model(raw, truth, "BAR001")
        corrected = apply_endface_face_angle_calibration_model(raw, model)
        self.assertEqual(corrected, truth)

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

    def test_slope_offset_calibration_preserves_raw_and_reaches_truth(self) -> None:
        axis = np.array([0.0, 1.0, 0.0])
        raw = {
            "head": EndFaceFit(end="head", valid=True, slope_x=0.004, slope_z=-0.003, verticality_deg=math.degrees(math.atan(0.005))),
            "tail": EndFaceFit(end="tail", valid=True, slope_x=-0.002, slope_z=0.003, verticality_deg=math.degrees(math.atan(math.hypot(0.002, 0.003)))),
        }
        truth = {
            "head": EndFaceFit(end="head", valid=True, slope_x=0.001, slope_z=0.002, verticality_deg=math.degrees(math.atan(math.hypot(0.001, 0.002)))),
            "tail": EndFaceFit(end="tail", valid=True, slope_x=-0.001, slope_z=-0.0015, verticality_deg=math.degrees(math.atan(math.hypot(0.001, 0.0015)))),
        }
        model = build_endface_calibration_model(raw, truth, axis, "BAR001")
        corrected = apply_endface_calibration_model(raw["head"], axis, model)
        self.assertAlmostEqual(corrected.slope_x, truth["head"].slope_x, places=9)
        self.assertAlmostEqual(corrected.slope_z, truth["head"].slope_z, places=9)
        self.assertEqual(raw["head"].slope_x, 0.004)


if __name__ == "__main__":
    unittest.main()
