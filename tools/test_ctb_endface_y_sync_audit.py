import unittest

import numpy as np

from tools.ctb_endface_y_sync_audit import (
    fit_camera_y_offsets,
    shifted_segment,
    zero_sum_basis,
)
from tools.endface_wireframe_turnover_audit import SegmentPoints


class CtbEndfaceYSyncAuditTests(unittest.TestCase):
    def test_recovers_three_parameter_zero_sum_camera_offsets(self) -> None:
        truth = np.asarray([0.30, -0.55, -0.85, 1.10], dtype=float)
        pairs = ((1, 3), (1, 4), (2, 4), (2, 3))
        rows = []
        for capture_index in range(4):
            capture = f"capture_{capture_index}"
            for end_index, end in enumerate(("head", "tail")):
                for face_index, (first, second) in enumerate(pairs):
                    deterministic_noise = (capture_index - 1.5) * 0.001 + (end_index + face_index - 2.0) * 0.0005
                    rows.append(
                        {
                            "capture": capture,
                            "first_camera": first,
                            "second_camera": second,
                            "raw_signed_seam_mm": -(truth[first - 1] - truth[second - 1])
                            + deterministic_noise,
                            "end": end,
                            "core_face": "ABCD"[face_index],
                        }
                    )
        fitted, details = fit_camera_y_offsets(
            rows,
            {f"capture_{index}" for index in range(4)},
            "synthetic",
        )
        np.testing.assert_allclose(fitted, truth, atol=0.003)
        self.assertAlmostEqual(float(np.sum(fitted)), 0.0, places=12)
        self.assertFalse(details["uses_endface_truth"])
        self.assertFalse(details["forbidden_final_angle_parameters_present"])

    def test_zero_sum_basis_has_three_independent_modes(self) -> None:
        basis = zero_sum_basis()
        self.assertEqual(basis.shape, (4, 3))
        self.assertEqual(np.linalg.matrix_rank(basis), 3)
        np.testing.assert_allclose(np.sum(basis, axis=0), np.zeros(3), atol=1e-12)

    def test_shifted_segment_changes_only_camera_y_origin(self) -> None:
        segment = SegmentPoints(
            camera=2,
            coordinate=np.asarray([0.0, 1.0, 2.0]),
            y_mm=np.asarray([1.0, 1.2, 1.4]),
            kept_count=3,
            raw_count=3,
            line_slope=0.2,
            line_intercept=1.0,
        )
        shifted = shifted_segment(segment, -0.4)
        np.testing.assert_allclose(shifted.y_mm, [0.6, 0.8, 1.0])
        self.assertAlmostEqual(shifted.line_intercept, 0.6)
        self.assertAlmostEqual(shifted.line_slope, segment.line_slope)
        np.testing.assert_allclose(segment.y_mm, [1.0, 1.2, 1.4])


if __name__ == "__main__":
    unittest.main()
