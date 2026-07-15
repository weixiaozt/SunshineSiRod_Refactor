"""Shared truth-free wireframe geometry for production and offline audits."""

from __future__ import annotations

import math
from dataclasses import dataclass
from typing import Dict, Tuple

import numpy as np


ENDS = ("head", "tail")
CORE_FACE_ENDPOINTS = {
    "A": ("P3", "P1"),
    "B": ("P1", "P4"),
    "C": ("P2", "P4"),
    "D": ("P3", "P2"),
}
CAMERA_ADJACENT_CORE_FACES = {
    1: ("A", "B"),
    2: ("C", "D"),
    3: ("A", "D"),
    4: ("B", "C"),
}
REPORT_FACE_CONFIG = {
    "A": {"core_face": "A", "endpoints": ("P3", "P1"), "positions": ("left", "right")},
    "B": {"core_face": "B", "endpoints": ("P1", "P4"), "positions": ("top", "bottom")},
    "C": {"core_face": "D", "endpoints": ("P3", "P2"), "positions": ("top", "bottom")},
    "D": {"core_face": "C", "endpoints": ("P2", "P4"), "positions": ("left", "right")},
}
LOCAL_CHANNELS = (
    "A_left",
    "A_right",
    "B_top",
    "B_bottom",
    "C_top",
    "C_bottom",
    "D_left",
    "D_right",
)
TURNOVER_LOCAL_CHANNEL_MAP = {
    "A_left": "A_right",
    "A_right": "A_left",
    "B_top": "C_top",
    "B_bottom": "C_bottom",
    "C_top": "B_top",
    "C_bottom": "B_bottom",
    "D_left": "D_right",
    "D_right": "D_left",
}
CORNER_ADJACENT_EDGE_ENDPOINTS = {
    "P1": (("A", 1), ("B", 0)),
    "P2": (("C", 0), ("D", 1)),
    "P3": (("D", 0), ("A", 0)),
    "P4": (("B", 1), ("C", 1)),
}
POINT_TO_CAMERA = {"P1": 1, "P2": 2, "P3": 3, "P4": 4}


@dataclass
class SegmentPoints:
    camera: int
    coordinate: np.ndarray
    y_mm: np.ndarray
    kept_count: int
    raw_count: int
    line_slope: float
    line_intercept: float


@dataclass
class FaceEdgeFit:
    core_face: str
    direction: np.ndarray
    endpoint_y_mm: Tuple[float, float]
    slope_y_per_mm: float
    fit_rms_mm: float
    fit_p95_mm: float
    seam_at_face_mid_mm: float
    point_count: int
    raw_point_count: int
    per_camera_counts: Dict[int, int]


def normalized(vector: np.ndarray) -> np.ndarray:
    values = np.asarray(vector, dtype=float)
    norm = float(np.linalg.norm(values))
    if not math.isfinite(norm) or norm <= 1e-12:
        raise ValueError("Cannot normalize a zero or non-finite vector")
    return values / norm


def robust_linear_fit(
    coordinate: np.ndarray,
    values: np.ndarray,
    minimum_residual_mm: float = 0.12,
) -> Tuple[float, float, np.ndarray]:
    coordinate = np.asarray(coordinate, dtype=float)
    values = np.asarray(values, dtype=float)
    if len(coordinate) < 6 or len(values) != len(coordinate):
        raise ValueError("At least six paired values are required for a robust line fit")
    keep = np.ones(len(values), dtype=bool)
    for _ in range(5):
        slope, intercept = np.polyfit(coordinate[keep], values[keep], 1)
        residual = values - (slope * coordinate + intercept)
        centre = float(np.median(residual[keep]))
        mad = float(np.median(np.abs(residual[keep] - centre)))
        threshold = max(minimum_residual_mm, 4.0 * 1.4826 * mad)
        candidate = np.abs(residual - centre) <= threshold
        minimum_keep = max(6, int(math.ceil(0.65 * len(values))))
        if int(candidate.sum()) < minimum_keep or np.array_equal(candidate, keep):
            break
        keep = candidate
    slope, intercept = np.polyfit(coordinate[keep], values[keep], 1)
    return float(slope), float(intercept), keep


