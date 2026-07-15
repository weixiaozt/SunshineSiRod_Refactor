#!/usr/bin/env python3
"""Audit CTB full-length camera stability, thickness, and end-face impact.

This is an offline, single-bar-only diagnostic.  It keeps the normal camera
calibration fixed for both physical poses, fits only a row-dependent local X/Z
curve, and evaluates every capture with a curve that did not use that capture.
It never reads end-face truth and never fits or applies final angle offsets.
"""

from __future__ import annotations

import argparse
import csv
import json
import math
import re
import sys
from dataclasses import dataclass
from pathlib import Path
from typing import Dict, Iterable, Optional, Tuple

import numpy as np

try:
    from .endface_wireframe_turnover_audit import (
        CAMERA_ADJACENT_CORE_FACES,
        CORE_FACE_ENDPOINTS,
        ENDS,
        LOCAL_CHANNELS,
        REPORT_FACE_CONFIG,
        fit_face_edge,
        projected_angle_deg,
        robust_ridge_direction,
        segment_for_camera_face,
        summarize_end_angles,
        turnover_equivariance_rows,
        wireframe_shape_diagnostics,
    )
    from .measure_square_rod_edges import (
        HobjSource,
        X_SCALE_MM,
        Y_SCALE_MM,
        calibration_for_orientation,
        cross_section_geometry,
        drift_fraction_for_row,
        endface_boundary_points_for_camera,
        extract_corner_from_row,
        global_face_line_observations,
        longitudinal_side_plane_normals,
        mean_cross_section_points,
        same_face_line_pair_difference,
        source_valid_ranges,
        subtract_local_drift,
    )
    from .mechanical_drift import local_shift as mechanical_drift_local_shift
    from .virtual_perfect_bar_drift_audit import (
        POINT_ORDER,
        POINT_TO_OBJECT,
        ideal_fit_residuals,
        ideal_points,
        local_delta_from_global,
        thickness_from_face_observations,
    )
except ImportError:
    sys.path.insert(0, str(Path(__file__).resolve().parent))
    from endface_wireframe_turnover_audit import (
        CAMERA_ADJACENT_CORE_FACES,
        CORE_FACE_ENDPOINTS,
        ENDS,
        LOCAL_CHANNELS,
        REPORT_FACE_CONFIG,
        fit_face_edge,
        projected_angle_deg,
        robust_ridge_direction,
        segment_for_camera_face,
        summarize_end_angles,
        turnover_equivariance_rows,
        wireframe_shape_diagnostics,
    )
    from measure_square_rod_edges import (
        HobjSource,
        X_SCALE_MM,
        Y_SCALE_MM,
        calibration_for_orientation,
        cross_section_geometry,
        drift_fraction_for_row,
        endface_boundary_points_for_camera,
        extract_corner_from_row,
        global_face_line_observations,
        longitudinal_side_plane_normals,
        mean_cross_section_points,
        same_face_line_pair_difference,
        source_valid_ranges,
        subtract_local_drift,
    )
    from mechanical_drift import local_shift as mechanical_drift_local_shift
    from virtual_perfect_bar_drift_audit import (
        POINT_ORDER,
        POINT_TO_OBJECT,
        ideal_fit_residuals,
        ideal_points,
        local_delta_from_global,
        thickness_from_face_observations,
    )


CAMERAS = (1, 2, 3, 4)
CORE_FACES = ("A", "B", "C", "D")
SAME_FACE_PAIRS = {"A": (1, 3), "B": (1, 4), "C": (2, 4), "D": (2, 3)}


@dataclass
class CaptureProfile:
    path: Path
    group: str
    fold: int
    rows: np.ndarray
    fractions: np.ndarray
    local_vertices: Dict[int, np.ndarray]
    centred_local_residuals: Dict[int, np.ndarray]
    raw_station_rows: list[Dict[str, object]]


