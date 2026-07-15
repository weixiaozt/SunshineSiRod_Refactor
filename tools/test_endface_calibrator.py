import csv
import json
import tempfile
import unittest
from pathlib import Path
from types import SimpleNamespace
from unittest.mock import patch

import numpy as np

from tools.endface_calibrator import (
    INSTITUTION_TO_SOFTWARE_FACE_MAP,
    PhysicalCaptureMeasurement,
    audit_calibration_row_order,
    calibration_state_reference,
    discover_captures,
    expand_camera_y_parameters,
    fit_decomposed_camera_y_offsets,
    image_model_candidate_acceptance,
    oriented_professional_truth,
    offsets_from_physical_modes,
    physical_angles_for_capture,
    physical_plane_residuals,
    professional_turnover_face_map,
    read_professional_truth,
    scan_angles_in_physical_standard_frame,
    synchronization_mode_basis,
    validate_rigid_physical_turnover_measurements,
    wireframe_turnover_equivariance_validation,
)
from tools.endface_wireframe_geometry import LOCAL_CHANNELS, mapped_turnover_channel
from tools.measure_square_rod_edges import (
    endface_calibration_applicability,
    institution_labeled_face_angles,
    resolve_measurement_orientation,
    validate_professional_endface_model,
)
from tools.measurement_dashboard import is_usable_physical_endface_model
from tools.build_endface_calibration_toolkit import (
    calibration_batch as toolkit_calibration_batch,
    camera_state_batch as toolkit_camera_state_batch,
)


def synthetic_capture(camera_biases: np.ndarray | None = None) -> PhysicalCaptureMeasurement:
    """Build image-like boundary clouds without consulting any truth angles."""

    biases = np.zeros(4, dtype=float) if camera_biases is None else camera_biases
    centres = {
        1: (80.0, 35.0),
        2: (-80.0, -35.0),
        3: (-80.0, 35.0),
        4: (80.0, -35.0),
    }
    boundary: dict[str, dict[int, np.ndarray]] = {"head": {}, "tail": {}}
    for end, intercept in (("head", 2.0), ("tail", 470.0)):
        for obj, (centre_x, centre_z) in centres.items():
            xs = np.linspace(centre_x - 30.0, centre_x + 30.0, 31)
            zs = np.linspace(centre_z - 10.0, centre_z + 10.0, 31)
            ys = 0.0015 * xs - 0.0020 * zs + intercept + biases[obj - 1]
            boundary[end][obj] = np.column_stack([xs, ys, zs])
    return PhysicalCaptureMeasurement(
        path=Path("synthetic.hobj"),
        orientation="normal",
        split="train",
        rod_axis=np.array([0.0, 1.0, 0.0]),
        section_points={
            "P1": (105.0, 52.5),
            "P2": (-105.0, -52.5),
            "P3": (-105.0, 52.5),
            "P4": (105.0, -52.5),
        },
        side_plane_normals={
            "A": np.array([0.0, 0.0, 1.0]),
            "B": np.array([1.0, 0.0, 0.0]),
            "C": np.array([0.0, 0.0, -1.0]),
            "D": np.array([-1.0, 0.0, 0.0]),
        },
        boundary_points=boundary,
        scan_truth={end: {face: 90.0 for face in "ABCD"} for end in ("head", "tail")},
    )


