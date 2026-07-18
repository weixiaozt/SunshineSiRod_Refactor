from __future__ import annotations

import sys
import unittest
from pathlib import Path

import numpy as np


PACKAGE_ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(PACKAGE_ROOT / "src"))

from square_bar.core import (  # noqa: E402
    REVERSE_PHYSICAL_MAPPING,
    build_specimen_reports,
    caliper_support,
    load_calibration,
    trimmed_mean,
)


class CoreTests(unittest.TestCase):
    def test_trimmed_mean_each_tail(self):
        result = trimmed_mean(list(range(10)), 0.10)
        self.assertEqual(result["trim_each_tail"], 1)
        self.assertEqual(result["kept_count"], 8)
        self.assertAlmostEqual(result["mean"], 4.5)

    def test_fixed_direction_support(self):
        positive = np.array([[5.0, 0.0], [4.0, 9.0]])
        negative = np.array([[0.0, 0.0], [1.0, -9.0]])
        result = caliper_support(positive, negative, np.array([1.0, 0.0]))
        self.assertAlmostEqual(result["caliper_mm"], 5.0)

    def test_reverse_mapping(self):
        self.assertEqual(REVERSE_PHYSICAL_MAPPING, {
            "A": "A", "B": "D", "C": "C", "D": "B", "D1": "D2", "D2": "D1"
        })

    def test_frozen_exchange_values(self):
        calibration = load_calibration(PACKAGE_ROOT / "calibration")
        self.assertAlmostEqual(calibration.exchange_payload["k_mm"], 0.013402766012028167, places=15)
        self.assertAlmostEqual(calibration.corrections_mm["B"], -0.006701383006014083, places=15)
        self.assertAlmostEqual(calibration.corrections_mm["D"], 0.006701383006014083, places=15)
        self.assertAlmostEqual(calibration.corrections_mm["D1"], calibration.corrections_mm["B"], places=15)
        self.assertAlmostEqual(calibration.corrections_mm["D2"], calibration.corrections_mm["D"], places=15)

    def test_significance_definition(self):
        metrics = ("A", "B", "C", "D", "D1", "D2")
        positive = {
            "specimen": "X", "direction": "positive", "scan": "p",
            **{f"aligned_{m}": v for m, v in zip(metrics, [210, 105.03, 210, 105.00, 234.04, 234.00])},
        }
        reverse = {
            "specimen": "X", "direction": "reverse", "scan": "r",
            **{f"aligned_{m}": v for m, v in zip(metrics, [210, 105.02, 210, 105.00, 234.03, 234.00])},
        }
        summary, _ = build_specimen_reports([positive, reverse])
        self.assertAlmostEqual(summary[0]["BD_exchange_significance"], 3.0)
        self.assertAlmostEqual(summary[0]["diagonal_exchange_significance"], 4.0)


if __name__ == "__main__":
    unittest.main()