def robust_ridge_direction(
    rows: list[Dict[str, object]],
    point_name: str,
    camera_y_offset_mm: float = 0.0,
) -> np.ndarray:
    if len(rows) < 4:
        raise ValueError(f"Ridge {point_name} requires at least four valid section rows")
    y = np.asarray(
        [float(row["y_mm"]) + float(camera_y_offset_mm) for row in rows],
        dtype=float,
    )
    x = np.asarray([float(row[f"{point_name}_x"]) for row in rows], dtype=float)
    z = np.asarray([float(row[f"{point_name}_z"]) for row in rows], dtype=float)
    if not bool(np.all(np.isfinite(y))) or not bool(np.all(np.isfinite(x))) or not bool(np.all(np.isfinite(z))):
        raise ValueError(f"Ridge {point_name} contains non-finite coordinates")
    keep = np.ones(len(y), dtype=bool)
    for _ in range(4):
        dx_dy, x0 = np.polyfit(y[keep], x[keep], 1)
        dz_dy, z0 = np.polyfit(y[keep], z[keep], 1)
        residual = np.hypot(x - (dx_dy * y + x0), z - (dz_dy * y + z0))
        centre = float(np.median(residual[keep]))
        mad = float(np.median(np.abs(residual[keep] - centre)))
        candidate = residual <= centre + max(0.02, 4.0 * 1.4826 * mad)
        if int(candidate.sum()) < 4 or np.array_equal(candidate, keep):
            break
        keep = candidate
    dx_dy, _ = np.polyfit(y[keep], x[keep], 1)
    dz_dy, _ = np.polyfit(y[keep], z[keep], 1)
    return normalized(np.array([float(dx_dy), 1.0, float(dz_dy)], dtype=float))


def distance_to_cross_section_line(
    xz: np.ndarray,
    core_face: str,
    section_points: Dict[str, Tuple[float, float]],
) -> float:
    first_name, second_name = CORE_FACE_ENDPOINTS[core_face]
    first = np.asarray(section_points[first_name], dtype=float)
    second = np.asarray(section_points[second_name], dtype=float)
    direction = second - first
    relative = np.asarray(xz, dtype=float) - first
    denominator = float(np.linalg.norm(direction))
    if denominator <= 1e-12:
        return math.inf
    return abs(float(direction[0] * relative[1] - direction[1] * relative[0])) / denominator


def cross_section_coordinate(
    cloud: np.ndarray,
    core_face: str,
    section_points: Dict[str, Tuple[float, float]],
) -> Tuple[np.ndarray, np.ndarray, float]:
    first_name, second_name = CORE_FACE_ENDPOINTS[core_face]
    first = np.asarray(section_points[first_name], dtype=float)
    second = np.asarray(section_points[second_name], dtype=float)
    face_vector = second - first
    face_length = float(np.linalg.norm(face_vector))
    if not math.isfinite(face_length) or face_length <= 1e-12:
        raise ValueError(f"Cross-section face {core_face} is degenerate")
    face_direction = face_vector / face_length
    coordinate = (cloud[:, [0, 2]] - first) @ face_direction
    line_distance = np.abs(
        face_direction[0] * (cloud[:, 2] - first[1])
        - face_direction[1] * (cloud[:, 0] - first[0])
    )
    return coordinate, line_distance, face_length