def write_csv(path: Path, rows: list[Dict[str, object]]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    if not rows:
        path.write_text("", encoding="utf-8")
        return
    fieldnames: list[str] = []
    seen: set[str] = set()
    for row in rows:
        for key in row:
            if key not in seen:
                seen.add(key)
                fieldnames.append(key)
    with path.open("w", encoding="utf-8-sig", newline="") as handle:
        writer = csv.DictWriter(handle, fieldnames=fieldnames)
        writer.writeheader()
        writer.writerows(rows)


def finite(values: Iterable[object]) -> np.ndarray:
    output = []
    for value in values:
        try:
            number = float(value)
        except (TypeError, ValueError):
            continue
        if math.isfinite(number):
            output.append(number)
    return np.asarray(output, dtype=float)


def robust_smooth(values: np.ndarray, window: int = 5) -> np.ndarray:
    array = np.asarray(values, dtype=float)
    if len(array) < window:
        return array.copy()
    radius = window // 2
    padded = np.pad(array, ((radius, radius), (0, 0)), mode="edge")
    median = np.vstack([np.median(padded[index : index + window], axis=0) for index in range(len(array))])
    padded = np.pad(median, ((radius, radius), (0, 0)), mode="edge")
    return np.vstack([np.mean(padded[index : index + window], axis=0) for index in range(len(array))])


def read_dimensions(path: Path) -> Dict[str, float]:
    with path.open("r", encoding="utf-8-sig", newline="") as handle:
        rows = list(csv.DictReader(handle))
    row = next((item for item in rows if str(item.get("record_type", "")).strip() == "cross_section"), None)
    if row is None:
        raise ValueError(f"No cross_section row in {path}")
    dimensions = {face: float(row[f"{face}_mm"]) for face in CORE_FACES}
    if abs(dimensions["A"] - dimensions["C"]) > 0.20 or abs(dimensions["B"] - dimensions["D"]) > 0.20:
        raise ValueError("This audit requires approximately equal opposite CTB edge truths")
    return dimensions


def model_capture_key(path: Path, group: str) -> str:
    stem = re.sub(r"[A-Za-z]$", "", path.stem)
    return f"2606005B22-CTB_3/{'turnover/' if group == 'tail_to_head' else ''}{stem}"


def shared_absolute_range(
    paths: list[Tuple[Path, str]], calibration: Dict[str, object]
) -> Tuple[int, int]:
    ranges = calibration.get("common_ranges", {})
    selected = []
    for path, group in paths:
        key = model_capture_key(path, group)
        if key not in ranges:
            raise ValueError(f"Camera model has no absolute-row range for {key}")
        selected.append(tuple(int(value) for value in ranges[key]))
    start = max(value[0] for value in selected)
    end = min(value[1] for value in selected)
    if end <= start:
        raise ValueError("Six captures have no shared absolute device-row range")
    return start, end


def same_face_diagnostics(
    corners: Dict[int, object],
    transforms: Dict[int, object],
    calibration: Dict[str, object],
) -> Dict[str, float]:
    observations = global_face_line_observations(corners, transforms, calibration)
    result: Dict[str, float] = {}
    for face, pair in SAME_FACE_PAIRS.items():
        first = observations.get((pair[0], face))
        second = observations.get((pair[1], face))
        if first is None or second is None:
            continue
        seam, angle = same_face_line_pair_difference(first, second, face)
        result[f"same_face_{face}_seam_mm"] = seam
        result[f"same_face_{face}_angle_deg"] = angle
    return result


def extract_capture_profile(
    path: Path,
    group: str,
    fold: int,
    calibration: Dict[str, object],
    reference: Dict[str, np.ndarray],
    fractions: np.ndarray,
    start_row: int,
    end_row: int,
) -> CaptureProfile:
    active = calibration_for_orientation(calibration, "normal")
    transforms = {int(key): value for key, value in active["transforms"].items()}
    source = HobjSource(str(path))
    rows = np.asarray(
        sorted(set(int(round(start_row + float(fraction) * (end_row - start_row))) for fraction in fractions)),
        dtype=int,
    )
    actual_fractions = (rows.astype(float) - start_row) / float(end_row - start_row)
    vertices: Dict[int, list[np.ndarray]] = {obj: [] for obj in CAMERAS}
    residuals: Dict[int, list[np.ndarray]] = {obj: [] for obj in CAMERAS}
    station_rows: list[Dict[str, object]] = []
    for row, fraction in zip(rows, actual_fractions):
        corners = {obj: extract_corner_from_row(source.row(obj, int(row)), X_SCALE_MM) for obj in CAMERAS}
        invalid = [f"obj{obj}:{corner.reason}" for obj, corner in corners.items() if not corner.valid]
        if invalid:
            raise RuntimeError(f"{path.name} row {row}: {'; '.join(invalid)}")
        points, _, lengths = cross_section_geometry(corners, transforms, active, "free")
        point_residuals, _, fit_rms, _, _ = ideal_fit_residuals(points, reference)
        for point_name in POINT_ORDER:
            obj = POINT_TO_OBJECT[point_name]
            vertices[obj].append(np.asarray([corners[obj].vx, corners[obj].vz], dtype=float))
            residuals[obj].append(local_delta_from_global(transforms[obj], point_residuals[point_name]))
        thickness = thickness_from_face_observations(corners, transforms, active)
        station: Dict[str, object] = {
            "capture": path.name,
            "group": group,
            "fold": fold,
            "absolute_row": int(row),
            "fraction": float(fraction),
            "raw_slice_ideal_fit_rms_mm": fit_rms,
        }
        for face in CORE_FACES:
            station[f"raw_edge_{face}_mm"] = float(lengths[face])
        for key, value in thickness.items():
            station[f"raw_{key}"] = value
        station.update({f"raw_{key}": value for key, value in same_face_diagnostics(corners, transforms, active).items()})
        station_rows.append(station)
    local_vertices = {obj: np.vstack(values) for obj, values in vertices.items()}
    centred_residuals: Dict[int, np.ndarray] = {}
    for obj, values in residuals.items():
        array = np.vstack(values)
        centred_residuals[obj] = array - np.median(array, axis=0, keepdims=True)
    return CaptureProfile(
        path=path,
        group=group,
        fold=fold,
        rows=rows,
        fractions=actual_fractions,
        local_vertices=local_vertices,
        centred_local_residuals=centred_residuals,
        raw_station_rows=station_rows,
    )


def build_candidate_model(
    profiles: list[CaptureProfile],
    train_names: set[str],
    start_row: int,
    end_row: int,
    dimensions: Dict[str, float],
    model_id: str,
) -> Dict[str, object]:
    training = [profile for profile in profiles if profile.path.name in train_names]
    if len(training) < 4:
        raise ValueError("Candidate curve requires at least four training captures")
    fractions = training[0].fractions
    curves: Dict[str, Dict[str, list[float]]] = {}
    for obj in CAMERAS:
        stack = np.stack([profile.centred_local_residuals[obj] for profile in training])
        curve = robust_smooth(np.median(stack, axis=0), window=5)
        curve -= np.median(curve, axis=0, keepdims=True)
        curves[str(obj)] = {"x": curve[:, 0].tolist(), "z": curve[:, 1].tolist()}
    return {
        "version": 3,
        "model": "four_camera_longitudinal_local_drift",
        "model_id": model_id,
        "valid": False,
        "releaseable": False,
        "scope": "single_bar_only_diagnostic",
        "bar_id": "2606005B22-CTB_3",
        "specification": "210_105",
        "coordinate_stage": "local_xz_before_camera_calibration",
        "longitudinal_coordinate": "absolute_scan_row",
        "reference_common_start_row": int(start_row),
        "reference_common_end_row": int(end_row),
        "sample_fractions": fractions.tolist(),
        "drift_vectors_local_mm": curves,
        "normal_reference_local_mm": {
            str(obj): {"x": [0.0] * len(fractions), "z": [0.0] * len(fractions)} for obj in CAMERAS
        },
        "training_capture_ids": sorted(train_names),
        "dimensions_used_mm": dimensions,
        "uses_endface_truth": False,
        "forbidden_final_angle_parameters_present": False,
        "fit_definition": "median centred per-camera residual after per-slice rigid alignment to CTB dimensions",
        "warning": (
            "HOBJ alone cannot fully separate the standard bar's real bending/taper/twist from device drift. "
            "This curve is evaluated offline and must not be used as a cross-bar production correction."
        ),
    }


def shifted_corners(
    corners: Dict[int, object],
    model: Optional[Dict[str, object]],
    row: int,
    start_row: int,
    end_row: int,
) -> Dict[int, object]:
    if model is None:
        return corners
    fraction = drift_fraction_for_row(model, row, start_row, end_row)
    output = {}
    for obj, corner in corners.items():
        shift_x, shift_z = mechanical_drift_local_shift(model, obj, fraction, 1.0)
        output[obj] = subtract_local_drift(corner, shift_x, shift_z)
    return output


def add_cross_validated_station_results(
    profile: CaptureProfile,
    model: Dict[str, object],
    calibration: Dict[str, object],
    reference: Dict[str, np.ndarray],
    start_row: int,
    end_row: int,
) -> list[Dict[str, object]]:
    active = calibration_for_orientation(calibration, "normal")
    transforms = {int(key): value for key, value in active["transforms"].items()}
    source = HobjSource(str(profile.path))
    output: list[Dict[str, object]] = []
    for base in profile.raw_station_rows:
        row = int(base["absolute_row"])
        corners = {obj: extract_corner_from_row(source.row(obj, row), X_SCALE_MM) for obj in CAMERAS}
        corrected = shifted_corners(corners, model, row, start_row, end_row)
        points, _, lengths = cross_section_geometry(corrected, transforms, active, "free")
        residuals, _, fit_rms, _, _ = ideal_fit_residuals(points, reference)
        thickness = thickness_from_face_observations(corrected, transforms, active)
        item = dict(base)
        item.update({
            "validation": "paired_leave_one_fold_out",
            "correction_model_id": model["model_id"],
            "corrected_slice_ideal_fit_rms_mm": fit_rms,
        })
        for face in CORE_FACES:
            item[f"corrected_edge_{face}_mm"] = float(lengths[face])
        for key, value in thickness.items():
            item[f"corrected_{key}"] = value
        item.update(
            {f"corrected_{key}": value for key, value in same_face_diagnostics(corrected, transforms, active).items()}
        )
        item["raw_point_residual_rms_mm"] = float(
            base["raw_slice_ideal_fit_rms_mm"]
        )
        item["corrected_point_residual_rms_mm"] = float(
            np.sqrt(np.mean([float(np.linalg.norm(residuals[point])) ** 2 for point in POINT_ORDER]))
        )
        output.append(item)
    return output


def capture_summary(rows: list[Dict[str, object]], dimensions: Dict[str, float]) -> Dict[str, object]:
    first = rows[0]
    result: Dict[str, object] = {
        "capture": first["capture"],
        "group": first["group"],
        "fold": first["fold"],
        "sample_count": len(rows),
        "validation": first["validation"],
        "correction_model_id": first["correction_model_id"],
    }
    for state in ("raw", "corrected"):
        fit = finite(row[f"{state}_slice_ideal_fit_rms_mm"] for row in rows)
        result[f"{state}_slice_fit_rms_median_mm"] = float(np.median(fit))
        result[f"{state}_slice_fit_rms_p95_mm"] = float(np.quantile(fit, 0.95))
        thickness_specs = {
            "thickness_ac_mm": 0.5 * (dimensions["B"] + dimensions["D"]),
            "thickness_bd_mm": 0.5 * (dimensions["A"] + dimensions["C"]),
        }
        for field, truth in thickness_specs.items():
            values = finite(row.get(f"{state}_{field}") for row in rows)
            error = values - truth
            result[f"{state}_{field}_median"] = float(np.median(values))
            result[f"{state}_{field}_span_mm"] = float(np.ptp(values))
            result[f"{state}_{field}_error_rms_mm"] = float(np.sqrt(np.mean(np.square(error))))
            result[f"{state}_{field}_error_max_abs_mm"] = float(np.max(np.abs(error)))
    return result


def reconstruct_longitudinal_rows(
    source: HobjSource,
    calibration: Dict[str, object],
    common_start: int,
    common_end: int,
    model: Optional[Dict[str, object]],
) -> list[Dict[str, object]]:
    active = calibration_for_orientation(calibration, "normal")
    transforms = {int(key): value for key, value in active["transforms"].items()}
    output: list[Dict[str, object]] = []
    for fraction in np.linspace(0.1, 0.9, 9):
        row = int(round(common_start + float(fraction) * (common_end - common_start)))
        corners = {obj: extract_corner_from_row(source.row(obj, row), X_SCALE_MM) for obj in CAMERAS}
        if not all(corner.valid for corner in corners.values()):
            continue
        corners = shifted_corners(corners, model, row, common_start, common_end)
        points, _, _ = cross_section_geometry(corners, transforms, active, "free")
        item: Dict[str, object] = {
            "record": "slice",
            "valid": True,
            "y_mm": (row - common_start) * Y_SCALE_MM,
        }
        for point_name, (x, z) in points.items():
            item[f"{point_name}_x"] = x
            item[f"{point_name}_z"] = z
        output.append(item)
    if len(output) < 3:
        raise ValueError("Fewer than three corrected longitudinal slices")
    return output


def measure_wireframe(
    profile: CaptureProfile,
    calibration: Dict[str, object],
    valid_ranges: Dict[int, Tuple[int, int]],
    common_start: int,
    common_end: int,
    endface_window_mm: float,
    model: Optional[Dict[str, object]],
) -> Dict[str, object]:
    active = calibration_for_orientation(calibration, "normal")
    transforms = {int(key): value for key, value in active["transforms"].items()}
    source = HobjSource(str(profile.path))
    rows = reconstruct_longitudinal_rows(source, calibration, common_start, common_end, model)
    section_points = mean_cross_section_points(rows)
    side_normals = longitudinal_side_plane_normals(rows)
    ridges = {point: robust_ridge_direction(rows, point) for point in POINT_ORDER}
    result: Dict[str, object] = {
        "group": profile.group,
        "capture": profile.path.name,
        "fold": profile.fold,
        "uses_endface_truth": False,
        "final_angle_correction_applied": False,
        "row_dependent_local_xz_correction_applied": model is not None,
        "correction_model_id": "none" if model is None else model["model_id"],
    }
    for end in ENDS:
        clouds = {
            obj: endface_boundary_points_for_camera(
                source,
                obj,
                end,
                valid_ranges[obj],
                transforms[obj],
                active,
                common_start,
                X_SCALE_MM,
                Y_SCALE_MM,
                endface_window_mm,
                common_end=common_end,
                drift_model=model,
                drift_amplitude=1.0 if model is not None else 0.0,
            )
            for obj in CAMERAS
        }
        segments = {face: [] for face in CORE_FACE_ENDPOINTS}
        for obj, cloud in clouds.items():
            adjacent = CAMERA_ADJACENT_CORE_FACES[obj]
            for face in adjacent:
                segments[face].append(segment_for_camera_face(obj, cloud, face, adjacent, section_points))
        fits = {
            face: fit_face_edge(face, values, section_points, side_normals[face])
            for face, values in segments.items()
        }
        angles: Dict[str, float] = {}
        for report_face, config in REPORT_FACE_CONFIG.items():
            core_face = str(config["core_face"])
            endpoints = tuple(config["endpoints"])
            positions = tuple(config["positions"])
            fit = fits[core_face]
            for index, (point_name, position) in enumerate(zip(endpoints, positions)):
                edge_ray = -fit.direction if index == 0 else fit.direction
                channel = f"{report_face}_{position}"
                angle = projected_angle_deg(edge_ray, ridges[point_name], side_normals[core_face])
                angles[channel] = angle
                result[f"{end}_{channel}_angle_deg"] = angle
            result[f"{end}_{report_face}_dual_camera_seam_mid_mm"] = fit.seam_at_face_mid_mm
        for key, value in summarize_end_angles(angles).items():
            result[f"{end}_endface_{key}"] = value
        for key, value in wireframe_shape_diagnostics(fits, section_points).items():
            result[f"{end}_{key}"] = value
    return result


def endface_comparison_rows(
    raw_rows: list[Dict[str, object]], corrected_rows: list[Dict[str, object]]
) -> Tuple[list[Dict[str, object]], list[Dict[str, object]]]:
    corrected_by_capture = {str(row["capture"]): row for row in corrected_rows}
    wide: list[Dict[str, object]] = []
    delta: list[Dict[str, object]] = []
    for raw in raw_rows:
        corrected = corrected_by_capture[str(raw["capture"])]
        item: Dict[str, object] = {
            "capture": raw["capture"],
            "group": raw["group"],
            "fold": raw["fold"],
            "validation": "paired_leave_one_fold_out",
            "correction_model_id": corrected["correction_model_id"],
            "uses_endface_truth": False,
            "final_angle_correction_applied": False,
        }
        capture_deltas = []
        for end in ENDS:
            for channel in LOCAL_CHANNELS:
                field = f"{end}_{channel}_angle_deg"
                raw_value = float(raw[field])
                corrected_value = float(corrected[field])
                difference = corrected_value - raw_value
                item[f"raw_{field}"] = raw_value
                item[f"corrected_{field}"] = corrected_value
                item[f"delta_{field}"] = difference
                capture_deltas.append(abs(difference))
                delta.append({
                    "capture": raw["capture"],
                    "group": raw["group"],
                    "fold": raw["fold"],
                    "end": end,
                    "local_channel": channel,
                    "raw_angle_deg": raw_value,
                    "cross_validated_corrected_angle_deg": corrected_value,
                    "corrected_minus_raw_deg": difference,
                    "abs_impact_deg": abs(difference),
                    "uses_endface_truth": False,
                    "final_angle_correction_applied": False,
                })
        item["median_abs_16_angle_impact_deg"] = float(np.median(capture_deltas))
        item["max_abs_16_angle_impact_deg"] = float(np.max(capture_deltas))
        wide.append(item)
    return wide, delta


def camera_stability_rows(profiles: list[CaptureProfile]) -> list[Dict[str, object]]:
    output: list[Dict[str, object]] = []
    for group in ("head_to_tail", "tail_to_head"):
        selected = [profile for profile in profiles if profile.group == group]
        for obj in CAMERAS:
            curves = np.stack(
                [profile.local_vertices[obj] - profile.local_vertices[obj][0] for profile in selected]
            )
            median_curve = np.median(curves, axis=0)
            dispersion = np.linalg.norm(curves - median_curve, axis=2)
            head_tail = curves[:, -1] - curves[:, 0]
            head_tail_norm = np.linalg.norm(head_tail, axis=1)
            repeat_p95 = float(np.quantile(dispersion, 0.95))
            amplitude = float(np.linalg.norm(np.median(head_tail, axis=0)))
            output.append({
                "group": group,
                "camera": obj,
                "capture_count": len(selected),
                "observed_head_to_tail_x_mm": float(np.median(head_tail[:, 0])),
                "observed_head_to_tail_z_mm": float(np.median(head_tail[:, 1])),
                "observed_head_to_tail_vector_mm": amplitude,
                "head_to_tail_vector_span_mm": float(np.ptp(head_tail_norm)),
                "repeatability_p95_mm": repeat_p95,
                "repeatability_max_mm": float(np.max(dispersion)),
                "systematic_to_repeatability_ratio": amplitude / max(repeat_p95, 1e-12),
                "systematic_evidence": amplitude >= 3.0 * max(repeat_p95, 0.02),
                "interpretation": "camera-observed trajectory; still contains the physical CTB longitudinal shape",
            })
    return output


def same_face_seam_summary_rows(station_rows: list[Dict[str, object]]) -> list[Dict[str, object]]:
    output: list[Dict[str, object]] = []
    captures = sorted({str(row["capture"]) for row in station_rows})
    for capture in captures:
        rows = [row for row in station_rows if row["capture"] == capture]
        for face in CORE_FACES:
            item: Dict[str, object] = {
                "capture": capture,
                "group": rows[0]["group"],
                "fold": rows[0]["fold"],
                "face": face,
                "validation": rows[0]["validation"],
            }
            for state in ("raw", "corrected"):
                values = finite(row[f"{state}_same_face_{face}_seam_mm"] for row in rows)
                item[f"{state}_seam_median_mm"] = float(np.median(values))
                item[f"{state}_seam_span_mm"] = float(np.ptp(values))
                item[f"{state}_seam_rms_mm"] = float(np.sqrt(np.mean(np.square(values))))
            output.append(item)
    return output


def anomaly_rows(profiles: list[CaptureProfile]) -> list[Dict[str, object]]:
    output: list[Dict[str, object]] = []
    for group in ("head_to_tail", "tail_to_head"):
        selected = [profile for profile in profiles if profile.group == group]
        stack = np.stack(
            [np.stack([profile.centred_local_residuals[obj] for obj in CAMERAS]) for profile in selected]
        )
        centre = np.median(stack, axis=0)
        scores = np.sqrt(np.mean(np.square(stack - centre), axis=(1, 2, 3)))
        median = float(np.median(scores))
        mad = float(np.median(np.abs(scores - median)))
        limit = max(0.15, median + 6.0 * 1.4826 * mad)
        for profile, score in zip(selected, scores):
            output.append({
                "capture": profile.path.name,
                "group": group,
                "image_geometry_deviation_rms_mm": float(score),
                "group_robust_limit_mm": limit,
                "abnormal": bool(score > limit),
                "decision_basis": "truth-free deviation from same-pose median centred camera curves",
            })
    return output


def svg_polyline(
    values_x: Iterable[float],
    values_y: Iterable[float],
    box: Tuple[float, float, float, float],
    x_limits: Tuple[float, float],
    y_limits: Tuple[float, float],
    color: str,
    opacity: float = 1.0,
    width: float = 2.0,
) -> str:
    left, top, plot_width, plot_height = box
    x0, x1 = x_limits
    y0, y1 = y_limits
    points = []
    for x_value, y_value in zip(values_x, values_y):
        x = left + (float(x_value) - x0) / max(x1 - x0, 1e-12) * plot_width
        y = top + plot_height - (float(y_value) - y0) / max(y1 - y0, 1e-12) * plot_height
        points.append(f"{x:.2f},{y:.2f}")
    return (
        f'<polyline points="{" ".join(points)}" fill="none" stroke="{color}" '
        f'stroke-width="{width:.2f}" opacity="{opacity:.3f}" />'
    )


def save_svg_plots(
    output_dir: Path,
    profiles: list[CaptureProfile],
    station_rows: list[Dict[str, object]],
    endface_wide: list[Dict[str, object]],
    dimensions: Dict[str, float],
) -> list[str]:
    created: list[str] = []
    group_colors = {"head_to_tail": "#1976d2", "tail_to_head": "#d32f2f"}
    svg = [
        '<svg xmlns="http://www.w3.org/2000/svg" width="1200" height="820" viewBox="0 0 1200 820">',
        '<rect width="1200" height="820" fill="#ffffff"/>',
        '<text x="600" y="34" text-anchor="middle" font-size="22" font-family="sans-serif">CTB camera-observed longitudinal trajectories</text>',
    ]
    panel_boxes = [(80, 80, 470, 290), (650, 80, 470, 290), (80, 455, 470, 290), (650, 455, 470, 290)]
    for obj, box in zip(CAMERAS, panel_boxes):
        left, top, width, height = box
        all_magnitude = []
        curves_by_group = {}
        for group in group_colors:
            selected = [profile for profile in profiles if profile.group == group]
            curves = np.stack([profile.local_vertices[obj] - profile.local_vertices[obj][0] for profile in selected])
            magnitude = np.linalg.norm(curves, axis=2)
            curves_by_group[group] = (selected, magnitude)
            all_magnitude.extend(magnitude.reshape(-1).tolist())
        upper = max(0.05, max(all_magnitude) * 1.08)
        svg.extend([
            f'<rect x="{left}" y="{top}" width="{width}" height="{height}" fill="#fafafa" stroke="#bdbdbd"/>',
            f'<text x="{left + 8}" y="{top + 22}" font-size="17" font-family="sans-serif">obj{obj}</text>',
            f'<text x="{left + width / 2}" y="{top + height + 28}" text-anchor="middle" font-size="13" font-family="sans-serif">absolute device row (%)</text>',
        ])
        for group, (selected, magnitude) in curves_by_group.items():
            x_values = selected[0].fractions * 100.0
            for curve in magnitude:
                svg.append(svg_polyline(x_values, curve, box, (1.0, 99.0), (0.0, upper), group_colors[group], 0.22, 1.2))
            svg.append(
                svg_polyline(
                    x_values,
                    np.median(magnitude, axis=0),
                    box,
                    (1.0, 99.0),
                    (0.0, upper),
                    group_colors[group],
                    1.0,
                    2.8,
                )
            )
        svg.append(f'<text x="{left - 8}" y="{top + 14}" text-anchor="end" font-size="12" font-family="sans-serif">{upper:.2f} mm</text>')
    svg.extend([
        '<line x1="420" y1="790" x2="455" y2="790" stroke="#1976d2" stroke-width="3"/><text x="465" y="795" font-size="14" font-family="sans-serif">head_to_tail</text>',
        '<line x1="650" y1="790" x2="685" y2="790" stroke="#d32f2f" stroke-width="3"/><text x="695" y="795" font-size="14" font-family="sans-serif">tail_to_head</text>',
        '</svg>',
    ])
    path = output_dir / "01_camera_observed_trajectories.svg"
    path.write_text("\n".join(svg), encoding="utf-8")
    created.append(str(path))

    capture_names = sorted({str(row["capture"]) for row in station_rows})
    palette = ["#1565c0", "#00897b", "#7b1fa2", "#ef6c00", "#c62828", "#455a64"]
    panels = [
        ("thickness_bd_mm", 0.5 * (dimensions["A"] + dimensions["C"]), "B-D thickness -> A/C length"),
        ("thickness_ac_mm", 0.5 * (dimensions["B"] + dimensions["D"]), "A-C thickness -> B/D length"),
    ]
    svg = [
        '<svg xmlns="http://www.w3.org/2000/svg" width="1200" height="820" viewBox="0 0 1200 820">',
        '<rect width="1200" height="820" fill="#ffffff"/>',
        '<text x="600" y="34" text-anchor="middle" font-size="22" font-family="sans-serif">Raw faint; paired holdout corrected solid</text>',
    ]
    for panel_index, (field, truth, title) in enumerate(panels):
        box = (90.0, 85.0 + panel_index * 365.0, 1020.0, 275.0)
        values = []
        for row in station_rows:
            values.extend([float(row[f"raw_{field}"]), float(row[f"corrected_{field}"])])
        lower = min(values + [truth]) - 0.03
        upper = max(values + [truth]) + 0.03
        left, top, width, height = box
        svg.extend([
            f'<rect x="{left}" y="{top}" width="{width}" height="{height}" fill="#fafafa" stroke="#bdbdbd"/>',
            f'<text x="{left + 8}" y="{top + 22}" font-size="17" font-family="sans-serif">{title}</text>',
            f'<text x="{left - 8}" y="{top + 14}" text-anchor="end" font-size="12" font-family="sans-serif">{upper:.3f}</text>',
            f'<text x="{left - 8}" y="{top + height}" text-anchor="end" font-size="12" font-family="sans-serif">{lower:.3f}</text>',
            svg_polyline([1.0, 99.0], [truth, truth], box, (1.0, 99.0), (lower, upper), "#111111", 1.0, 2.0),
        ])
        for color, capture in zip(palette, capture_names):
            rows = [row for row in station_rows if row["capture"] == capture]
            x_values = [float(row["fraction"]) * 100.0 for row in rows]
            svg.append(svg_polyline(x_values, [float(row[f"raw_{field}"]) for row in rows], box, (1.0, 99.0), (lower, upper), color, 0.22, 1.1))
            svg.append(svg_polyline(x_values, [float(row[f"corrected_{field}"]) for row in rows], box, (1.0, 99.0), (lower, upper), color, 0.95, 1.8))
    svg.append('</svg>')
    path = output_dir / "02_thickness_raw_vs_cross_validated.svg"
    path.write_text("\n".join(svg), encoding="utf-8")
    created.append(str(path))

    width, height = 1100, 560
    left, top, plot_width, plot_height = 90.0, 80.0, 940.0, 390.0
    labels = [str(row["capture"]) for row in endface_wide]
    medians = [float(row["median_abs_16_angle_impact_deg"]) for row in endface_wide]
    maxima = [float(row["max_abs_16_angle_impact_deg"]) for row in endface_wide]
    upper = max(maxima) * 1.18
    svg = [
        f'<svg xmlns="http://www.w3.org/2000/svg" width="{width}" height="{height}" viewBox="0 0 {width} {height}">',
        f'<rect width="{width}" height="{height}" fill="#ffffff"/>',
        '<text x="550" y="34" text-anchor="middle" font-size="22" font-family="sans-serif">Row-dependent drift influence on 16 end-face angles</text>',
        f'<rect x="{left}" y="{top}" width="{plot_width}" height="{plot_height}" fill="#fafafa" stroke="#bdbdbd"/>',
    ]
    group_width = plot_width / len(labels)
    for index, (label, median, maximum) in enumerate(zip(labels, medians, maxima)):
        centre = left + group_width * (index + 0.5)
        for offset, value, color in ((-18.0, median, "#1976d2"), (18.0, maximum, "#ef6c00")):
            bar_height = value / max(upper, 1e-12) * plot_height
            svg.append(f'<rect x="{centre + offset - 14:.2f}" y="{top + plot_height - bar_height:.2f}" width="28" height="{bar_height:.2f}" fill="{color}"/>')
        svg.append(f'<text x="{centre:.2f}" y="{top + plot_height + 28}" text-anchor="middle" font-size="13" font-family="sans-serif">{label}</text>')
    svg.extend([
        f'<text x="{left - 8}" y="{top + 10}" text-anchor="end" font-size="12" font-family="sans-serif">{upper:.3f}°</text>',
        '<rect x="390" y="520" width="18" height="12" fill="#1976d2"/><text x="416" y="531" font-size="14" font-family="sans-serif">median impact</text>',
        '<rect x="600" y="520" width="18" height="12" fill="#ef6c00"/><text x="626" y="531" font-size="14" font-family="sans-serif">maximum impact</text>',
        '</svg>',
    ])
    path = output_dir / "03_endface_16_angle_impact.svg"
    path.write_text("\n".join(svg), encoding="utf-8")
    created.append(str(path))
    return created


def save_plots(
    output_dir: Path,
    profiles: list[CaptureProfile],
    station_rows: list[Dict[str, object]],
    endface_wide: list[Dict[str, object]],
    dimensions: Dict[str, float],
) -> list[str]:
    try:
        import matplotlib.pyplot as plt
    except ImportError:
        return save_svg_plots(output_dir, profiles, station_rows, endface_wide, dimensions)
    created: list[str] = []
    colors = {"head_to_tail": "#1f77b4", "tail_to_head": "#d62728"}
    fig, axes = plt.subplots(2, 2, figsize=(12, 8), sharex=True)
    for obj, axis in zip(CAMERAS, axes.flat):
        for group in colors:
            selected = [profile for profile in profiles if profile.group == group]
            curves = np.stack([profile.local_vertices[obj] - profile.local_vertices[obj][0] for profile in selected])
            median = np.median(curves, axis=0)
            magnitude = np.linalg.norm(median, axis=1)
            axis.plot(selected[0].fractions * 100.0, magnitude, color=colors[group], label=group)
            for curve in curves:
                axis.plot(selected[0].fractions * 100.0, np.linalg.norm(curve, axis=1), color=colors[group], alpha=0.18)
        axis.set_title(f"Camera obj{obj}")
        axis.set_ylabel("Observed displacement (mm)")
        axis.grid(alpha=0.25)
    axes[-1, 0].set_xlabel("Absolute device row (%)")
    axes[-1, 1].set_xlabel("Absolute device row (%)")
    axes[0, 0].legend()
    fig.suptitle("CTB camera-observed longitudinal trajectories")
    fig.tight_layout()
    path = output_dir / "01_camera_observed_trajectories.png"
    fig.savefig(path, dpi=160)
    plt.close(fig)
    created.append(str(path))

    fig, axes = plt.subplots(2, 1, figsize=(12, 8), sharex=True)
    for capture in sorted({str(row["capture"]) for row in station_rows}):
        rows = [row for row in station_rows if row["capture"] == capture]
        x = [float(row["fraction"]) * 100.0 for row in rows]
        axes[0].plot(x, [float(row["raw_thickness_bd_mm"]) for row in rows], alpha=0.35)
        axes[0].plot(x, [float(row["corrected_thickness_bd_mm"]) for row in rows], linewidth=1.6)
        axes[1].plot(x, [float(row["raw_thickness_ac_mm"]) for row in rows], alpha=0.35)
        axes[1].plot(x, [float(row["corrected_thickness_ac_mm"]) for row in rows], linewidth=1.6)
    axes[0].axhline(0.5 * (dimensions["A"] + dimensions["C"]), color="black", linestyle="--", label="CTB truth")
    axes[1].axhline(0.5 * (dimensions["B"] + dimensions["D"]), color="black", linestyle="--", label="CTB truth")
    axes[0].set_ylabel("B-D thickness -> A/C (mm)")
    axes[1].set_ylabel("A-C thickness -> B/D (mm)")
    axes[1].set_xlabel("Absolute device row (%)")
    for axis in axes:
        axis.grid(alpha=0.25)
        axis.legend()
    fig.suptitle("Raw faint lines; cross-validated corrected solid lines")
    fig.tight_layout()
    path = output_dir / "02_thickness_raw_vs_cross_validated.png"
    fig.savefig(path, dpi=160)
    plt.close(fig)
    created.append(str(path))

    labels = [str(row["capture"]) for row in endface_wide]
    median = [float(row["median_abs_16_angle_impact_deg"]) for row in endface_wide]
    maximum = [float(row["max_abs_16_angle_impact_deg"]) for row in endface_wide]
    x = np.arange(len(labels))
    fig, axis = plt.subplots(figsize=(11, 5))
    axis.bar(x - 0.18, median, width=0.36, label="median impact")
    axis.bar(x + 0.18, maximum, width=0.36, label="max impact")
    axis.set_xticks(x, labels, rotation=20)
    axis.set_ylabel("abs(corrected - raw), degrees")
    axis.set_title("CTB row-dependent drift influence on 16 end-face angles")
    axis.grid(axis="y", alpha=0.25)
    axis.legend()
    fig.tight_layout()
    path = output_dir / "03_endface_16_angle_impact.png"
    fig.savefig(path, dpi=160)
    plt.close(fig)
    created.append(str(path))
    return created


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Audit CTB long-bar drift, thickness, and 16 end-face angles")
    parser.add_argument(
        "--input",
        type=Path,
        default=Path(__file__).resolve().parent / "calibration" / "hobj" / "2606005B22-CTB_3_length",
    )
    parser.add_argument(
        "--camera-calibration",
        type=Path,
        default=Path(__file__).resolve().parent / "calibration" / "models" / "camera_calibration_model_210_105.json",
    )
    parser.add_argument("--truth-csv", type=Path, default=None)
    parser.add_argument(
        "--output-dir",
        type=Path,
        default=Path(__file__).resolve().parent / "results" / "ctb_length_drift_thickness_endface_audit",
    )
    parser.add_argument("--sample-count", type=int, default=61)
    parser.add_argument("--endface-window-mm", type=float, default=15.0)
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    input_dir = args.input.resolve()
    output_dir = args.output_dir.resolve()
    truth_csv = args.truth_csv.resolve() if args.truth_csv else input_dir / "CTB-长度数据.csv"
    paths = sorted(input_dir.glob("*.hobj"), key=lambda path: path.name.casefold())
    if len(paths) != 6:
        raise ValueError(f"Expected exactly six CTB HOBJ captures, found {len(paths)}")
    calibration = json.loads(args.camera_calibration.read_text(encoding="utf-8"))
    metadata = calibration.get("single_bar_metadata", {})
    if metadata.get("physical_bar_id") != "2606005B22-CTB_3":
        raise ValueError("Camera model is not the CTB single-bar model")
    dimensions = read_dimensions(truth_csv)
    long_edge = 0.5 * (dimensions["A"] + dimensions["C"])
    short_edge = 0.5 * (dimensions["B"] + dimensions["D"])
    reference = ideal_points(long_edge, short_edge)
    captures: list[Tuple[Path, str, int]] = []
    for index, path in enumerate(paths):
        group = "head_to_tail" if index < 3 else "tail_to_head"
        captures.append((path, group, index % 3))
    start_row, end_row = shared_absolute_range([(path, group) for path, group, _ in captures], calibration)
    fractions = np.linspace(0.01, 0.99, max(11, int(args.sample_count)))

    profiles: list[CaptureProfile] = []
    for index, (path, group, fold) in enumerate(captures, start=1):
        print(f"[{index}/6] full-length profile {group:12s} {path.name}", flush=True)
        profiles.append(
            extract_capture_profile(path, group, fold, calibration, reference, fractions, start_row, end_row)
        )

    anomaly = anomaly_rows(profiles)
    abnormal_names = {str(row["capture"]) for row in anomaly if row["abnormal"]}
    if abnormal_names:
        raise RuntimeError(f"Image-geometry anomaly screen rejected: {sorted(abnormal_names)}")

    fold_models: Dict[int, Dict[str, object]] = {}
    station_rows: list[Dict[str, object]] = []
    for fold in range(3):
        holdout = {profile.path.name for profile in profiles if profile.fold == fold}
        train = {profile.path.name for profile in profiles if profile.fold != fold}
        model = build_candidate_model(
            profiles, train, start_row, end_row, dimensions, f"ctb_pair_fold_{fold + 1}_train4_holdout2"
        )
        fold_models[fold] = model
        for profile in profiles:
            if profile.fold == fold:
                station_rows.extend(
                    add_cross_validated_station_results(
                        profile, model, calibration, reference, start_row, end_row
                    )
                )

    capture_summaries = []
    for profile in profiles:
        rows = [row for row in station_rows if row["capture"] == profile.path.name]
        capture_summaries.append(capture_summary(rows, dimensions))

    raw_endface: list[Dict[str, object]] = []
    corrected_endface: list[Dict[str, object]] = []
    for index, profile in enumerate(profiles, start=1):
        print(f"[{index}/6] end-face raw/cv {profile.group:12s} {profile.path.name}", flush=True)
        source = HobjSource(str(profile.path))
        valid_ranges = source_valid_ranges(source)
        common_start = max(value[0] for value in valid_ranges.values())
        common_end = min(value[1] for value in valid_ranges.values())
        raw_endface.append(
            measure_wireframe(
                profile,
                calibration,
                valid_ranges,
                common_start,
                common_end,
                args.endface_window_mm,
                None,
            )
        )
        corrected_endface.append(
            measure_wireframe(
                profile,
                calibration,
                valid_ranges,
                common_start,
                common_end,
                args.endface_window_mm,
                fold_models[profile.fold],
            )
        )

    endface_wide, endface_delta = endface_comparison_rows(raw_endface, corrected_endface)
    raw_equivariance = turnover_equivariance_rows(raw_endface)
    corrected_equivariance = turnover_equivariance_rows(corrected_endface)
    for row in raw_equivariance:
        row["measurement_state"] = "raw"
    for row in corrected_equivariance:
        row["measurement_state"] = "paired_leave_one_fold_out_corrected"
    equivariance = raw_equivariance + corrected_equivariance

    final_model = build_candidate_model(
        profiles,
        {profile.path.name for profile in profiles},
        start_row,
        end_row,
        dimensions,
        "ctb_all_six_diagnostic_only_not_releaseable",
    )
    output_dir.mkdir(parents=True, exist_ok=True)
    model_path = output_dir / "ctb_single_bar_candidate_drift_model_diagnostic_only.json"
    model_path.write_text(json.dumps(final_model, ensure_ascii=False, indent=2), encoding="utf-8")

    stability = camera_stability_rows(profiles)
    seam_summary = same_face_seam_summary_rows(station_rows)
    write_csv(output_dir / "ctb_capture_anomaly_screen.csv", anomaly)
    write_csv(output_dir / "ctb_camera_systematic_drift.csv", stability)
    write_csv(output_dir / "ctb_same_face_seam_summary.csv", seam_summary)
    write_csv(output_dir / "ctb_station_thickness_cross_validation.csv", station_rows)
    write_csv(output_dir / "ctb_capture_cross_validation_summary.csv", capture_summaries)
    write_csv(output_dir / "ctb_endface_16_angles_raw_and_cv_corrected.csv", endface_wide)
    write_csv(output_dir / "ctb_endface_angle_deltas.csv", endface_delta)
    write_csv(output_dir / "ctb_endface_turnover_equivariance.csv", equivariance)
    plots = save_plots(output_dir, profiles, station_rows, endface_wide, dimensions)

    raw_thickness_rms = finite(
        row["raw_thickness_bd_mm_error_rms_mm"] for row in capture_summaries
    )
    corrected_thickness_rms = finite(
        row["corrected_thickness_bd_mm_error_rms_mm"] for row in capture_summaries
    )
    raw_short_thickness_rms = finite(
        row["raw_thickness_ac_mm_error_rms_mm"] for row in capture_summaries
    )
    corrected_short_thickness_rms = finite(
        row["corrected_thickness_ac_mm_error_rms_mm"] for row in capture_summaries
    )
    angle_impacts = finite(row["abs_impact_deg"] for row in endface_delta)
    raw_eq = finite(row["group_median_sum_residual_deg"] for row in raw_equivariance)
    corrected_eq = finite(row["group_median_sum_residual_deg"] for row in corrected_equivariance)
    summary = {
        "algorithm": "ctb_single_bar_absolute_row_paired_leave_one_fold_out_audit",
        "input_dir": str(input_dir),
        "camera_calibration": str(args.camera_calibration.resolve()),
        "truth_csv": str(truth_csv),
        "capture_count": len(profiles),
        "head_to_tail_count": 3,
        "tail_to_head_count": 3,
        "shared_absolute_row_range": [start_row, end_row],
        "ctb_dimensions_mm": dimensions,
        "abnormal_capture_count": len(abnormal_names),
        "systematic_camera_group_evidence_count": sum(
            bool(row["systematic_evidence"]) for row in stability
        ),
        "camera_group_combination_count": len(stability),
        "all_eight_camera_group_combinations_show_systematic_evidence": all(
            bool(row["systematic_evidence"]) for row in stability
        ),
        "raw_long_dimension_thickness_error_rms_mean_mm": float(np.mean(raw_thickness_rms)),
        "cv_corrected_long_dimension_thickness_error_rms_mean_mm": float(np.mean(corrected_thickness_rms)),
        "raw_short_dimension_thickness_error_rms_mean_mm": float(
            np.mean(raw_short_thickness_rms)
        ),
        "cv_corrected_short_dimension_thickness_error_rms_mean_mm": float(
            np.mean(corrected_short_thickness_rms)
        ),
        "all_six_holdout_captures_improved_both_thickness_axes": all(
            float(row["corrected_thickness_bd_mm_error_rms_mm"])
            < float(row["raw_thickness_bd_mm_error_rms_mm"])
            and float(row["corrected_thickness_ac_mm_error_rms_mm"])
            < float(row["raw_thickness_ac_mm_error_rms_mm"])
            for row in capture_summaries
        ),
        "raw_same_face_seam_span_median_mm": float(
            np.median(finite(row["raw_seam_span_mm"] for row in seam_summary))
        ),
        "raw_same_face_seam_span_max_mm": float(
            np.max(finite(row["raw_seam_span_mm"] for row in seam_summary))
        ),
        "endface_16_angle_abs_impact_median_deg": float(np.median(angle_impacts)),
        "endface_16_angle_abs_impact_p95_deg": float(np.quantile(angle_impacts, 0.95)),
        "endface_16_angle_abs_impact_max_deg": float(np.max(angle_impacts)),
        "raw_turnover_equivariance_rms_deg": float(np.sqrt(np.mean(np.square(raw_eq)))),
        "cv_corrected_turnover_equivariance_rms_deg": float(
            np.sqrt(np.mean(np.square(corrected_eq)))
        ),
        "turnover_equivariance_rms_improvement_deg": float(
            np.sqrt(np.mean(np.square(raw_eq))) - np.sqrt(np.mean(np.square(corrected_eq)))
        ),
        "candidate_model_releaseable": False,
        "candidate_model_path": str(model_path),
        "release_blocker": (
            "Same-bar HOBJ and constant A/B/C/D truths do not fully identify device drift separately "
            "from real CTB bending, taper, twist, and end slope."
        ),
        "plots": plots,
    }
    summary_path = output_dir / "ctb_audit_summary.json"
    summary_path.write_text(json.dumps(summary, ensure_ascii=False, indent=2), encoding="utf-8")

    print(f"Audit output: {output_dir}")
    print(
        "Long-thickness error RMS mean: "
        f"raw={summary['raw_long_dimension_thickness_error_rms_mean_mm']:.6f} mm, "
        f"cv={summary['cv_corrected_long_dimension_thickness_error_rms_mean_mm']:.6f} mm"
    )
    print(
        "End-face 16-angle drift impact: "
        f"median={summary['endface_16_angle_abs_impact_median_deg']:.6f} deg, "
        f"p95={summary['endface_16_angle_abs_impact_p95_deg']:.6f} deg, "
        f"max={summary['endface_16_angle_abs_impact_max_deg']:.6f} deg"
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