def valid_v15_model() -> dict[str, object]:
    return {
        "version": 15,
        "valid": True,
        "model": "endface_camera_geometry_calibration",
        "strategy": "physical_decomposed_camera_scan_row_synchronization",
        "runtime_orientation_detection": "none",
        "self_consistency_selected_model_level": "M1",
        "runtime_selected_model_level": "M1",
        "runtime_correction_applied": True,
        "runtime_endpoint_label_contract": {
            "head": "hobj_device_row_min",
            "tail": "hobj_device_row_max",
            "physical_R_L_inferred": False,
            "reason": "HOBJ contains no physical endpoint identity metadata",
        },
        "report_to_software_face_map": {"A": "A", "B": "B", "C": "D", "D": "C"},
        "camera_y_offsets_mm": {"1": 0.2, "2": -0.4, "3": -0.9, "4": 1.1},
        "y_coordinate_correction": {
            "method": "per_camera_scan_row_offset",
            "apply_to": "all_longitudinal_side_points_and_end_boundary_points",
        },
        "nominal_spec": "210_105",
        "institution_or_manual_endface_truth_usage": {
            "loaded": False,
            "used_for_fit": False,
            "used_for_validation": False,
            "used_for_runtime": False,
        },
        "release_readiness": {
            "ready": True,
            "selected_model_level": "M1",
            "wireframe_16_angle_holdout_passed": True,
            "wireframe_16_angle_cross_validation_passed": True,
        },
        "model_comparison": {
            "M1": {"passed": True, "selectable_for_runtime": True},
            "M2": {"passed": False, "selectable_for_runtime": False},
        },
        "wireframe_validation": {
            "method": "shared_production_16_angle_holdout_and_paired_cross_validation",
            "shared_geometry_module": "endface_wireframe_geometry.measure_wireframe_angles",
            "fit_evidence": "image_only_physical_turnover_no_endface_truth",
            "selected_model_level": "M1",
            "passed": True,
            "raw_fixed_blind_holdout": {
                "statistics": {
                    "ordinal_pair_rmse_deg": 0.50,
                    "ordinal_pair_max_abs_error_deg": 0.80,
                },
            },
            "fixed_blind_holdout": {
                "method": "shared_production_wireframe_16_angle_physical_turnover_no_truth_lookup",
                "shared_geometry_module": "endface_wireframe_geometry.measure_wireframe_angles",
                "uses_professional_truth": False,
                "final_angle_correction_applied": False,
                "passed": True,
                "capture_counts": {"head_to_tail": 2, "tail_to_head": 2},
                "statistics": {
                    "group_median_rmse_deg": 0.20,
                    "ordinal_pair_rmse_deg": 0.21,
                    "group_median_max_abs_error_deg": 0.40,
                    "ordinal_pair_max_abs_error_deg": 0.42,
                    "maximum_repeatability_range_deg": 0.10,
                },
                "limits": {
                    "max_rmse_deg": 0.30,
                    "max_error_deg": 0.60,
                    "max_repeatability_range_deg": 0.20,
                },
            },
            "paired_cross_validation": {
                "method": "paired_capture_fold_validation_shared_16_angle_geometry",
                "fold_count": 5,
                "uses_each_usable_capture_as_holdout": True,
                "fit_uses_professional_truth_only_for_low_level_camera_y_parameters": False,
                "fit_uses_only_hobj_geometry_and_physical_turnover": True,
                "passed": True,
                "validation_uses_professional_truth": False,
                "final_angle_correction_applied": False,
                "statistics": {
                    "raw_group_median_rmse_deg": 0.50,
                    "raw_group_median_max_abs_error_deg": 0.80,
                    "raw_ordinal_pair_rmse_deg": 0.50,
                    "raw_ordinal_pair_max_abs_error_deg": 0.80,
                    "raw_maximum_repeatability_range_deg": 0.10,
                    "physical_corrected_group_median_rmse_deg": 0.20,
                    "physical_corrected_ordinal_pair_rmse_deg": 0.21,
                    "physical_corrected_group_median_max_abs_error_deg": 0.40,
                    "physical_corrected_ordinal_pair_max_abs_error_deg": 0.42,
                    "physical_corrected_maximum_repeatability_range_deg": 0.10,
                    "maximum_parameter_range_mm": 0.02,
                    "improvement_fraction": 0.50,
                },
                "limits": {
                    "max_rmse_deg": 0.30,
                    "max_error_deg": 0.60,
                    "max_repeatability_range_deg": 0.20,
                    "max_parameter_spread_mm": 0.15,
                    "minimum_improvement_fraction": 0.20,
                },
            },
        },
        "runtime_output_contract": {
            "local_angles_per_end": 8,
            "total_local_angles": 16,
            "representative_value_per_end": "actual_local_angle_farthest_from_90_not_average",
            "raw_and_physical_corrected_retained": True,
        },
        "calibration_state_reference": {
            "method": "same_physical_face_dual_camera_line_separation_no_truth",
            "fit_target": "image_geometry_applicability_only_no_correction",
            "all_captures_stable": True,
            "complete": True,
            "faces": {
                face: {
                    "valid": True,
                    "seam_median_min_mm": -0.3,
                    "seam_median_max_mm": 0.3,
                    "seam_span_max_mm": 0.6,
                    "pair_angle_p90_max_deg": 0.3,
                }
                for face in "ABCD"
            },
        },
        "physical_mode_basis": {
            "reference_camera_x_mm": [105.0, -105.0, -105.0, 105.0],
            "reference_camera_z_mm": [52.5, -52.5, 52.5, -52.5],
            "affine_x_mode": [0.5, -0.5, -0.5, 0.5],
            "affine_z_mode": [0.5, -0.5, 0.5, -0.5],
            "nonplanar_mode": [-1.0, -1.0, 1.0, 1.0],
        },
        "fit_details": {
            "selected": {
                "method": "single_nonplanar_camera_scan_row_synchronization",
                "model_level": "M1",
                "nonplanar_mode": {
                    "fit_target": "image_point_to_end_plane_residual_only_no_institution_truth"
                },
                "affine_modes": {
                    "enabled": False,
                },
            }
        },
    }