def segment_for_camera_face(
    camera: int,
    cloud: np.ndarray,
    core_face: str,
    adjacent_faces: Tuple[str, str],
    section_points: Dict[str, Tuple[float, float]],
) -> SegmentPoints:
    assigned = np.asarray(
        [
            point
            for point in cloud
            if min(
                adjacent_faces,
                key=lambda face: distance_to_cross_section_line(point[[0, 2]], face, section_points),
            )
            == core_face
        ],
        dtype=float,
    )
    if len(assigned) < 12:
        raise ValueError(f"Camera {camera} face {core_face} has fewer than 12 boundary points")
    coordinate, line_distance, _ = cross_section_coordinate(assigned, core_face, section_points)
    distance_centre = float(np.median(line_distance))
    distance_mad = float(np.median(np.abs(line_distance - distance_centre)))
    distance_limit = max(0.25, distance_centre + 4.0 * 1.4826 * distance_mad)
    geometric_keep = line_distance <= distance_limit
    if int(geometric_keep.sum()) < 12:
        geometric_keep = np.ones(len(assigned), dtype=bool)
    coordinate = coordinate[geometric_keep]
    y_mm = assigned[geometric_keep, 1]
    slope, intercept, line_keep = robust_linear_fit(coordinate, y_mm)
    return SegmentPoints(
        camera=camera,
        coordinate=coordinate[line_keep],
        y_mm=y_mm[line_keep],
        kept_count=int(line_keep.sum()),
        raw_count=len(assigned),
        line_slope=slope,
        line_intercept=intercept,
    )


def fit_face_edge(
    core_face: str,
    segments: list[SegmentPoints],
    section_points: Dict[str, Tuple[float, float]],
    side_normal: np.ndarray,
) -> FaceEdgeFit:
    if len(segments) != 2:
        raise ValueError(f"Face {core_face} requires exactly two camera segments")
    coordinate = np.concatenate([segment.coordinate for segment in segments])
    y_mm = np.concatenate([segment.y_mm for segment in segments])
    weights = np.concatenate(
        [np.full(segment.kept_count, 1.0 / segment.kept_count, dtype=float) for segment in segments]
    )
    design = np.column_stack([coordinate, np.ones(len(coordinate), dtype=float)])
    normal_matrix = design.T @ (weights[:, None] * design)
    target = design.T @ (weights * y_mm)
    slope, intercept = np.linalg.solve(normal_matrix, target)
    predicted = slope * coordinate + intercept
    residual = y_mm - predicted

    first_name, second_name = CORE_FACE_ENDPOINTS[core_face]
    first = np.asarray(section_points[first_name], dtype=float)
    second = np.asarray(section_points[second_name], dtype=float)
    cross_direction = normalized(second - first)
    face_length = float(np.linalg.norm(second - first))
    direction = np.array([cross_direction[0], float(slope), cross_direction[1]], dtype=float)
    side_normal = normalized(np.asarray(side_normal, dtype=float))
    direction = normalized(direction - side_normal * float(direction @ side_normal))

    midpoint = 0.5 * face_length
    individual_mid_y = [
        segment.line_slope * midpoint + segment.line_intercept
        for segment in segments
    ]
    return FaceEdgeFit(
        core_face=core_face,
        direction=direction,
        endpoint_y_mm=(float(intercept), float(intercept + slope * face_length)),
        slope_y_per_mm=float(slope),
        fit_rms_mm=float(np.sqrt(np.mean(np.square(residual)))),
        fit_p95_mm=float(np.percentile(np.abs(residual), 95)),
        seam_at_face_mid_mm=abs(float(individual_mid_y[0] - individual_mid_y[1])),
        point_count=len(coordinate),
        raw_point_count=sum(segment.raw_count for segment in segments),
        per_camera_counts={segment.camera: segment.kept_count for segment in segments},
    )


