#!/usr/bin/env python3
"""Audit a virtual perfect-bar drift field without changing production results.

This tool treats a 210 x 105 rectangle as a diagnostic reference, estimates a
repeatable per-camera local X/Z drift curve on absolute device rows, and reports
raw/corrected self-consistency.  It never writes a production calibration model
and never applies final A/B/C/D or end-face answer offsets.
"""

from __future__ import annotations

import argparse
import csv
import json
import math
import os
import sys
from dataclasses import dataclass
from pathlib import Path
from typing import Dict, Iterable, Optional, Tuple

import numpy as np

try:
    from .measure_square_rod_edges import (
        FACE_OUTWARD_NORMALS,
        HobjSource,
        OBJECT_TO_POINT,
        SAME_FACE_CAMERA_PAIRS,
        X_SCALE_MM,
        Y_SCALE_MM,
        calibration_for_orientation,
        cross_section_geometry,
        extract_corner_from_row,
        global_face_line_observations,
        source_common_range,
        source_valid_ranges,
        subtract_local_drift,
    )
except ImportError:
    sys.path.insert(0, str(Path(__file__).resolve().parent))
    from measure_square_rod_edges import (
        FACE_OUTWARD_NORMALS,
        HobjSource,
        OBJECT_TO_POINT,
        SAME_FACE_CAMERA_PAIRS,
        X_SCALE_MM,
        Y_SCALE_MM,
        calibration_for_orientation,
        cross_section_geometry,
        extract_corner_from_row,
        global_face_line_observations,
        source_common_range,
        source_valid_ranges,
        subtract_local_drift,
    )


POINT_ORDER = ("P1", "P2", "P3", "P4")
POINT_TO_OBJECT = {point: obj for obj, point in OBJECT_TO_POINT.items()}
GROUPS = (("head_to_tail", "normal"), ("tail_to_head", "turnover"))


@dataclass
class DriftField:
    group: str
    orientation: str
    start_row: int
    end_row: int
    fractions: np.ndarray
    local_shift: Dict[int, np.ndarray]
    sample_count: Dict[Tuple[int, float], int]


def ideal_points(long_edge_mm: float, short_edge_mm: float) -> Dict[str, np.ndarray]:
    return {
        "P1": np.array([long_edge_mm, short_edge_mm], dtype=float),
        "P2": np.array([0.0, 0.0], dtype=float),
        "P3": np.array([0.0, short_edge_mm], dtype=float),
        "P4": np.array([long_edge_mm, 0.0], dtype=float),
    }


def rigid_align_2d(source_points: np.ndarray, target_points: np.ndarray) -> Tuple[np.ndarray, np.ndarray]:
    """Return rotation/translation that maps source points to target points."""

    source = np.asarray(source_points, dtype=float)
    target = np.asarray(target_points, dtype=float)
    source_center = np.mean(source, axis=0)
    target_center = np.mean(target, axis=0)
    source_zero = source - source_center
    target_zero = target - target_center
    u, _, vt = np.linalg.svd(source_zero.T @ target_zero)
    rotation = vt.T @ u.T
    if np.linalg.det(rotation) < 0.0:
        vt[-1, :] *= -1.0
        rotation = vt.T @ u.T
    translation = target_center - rotation @ source_center
    return rotation, translation


def linear_transform_matrix(transform: object) -> np.ndarray:
    if isinstance(transform, dict):
        if "matrix" in transform:
            return np.asarray(transform["matrix"], dtype=float)
        phi = float(transform["rotation_rad"])
        return np.array([[math.cos(phi), -math.sin(phi)], [math.sin(phi), math.cos(phi)]], dtype=float)
    matrix = np.asarray(transform, dtype=float)
    if matrix.shape == (2, 3):
        return matrix[:, :2]
    return matrix


def local_delta_from_global(transform: object, global_delta: np.ndarray) -> np.ndarray:
    matrix = linear_transform_matrix(transform)
    return np.linalg.pinv(matrix) @ np.asarray(global_delta, dtype=float)


def hobj_paths(root: Path) -> list[Path]:
    return sorted(root.rglob("*.hobj"), key=lambda value: str(value).casefold())


