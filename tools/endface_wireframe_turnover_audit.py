#!/usr/bin/env python3
"""Truth-free four-edge end-face audit with physical-turnover equivariance.

The audit never reads professional end-face truth and never applies final-angle
offsets.  It uses the fixed camera-to-common-space model only to express the
four cameras in one XYZ coordinate system.  Each end is represented by four
independently fitted boundary edges and four longitudinal corner traces.
"""

from __future__ import annotations

import argparse
import csv
import json
import math
from dataclasses import dataclass
from pathlib import Path
from typing import Dict, Iterable, Tuple

import numpy as np

try:
    from .measure_square_rod_edges import (
        HobjSource,
        X_SCALE_MM,
        Y_SCALE_MM,
        calibration_for_orientation,
        endface_boundary_points_for_camera,
        longitudinal_side_plane_normals,
        mean_cross_section_points,
        sampled_cross_section_rows_and_edges,
        source_common_range,
        source_valid_ranges,
    )
except ImportError:
    from measure_square_rod_edges import (
        HobjSource,
        X_SCALE_MM,
        Y_SCALE_MM,
        calibration_for_orientation,
        endface_boundary_points_for_camera,
        longitudinal_side_plane_normals,
        mean_cross_section_points,
        sampled_cross_section_rows_and_edges,
        source_common_range,
        source_valid_ranges,
    )


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
    y = np.asarray(
        [float(row["y_mm"]) + float(camera_y_offset_mm) for row in rows],
        dtype=float,
    )
    x = np.asarray([float(row[f"{point_name}_x"]) for row in rows], dtype=float)
    z = np.asarray([float(row[f"{point_name}_z"]) for row in rows], dtype=float)
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


# Production and offline audits execute the same geometry implementation.  The
# local definitions above remain temporarily for backward-compatible imports,
# but all runtime calls below are rebound to the shared module.
try:
    from .endface_wireframe_geometry import (
        CAMERA_ADJACENT_CORE_FACES as SHARED_CAMERA_ADJACENT_CORE_FACES,
        CORE_FACE_ENDPOINTS as SHARED_CORE_FACE_ENDPOINTS,
        LOCAL_CHANNELS as SHARED_LOCAL_CHANNELS,
        REPORT_FACE_CONFIG as SHARED_REPORT_FACE_CONFIG,
        fit_face_edge as shared_fit_face_edge,
        projected_angle_deg as shared_projected_angle_deg,
        robust_ridge_direction as shared_robust_ridge_direction,
        segment_for_camera_face as shared_segment_for_camera_face,
        summarize_end_angles as shared_summarize_end_angles,
        wireframe_shape_diagnostics as shared_wireframe_shape_diagnostics,
    )
except ImportError:
    from endface_wireframe_geometry import (
        CAMERA_ADJACENT_CORE_FACES as SHARED_CAMERA_ADJACENT_CORE_FACES,
        CORE_FACE_ENDPOINTS as SHARED_CORE_FACE_ENDPOINTS,
        LOCAL_CHANNELS as SHARED_LOCAL_CHANNELS,
        REPORT_FACE_CONFIG as SHARED_REPORT_FACE_CONFIG,
        fit_face_edge as shared_fit_face_edge,
        projected_angle_deg as shared_projected_angle_deg,
        robust_ridge_direction as shared_robust_ridge_direction,
        segment_for_camera_face as shared_segment_for_camera_face,
        summarize_end_angles as shared_summarize_end_angles,
        wireframe_shape_diagnostics as shared_wireframe_shape_diagnostics,
    )

CAMERA_ADJACENT_CORE_FACES = SHARED_CAMERA_ADJACENT_CORE_FACES
CORE_FACE_ENDPOINTS = SHARED_CORE_FACE_ENDPOINTS
LOCAL_CHANNELS = SHARED_LOCAL_CHANNELS
REPORT_FACE_CONFIG = SHARED_REPORT_FACE_CONFIG
fit_face_edge = shared_fit_face_edge
projected_angle_deg = shared_projected_angle_deg
robust_ridge_direction = shared_robust_ridge_direction
segment_for_camera_face = shared_segment_for_camera_face
summarize_end_angles = shared_summarize_end_angles
wireframe_shape_diagnostics = shared_wireframe_shape_diagnostics