def measure_wireframe_angles(
    longitudinal_rows: list[Dict[str, object]],
    section_points: Dict[str, Tuple[float, float]],
    boundary_points: Dict[str, Dict[int, np.ndarray]],
    side_plane_normals: Dict[str, np.ndarray],
    camera_y_offsets_mm: Dict[int, float] | None = None,
) -> Dict[str, object]:
    """Rebuild both end wireframes from prepared image geometry.

    Boundary points must still be in their base common-space coordinates.  The
    same per-camera Y offsets used to fit ``side_plane_normals`` are applied
    here to the boundary points and longitudinal ridge coordinates.  No truth,
    final angle offset, end identity inference, or specification value enters
    this function.
    """

    offsets = {camera: 0.0 for camera in (1, 2, 3, 4)}
    if camera_y_offsets_mm is not None:
        for camera in offsets:
            value = float(camera_y_offsets_mm.get(camera, 0.0))
            if not math.isfinite(value):
                raise ValueError(f"Non-finite camera {camera} wireframe Y offset")
            offsets[camera] = value
    missing_normals = sorted(set(CORE_FACE_ENDPOINTS) - set(side_plane_normals))
    if missing_normals:
        raise ValueError(f"Wireframe side-plane normals are incomplete: {missing_normals}")
    ridge_directions = {
        point_name: robust_ridge_direction(
            longitudinal_rows,
            point_name,
            camera_y_offset_mm=offsets[camera],
        )
        for point_name, camera in POINT_TO_CAMERA.items()
    }
    result: Dict[str, object] = {"angles": {}, "summaries": {}, "diagnostics": {}}
    for end in ENDS:
        if end not in boundary_points:
            raise ValueError(f"Wireframe boundary points are missing end: {end}")
        clouds: Dict[int, np.ndarray] = {}
        for camera in (1, 2, 3, 4):
            try:
                cloud = np.asarray(boundary_points[end][camera], dtype=float).copy()
            except KeyError as exc:
                raise ValueError(f"Wireframe boundary points are missing {end}-camera{camera}") from exc
            if cloud.ndim != 2 or cloud.shape[1] != 3:
                raise ValueError(f"Wireframe {end}-camera{camera} boundary cloud must be N x 3")
            if len(cloud):
                cloud[:, 1] += offsets[camera]
            clouds[camera] = cloud

        segments_by_face = {face: [] for face in CORE_FACE_ENDPOINTS}
        for camera, cloud in clouds.items():
            adjacent_faces = CAMERA_ADJACENT_CORE_FACES[camera]
            for core_face in adjacent_faces:
                segments_by_face[core_face].append(
                    segment_for_camera_face(
                        camera,
                        cloud,
                        core_face,
                        adjacent_faces,
                        section_points,
                    )
                )
        edge_fits = {
            core_face: fit_face_edge(
                core_face,
                segments,
                section_points,
                side_plane_normals[core_face],
            )
            for core_face, segments in segments_by_face.items()
        }
        local_angles: Dict[str, float] = {}
        diagnostics: Dict[str, object] = {}
        for report_face, config in REPORT_FACE_CONFIG.items():
            core_face = str(config["core_face"])
            edge_fit = edge_fits[core_face]
            for index, (point_name, position) in enumerate(
                zip(tuple(config["endpoints"]), tuple(config["positions"]))
            ):
                edge_ray = -edge_fit.direction if index == 0 else edge_fit.direction
                local_angles[f"{report_face}_{position}"] = projected_angle_deg(
                    edge_ray,
                    ridge_directions[str(point_name)],
                    side_plane_normals[core_face],
                )
            diagnostics[f"{report_face}_dual_camera_seam_mid_mm"] = edge_fit.seam_at_face_mid_mm
            diagnostics[f"{report_face}_edge_fit_rms_mm"] = edge_fit.fit_rms_mm
            diagnostics[f"{report_face}_edge_fit_p95_mm"] = edge_fit.fit_p95_mm
        result["angles"][end] = local_angles
        result["summaries"][end] = summarize_end_angles(local_angles)
        diagnostics.update(wireframe_shape_diagnostics(edge_fits, section_points))
        diagnostics["max_dual_camera_seam_mid_mm"] = max(
            fit.seam_at_face_mid_mm for fit in edge_fits.values()
        )
        result["diagnostics"][end] = diagnostics
    return result