def group_capture_paths(professional_root: Path) -> Dict[str, list[Path]]:
    groups: Dict[str, list[Path]] = {}
    for group, _ in GROUPS:
        folder = professional_root / group
        groups[group] = hobj_paths(folder) if folder.is_dir() else []
    missing = [group for group, paths in groups.items() if not paths]
    if missing:
        raise ValueError(f"Missing professional calibration HOBJ groups: {', '.join(missing)}")
    return groups


def load_source_info(path: Path) -> Tuple[HobjSource, Dict[int, Tuple[int, int]], int, int]:
    source = HobjSource(str(path))
    valid_ranges = source_valid_ranges(source)
    common_start, common_end = source_common_range(source, valid_ranges)
    return source, valid_ranges, common_start, common_end


def extract_points_for_row(
    source: HobjSource,
    row: int,
    transforms: Dict[int, object],
    calibration: Dict[str, object],
) -> Tuple[Optional[Dict[int, object]], Optional[Dict[str, Tuple[float, float]]], str]:
    corners = {obj: extract_corner_from_row(source.row(obj, row), X_SCALE_MM) for obj in (1, 2, 3, 4)}
    invalid = [f"obj{obj}:{corner.reason}" for obj, corner in corners.items() if not corner.valid]
    if invalid:
        return None, None, "; ".join(invalid)
    points, _, _ = cross_section_geometry(corners, transforms, calibration, "free")
    return corners, points, ""


def ideal_fit_residuals(
    points: Dict[str, Tuple[float, float]],
    reference: Dict[str, np.ndarray],
) -> Tuple[Dict[str, np.ndarray], Dict[str, np.ndarray], float, np.ndarray, np.ndarray]:
    observed = np.vstack([points[point] for point in POINT_ORDER])
    ideal = np.vstack([reference[point] for point in POINT_ORDER])
    rotation, translation = rigid_align_2d(ideal, observed)
    fitted = (rotation @ ideal.T).T + translation
    residuals = {
        point: observed[index] - fitted[index]
        for index, point in enumerate(POINT_ORDER)
    }
    vector_norms = np.asarray([np.linalg.norm(residuals[point]) for point in POINT_ORDER], dtype=float)
    rms = float(np.sqrt(np.mean(vector_norms * vector_norms)))
    return residuals, {point: fitted[index] for index, point in enumerate(POINT_ORDER)}, rms, rotation, translation


def thickness_from_face_observations(
    corners: Dict[int, object],
    transforms: Dict[int, object],
    calibration: Dict[str, object],
) -> Dict[str, float]:
    """Return opposite-face thicknesses from same-face dual-camera observations.

    This diagnostic deliberately avoids intersecting adjacent side lines into
    corners.  It uses the two camera observations of each physical face, takes
    their observed line anchor points as a face centre estimate, then measures
    A-to-C and B-to-D separation along the nominal outward axes.
    """

    observations = global_face_line_observations(corners, transforms, calibration)
    centres: Dict[str, np.ndarray] = {}
    pair_angles: Dict[str, float] = {}
    for face, objects in SAME_FACE_CAMERA_PAIRS.items():
        points = []
        directions = []
        for obj in objects:
            observation = observations.get((obj, face))
            if observation is None:
                continue
            point, direction = observation
            points.append(np.asarray(point, dtype=float))
            directions.append(np.asarray(direction, dtype=float))
        if len(points) != 2:
            continue
        if float(directions[0] @ directions[1]) < 0.0:
            directions[1] = -directions[1]
        cosine = float(np.clip(directions[0] @ directions[1], -1.0, 1.0))
        pair_angles[face] = math.degrees(math.acos(cosine))
        centres[face] = np.mean(np.vstack(points), axis=0)

    result: Dict[str, float] = {}
    if "A" in centres and "C" in centres:
        axis = FACE_OUTWARD_NORMALS["A"] / float(np.linalg.norm(FACE_OUTWARD_NORMALS["A"]))
        result["thickness_ac_mm"] = float((centres["A"] - centres["C"]) @ axis)
    if "B" in centres and "D" in centres:
        axis = FACE_OUTWARD_NORMALS["B"] / float(np.linalg.norm(FACE_OUTWARD_NORMALS["B"]))
        result["thickness_bd_mm"] = float((centres["B"] - centres["D"]) @ axis)
    for face, angle in pair_angles.items():
        result[f"same_face_{face}_pair_angle_deg"] = float(angle)
    return result


