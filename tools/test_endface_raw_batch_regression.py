from __future__ import annotations

import unittest

from tools.endface_raw_batch_regression import LOCAL_CHANNELS, dataset_summary


class EndfaceRawBatchRegressionTests(unittest.TestCase):
    def test_summary_excludes_rejected_capture_from_turnover_audit(self) -> None:
        rows = []
        for group, base in (("head_to_tail", 89.8), ("tail_to_head", 90.2)):
            row = {
                "processing_status": "ok",
                "group": group,
                "endface_raw_quality_status": "pass",
                "endface_raw_quality_accepted": "True",
                "relative_camera_max_seam_span_mm": "0.4",
                "head_endface_raw_plane_rmse_mm": "0.1",
                "tail_endface_raw_plane_rmse_mm": "0.1",
                "head_endface_raw_representative_angle_deg": str(base),
                "tail_endface_raw_representative_angle_deg": str(base),
            }
            for end in ("head", "tail"):
                for channel in LOCAL_CHANNELS:
                    row[f"{end}_{channel}_angle_deg"] = base
            rows.append(row)
        rejected = dict(rows[0])
        rejected["endface_raw_quality_status"] = "rejected"
        rejected["endface_raw_quality_accepted"] = "False"
        rejected["head_A_left_angle_deg"] = 120.0
        rows.append(rejected)

        summary = dataset_summary(rows)

        self.assertEqual(summary["capture_count"], 3)
        self.assertEqual(summary["accepted_count"], 2)
        self.assertEqual(summary["quality_status_counts"]["rejected"], 1)
        self.assertAlmostEqual(
            summary["turnover_equivariance"]["group_median_rms_deg"],
            0.0,
            places=9,
        )
        self.assertFalse(summary["uses_professional_truth"])
        self.assertFalse(summary["correction_applied"])


if __name__ == "__main__":
    unittest.main()