def projected_angle_deg(first: np.ndarray, second: np.ndarray, plane_normal: np.ndarray) -> float:
    plane_normal = normalized(plane_normal)
    first = normalized(np.asarray(first, dtype=float) - plane_normal * float(first @ plane_normal))
    second = normalized(np.asarray(second, dtype=float) - plane_normal * float(second @ plane_normal))
    return math.degrees(math.acos(float(np.clip(first @ second, -1.0, 1.0))))


def summarize_end_angles(angles: Dict[str, float]) -> Dict[str, object]:
    if set(angles) != set(LOCAL_CHANNELS):
        missing = sorted(set(LOCAL_CHANNELS) - set(angles))
        raise ValueError(f"End-face local angles are incomplete: {missing}")
    worst_channel = max(LOCAL_CHANNELS, key=lambda channel: abs(float(angles[channel]) - 90.0))
    values = np.asarray([float(angles[channel]) for channel in LOCAL_CHANNELS], dtype=float)
    errors = np.abs(values - 90.0)
    return {
        "worst_local_angle_deg": float(angles[worst_channel]),
        "worst_local_channel": worst_channel,
        "worst_local_error_deg": float(np.max(errors)),
        "rms_error_from_90_deg": float(np.sqrt(np.mean(np.square(errors)))),
        "angle_span_deg": float(np.max(values) - np.min(values)),
    }


def wireframe_shape_diagnostics(
    edge_fits: Dict[str, FaceEdgeFit],
    section_points: Dict[str, Tuple[float, float]],
) -> Dict[str, object]:
    closure_gaps: Dict[str, float] = {}
    corner_y: Dict[str, float] = {}
    for point_name, adjacent in CORNER_ADJACENT_EDGE_ENDPOINTS.items():
        values = [edge_fits[face].endpoint_y_mm[index] for face, index in adjacent]
        closure_gaps[point_name] = abs(float(values[0] - values[1]))
        corner_y[point_name] = float(np.mean(values))

    design = np.asarray(
        [
            [section_points[point_name][0], section_points[point_name][1], 1.0]
            for point_name in ("P1", "P2", "P3", "P4")
        ],
        dtype=float,
    )
    target = np.asarray([corner_y[name] for name in ("P1", "P2", "P3", "P4")], dtype=float)
    coefficients, _, _, _ = np.linalg.lstsq(design, target, rcond=None)
    residual = target - design @ coefficients
    diagonal_twist = abs((corner_y["P1"] + corner_y["P2"]) - (corner_y["P3"] + corner_y["P4"])) / 2.0
    result: Dict[str, object] = {
        "max_corner_closure_gap_mm": max(closure_gaps.values()),
        "corner_plane_rms_mm": float(np.sqrt(np.mean(np.square(residual)))),
        "corner_plane_peak_to_peak_mm": float(np.ptp(residual)),
        "wireframe_diagonal_twist_mm": float(diagonal_twist),
    }
    for point_name, value in closure_gaps.items():
        result[f"{point_name}_closure_gap_mm"] = value
    for point_name, value in corner_y.items():
        result[f"{point_name}_wireframe_y_mm"] = value
    return result


def mapped_turnover_channel(end: str, local_channel: str) -> Tuple[str, str]:
    if end not in ENDS:
        raise ValueError(f"Unknown end: {end}")
    if local_channel not in TURNOVER_LOCAL_CHANNEL_MAP:
        raise ValueError(f"Unknown local channel: {local_channel}")
    return ("tail" if end == "head" else "head", TURNOVER_LOCAL_CHANNEL_MAP[local_channel])