def raw_shift_samples(
    paths: list[Path],
    orientation: str,
    calibration: Dict[str, object],
    reference: Dict[str, np.ndarray],
    fractions: np.ndarray,
) -> Tuple[list[Dict[str, object]], int, int]:
    source_infos = []
    ranges = []
    for path in paths:
        source, valid_ranges, common_start, common_end = load_source_info(path)
        source_infos.append((path, source, valid_ranges, common_start, common_end))
        ranges.append((common_start, common_end))
    start_row = max(start for start, _ in ranges)
    end_row = min(end for _, end in ranges)
    if end_row <= start_row:
        raise ValueError(f"No shared absolute row overlap for {orientation}")

    active = calibration_for_orientation(calibration, orientation)
    transforms = {int(key): value for key, value in active["transforms"].items()}
    rows = [int(round(start_row + float(fraction) * (end_row - start_row))) for fraction in fractions]
    samples: list[Dict[str, object]] = []
    for path, source, _, _, _ in source_infos:
        for fraction, row in zip(fractions, rows):
            corners, points, reason = extract_points_for_row(source, row, transforms, active)
            if corners is None or points is None:
                samples.append({
                    "capture": path.name,
                    "path": str(path),
                    "row": row,
                    "fraction": float(fraction),
                    "valid": False,
                    "reason": reason,
                })
                continue
            residuals, _, fit_rms, rotation, translation = ideal_fit_residuals(points, reference)
            for point in POINT_ORDER:
                obj = POINT_TO_OBJECT[point]
                local_shift = local_delta_from_global(transforms[obj], residuals[point])
                samples.append({
                    "capture": path.name,
                    "path": str(path),
                    "row": row,
                    "fraction": float(fraction),
                    "valid": True,
                    "obj": obj,
                    "point": point,
                    "raw_residual_global_x_mm": float(residuals[point][0]),
                    "raw_residual_global_z_mm": float(residuals[point][1]),
                    "raw_residual_vector_mm": float(np.linalg.norm(residuals[point])),
                    "local_shift_x_mm": float(local_shift[0]),
                    "local_shift_z_mm": float(local_shift[1]),
                    "slice_ideal_fit_rms_mm": fit_rms,
                    "slice_fit_rotation_deg": float(math.degrees(math.atan2(rotation[1, 0], rotation[0, 0]))),
                    "slice_fit_translation_x_mm": float(translation[0]),
                    "slice_fit_translation_z_mm": float(translation[1]),
                    "reason": "",
                })
    return samples, start_row, end_row


def build_drift_field(
    group: str,
    paths: list[Path],
    orientation: str,
    calibration: Dict[str, object],
    reference: Dict[str, np.ndarray],
    fractions: np.ndarray,
    outlier_limit_mm: float,
) -> Tuple[DriftField, list[Dict[str, object]]]:
    samples, start_row, end_row = raw_shift_samples(paths, orientation, calibration, reference, fractions)
    preliminary: Dict[Tuple[int, float], np.ndarray] = {}
    for obj in (1, 2, 3, 4):
        for fraction in fractions:
            values = np.asarray(
                [
                    [sample["local_shift_x_mm"], sample["local_shift_z_mm"]]
                    for sample in samples
                    if sample.get("valid") is True
                    and int(sample["obj"]) == obj
                    and abs(float(sample["fraction"]) - float(fraction)) <= 1e-12
                ],
                dtype=float,
            )
            if len(values):
                preliminary[(obj, float(fraction))] = np.median(values, axis=0)

    curve: Dict[int, list[np.ndarray]] = {obj: [] for obj in (1, 2, 3, 4)}
    sample_count: Dict[Tuple[int, float], int] = {}
    for obj in (1, 2, 3, 4):
        for fraction in fractions:
            key = (obj, float(fraction))
            values = []
            for sample in samples:
                if (
                    sample.get("valid") is not True
                    or int(sample["obj"]) != obj
                    or abs(float(sample["fraction"]) - float(fraction)) > 1e-12
                ):
                    continue
                value = np.array([sample["local_shift_x_mm"], sample["local_shift_z_mm"]], dtype=float)
                centre = preliminary.get(key)
                if centre is not None and float(np.linalg.norm(value - centre)) > outlier_limit_mm:
                    sample["used_for_drift_curve"] = False
                    sample["drift_curve_reject_reason"] = "sample_deviation_above_limit"
                    continue
                sample["used_for_drift_curve"] = True
                sample["drift_curve_reject_reason"] = ""
                values.append(value)
            if values:
                sample_count[key] = len(values)
                curve[obj].append(np.median(np.vstack(values), axis=0))
            else:
                sample_count[key] = 0
                curve[obj].append(np.array([math.nan, math.nan], dtype=float))
    field = DriftField(
        group=group,
        orientation=orientation,
        start_row=start_row,
        end_row=end_row,
        fractions=np.asarray(fractions, dtype=float),
        local_shift={obj: np.vstack(curve[obj]) for obj in (1, 2, 3, 4)},
        sample_count=sample_count,
    )
    for sample in samples:
        sample["group"] = group
        sample["orientation"] = orientation
        sample.setdefault("used_for_drift_curve", False)
        sample.setdefault("drift_curve_reject_reason", "")
    return field, samples


