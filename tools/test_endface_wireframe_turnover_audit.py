import unittest

from tools.endface_wireframe_turnover_audit import (
    mapped_turnover_channel,
    summarize_end_angles,
)


class EndfaceWireframeTurnoverAuditTests(unittest.TestCase):
    def test_turnover_mapping_matches_fixed_head_view(self) -> None:
        self.assertEqual(mapped_turnover_channel("head", "A_right"), ("tail", "A_left"))
        self.assertEqual(mapped_turnover_channel("tail", "D_right"), ("head", "D_left"))
        self.assertEqual(mapped_turnover_channel("head", "B_top"), ("tail", "C_top"))
        self.assertEqual(mapped_turnover_channel("tail", "C_bottom"), ("head", "B_bottom"))

    def test_final_value_is_real_worst_local_angle(self) -> None:
        angles = {
            "A_left": 89.9,
            "A_right": 90.1,
            "B_top": 89.8,
            "B_bottom": 90.2,
            "C_top": 88.6,
            "C_bottom": 90.3,
            "D_left": 89.7,
            "D_right": 91.0,
        }
        summary = summarize_end_angles(angles)
        self.assertEqual(summary["worst_local_channel"], "C_top")
        self.assertAlmostEqual(summary["worst_local_angle_deg"], 88.6)
        self.assertAlmostEqual(summary["worst_local_error_deg"], 1.4)


if __name__ == "__main__":
    unittest.main()
