from __future__ import annotations

import unittest

from tools.measure_endface_raw import locked_raw_arguments


class MeasureEndfaceRawTests(unittest.TestCase):
    def test_adds_locked_raw_mode_arguments(self) -> None:
        arguments = locked_raw_arguments(["--input", "sample.hobj", "--calibration", "camera.json"])
        self.assertIn("--endface-only", arguments)
        self.assertEqual(arguments[arguments.index("--orientation") + 1], "normal")

    def test_rejects_truth_and_correction_models(self) -> None:
        for option in ("--drift-calibration", "--endface-calibration", "--endface-truth-csv"):
            with self.subTest(option=option):
                with self.assertRaisesRegex(ValueError, "forbids"):
                    locked_raw_arguments(
                        ["--input", "sample.hobj", "--calibration", "camera.json", option, "bad.json"]
                    )

    def test_rejects_turnover_transform_switch(self) -> None:
        with self.assertRaisesRegex(ValueError, "orientation=normal"):
            locked_raw_arguments(
                [
                    "--input",
                    "sample.hobj",
                    "--calibration",
                    "camera.json",
                    "--orientation",
                    "turnover",
                ]
            )


if __name__ == "__main__":
    unittest.main()