def measure_capture(
    path: Path,
    group: str,
    calibration: Dict[str, object],
    endface_window_mm: float,
) -> Dict[str, object]:
    active = calibration_for_orientation(calibration, "normal")
    transforms = {int(key): value for key, value in active["transforms"].items()}
    source = HobjSource(str(path))
    valid_ranges = source_valid_ranges(source)
    common_start, common_end = source_common_range(source, valid_ranges)
    rows, _ = sampled_cross_section_rows_and_edges(
        source,
        calibration,
        "normal",
        X_SCALE_MM,
        Y_SCALE_MM,
        valid_ranges,
    )
    section_points = mean_cross_section_points(rows)
    side_normals = longitudinal_side_plane_normals(rows)
    ridge_directions = {
        point_name: robust_ridge_direction(rows, point_name)
        for point_name in ("P1", "P2", "P3", "P4")
    }
    result: Dict[str, object] = {
        "group": group,
        "capture": path.name,
        "input_path": str(path),
        "uses_professional_truth": False,
        "final_angle_correction_applied": False,
        "camera_geometry_orientation": "fixed_normal_for_both_physical_poses",
        "common_start_row": common_start,
        "common_end_row": common_end,
    }

    for end in ENDS:
        camera_clouds = {
            camera: endface_boundary_points_for_camera(
                source,
                camera,
                end,
                valid_ranges[camera],
                transforms[camera],
                active,
                common_start,
                X_SCALE_MM,
                Y_SCALE_MM,
                endface_window_mm,
                common_end=common_end,
            )
            for camera in (1, 2, 3, 4)
        }
        segments_by_face: Dict[str, list[SegmentPoints]] = {face: [] for face in CORE_FACE_ENDPOINTS}
        for camera, cloud in camera_clouds.items():
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
                side_normals[core_face],
            )
            for core_face, segments in segments_by_face.items()
        }

        local_angles: Dict[str, float] = {}
        for report_face, config in REPORT_FACE_CONFIG.items():
            core_face = str(config["core_face"])
            endpoint_names = tuple(config["endpoints"])
            positions = tuple(config["positions"])
            edge_fit = edge_fits[core_face]
            for index, (point_name, position) in enumerate(zip(endpoint_names, positions)):
                edge_ray = -edge_fit.direction if index == 0 else edge_fit.direction
                channel = f"{report_face}_{position}"
                angle = projected_angle_deg(
                    edge_ray,
                    ridge_directions[point_name],
                    side_normals[core_face],
                )
                local_angles[channel] = angle
                result[f"{end}_{channel}_angle_deg"] = angle

            result[f"{end}_{report_face}_edge_slope_y_per_mm"] = edge_fit.slope_y_per_mm
            result[f"{end}_{report_face}_edge_fit_rms_mm"] = edge_fit.fit_rms_mm
            result[f"{end}_{report_face}_edge_fit_p95_mm"] = edge_fit.fit_p95_mm
            result[f"{end}_{report_face}_dual_camera_seam_mid_mm"] = edge_fit.seam_at_face_mid_mm
            result[f"{end}_{report_face}_edge_point_count"] = edge_fit.point_count

        summary = summarize_end_angles(local_angles)
        for key, value in summary.items():
            result[f"{end}_endface_{key}"] = value
        shape = wireframe_shape_diagnostics(edge_fits, section_points)
        for key, value in shape.items():
            result[f"{end}_{key}"] = value
        result[f"{end}_max_dual_camera_seam_mid_mm"] = max(
            fit.seam_at_face_mid_mm for fit in edge_fits.values()
        )
        result[f"{end}_max_edge_fit_rms_mm"] = max(fit.fit_rms_mm for fit in edge_fits.values())
    return result