def has_valid_wireframe_release_evidence(model: Dict[str, object]) -> bool:
    """Validate v15 M1 image-only shared-geometry evidence without trusting flags alone."""

    validation = model.get("wireframe_validation")
    output_contract = model.get("runtime_output_contract")
    readiness = model.get("release_readiness")
    truth_usage = model.get("institution_or_manual_endface_truth_usage")
    comparison = model.get("model_comparison")
    if not (
        int(model.get("version", 0)) >= 15
        and model.get("self_consistency_selected_model_level") == "M1"
        and model.get("runtime_selected_model_level") == "M1"
        and model.get("runtime_correction_applied") is True
        and isinstance(validation, dict)
        and validation.get("method")
        == "shared_production_16_angle_holdout_and_paired_cross_validation"
        and validation.get("shared_geometry_module")
        == "endface_wireframe_geometry.measure_wireframe_angles"
        and validation.get("fit_evidence")
        == "image_only_physical_turnover_no_endface_truth"
        and validation.get("selected_model_level") == "M1"
        and validation.get("passed") is True
        and isinstance(comparison, dict)
        and isinstance(comparison.get("M1"), dict)
        and comparison["M1"].get("passed") is True
        and comparison["M1"].get("selectable_for_runtime") is True
        and isinstance(comparison.get("M2"), dict)
        and comparison["M2"].get("selectable_for_runtime") is False
        and isinstance(truth_usage, dict)
        and truth_usage.get("loaded") is False
        and truth_usage.get("used_for_fit") is False
        and truth_usage.get("used_for_validation") is False
        and truth_usage.get("used_for_runtime") is False
        and isinstance(output_contract, dict)
        and output_contract.get("local_angles_per_end") == 8
        and output_contract.get("total_local_angles") == 16
        and output_contract.get("representative_value_per_end")
        == "actual_local_angle_farthest_from_90_not_average"
        and output_contract.get("raw_and_physical_corrected_retained") is True
        and isinstance(readiness, dict)
        and readiness.get("selected_model_level") == "M1"
        and readiness.get("wireframe_16_angle_holdout_passed") is True
        and readiness.get("wireframe_16_angle_cross_validation_passed") is True
    ):
        return False

    fixed = validation.get("fixed_blind_holdout")
    cross_validation = validation.get("paired_cross_validation")
    if not (
        isinstance(fixed, dict)
        and fixed.get("method")
        == "shared_production_wireframe_16_angle_physical_turnover_no_truth_lookup"
        and fixed.get("shared_geometry_module")
        == "endface_wireframe_geometry.measure_wireframe_angles"
        and fixed.get("uses_professional_truth") is False
        and fixed.get("final_angle_correction_applied") is False
        and fixed.get("passed") is True
        and isinstance(cross_validation, dict)
        and cross_validation.get("method")
        == "paired_capture_fold_validation_shared_16_angle_geometry"
        and int(cross_validation.get("fold_count", 0)) >= 5
        and cross_validation.get("uses_each_usable_capture_as_holdout") is True
        and cross_validation.get(
            "fit_uses_professional_truth_only_for_low_level_camera_y_parameters"
        )
        is False
        and cross_validation.get("fit_uses_only_hobj_geometry_and_physical_turnover")
        is True
        and cross_validation.get("validation_uses_professional_truth") is False
        and cross_validation.get("final_angle_correction_applied") is False
        and cross_validation.get("passed") is True
    ):
        return False

    try:
        counts = fixed["capture_counts"]
        fixed_statistics = fixed["statistics"]
        fixed_limits = fixed["limits"]
        cross_statistics = cross_validation["statistics"]
        cross_limits = cross_validation["limits"]
        if not all(
            isinstance(item, dict)
            for item in (counts, fixed_statistics, fixed_limits, cross_statistics, cross_limits)
        ):
            return False
        if int(counts["head_to_tail"]) < 2 or int(counts["tail_to_head"]) < 2:
            return False

        fixed_rmse_limit = float(fixed_limits["max_rmse_deg"])
        fixed_error_limit = float(fixed_limits["max_error_deg"])
        fixed_repeatability_limit = float(fixed_limits["max_repeatability_range_deg"])
        fixed_values = [
            float(fixed_statistics["group_median_rmse_deg"]),
            float(fixed_statistics["ordinal_pair_rmse_deg"]),
            float(fixed_statistics["group_median_max_abs_error_deg"]),
            float(fixed_statistics["ordinal_pair_max_abs_error_deg"]),
            float(fixed_statistics["maximum_repeatability_range_deg"]),
            fixed_rmse_limit,
            fixed_error_limit,
            fixed_repeatability_limit,
        ]
        if not all(math.isfinite(value) for value in fixed_values):
            return False
        if fixed_rmse_limit > 0.30 or fixed_error_limit > 0.60 or fixed_repeatability_limit > 0.20:
            return False
        if max(fixed_values[0], fixed_values[1]) > fixed_rmse_limit:
            return False
        if max(fixed_values[2], fixed_values[3]) > fixed_error_limit:
            return False
        if fixed_values[4] > fixed_repeatability_limit:
            return False

        cross_rmse_limit = float(cross_limits["max_rmse_deg"])
        cross_error_limit = float(cross_limits["max_error_deg"])
        cross_repeatability_limit = float(cross_limits["max_repeatability_range_deg"])
        cross_parameter_limit = float(cross_limits["max_parameter_spread_mm"])
        minimum_improvement = float(cross_limits["minimum_improvement_fraction"])
        cross_values = [
            float(cross_statistics["physical_corrected_group_median_rmse_deg"]),
            float(cross_statistics["physical_corrected_ordinal_pair_rmse_deg"]),
            float(cross_statistics["physical_corrected_group_median_max_abs_error_deg"]),
            float(cross_statistics["physical_corrected_ordinal_pair_max_abs_error_deg"]),
            float(cross_statistics["physical_corrected_maximum_repeatability_range_deg"]),
            float(cross_statistics["maximum_parameter_range_mm"]),
            float(cross_statistics["improvement_fraction"]),
            cross_rmse_limit,
            cross_error_limit,
            cross_repeatability_limit,
            cross_parameter_limit,
            minimum_improvement,
        ]
        if not all(math.isfinite(value) for value in cross_values):
            return False
        if (
            cross_rmse_limit > 0.30
            or cross_error_limit > 0.60
            or cross_repeatability_limit > 0.20
            or cross_parameter_limit > 0.15
            or minimum_improvement < 0.20
        ):
            return False
        if max(cross_values[0], cross_values[1]) > cross_rmse_limit:
            return False
        if max(cross_values[2], cross_values[3]) > cross_error_limit:
            return False
        if cross_values[4] > cross_repeatability_limit:
            return False
        if cross_values[5] > cross_parameter_limit or cross_values[6] < minimum_improvement:
            return False
        raw_fixed_rmse = float(
            model["wireframe_validation"]["raw_fixed_blind_holdout"]["statistics"][
                "ordinal_pair_rmse_deg"
            ]
        )
        raw_fixed_max = float(
            model["wireframe_validation"]["raw_fixed_blind_holdout"]["statistics"][
                "ordinal_pair_max_abs_error_deg"
            ]
        )
        raw_cross_rmse = float(cross_statistics["raw_ordinal_pair_rmse_deg"])
        raw_cross_max = float(cross_statistics["raw_ordinal_pair_max_abs_error_deg"])
        raw_repeatability = float(cross_statistics["raw_maximum_repeatability_range_deg"])
        if not all(
            math.isfinite(value)
            for value in (
                raw_fixed_rmse,
                raw_fixed_max,
                raw_cross_rmse,
                raw_cross_max,
                raw_repeatability,
            )
        ):
            return False
        if not (
            fixed_values[1] < raw_fixed_rmse
            and fixed_values[3] < raw_fixed_max
            and cross_values[1] < raw_cross_rmse
            and cross_values[3] < raw_cross_max
            and cross_values[4] <= raw_repeatability + 0.02
        ):
            return False
    except (KeyError, TypeError, ValueError, OverflowError):
        return False
    return True