class EndfaceCalibratorTests(unittest.TestCase):
    def test_m1_selection_requires_improvement_without_repeatability_instability(self) -> None:
        raw_holdout = {
            "statistics": {
                "ordinal_pair_rmse_deg": 0.60,
                "ordinal_pair_max_abs_error_deg": 1.00,
            }
        }
        corrected_holdout = {
            "passed": True,
            "statistics": {
                "ordinal_pair_rmse_deg": 0.25,
                "ordinal_pair_max_abs_error_deg": 0.50,
            },
            "failures": [],
        }
        cross_validation = {
            "passed": True,
            "statistics": {
                "raw_ordinal_pair_rmse_deg": 0.60,
                "raw_ordinal_pair_max_abs_error_deg": 1.00,
                "raw_maximum_repeatability_range_deg": 0.10,
                "physical_corrected_ordinal_pair_rmse_deg": 0.25,
                "physical_corrected_ordinal_pair_max_abs_error_deg": 0.50,
                "physical_corrected_maximum_repeatability_range_deg": 0.11,
            },
            "failures": [],
        }
        accepted = image_model_candidate_acceptance(
            "M1",
            np.asarray([-0.1, -0.1, 0.1, 0.1]),
            raw_holdout,
            corrected_holdout,
            cross_validation,
        )
        self.assertTrue(accepted["passed"])
        unstable = json.loads(json.dumps(cross_validation))
        unstable["statistics"][
            "physical_corrected_maximum_repeatability_range_deg"
        ] = 0.13
        rejected = image_model_candidate_acceptance(
            "M1",
            np.asarray([-0.1, -0.1, 0.1, 0.1]),
            raw_holdout,
            corrected_holdout,
            unstable,
        )
        self.assertFalse(rejected["passed"])

    def test_offline_calibration_toolkit_requires_two_physical_pose_groups(self) -> None:
        batch = toolkit_calibration_batch()
        for folder in ("head_to_tail", "tail_to_head"):
            self.assertIn(folder, batch)
        self.assertNotIn("--physical-turnover-before", batch)
        self.assertNotIn("--physical-turnover-after", batch)
        self.assertIn("物理调头180度", batch)
        self.assertNotIn("--activate-dashboard-config", batch)

    def test_offline_camera_state_preflight_is_truth_free_and_model_free(self) -> None:
        batch = toolkit_camera_state_batch()
        self.assertIn("--camera-state-only", batch)
        self.assertIn("--camera-state-report-csv", batch)
        self.assertNotIn("--truth-csv", batch)
        self.assertNotIn("--output-model", batch)

    def test_calibrator_cli_has_no_dashboard_activation_bypass(self) -> None:
        source = (Path(__file__).parent / "endface_calibrator.py").read_text(
            encoding="utf-8"
        )
        self.assertNotIn("--activate-dashboard-config", source)
        self.assertNotIn("activate_dashboard_config", source)

    def test_endface_runtime_never_selects_camera_model_from_turnover_path(self) -> None:
        self.assertEqual(
            resolve_measurement_orientation(
                "auto", r"D:\capture\physical_diaotou\x.hobj", endface_only=True
            ),
            "normal",
        )
        self.assertEqual(
            resolve_measurement_orientation(
                "auto", r"D:\capture\physical_diaotou\x.hobj", endface_only=False
            ),
            "turnover",
        )

    def test_calibration_discovery_excludes_independent_turnover_validation_folders(self) -> None:
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            for relative in (
                "head_to_tail/a.hobj",
                "head_to_tail/b.hobj",
                "tail_to_head/c.hobj",
                "tail_to_head/d.hobj",
                "physical_turnover_before/e.hobj",
                "physical_turnover_after/f.hobj",
            ):
                path = root / relative
                path.parent.mkdir(parents=True, exist_ok=True)
                path.touch()
            discovered = discover_captures(root, 0.25)
        names = {path.name for path, _, _ in discovered}
        self.assertEqual(names, {"a.hobj", "b.hobj", "c.hobj", "d.hobj"})

    def test_dashboard_blocks_v8_and_malformed_physical_models(self) -> None:
        camera = {"single_bar_metadata": {"specification": "210_105"}}
        v8 = {
            "version": 8,
            "valid": True,
            "model": "endface_face_angle_offset",
            "angle_offsets_deg": {"head": {}, "tail": {}},
        }
        self.assertFalse(is_usable_physical_endface_model(v8, camera))

        physical = valid_v15_model()
        self.assertTrue(is_usable_physical_endface_model(physical, camera))
        no_state_gate = json.loads(json.dumps(physical))
        no_state_gate["version"] = 10
        no_state_gate.pop("calibration_state_reference")
        self.assertFalse(is_usable_physical_endface_model(no_state_gate, camera))
        physical["orientation_detector"] = {"forbidden": True}
        self.assertFalse(is_usable_physical_endface_model(physical, camera))

    def test_endface_physical_correction_is_withheld_outside_camera_state(self) -> None:
        model = valid_v15_model()
        stable = {
            "status": "relative_geometry_stable",
            "per_face": {
                face: {
                    "seam_median_mm": 0.0,
                    "seam_span_p05_p95_mm": 0.2,
                    "pair_angle_p90_deg": 0.1,
                }
                for face in "ABCD"
            },
        }
        matched = endface_calibration_applicability(
            model, stable, {"status": "normal", "valid": True}
        )
        self.assertTrue(matched["applicable"])
        self.assertEqual(matched["status"], "camera_state_matched")

        shifted = json.loads(json.dumps(stable))
        shifted["per_face"]["B"]["seam_median_mm"] = 0.8
        unmatched = endface_calibration_applicability(
            model, shifted, {"status": "normal", "valid": True}
        )
        self.assertFalse(unmatched["applicable"])
        self.assertEqual(unmatched["status"], "camera_state_unmatched_unadjusted")
        self.assertFalse(unmatched["correction_applied"])

        drift_unknown = endface_calibration_applicability(
            model, stable, {"status": "unmatched_unadjusted", "valid": True}
        )
        self.assertFalse(drift_unknown["applicable"])
        self.assertEqual(
            drift_unknown["status"], "mechanical_drift_unmatched_unadjusted"
        )

    def test_calibration_state_reference_requires_every_capture_stable(self) -> None:
        captures = [synthetic_capture() for _ in range(3)]
        for index, capture in enumerate(captures):
            capture.path = Path(f"state_{index}.hobj")
            capture.relative_camera_diagnostic = {
                "status": "relative_geometry_stable",
                "maximum_seam_span_p05_p95_mm": 0.3,
                "per_face": {
                    face: {
                        "seam_median_mm": index * 0.01,
                        "seam_span_p05_p95_mm": 0.2 + index * 0.01,
                        "pair_angle_p90_deg": 0.1,
                    }
                    for face in "ABCD"
                },
            }
        stable = calibration_state_reference(captures)
        self.assertTrue(stable["all_captures_stable"])
        self.assertEqual(stable["capture_count"], 3)

        captures[-1].relative_camera_diagnostic["status"] = (
            "relative_motion_or_face_nonplanarity_warning"
        )
        unsafe = calibration_state_reference(captures)
        self.assertFalse(unsafe["all_captures_stable"])

    def test_professional_truth_maps_report_ends_and_faces(self) -> None:
        with tempfile.TemporaryDirectory() as folder:
            path = Path(folder) / "truth.csv"
            with path.open("w", encoding="utf-8-sig", newline="") as handle:
                writer = csv.DictWriter(
                    handle,
                    fieldnames=[
                        "report_id", "sample_id", "nominal_spec", "record_type",
                        "location", "face", "value",
                    ],
                )
                writer.writeheader()
                for location, base in (("L", 89.0), ("R", 90.0)):
                    for index, face in enumerate("ABCD"):
                        writer.writerow({
                            "report_id": "R1", "sample_id": "BAR",
                            "nominal_spec": "210_105", "record_type": "endface_angle",
                            "location": location, "face": face, "value": base + index / 10,
                        })
            truth = read_professional_truth(path, "BAR", "R")
        self.assertEqual(truth.head_label, "R")
        self.assertEqual(truth.tail_label, "L")
        self.assertEqual(truth.face_map, INSTITUTION_TO_SOFTWARE_FACE_MAP)
        self.assertAlmostEqual(truth.angles["head"]["B"], 90.1)
        self.assertAlmostEqual(truth.angles["head"]["D"], 90.2)
        self.assertAlmostEqual(truth.angles["head"]["C"], 90.3)

    def test_physical_turnover_maps_end_face_and_directed_angle(self) -> None:
        truth = {
            "head": {"A": 1.0, "B": 2.0, "C": 3.0, "D": 4.0},
            "tail": {"A": 5.0, "B": 6.0, "C": 7.0, "D": 8.0},
        }
        self.assertEqual(
            oriented_professional_truth(truth, "turnover"),
            {
                "head": {"A": 175.0, "B": 172.0, "C": 173.0, "D": 174.0},
                "tail": {"A": 179.0, "B": 176.0, "C": 177.0, "D": 178.0},
            },
        )

    def test_image_only_row_order_audit_rejects_reversed_capture(self) -> None:
        captures: list[PhysicalCaptureMeasurement] = []
        base = np.asarray(
            [
                [[float(station), float(obj * station * 0.1)] for station in range(9)]
                for obj in range(1, 5)
            ],
            dtype=float,
        )
        base -= np.median(base, axis=1, keepdims=True)
        for index in range(3):
            item = synthetic_capture()
            item.path = Path(f"forward_{index}.hobj")
            item.longitudinal_profile = base.copy()
            captures.append(item)
        turnover_base = base[:, ::-1, :].copy()
        turnover_group = []
        for index in range(3):
            item = synthetic_capture()
            item.path = Path(f"physically_turned_{index}.hobj")
            item.orientation = "turnover"
            item.longitudinal_profile = turnover_base.copy()
            captures.append(item)
            turnover_group.append(item)
        invalid = synthetic_capture()
        invalid.path = Path("actually_reversed_rows.hobj")
        invalid.orientation = "turnover"
        invalid.longitudinal_profile = base.copy()
        captures.append(invalid)
        audit = audit_calibration_row_order(captures)
        self.assertTrue(all(item.row_order_status == "canonical" for item in turnover_group))
        self.assertEqual(invalid.row_order_status, "reversed")
        self.assertEqual(
            audit["method"],
            "image_only_within_physical_pose_centered_four_corner_longitudinal_trajectory",
        )

    def test_real_specimen_turnover_keeps_ad_and_exchanges_institution_bc(self) -> None:
        # Software C=institution D and software D=institution C, hence B/D swap internally.
        self.assertEqual(
            professional_turnover_face_map(),
            {"A": "A", "B": "D", "C": "C", "D": "B"},
        )

    def test_rigid_turnover_is_reconstructed_from_geometry_not_answer_swapping(self) -> None:
        original = synthetic_capture()
        for cloud in original.boundary_points["head"].values():
            cloud[:, 1] += 0.008 * cloud[:, 0] + 0.003 * cloud[:, 2]
        for cloud in original.boundary_points["tail"].values():
            cloud[:, 1] += -0.004 * cloud[:, 0] + 0.006 * cloud[:, 2]
        original.side_plane_normals = {
            "A": np.array([0.0, 0.004, 1.0]),
            "B": np.array([1.0, 0.003, 0.0]),
            "C": np.array([0.0, -0.005, -1.0]),
            "D": np.array([-1.0, 0.002, 0.0]),
        }
        original_angles, _ = physical_angles_for_capture(original, np.zeros(4))

        # A real end-for-end rotation about device Z maps X/Y -> -X/-Y.
        # No result columns or truth values are consulted: transform only the
        # synthetic image geometry and let the normal fitter run again.
        rotation = np.diag([-1.0, -1.0, 1.0])
        camera_map = {1: 3, 3: 1, 2: 4, 4: 2}
        point_map = {"P1": "P3", "P3": "P1", "P2": "P4", "P4": "P2"}
        software_face_map = professional_turnover_face_map()
        turned_boundaries = {"head": {}, "tail": {}}
        for old_end, new_end in (("head", "tail"), ("tail", "head")):
            for old_camera, new_camera in camera_map.items():
                turned_boundaries[new_end][new_camera] = (
                    original.boundary_points[old_end][old_camera] @ rotation.T
                )
        turned = PhysicalCaptureMeasurement(
            path=Path("rigidly_turned.hobj"),
            orientation="normal",
            split="holdout",
            rod_axis=np.array([0.0, 1.0, 0.0]),
            section_points={
                point_map[name]: (-value[0], value[1])
                for name, value in original.section_points.items()
            },
            side_plane_normals={
                software_face_map[face]: rotation @ normal
                for face, normal in original.side_plane_normals.items()
            },
            boundary_points=turned_boundaries,
            scan_truth=original.scan_truth,
        )
        turned_angles, _ = physical_angles_for_capture(turned, np.zeros(4))
        for old_end, new_end in (("head", "tail"), ("tail", "head")):
            for old_face, new_face in software_face_map.items():
                self.assertAlmostEqual(
                    turned_angles[new_end][new_face],
                    180.0 - original_angles[old_end][old_face],
                    places=8,
                )
        mapped_back = scan_angles_in_physical_standard_frame(turned_angles, "turnover")
        for end in ("head", "tail"):
            for face in "ABCD":
                self.assertAlmostEqual(
                    mapped_back[end][face],
                    original_angles[end][face],
                    places=8,
                )
        validation = validate_rigid_physical_turnover_measurements(
            [original, original, original],
            [turned, turned, turned],
            np.zeros(4),
        )
        self.assertTrue(validation["passed"])
        self.assertLess(validation["statistics"]["max_abs_error_deg"], 1e-8)

    def test_product_face_labels_are_institution_abcd(self) -> None:
        software = {
            "head": {"A": 1.0, "B": 2.0, "C": 3.0, "D": 4.0},
            "tail": {"A": 5.0, "B": 6.0, "C": 7.0, "D": 8.0},
        }
        self.assertEqual(
            institution_labeled_face_angles(software),
            {
                "head": {"A": 1.0, "B": 2.0, "C": 4.0, "D": 3.0},
                "tail": {"A": 5.0, "B": 6.0, "C": 8.0, "D": 7.0},
            },
        )

    def test_camera_offset_gauge_is_zero_sum(self) -> None:
        expanded = expand_camera_y_parameters([0.1, -0.2, 0.3])
        np.testing.assert_allclose(expanded, [0.1, -0.2, 0.3, -0.2])
        self.assertAlmostEqual(float(np.sum(expanded)), 0.0)

    def test_decomposed_physical_modes_recover_camera_bias_without_channel_offsets(self) -> None:
        biases = np.array([0.18, -0.08, -0.12, 0.02], dtype=float)
        clean = synthetic_capture()
        clean_angles, _ = physical_angles_for_capture(clean, np.zeros(4))
        captures = [synthetic_capture(biases) for _ in range(4)]
        for capture in captures:
            capture.scan_truth = {
                end: dict(clean_angles[end]) for end in ("head", "tail")
            }
        before = physical_plane_residuals(captures, np.zeros(3), equalize_capture_weight=False)
        basis = synchronization_mode_basis(captures)
        offsets, details = fit_decomposed_camera_y_offsets(captures, basis)
        after = physical_plane_residuals(captures, offsets[:3], equalize_capture_weight=False)
        self.assertEqual(
            details["nonplanar_mode"]["fit_target"],
            "image_point_to_end_plane_residual_only_no_institution_truth",
        )
        self.assertLess(float(np.sqrt(np.mean(after * after))), float(np.sqrt(np.mean(before * before))))
        self.assertAlmostEqual(float(np.sum(offsets)), 0.0, places=9)
        np.testing.assert_allclose(offsets, -biases, atol=0.03)

    def test_mode_basis_separates_affine_and_nonplanar_camera_states(self) -> None:
        basis = synchronization_mode_basis([synthetic_capture()])
        nonplanar = np.asarray(basis["nonplanar_mode"], dtype=float)
        x = np.asarray(basis["reference_camera_x_mm"], dtype=float)
        z = np.asarray(basis["reference_camera_z_mm"], dtype=float)
        self.assertAlmostEqual(float(np.sum(nonplanar)), 0.0, places=9)
        self.assertAlmostEqual(float(nonplanar @ x), 0.0, places=8)
        self.assertAlmostEqual(float(nonplanar @ z), 0.0, places=8)
        offsets = offsets_from_physical_modes(basis, 0.2, (0.1, -0.1))
        self.assertAlmostEqual(float(np.sum(offsets)), 0.0, places=9)

    def test_synthetic_end_tilt_changes_measured_angles_instead_of_being_clamped(self) -> None:
        base = synthetic_capture()
        base_angles, _ = physical_angles_for_capture(base, np.zeros(4))
        tilted = synthetic_capture()
        for cloud in tilted.boundary_points["head"].values():
            cloud[:, 1] += 0.010 * cloud[:, 0]
        tilted_angles, _ = physical_angles_for_capture(tilted, np.zeros(4))
        changes = [
            abs(tilted_angles["head"][face] - base_angles["head"][face])
            for face in "ABCD"
        ]
        self.assertGreater(max(changes), 0.50)
        self.assertLess(max(changes), 0.70)
        self.assertAlmostEqual(
            sum(tilted_angles["head"].values()) / 4.0,
            90.0,
            delta=0.01,
            msg="The four-angle mean is near 90 by geometry and is not a quality metric",
        )

    def test_wireframe_turnover_gate_uses_all_16_geometry_channels_without_truth(self) -> None:
        normal_angles = {
            end: {
                channel: 89.4 + 0.04 * index
                for index, channel in enumerate(LOCAL_CHANNELS)
            }
            for end in ("head", "tail")
        }
        turnover_angles = {
            end: {channel: 90.0 for channel in LOCAL_CHANNELS}
            for end in ("head", "tail")
        }
        for end in ("head", "tail"):
            for channel in LOCAL_CHANNELS:
                mapped_end, mapped_channel = mapped_turnover_channel(end, channel)
                turnover_angles[mapped_end][mapped_channel] = 180.0 - normal_angles[end][channel]
        normal = SimpleNamespace(path=Path("normal.hobj"), result={"angles": normal_angles})
        turnover = SimpleNamespace(path=Path("turnover.hobj"), result={"angles": turnover_angles})
        with patch(
            "tools.endface_calibrator.wireframe_angles_for_capture",
            side_effect=lambda item, _offsets: item.result,
        ):
            result = wireframe_turnover_equivariance_validation(
                [normal],
                [turnover],
                np.zeros(4),
                max_rmse_deg=0.01,
                max_error_deg=0.01,
                max_repeatability_range_deg=0.01,
            )
        self.assertTrue(result["passed"])
        self.assertFalse(result["uses_professional_truth"])
        self.assertFalse(result["final_angle_correction_applied"])
        self.assertEqual(len(result["channels"]), 16)
        self.assertAlmostEqual(
            result["statistics"]["group_median_max_abs_error_deg"],
            0.0,
            places=9,
        )

    def test_wireframe_turnover_gate_does_not_hide_paired_outliers_behind_median(self) -> None:
        exact = {
            end: {channel: 90.0 for channel in LOCAL_CHANNELS}
            for end in ("head", "tail")
        }
        turnover_high = json.loads(json.dumps(exact))
        turnover_low = json.loads(json.dumps(exact))
        mapped_end, mapped_channel = mapped_turnover_channel("head", "A_left")
        turnover_high[mapped_end][mapped_channel] = 91.0
        turnover_low[mapped_end][mapped_channel] = 89.0
        captures = [
            SimpleNamespace(path=Path("normal_1.hobj"), result={"angles": exact}),
            SimpleNamespace(path=Path("normal_2.hobj"), result={"angles": exact}),
        ]
        turnovers = [
            SimpleNamespace(path=Path("turnover_1.hobj"), result={"angles": turnover_high}),
            SimpleNamespace(path=Path("turnover_2.hobj"), result={"angles": turnover_low}),
        ]
        with patch(
            "tools.endface_calibrator.wireframe_angles_for_capture",
            side_effect=lambda item, _offsets: item.result,
        ):
            result = wireframe_turnover_equivariance_validation(
                captures,
                turnovers,
                np.zeros(4),
                max_rmse_deg=0.30,
                max_error_deg=0.60,
                max_repeatability_range_deg=3.0,
            )
        self.assertAlmostEqual(
            result["statistics"]["group_median_max_abs_error_deg"],
            0.0,
            places=9,
        )
        self.assertAlmostEqual(
            result["statistics"]["ordinal_pair_max_abs_error_deg"],
            1.0,
            places=9,
        )
        self.assertFalse(result["passed"])

    def test_runtime_accepts_only_image_only_physical_v15_m1_and_rejects_answer_offsets(self) -> None:
        calibration = {"single_bar_metadata": {"specification": "210_105"}}
        valid = valid_v15_model()
        validate_professional_endface_model(valid, calibration)
        engineering_only = json.loads(json.dumps(valid))
        engineering_only["release_readiness"] = {
            "ready": False,
            "reason": "missing verified physical-turnover HOBJ",
        }
        with self.assertRaises(ValueError):
            validate_professional_endface_model(engineering_only, calibration)
        self.assertFalse(is_usable_physical_endface_model(engineering_only, calibration))
        forbidden = json.loads(json.dumps(valid))
        forbidden["angle_offsets_deg"] = {
            end: {face: 0.0 for face in "ABCD"} for end in ("head", "tail")
        }
        with self.assertRaises(ValueError):
            validate_professional_endface_model(forbidden, calibration)
        fake_endpoint_identity = json.loads(json.dumps(valid))
        fake_endpoint_identity["runtime_endpoint_label_contract"]["physical_R_L_inferred"] = True
        with self.assertRaisesRegex(ValueError, "must not infer physical R/L"):
            validate_professional_endface_model(fake_endpoint_identity, calibration)
        self.assertFalse(is_usable_physical_endface_model(fake_endpoint_identity, calibration))
        missing_wireframe = json.loads(json.dumps(valid))
        missing_wireframe.pop("wireframe_validation")
        with self.assertRaisesRegex(ValueError, "16-angle holdout"):
            validate_professional_endface_model(missing_wireframe, calibration)
        self.assertFalse(is_usable_physical_endface_model(missing_wireframe, calibration))
        hidden_outlier = json.loads(json.dumps(valid))
        hidden_outlier["wireframe_validation"]["paired_cross_validation"]["statistics"][
            "physical_corrected_ordinal_pair_max_abs_error_deg"
        ] = 0.61
        with self.assertRaisesRegex(ValueError, "16-angle holdout"):
            validate_professional_endface_model(hidden_outlier, calibration)
        self.assertFalse(is_usable_physical_endface_model(hidden_outlier, calibration))
        old_v8 = {
            "version": 8,
            "model": "endface_face_angle_offset",
            "strategy": "single_physical_endpoint_offset_with_hobj_orientation_classifier",
            "valid": True,
            "nominal_spec": "210_105",
            "angle_offsets_deg": {},
        }
        with self.assertRaises(ValueError):
            validate_professional_endface_model(old_v8, calibration)


if __name__ == "__main__":
    unittest.main()