def mapped_turnover_channel(end: str, local_channel: str) -> Tuple[str, str]:
    if end not in ENDS:
        raise ValueError(f"Unknown end: {end}")
    if local_channel not in TURNOVER_LOCAL_CHANNEL_MAP:
        raise ValueError(f"Unknown local channel: {local_channel}")
    return ("tail" if end == "head" else "head", TURNOVER_LOCAL_CHANNEL_MAP[local_channel])


def finite_values(rows: Iterable[Dict[str, object]], field: str) -> np.ndarray:
    values = []
    for row in rows:
        try:
            value = float(row[field])
        except (KeyError, TypeError, ValueError):
            continue
        if math.isfinite(value):
            values.append(value)
    return np.asarray(values, dtype=float)


def turnover_equivariance_rows(capture_rows: list[Dict[str, object]]) -> list[Dict[str, object]]:
    normal_rows = [row for row in capture_rows if row.get("group") == "head_to_tail"]
    turnover_rows = [row for row in capture_rows if row.get("group") == "tail_to_head"]
    if not normal_rows or not turnover_rows:
        raise ValueError("Both head_to_tail and tail_to_head capture groups are required")
    output: list[Dict[str, object]] = []
    for end in ENDS:
        for local_channel in LOCAL_CHANNELS:
            turnover_end, turnover_channel = mapped_turnover_channel(end, local_channel)
            normal_field = f"{end}_{local_channel}_angle_deg"
            turnover_field = f"{turnover_end}_{turnover_channel}_angle_deg"
            normal = finite_values(normal_rows, normal_field)
            turnover = finite_values(turnover_rows, turnover_field)
            if not len(normal) or not len(turnover):
                continue
            pair_count = min(len(normal), len(turnover))
            paired_residual = normal[:pair_count] + turnover[:pair_count] - 180.0
            output.append(
                {
                    "physical_end_in_head_to_tail": end,
                    "local_channel_in_head_to_tail": local_channel,
                    "mapped_device_end_in_tail_to_head": turnover_end,
                    "mapped_local_channel_in_tail_to_head": turnover_channel,
                    "expected_relation": "theta_after=180-theta_before",
                    "head_to_tail_count": len(normal),
                    "tail_to_head_count": len(turnover),
                    "head_to_tail_median_deg": float(np.median(normal)),
                    "tail_to_head_mapped_median_deg": float(np.median(turnover)),
                    "group_median_sum_residual_deg": float(np.median(normal) + np.median(turnover) - 180.0),
                    "ordinal_pair_rms_residual_deg": float(np.sqrt(np.mean(np.square(paired_residual)))),
                    "ordinal_pair_max_abs_residual_deg": float(np.max(np.abs(paired_residual))),
                    "head_to_tail_span_deg": float(np.ptp(normal)),
                    "tail_to_head_span_deg": float(np.ptp(turnover)),
                    "uses_professional_truth": False,
                    "correction_applied": False,
                }
            )
    return output