def interpolate_shift(field: DriftField, obj: int, row: int) -> Optional[np.ndarray]:
    if field.end_row <= field.start_row:
        return None
    fraction = (float(row) - float(field.start_row)) / float(field.end_row - field.start_row)
    if fraction < float(field.fractions[0]) or fraction > float(field.fractions[-1]):
        return None
    values = field.local_shift[obj]
    if not np.all(np.isfinite(values)):
        return None
    x = float(np.interp(fraction, field.fractions, values[:, 0]))
    z = float(np.interp(fraction, field.fractions, values[:, 1]))
    return np.array([x, z], dtype=float)


def evaluate_capture(
    path: Path,
    source: HobjSource,
    common_start: int,
    common_end: int,
    calibration: Dict[str, object],
    reference: Dict[str, np.ndarray],
    field: DriftField,
    fractions: np.ndarray,
) -> Tuple[list[Dict[str, object]], Dict[str, object]]:
    active = calibration_for_orientation(calibration, field.orientation)
    transforms = {int(key): value for key, value in active["transforms"].items()}
    rows = [
        int(round(max(common_start, field.start_row) + float(fraction) * (min(common_end, field.end_row) - max(common_start, field.start_row))))
        for fraction in fractions
    ]
    if min(common_end, field.end_row) <= max(common_start, field.start_row):
        return [], {
            "capture": path.name,
            "path": str(path),
            "field_group": field.group,
            "orientation_used": field.orientation,
            "status": "no_absolute_row_overlap",
        }
    detail: list[Dict[str, object]] = []
    raw_vectors: list[float] = []
    corrected_vectors: list[float] = []
    raw_fit_rms: list[float] = []
    corrected_fit_rms: list[float] = []
    raw_thickness_errors: list[float] = []
    corrected_thickness_errors: list[float] = []
    long_edge_mm = float(reference["P1"][0] - reference["P3"][0])
    short_edge_mm = float(reference["P3"][1] - reference["P2"][1])
    invalid_count = 0
    for row in rows:
        corners, raw_points, reason = extract_points_for_row(source, row, transforms, active)
        if corners is None or raw_points is None:
            invalid_count += 1
            continue
        raw_residuals, _, raw_rms, _, _ = ideal_fit_residuals(raw_points, reference)
        raw_thickness = thickness_from_face_observations(corners, transforms, active)
        corrected_corners = {}
        missing_shift = False
        for obj, corner in corners.items():
            shift = interpolate_shift(field, obj, row)
            if shift is None:
                missing_shift = True
                corrected_corners[obj] = corner
            else:
                corrected_corners[obj] = subtract_local_drift(corner, float(shift[0]), float(shift[1]))
        if missing_shift:
            invalid_count += 1
            continue
        corrected_points, _, _ = cross_section_geometry(corrected_corners, transforms, active, "free")
        corrected_residuals, _, corrected_rms, _, _ = ideal_fit_residuals(corrected_points, reference)
        corrected_thickness = thickness_from_face_observations(corrected_corners, transforms, active)
        raw_fit_rms.append(raw_rms)
        corrected_fit_rms.append(corrected_rms)
        for key, nominal in (("thickness_ac_mm", short_edge_mm), ("thickness_bd_mm", long_edge_mm)):
            if key in raw_thickness and math.isfinite(float(raw_thickness[key])):
                raw_thickness_errors.append(float(raw_thickness[key]) - nominal)
            if key in corrected_thickness and math.isfinite(float(corrected_thickness[key])):
                corrected_thickness_errors.append(float(corrected_thickness[key]) - nominal)
        for point in POINT_ORDER:
            raw_vector = float(np.linalg.norm(raw_residuals[point]))
            corrected_vector = float(np.linalg.norm(corrected_residuals[point]))
            raw_vectors.append(raw_vector)
            corrected_vectors.append(corrected_vector)
            detail.append({
                "capture": path.name,
                "path": str(path),
                "field_group": field.group,
                "orientation_used": field.orientation,
                "row": row,
                "obj": POINT_TO_OBJECT[point],
                "point": point,
                "raw_residual_x_mm": float(raw_residuals[point][0]),
                "raw_residual_z_mm": float(raw_residuals[point][1]),
                "raw_residual_vector_mm": raw_vector,
                "corrected_residual_x_mm": float(corrected_residuals[point][0]),
                "corrected_residual_z_mm": float(corrected_residuals[point][1]),
                "corrected_residual_vector_mm": corrected_vector,
                "raw_thickness_ac_mm": raw_thickness.get("thickness_ac_mm", ""),
                "raw_thickness_bd_mm": raw_thickness.get("thickness_bd_mm", ""),
                "raw_thickness_ac_error_mm": (
                    float(raw_thickness["thickness_ac_mm"]) - short_edge_mm
                    if "thickness_ac_mm" in raw_thickness
                    else ""
                ),
                "raw_thickness_bd_error_mm": (
                    float(raw_thickness["thickness_bd_mm"]) - long_edge_mm
                    if "thickness_bd_mm" in raw_thickness
                    else ""
                ),
                "corrected_thickness_ac_mm": corrected_thickness.get("thickness_ac_mm", ""),
                "corrected_thickness_bd_mm": corrected_thickness.get("thickness_bd_mm", ""),
                "corrected_thickness_ac_error_mm": (
                    float(corrected_thickness["thickness_ac_mm"]) - short_edge_mm
                    if "thickness_ac_mm" in corrected_thickness
                    else ""
                ),
                "corrected_thickness_bd_error_mm": (
                    float(corrected_thickness["thickness_bd_mm"]) - long_edge_mm
                    if "thickness_bd_mm" in corrected_thickness
                    else ""
                ),
            })

    if not detail:
        return detail, {
            "capture": path.name,
            "path": str(path),
            "field_group": field.group,
            "orientation_used": field.orientation,
            "status": "no_valid_samples",
            "invalid_sample_count": invalid_count,
        }
    raw_arr = np.asarray(raw_vectors, dtype=float)
    corrected_arr = np.asarray(corrected_vectors, dtype=float)
    raw_thickness_arr = np.asarray(raw_thickness_errors, dtype=float)
    corrected_thickness_arr = np.asarray(corrected_thickness_errors, dtype=float)
    summary = {
        "capture": path.name,
        "path": str(path),
        "field_group": field.group,
        "orientation_used": field.orientation,
        "status": "ok",
        "sample_count": len(raw_arr),
        "invalid_sample_count": invalid_count,
        "raw_point_rms_mm": float(np.sqrt(np.mean(raw_arr * raw_arr))),
        "raw_point_p95_mm": float(np.quantile(raw_arr, 0.95)),
        "raw_point_max_mm": float(np.max(raw_arr)),
        "corrected_point_rms_mm": float(np.sqrt(np.mean(corrected_arr * corrected_arr))),
        "corrected_point_p95_mm": float(np.quantile(corrected_arr, 0.95)),
        "corrected_point_max_mm": float(np.max(corrected_arr)),
        "raw_slice_fit_rms_mean_mm": float(np.mean(raw_fit_rms)),
        "corrected_slice_fit_rms_mean_mm": float(np.mean(corrected_fit_rms)),
        "improvement_rms_mm": float(np.sqrt(np.mean(raw_arr * raw_arr)) - np.sqrt(np.mean(corrected_arr * corrected_arr))),
    }
    if len(raw_thickness_arr) and len(corrected_thickness_arr):
        summary.update({
            "raw_thickness_error_rms_mm": float(np.sqrt(np.mean(raw_thickness_arr * raw_thickness_arr))),
            "raw_thickness_error_p95_abs_mm": float(np.quantile(np.abs(raw_thickness_arr), 0.95)),
            "raw_thickness_error_max_abs_mm": float(np.max(np.abs(raw_thickness_arr))),
            "corrected_thickness_error_rms_mm": float(np.sqrt(np.mean(corrected_thickness_arr * corrected_thickness_arr))),
            "corrected_thickness_error_p95_abs_mm": float(np.quantile(np.abs(corrected_thickness_arr), 0.95)),
            "corrected_thickness_error_max_abs_mm": float(np.max(np.abs(corrected_thickness_arr))),
            "thickness_improvement_rms_mm": (
                float(np.sqrt(np.mean(raw_thickness_arr * raw_thickness_arr)))
                - float(np.sqrt(np.mean(corrected_thickness_arr * corrected_thickness_arr)))
            ),
        })
    return detail, summary