def write_csv(path: Path, rows: list[Dict[str, object]]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    fieldnames: list[str] = []
    seen: set[str] = set()
    for row in rows:
        for key in row:
            if key not in seen:
                seen.add(key)
                fieldnames.append(key)
    with path.open("w", newline="", encoding="utf-8-sig") as handle:
        writer = csv.DictWriter(handle, fieldnames=fieldnames)
        writer.writeheader()
        writer.writerows(rows)


def build_summary(
    input_root: Path,
    camera_calibration: Path,
    capture_rows: list[Dict[str, object]],
    equivariance_rows: list[Dict[str, object]],
) -> Dict[str, object]:
    residuals = np.asarray(
        [float(row["group_median_sum_residual_deg"]) for row in equivariance_rows],
        dtype=float,
    )
    summary: Dict[str, object] = {
        "algorithm": "truth_free_four_end_edges_and_four_longitudinal_corner_traces",
        "input_root": str(input_root),
        "camera_geometry_model": str(camera_calibration),
        "uses_professional_truth": False,
        "final_angle_correction_applied": False,
        "capture_count": len(capture_rows),
        "head_to_tail_count": sum(row.get("group") == "head_to_tail" for row in capture_rows),
        "tail_to_head_count": sum(row.get("group") == "tail_to_head" for row in capture_rows),
        "local_angle_count_per_capture": 16,
        "turnover_contract": {
            "ends": "head<->tail",
            "institution_faces": "A/D unchanged, B<->C",
            "A_and_D_positions": "left<->right",
            "B_and_C_positions": "top/bottom unchanged while face swaps",
            "directed_angle": "theta_after=180-theta_before",
        },
        "equivariance_group_median_rms_deg": float(np.sqrt(np.mean(np.square(residuals)))),
        "equivariance_group_median_max_abs_deg": float(np.max(np.abs(residuals))),
    }
    for group in ("head_to_tail", "tail_to_head"):
        rows = [row for row in capture_rows if row.get("group") == group]
        for end in ENDS:
            for field in (
                "endface_worst_local_error_deg",
                "endface_rms_error_from_90_deg",
                "endface_angle_span_deg",
                "wireframe_diagonal_twist_mm",
                "max_corner_closure_gap_mm",
                "max_dual_camera_seam_mid_mm",
            ):
                values = finite_values(rows, f"{end}_{field}")
                if len(values):
                    summary[f"{group}_{end}_{field}_median"] = float(np.median(values))
                    summary[f"{group}_{end}_{field}_max"] = float(np.max(values))
    return summary


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Measure 16 truth-free local end-face angles and audit physical-turnover equivariance."
    )
    parser.add_argument(
        "--input",
        type=Path,
        default=Path(r"C:\Users\Administrator\Downloads\SunshineSiRod_Endface\calibration_hobj\210_105"),
    )
    parser.add_argument(
        "--camera-calibration",
        type=Path,
        default=Path(__file__).resolve().parent / "calibration" / "models" / "camera_calibration_model_210_105.json",
    )
    parser.add_argument(
        "--output-dir",
        type=Path,
        default=Path(__file__).resolve().parent / "results" / "endface_wireframe_truth_free",
    )
    parser.add_argument("--endface-window-mm", type=float, default=15.0)
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    input_root = args.input.resolve()
    camera_calibration = args.camera_calibration.resolve()
    output_dir = args.output_dir.resolve()
    if args.endface_window_mm <= 0.0:
        raise ValueError("--endface-window-mm must be positive")
    calibration = json.loads(camera_calibration.read_text(encoding="utf-8"))
    captures: list[Tuple[Path, str]] = []
    for group in ("head_to_tail", "tail_to_head"):
        group_dir = input_root / group
        paths = sorted(group_dir.glob("*.hobj"), key=lambda path: path.name.casefold())
        if not paths:
            raise ValueError(f"No HOBJ files found in required group: {group_dir}")
        captures.extend((path, group) for path in paths)

    capture_rows: list[Dict[str, object]] = []
    for index, (path, group) in enumerate(captures, start=1):
        print(f"[{index}/{len(captures)}] {group:12s} {path.name}", flush=True)
        capture_rows.append(
            measure_capture(path, group, calibration, args.endface_window_mm)
        )
    equivariance = turnover_equivariance_rows(capture_rows)
    summary = build_summary(input_root, camera_calibration, capture_rows, equivariance)

    captures_csv = output_dir / "endface_wireframe_16_angles_per_capture.csv"
    equivariance_csv = output_dir / "endface_wireframe_turnover_equivariance.csv"
    summary_json = output_dir / "endface_wireframe_summary.json"
    write_csv(captures_csv, capture_rows)
    write_csv(equivariance_csv, equivariance)
    output_dir.mkdir(parents=True, exist_ok=True)
    summary_json.write_text(json.dumps(summary, ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"Capture CSV: {captures_csv}")
    print(f"Equivariance CSV: {equivariance_csv}")
    print(f"Summary JSON: {summary_json}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