def write_csv(path: Path, rows: list[Dict[str, object]]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    if not rows:
        path.write_text("", encoding="utf-8")
        return
    fieldnames: list[str] = []
    seen = set()
    for row in rows:
        for key in row:
            if key not in seen:
                fieldnames.append(key)
                seen.add(key)
    with path.open("w", encoding="utf-8-sig", newline="") as handle:
        writer = csv.DictWriter(handle, fieldnames=fieldnames)
        writer.writeheader()
        writer.writerows(rows)


def drift_curve_summary(field: DriftField) -> list[Dict[str, object]]:
    rows: list[Dict[str, object]] = []
    for obj in (1, 2, 3, 4):
        values = field.local_shift[obj]
        norms = np.linalg.norm(values, axis=1)
        rows.append({
            "group": field.group,
            "orientation": field.orientation,
            "obj": obj,
            "start_row": field.start_row,
            "end_row": field.end_row,
            "station_count": len(field.fractions),
            "shift_vector_min_mm": float(np.min(norms)),
            "shift_vector_max_mm": float(np.max(norms)),
            "shift_vector_span_mm": float(np.max(norms) - np.min(norms)),
            "shift_x_p05_p95_span_mm": float(np.quantile(values[:, 0], 0.95) - np.quantile(values[:, 0], 0.05)),
            "shift_z_p05_p95_span_mm": float(np.quantile(values[:, 1], 0.95) - np.quantile(values[:, 1], 0.05)),
        })
    return rows


def discover_extra_hobjs(paths: Iterable[Path], professional_root: Path) -> list[Path]:
    excluded_root = professional_root.resolve()
    found: list[Path] = []
    for root in paths:
        if not root.exists():
            continue
        for path in hobj_paths(root):
            try:
                resolved = path.resolve()
                if str(resolved).casefold().startswith(str(excluded_root).casefold()):
                    continue
            except OSError:
                pass
            found.append(path)
    unique = {str(path.resolve()).casefold(): path for path in found}
    return sorted(unique.values(), key=lambda value: str(value).casefold())


def main() -> int:
    parser = argparse.ArgumentParser(description="Audit virtual perfect-bar per-camera drift consistency")
    parser.add_argument(
        "--professional-root",
        default=r"C:\Users\Administrator\Downloads\SunshineSiRod_Endface\calibration_hobj\210_105",
        help="Folder containing head_to_tail and tail_to_head HOBJ groups",
    )
    parser.add_argument(
        "--calibration",
        default=str(Path(__file__).resolve().parent / "calibration" / "models" / "camera_calibration_model_210_105.json"),
    )
    parser.add_argument(
        "--output-dir",
        default=str(Path(__file__).resolve().parent / "results" / "virtual_perfect_bar_drift_audit"),
    )
    parser.add_argument("--long-edge-mm", type=float, default=210.0)
    parser.add_argument("--short-edge-mm", type=float, default=105.0)
    parser.add_argument("--sample-count", type=int, default=19)
    parser.add_argument("--outlier-limit-mm", type=float, default=1.0)
    parser.add_argument("--extra-root", action="append", default=[])
    args = parser.parse_args()

    professional_root = Path(args.professional_root)
    output_dir = Path(args.output_dir)
    with open(args.calibration, "r", encoding="utf-8") as handle:
        calibration = json.load(handle)
    reference = ideal_points(args.long_edge_mm, args.short_edge_mm)
    fractions = np.linspace(0.05, 0.95, max(5, int(args.sample_count)))
    groups = group_capture_paths(professional_root)

    fields: Dict[str, DriftField] = {}
    training_samples: list[Dict[str, object]] = []
    drift_summaries: list[Dict[str, object]] = []
    for group, orientation in GROUPS:
        field, samples = build_drift_field(
            group,
            groups[group],
            orientation,
            calibration,
            reference,
            fractions,
            args.outlier_limit_mm,
        )
        fields[group] = field
        training_samples.extend(samples)
        drift_summaries.extend(drift_curve_summary(field))

    professional_detail: list[Dict[str, object]] = []
    professional_summary: list[Dict[str, object]] = []
    for group, _ in GROUPS:
        field = fields[group]
        for path in groups[group]:
            source, _, common_start, common_end = load_source_info(path)
            detail, summary = evaluate_capture(
                path,
                source,
                common_start,
                common_end,
                calibration,
                reference,
                field,
                fractions,
            )
            professional_detail.extend(detail)
            summary["acquisition_group"] = group
            professional_summary.append(summary)

    default_extra_roots = [
        Path(r"D:\Image_risen"),
        Path.cwd() / "image",
        Path.cwd() / "tools" / "calibration" / "hobj",
        Path(r"C:\Users\Administrator\Downloads\2606005B22-CTB"),
        Path(r"C:\Users\Administrator\Downloads\A61"),
        Path(r"C:\Users\Administrator\Downloads\B22-CTT"),
    ]
    extra_roots = [Path(value) for value in args.extra_root] if args.extra_root else default_extra_roots
    extra_paths = discover_extra_hobjs(extra_roots, professional_root)
    extra_detail: list[Dict[str, object]] = []
    extra_summary: list[Dict[str, object]] = []
    for path in extra_paths:
        try:
            source, _, common_start, common_end = load_source_info(path)
        except Exception as exc:
            extra_summary.append({"capture": path.name, "path": str(path), "status": "load_failed", "reason": str(exc)})
            continue
        for field in fields.values():
            try:
                detail, summary = evaluate_capture(
                    path,
                    source,
                    common_start,
                    common_end,
                    calibration,
                    reference,
                    field,
                    fractions,
                )
            except Exception as exc:
                detail = []
                summary = {
                    "capture": path.name,
                    "path": str(path),
                    "field_group": field.group,
                    "orientation_used": field.orientation,
                    "status": "evaluation_failed",
                    "reason": str(exc),
                }
            extra_detail.extend(detail)
            extra_summary.append(summary)

    write_csv(output_dir / "drift_curve_training_samples.csv", training_samples)
    write_csv(output_dir / "drift_curve_summary.csv", drift_summaries)
    write_csv(output_dir / "professional_20_self_consistency_summary.csv", professional_summary)
    write_csv(output_dir / "professional_20_self_consistency_detail.csv", professional_detail)
    write_csv(output_dir / "extra_hobj_self_consistency_summary.csv", extra_summary)
    write_csv(output_dir / "extra_hobj_self_consistency_detail.csv", extra_detail)

    best_extra: list[Dict[str, object]] = []
    by_path: Dict[str, list[Dict[str, object]]] = {}
    for row in extra_summary:
        if row.get("status") == "ok":
            by_path.setdefault(str(row["path"]), []).append(row)
    for path, rows in by_path.items():
        best = min(rows, key=lambda row: float(row["corrected_point_rms_mm"]))
        best_extra.append({
            "capture": best["capture"],
            "path": path,
            "diagnostic_best_field_group": best["field_group"],
            "diagnostic_best_orientation": best["orientation_used"],
            "raw_point_rms_mm": best["raw_point_rms_mm"],
            "corrected_point_rms_mm": best["corrected_point_rms_mm"],
            "corrected_point_p95_mm": best["corrected_point_p95_mm"],
            "corrected_point_max_mm": best["corrected_point_max_mm"],
            "note": "diagnostic best fit only; not a runtime orientation selector",
        })
    write_csv(output_dir / "extra_hobj_best_diagnostic_match.csv", best_extra)

    print(f"Audit written: {output_dir}")
    for row in drift_summaries:
        print(
            f"{row['group']} obj{row['obj']}: "
            f"shift span={row['shift_vector_span_mm']:.6f} mm, "
            f"x p05-p95={row['shift_x_p05_p95_span_mm']:.6f} mm, "
            f"z p05-p95={row['shift_z_p05_p95_span_mm']:.6f} mm"
        )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
