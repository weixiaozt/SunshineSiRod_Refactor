#!/usr/bin/env python3
"""Truth-free CTB end-face camera-Y synchronization audit.

The model has only four fixed camera scan-row offsets under a zero-sum gauge,
so there are three independent parameters.  Offsets are fitted from continuity
of the same physical end edge seen by adjacent cameras.  End-face truth and
final A/B/C/D angle offsets are never read or fitted.  Every capture is scored
with a model trained without that capture and its paired turnover capture.
"""

from __future__ import annotations

import argparse
import csv
import json
import math
import re
import sys
from dataclasses import dataclass, replace
from pathlib import Path
from typing import Dict, Iterable, Optional, Tuple

import numpy as np

try:
    from .endface_wireframe_geometry import (
        CAMERA_ADJACENT_CORE_FACES,
        CORE_FACE_ENDPOINTS,
        ENDS,
        LOCAL_CHANNELS,
        REPORT_FACE_CONFIG,
        SegmentPoints,
        fit_face_edge,
        projected_angle_deg,
        robust_ridge_direction,
        segment_for_camera_face,
        summarize_end_angles,
        wireframe_shape_diagnostics,
    )
    from .endface_wireframe_turnover_audit import turnover_equivariance_rows
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
    sys.path.insert(0, str(Path(__file__).resolve().parent))
    from endface_wireframe_geometry import (
        CAMERA_ADJACENT_CORE_FACES,
        CORE_FACE_ENDPOINTS,
        ENDS,
        LOCAL_CHANNELS,
        REPORT_FACE_CONFIG,
        SegmentPoints,
        fit_face_edge,
        projected_angle_deg,
        robust_ridge_direction,
        segment_for_camera_face,
        summarize_end_angles,
        wireframe_shape_diagnostics,
    )
    from endface_wireframe_turnover_audit import turnover_equivariance_rows
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


CAMERAS = (1, 2, 3, 4)
POINT_TO_CAMERA = {"P1": 1, "P2": 2, "P3": 3, "P4": 4}


@dataclass
class CaptureGeometry:
    path: Path
    group: str
    fold: int
    common_start: int
    common_end: int
    rows: list[Dict[str, object]]
    section_points: Dict[str, Tuple[float, float]]
    segments_by_end_face: Dict[str, Dict[str, list[SegmentPoints]]]


def write_csv(path: Path, rows: list[Dict[str, object]]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    if not rows:
        path.write_text("", encoding="utf-8")
        return
    fields: list[str] = []
    seen: set[str] = set()
    for row in rows:
        for key in row:
            if key not in seen:
                seen.add(key)
                fields.append(key)
    with path.open("w", encoding="utf-8-sig", newline="") as handle:
        writer = csv.DictWriter(handle, fieldnames=fields)
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


def extract_capture_geometry(
    path: Path,
    group: str,
    fold: int,
    calibration: Dict[str, object],
    endface_window_mm: float,
) -> CaptureGeometry:
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
    segments_by_end_face: Dict[str, Dict[str, list[SegmentPoints]]] = {}
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
            )
            for obj in CAMERAS
        }
        segments: Dict[str, list[SegmentPoints]] = {face: [] for face in CORE_FACE_ENDPOINTS}
        for obj, cloud in clouds.items():
            adjacent = CAMERA_ADJACENT_CORE_FACES[obj]
            for face in adjacent:
                segments[face].append(
                    segment_for_camera_face(obj, cloud, face, adjacent, section_points)
                )
        segments_by_end_face[end] = segments
    return CaptureGeometry(
        path=path,
        group=group,
        fold=fold,
        common_start=common_start,
        common_end=common_end,
        rows=rows,
        section_points=section_points,
        segments_by_end_face=segments_by_end_face,
    )


def segment_midpoint_y(
    segment: SegmentPoints,
    core_face: str,
    section_points: Dict[str, Tuple[float, float]],
) -> float:
    first_name, second_name = CORE_FACE_ENDPOINTS[core_face]
    first = np.asarray(section_points[first_name], dtype=float)
    second = np.asarray(section_points[second_name], dtype=float)
    midpoint = 0.5 * float(np.linalg.norm(second - first))
    return float(segment.line_slope * midpoint + segment.line_intercept)


def seam_equation_rows(geometries: list[CaptureGeometry]) -> list[Dict[str, object]]:
    output: list[Dict[str, object]] = []
    for geometry in geometries:
        for end in ENDS:
            for face, segments in geometry.segments_by_end_face[end].items():
                if len(segments) != 2:
                    raise ValueError(f"{geometry.path.name} {end} {face}: expected two camera segments")
                first, second = sorted(segments, key=lambda segment: segment.camera)
                first_y = segment_midpoint_y(first, face, geometry.section_points)
                second_y = segment_midpoint_y(second, face, geometry.section_points)
                output.append({
                    "capture": geometry.path.name,
                    "group": geometry.group,
                    "fold": geometry.fold,
                    "end": end,
                    "core_face": face,
                    "first_camera": first.camera,
                    "second_camera": second.camera,
                    "first_mid_y_mm": first_y,
                    "second_mid_y_mm": second_y,
                    "raw_signed_seam_mm": first_y - second_y,
                    "first_edge_fit_rms_mm": float(
                        np.sqrt(np.mean(np.square(first.y_mm - (first.line_slope * first.coordinate + first.line_intercept))))
                    ),
                    "second_edge_fit_rms_mm": float(
                        np.sqrt(np.mean(np.square(second.y_mm - (second.line_slope * second.coordinate + second.line_intercept))))
                    ),
                })
    return output


def zero_sum_basis() -> np.ndarray:
    return np.asarray(
        [[1.0, 0.0, 0.0], [0.0, 1.0, 0.0], [0.0, 0.0, 1.0], [-1.0, -1.0, -1.0]],
        dtype=float,
    )


def fit_camera_y_offsets(
    equation_rows: list[Dict[str, object]],
    train_names: set[str],
    model_id: str,
    huber_delta_mm: float = 0.20,
    minimum_equations: int = 32,
) -> Tuple[np.ndarray, Dict[str, object]]:
    selected = [row for row in equation_rows if str(row["capture"]) in train_names]
    if len(selected) < minimum_equations:
        raise ValueError(
            f"Y synchronization fit requires at least {minimum_equations} same-edge equations"
        )
    design = np.zeros((len(selected), 4), dtype=float)
    target = np.zeros(len(selected), dtype=float)
    for index, row in enumerate(selected):
        first = int(row["first_camera"]) - 1
        second = int(row["second_camera"]) - 1
        design[index, first] = 1.0
        design[index, second] = -1.0
        target[index] = -float(row["raw_signed_seam_mm"])
    reduced = design @ zero_sum_basis()
    weights = np.ones(len(target), dtype=float)
    parameters = np.zeros(3, dtype=float)
    history = []
    for iteration in range(20):
        weighted_design = np.sqrt(weights)[:, None] * reduced
        weighted_target = np.sqrt(weights) * target
        updated, _, _, _ = np.linalg.lstsq(weighted_design, weighted_target, rcond=None)
        residual = reduced @ updated - target
        updated_weights = np.minimum(1.0, huber_delta_mm / np.maximum(np.abs(residual), 1e-12))
        history.append({
            "iteration": iteration,
            "camera_y_offsets_mm": [float(value) for value in zero_sum_basis() @ updated],
            "residual_rms_mm": float(np.sqrt(np.mean(np.square(residual)))),
            "residual_p95_abs_mm": float(np.quantile(np.abs(residual), 0.95)),
        })
        parameters = updated
        if float(np.max(np.abs(updated_weights - weights))) < 1e-7:
            weights = updated_weights
            break
        weights = updated_weights
    offsets = zero_sum_basis() @ parameters
    residual = design @ offsets - target
    singular_values = np.linalg.svd(np.sqrt(weights)[:, None] * reduced, compute_uv=False)
    condition = (
        float(singular_values[0] / singular_values[-1])
        if len(singular_values) and singular_values[-1] > 1e-12
        else math.inf
    )
    details = {
        "model_id": model_id,
        "method": "robust_same_physical_end_edge_dual_camera_y_alignment",
        "fit_target": "signed_same_edge_midpoint_seam_only_no_endface_truth",
        "parameter_count": 3,
        "offset_gauge": "sum(camera_y_offsets_mm)=0",
        "training_capture_ids": sorted(train_names),
        "equation_count": len(selected),
        "huber_delta_mm": huber_delta_mm,
        "camera_y_offsets_mm": {str(obj): float(offsets[obj - 1]) for obj in CAMERAS},
        "camera_y_offsets_rows": {
            str(obj): float(offsets[obj - 1] / Y_SCALE_MM) for obj in CAMERAS
        },
        "weighted_design_singular_values": [float(value) for value in singular_values],
        "weighted_design_condition_number": condition,
        "training_residual_rms_mm": float(np.sqrt(np.mean(np.square(residual)))),
        "training_residual_p95_abs_mm": float(np.quantile(np.abs(residual), 0.95)),
        "history": history,
        "uses_endface_truth": False,
        "forbidden_final_angle_parameters_present": False,
    }
    return offsets, details


def shifted_segment(segment: SegmentPoints, offset_mm: float) -> SegmentPoints:
    return replace(
        segment,
        y_mm=np.asarray(segment.y_mm, dtype=float) + float(offset_mm),
        line_intercept=float(segment.line_intercept + offset_mm),
    )


def measure_geometry(
    geometry: CaptureGeometry,
    offsets: np.ndarray,
    model_id: str,
) -> Dict[str, object]:
    offset_map = {obj: float(offsets[obj - 1]) for obj in CAMERAS}
    side_normals = longitudinal_side_plane_normals(geometry.rows, offset_map)
    ridge_directions = {
        point: robust_ridge_direction(
            geometry.rows,
            point,
            camera_y_offset_mm=offset_map[POINT_TO_CAMERA[point]],
        )
        for point in POINT_TO_CAMERA
    }
    result: Dict[str, object] = {
        "capture": geometry.path.name,
        "group": geometry.group,
        "fold": geometry.fold,
        "camera_y_sync_model_id": model_id,
        "camera_y_sync_applied": bool(np.max(np.abs(offsets)) > 1e-12),
        "uses_endface_truth": False,
        "final_angle_correction_applied": False,
    }
    for obj in CAMERAS:
        result[f"camera_{obj}_y_offset_mm"] = offset_map[obj]
    for end in ENDS:
        edge_fits = {}
        signed_seams = {}
        for face, raw_segments in geometry.segments_by_end_face[end].items():
            segments = [shifted_segment(segment, offset_map[segment.camera]) for segment in raw_segments]
            edge_fits[face] = fit_face_edge(
                face,
                segments,
                geometry.section_points,
                side_normals[face],
            )
            first, second = sorted(segments, key=lambda segment: segment.camera)
            signed_seams[face] = (
                segment_midpoint_y(first, face, geometry.section_points)
                - segment_midpoint_y(second, face, geometry.section_points)
            )
        angles: Dict[str, float] = {}
        for report_face, config in REPORT_FACE_CONFIG.items():
            core_face = str(config["core_face"])
            endpoints = tuple(config["endpoints"])
            positions = tuple(config["positions"])
            edge_fit = edge_fits[core_face]
            for index, (point_name, position) in enumerate(zip(endpoints, positions)):
                edge_ray = -edge_fit.direction if index == 0 else edge_fit.direction
                channel = f"{report_face}_{position}"
                angle = projected_angle_deg(
                    edge_ray,
                    ridge_directions[point_name],
                    side_normals[core_face],
                )
                angles[channel] = angle
                result[f"{end}_{channel}_angle_deg"] = angle
            result[f"{end}_{report_face}_dual_camera_seam_mid_mm"] = edge_fit.seam_at_face_mid_mm
            result[f"{end}_{report_face}_signed_dual_camera_seam_mid_mm"] = signed_seams[core_face]
        for key, value in summarize_end_angles(angles).items():
            result[f"{end}_endface_{key}"] = value
        for key, value in wireframe_shape_diagnostics(edge_fits, geometry.section_points).items():
            result[f"{end}_{key}"] = value
        result[f"{end}_seam_rms_mm"] = float(
            np.sqrt(np.mean(np.square(list(signed_seams.values()))))
        )
        result[f"{end}_seam_max_abs_mm"] = float(np.max(np.abs(list(signed_seams.values()))))
    return result


def holdout_seam_rows(
    equation_rows: list[Dict[str, object]],
    fold_models: Dict[int, Tuple[np.ndarray, Dict[str, object]]],
) -> list[Dict[str, object]]:
    output = []
    for row in equation_rows:
        fold = int(row["fold"])
        offsets, details = fold_models[fold]
        first = int(row["first_camera"])
        second = int(row["second_camera"])
        raw = float(row["raw_signed_seam_mm"])
        corrected = raw + float(offsets[first - 1] - offsets[second - 1])
        item = dict(row)
        item.update({
            "validation": "paired_leave_one_fold_out",
            "camera_y_sync_model_id": details["model_id"],
            "corrected_signed_seam_mm": corrected,
            "raw_abs_seam_mm": abs(raw),
            "corrected_abs_seam_mm": abs(corrected),
        })
        output.append(item)
    return output


def fixed_offset_seam_rows(
    equation_rows: list[Dict[str, object]],
    offsets: np.ndarray,
    model_id: str,
) -> list[Dict[str, object]]:
    output = []
    for row in equation_rows:
        first = int(row["first_camera"])
        second = int(row["second_camera"])
        raw = float(row["raw_signed_seam_mm"])
        corrected = raw + float(offsets[first - 1] - offsets[second - 1])
        item = dict(row)
        item.update({
            "validation": "external_fixed_model_no_refit",
            "camera_y_sync_model_id": model_id,
            "corrected_signed_seam_mm": corrected,
            "raw_abs_seam_mm": abs(raw),
            "corrected_abs_seam_mm": abs(corrected),
        })
        output.append(item)
    return output


def comparison_rows(
    raw_rows: list[Dict[str, object]],
    corrected_rows: list[Dict[str, object]],
) -> Tuple[list[Dict[str, object]], list[Dict[str, object]]]:
    corrected_by_capture = {str(row["capture"]): row for row in corrected_rows}
    wide = []
    delta = []
    for raw in raw_rows:
        corrected = corrected_by_capture[str(raw["capture"])]
        item: Dict[str, object] = {
            "capture": raw["capture"],
            "group": raw["group"],
            "fold": raw["fold"],
            "validation": "paired_leave_one_fold_out",
            "camera_y_sync_model_id": corrected["camera_y_sync_model_id"],
            "uses_endface_truth": False,
            "final_angle_correction_applied": False,
        }
        impacts = []
        for end in ENDS:
            item[f"raw_{end}_seam_rms_mm"] = raw[f"{end}_seam_rms_mm"]
            item[f"corrected_{end}_seam_rms_mm"] = corrected[f"{end}_seam_rms_mm"]
            for channel in LOCAL_CHANNELS:
                field = f"{end}_{channel}_angle_deg"
                raw_value = float(raw[field])
                corrected_value = float(corrected[field])
                difference = corrected_value - raw_value
                impacts.append(abs(difference))
                item[f"raw_{field}"] = raw_value
                item[f"corrected_{field}"] = corrected_value
                item[f"delta_{field}"] = difference
                delta.append({
                    "capture": raw["capture"],
                    "group": raw["group"],
                    "fold": raw["fold"],
                    "end": end,
                    "local_channel": channel,
                    "raw_angle_deg": raw_value,
                    "cross_validated_y_corrected_angle_deg": corrected_value,
                    "corrected_minus_raw_deg": difference,
                    "abs_impact_deg": abs(difference),
                })
        item["median_abs_16_angle_impact_deg"] = float(np.median(impacts))
        item["max_abs_16_angle_impact_deg"] = float(np.max(impacts))
        wide.append(item)
    return wide, delta


def capture_seam_summary(rows: list[Dict[str, object]]) -> list[Dict[str, object]]:
    output = []
    for capture in sorted({str(row["capture"]) for row in rows}):
        selected = [row for row in rows if row["capture"] == capture]
        raw = finite(row["raw_signed_seam_mm"] for row in selected)
        corrected = finite(row["corrected_signed_seam_mm"] for row in selected)
        output.append({
            "capture": capture,
            "group": selected[0]["group"],
            "fold": selected[0]["fold"],
            "equation_count": len(selected),
            "raw_seam_rms_mm": float(np.sqrt(np.mean(np.square(raw)))),
            "corrected_seam_rms_mm": float(np.sqrt(np.mean(np.square(corrected)))),
            "raw_seam_p95_abs_mm": float(np.quantile(np.abs(raw), 0.95)),
            "corrected_seam_p95_abs_mm": float(np.quantile(np.abs(corrected), 0.95)),
            "seam_rms_improvement_percent": float(
                100.0 * (1.0 - np.sqrt(np.mean(np.square(corrected))) / np.sqrt(np.mean(np.square(raw))))
            ),
        })
    return output


def save_bar_svg(
    path: Path,
    title: str,
    labels: list[str],
    raw: list[float],
    corrected: list[float],
    y_label: str,
) -> None:
    width, height = 1100, 560
    left, top, plot_width, plot_height = 90.0, 75.0, 940.0, 390.0
    upper = max(raw + corrected + [1e-6]) * 1.18
    svg = [
        f'<svg xmlns="http://www.w3.org/2000/svg" width="{width}" height="{height}" viewBox="0 0 {width} {height}">',
        f'<rect width="{width}" height="{height}" fill="#ffffff"/>',
        f'<text x="550" y="34" text-anchor="middle" font-size="22" font-family="sans-serif">{title}</text>',
        f'<rect x="{left}" y="{top}" width="{plot_width}" height="{plot_height}" fill="#fafafa" stroke="#bdbdbd"/>',
        f'<text x="25" y="275" transform="rotate(-90 25 275)" text-anchor="middle" font-size="14" font-family="sans-serif">{y_label}</text>',
    ]
    group_width = plot_width / len(labels)
    for index, (label, raw_value, corrected_value) in enumerate(zip(labels, raw, corrected)):
        centre = left + group_width * (index + 0.5)
        for offset, value, color in ((-18.0, raw_value, "#9e9e9e"), (18.0, corrected_value, "#1976d2")):
            bar_height = value / upper * plot_height
            svg.append(
                f'<rect x="{centre + offset - 14:.2f}" y="{top + plot_height - bar_height:.2f}" '
                f'width="28" height="{bar_height:.2f}" fill="{color}"/>'
            )
        svg.append(
            f'<text x="{centre:.2f}" y="{top + plot_height + 28}" text-anchor="middle" '
            f'font-size="13" font-family="sans-serif">{label}</text>'
        )
    svg.extend([
        f'<text x="{left - 8}" y="{top + 10}" text-anchor="end" font-size="12" font-family="sans-serif">{upper:.3f}</text>',
        '<rect x="390" y="520" width="18" height="12" fill="#9e9e9e"/><text x="416" y="531" font-size="14" font-family="sans-serif">raw</text>',
        '<rect x="570" y="520" width="18" height="12" fill="#1976d2"/><text x="596" y="531" font-size="14" font-family="sans-serif">Y corrected holdout</text>',
        '</svg>',
    ])
    path.write_text("\n".join(svg), encoding="utf-8")


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Fit and blind-audit CTB camera Y synchronization")
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
    parser.add_argument(
        "--output-dir",
        type=Path,
        default=Path(__file__).resolve().parent / "results" / "ctb_endface_y_sync_audit",
    )
    parser.add_argument("--endface-window-mm", type=float, default=15.0)
    parser.add_argument(
        "--fixed-offset-model",
        type=Path,
        help=(
            "Optional v15 nested audit JSON. Its professional-bar M1 offsets are applied to "
            "CTB without refitting for external validation."
        ),
    )
    parser.add_argument(
        "--external-validation",
        type=Path,
        default=Path(r"C:\Users\Administrator\Downloads\SunshineSiRod_Endface\calibration_hobj\210_105"),
    )
    parser.add_argument(
        "--known-damaged-capture",
        action="append",
        default=["14_16.hobj"],
    )
    parser.add_argument("--skip-external-validation", action="store_true")
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    input_dir = args.input.resolve()
    output_dir = args.output_dir.resolve()
    paths = sorted(input_dir.glob("*.hobj"), key=lambda path: path.name.casefold())
    if len(paths) != 6:
        raise ValueError(f"Expected six CTB captures, found {len(paths)}")
    calibration = json.loads(args.camera_calibration.read_text(encoding="utf-8"))
    metadata = calibration.get("single_bar_metadata", {})
    if metadata.get("physical_bar_id") != "2606005B22-CTB_3":
        raise ValueError("Camera model is not the CTB single-bar camera model")
    captures = []
    for index, path in enumerate(paths):
        captures.append((path, "head_to_tail" if index < 3 else "tail_to_head", index % 3))

    geometries = []
    for index, (path, group, fold) in enumerate(captures, start=1):
        print(f"[{index}/6] extract edge geometry {group:12s} {path.name}", flush=True)
        geometries.append(
            extract_capture_geometry(path, group, fold, calibration, args.endface_window_mm)
        )
    equations = seam_equation_rows(geometries)

    fold_models: Dict[int, Tuple[np.ndarray, Dict[str, object]]] = {}
    fold_rows = []
    for fold in range(3):
        train_names = {geometry.path.name for geometry in geometries if geometry.fold != fold}
        offsets, details = fit_camera_y_offsets(
            equations,
            train_names,
            f"ctb_y_sync_fold_{fold + 1}_train4_holdout2",
        )
        fold_models[fold] = (offsets, details)
        fold_row: Dict[str, object] = {
            "fold": fold,
            "model_id": details["model_id"],
            "training_capture_ids": ";".join(details["training_capture_ids"]),
            "training_residual_rms_mm": details["training_residual_rms_mm"],
            "training_residual_p95_abs_mm": details["training_residual_p95_abs_mm"],
            "condition_number": details["weighted_design_condition_number"],
        }
        for obj in CAMERAS:
            fold_row[f"camera_{obj}_y_offset_mm"] = float(offsets[obj - 1])
            fold_row[f"camera_{obj}_y_offset_rows"] = float(offsets[obj - 1] / Y_SCALE_MM)
        fold_rows.append(fold_row)

    raw_measurements = []
    corrected_measurements = []
    for geometry in geometries:
        raw_measurements.append(measure_geometry(geometry, np.zeros(4), "raw_zero_offsets"))
        offsets, details = fold_models[geometry.fold]
        corrected_measurements.append(measure_geometry(geometry, offsets, str(details["model_id"])))

    seam_holdout = holdout_seam_rows(equations, fold_models)
    seam_summary = capture_seam_summary(seam_holdout)
    angle_wide, angle_delta = comparison_rows(raw_measurements, corrected_measurements)
    raw_equivariance = turnover_equivariance_rows(raw_measurements)
    corrected_equivariance = turnover_equivariance_rows(corrected_measurements)
    for row in raw_equivariance:
        row["measurement_state"] = "raw"
    for row in corrected_equivariance:
        row["measurement_state"] = "paired_leave_one_fold_out_y_corrected"

    all_names = {geometry.path.name for geometry in geometries}
    final_offsets, final_details = fit_camera_y_offsets(
        equations,
        all_names,
        "ctb_all_six_same_edge_y_sync_diagnostic_only",
    )
    end_specific_rows: list[Dict[str, object]] = []
    ctb_end_offsets: Dict[str, np.ndarray] = {}
    for end in ENDS:
        offsets, details = fit_camera_y_offsets(
            [row for row in equations if row["end"] == end],
            all_names,
            f"ctb_{end}_only_nonapplicable_diagnostic",
            minimum_equations=16,
        )
        ctb_end_offsets[end] = offsets
        item: Dict[str, object] = {
            "dataset": "CTB",
            "end": end,
            "applied_as_correction": False,
            "diagnostic_reason": "test whether a universal higher-order camera-Y scale is identifiable",
            "residual_rms_mm": details["training_residual_rms_mm"],
        }
        for obj in CAMERAS:
            item[f"camera_{obj}_y_offset_mm"] = float(offsets[obj - 1])
        end_specific_rows.append(item)
    offset_stack = np.stack([model[0] for model in fold_models.values()])
    fold_spread = np.ptp(offset_stack, axis=0)
    raw_seam = finite(row["raw_signed_seam_mm"] for row in seam_holdout)
    corrected_seam = finite(row["corrected_signed_seam_mm"] for row in seam_holdout)
    raw_eq = finite(row["group_median_sum_residual_deg"] for row in raw_equivariance)
    corrected_eq = finite(row["group_median_sum_residual_deg"] for row in corrected_equivariance)
    angle_impacts = finite(row["abs_impact_deg"] for row in angle_delta)
    raw_seam_rms = float(np.sqrt(np.mean(np.square(raw_seam))))
    corrected_seam_rms = float(np.sqrt(np.mean(np.square(corrected_seam))))
    raw_eq_rms = float(np.sqrt(np.mean(np.square(raw_eq))))
    corrected_eq_rms = float(np.sqrt(np.mean(np.square(corrected_eq))))
    fixed_external_summary: Dict[str, object] = {
        "status": "skipped",
        "reason": "no_fixed_offset_model",
    }
    fixed_external_outputs: Dict[str, list[Dict[str, object]]] = {}
    combined_nested_audit_path: Optional[Path] = None
    if args.fixed_offset_model:
        fixed_model_path = args.fixed_offset_model.resolve()
        fixed_model = json.loads(fixed_model_path.read_text(encoding="utf-8"))
        truth_usage = fixed_model.get("institution_or_manual_endface_truth_usage")
        m1 = fixed_model.get("model_comparison", {}).get("M1", {})
        if not (
            int(fixed_model.get("version", 0)) >= 15
            and isinstance(truth_usage, dict)
            and truth_usage.get("loaded") is False
            and truth_usage.get("used_for_fit") is False
            and truth_usage.get("used_for_validation") is False
            and truth_usage.get("used_for_runtime") is False
            and isinstance(m1, dict)
            and m1.get("model_level") == "M1"
            and isinstance(m1.get("camera_y_offsets_mm"), dict)
        ):
            raise ValueError("Fixed external model is not a truth-free v15 M1 audit")
        fixed_offsets = np.asarray(
            [float(m1["camera_y_offsets_mm"][str(obj)]) for obj in CAMERAS],
            dtype=float,
        )
        fixed_model_id = f"{fixed_model_path.stem}:M1:no_refit"
        fixed_measurements = [
            measure_geometry(geometry, fixed_offsets, fixed_model_id)
            for geometry in geometries
        ]
        fixed_equivariance = turnover_equivariance_rows(fixed_measurements)
        for row in fixed_equivariance:
            row["measurement_state"] = "professional_M1_applied_to_CTB_no_refit"
        fixed_seams = fixed_offset_seam_rows(equations, fixed_offsets, fixed_model_id)
        fixed_seam_values = finite(row["corrected_signed_seam_mm"] for row in fixed_seams)
        fixed_eq_values = finite(
            row["group_median_sum_residual_deg"] for row in fixed_equivariance
        )
        fixed_seam_rms = float(np.sqrt(np.mean(np.square(fixed_seam_values))))
        fixed_eq_rms = float(np.sqrt(np.mean(np.square(fixed_eq_values))))
        fixed_eq_max = float(np.max(np.abs(fixed_eq_values)))
        seam_improvement = 1.0 - fixed_seam_rms / max(raw_seam_rms, 1e-12)
        equivariance_improvement = 1.0 - fixed_eq_rms / max(raw_eq_rms, 1e-12)
        fixed_failures = []
        if seam_improvement <= 0.0:
            fixed_failures.append("fixed M1 does not improve CTB same-edge seam RMS")
        if equivariance_improvement < 0.20:
            fixed_failures.append(
                f"fixed M1 improves CTB turnover RMS by only {100.0 * equivariance_improvement:.2f}%"
            )
        if fixed_eq_max > 0.60:
            fixed_failures.append(
                f"fixed M1 CTB turnover max error {fixed_eq_max:.6f} deg exceeds 0.600000 deg"
            )
        fixed_external_summary = {
            "status": "completed",
            "input_model": str(fixed_model_path),
            "source_model_level": "M1",
            "model_refit_on_ctb": False,
            "uses_endface_truth": False,
            "camera_y_offsets_mm": {
                str(obj): float(fixed_offsets[obj - 1]) for obj in CAMERAS
            },
            "raw_same_edge_seam_rms_mm": raw_seam_rms,
            "fixed_m1_same_edge_seam_rms_mm": fixed_seam_rms,
            "same_edge_seam_improvement_percent": 100.0 * seam_improvement,
            "raw_turnover_equivariance_rms_deg": raw_eq_rms,
            "fixed_m1_turnover_equivariance_rms_deg": fixed_eq_rms,
            "fixed_m1_turnover_max_abs_error_deg": fixed_eq_max,
            "turnover_equivariance_improvement_percent": 100.0
            * equivariance_improvement,
            "passed": not fixed_failures,
            "failures": fixed_failures,
        }
        fixed_external_outputs = {
            "seams": fixed_seams,
            "equivariance": raw_equivariance + fixed_equivariance,
            "measurements": fixed_measurements,
        }
        combined_model = json.loads(json.dumps(fixed_model))
        combined_model["external_noncalibration_bar_validation"] = fixed_external_summary
        combined_failures = [str(value) for value in combined_model.get("failures", [])]
        if fixed_failures:
            combined_failures.extend(
                "CTB external no-refit M1 gate " + value for value in fixed_failures
            )
        combined_model["failures"] = combined_failures
        combined_model["valid"] = False
        combined_model["runtime_selected_model_level"] = "M0"
        combined_model["runtime_correction_applied"] = False
        combined_readiness = combined_model.get("release_readiness")
        if not isinstance(combined_readiness, dict):
            combined_readiness = {}
        combined_readiness["ready"] = False
        combined_readiness["selected_model_level"] = "M0"
        combined_readiness["external_noncalibration_bar_validation_passed"] = bool(
            fixed_external_summary["passed"]
        )
        combined_readiness["reason"] = "; ".join(combined_failures)
        combined_model["release_readiness"] = combined_readiness
        output_dir.mkdir(parents=True, exist_ok=True)
        combined_nested_audit_path = (
            output_dir / "endface_v15_nested_with_ctb_rejected.json"
        )
        combined_nested_audit_path.write_text(
            json.dumps(combined_model, ensure_ascii=False, indent=2),
            encoding="utf-8",
        )
        fixed_external_summary["combined_nested_audit_path"] = str(
            combined_nested_audit_path
        )
    model = {
        "version": 12,
        "model": "endface_camera_geometry_calibration",
        "strategy": "physical_same_edge_camera_scan_row_synchronization",
        "valid": False,
        "release_readiness": {"ready": False},
        "scope": "ctb_single_bar_diagnostic_only",
        "sample_id": "2606005B22-CTB_3",
        "camera_y_offsets_mm": {str(obj): float(final_offsets[obj - 1]) for obj in CAMERAS},
        "camera_y_offsets_rows": {
            str(obj): float(final_offsets[obj - 1] / Y_SCALE_MM) for obj in CAMERAS
        },
        "offset_gauge_constraint": "sum(camera_y_offsets_mm)=0",
        "y_coordinate_correction": {
            "apply_to": "all_longitudinal_side_points_and_end_boundary_points",
        },
        "fit": final_details,
        "paired_leave_one_fold_out": {
            "raw_same_edge_seam_rms_mm": raw_seam_rms,
            "corrected_same_edge_seam_rms_mm": corrected_seam_rms,
            "same_edge_seam_improvement_percent": 100.0 * (1.0 - corrected_seam_rms / raw_seam_rms),
            "camera_offset_fold_spread_mm": {
                str(obj): float(fold_spread[obj - 1]) for obj in CAMERAS
            },
            "raw_turnover_equivariance_rms_deg": raw_eq_rms,
            "corrected_turnover_equivariance_rms_deg": corrected_eq_rms,
            "turnover_equivariance_improvement_percent": 100.0 * (1.0 - corrected_eq_rms / raw_eq_rms),
        },
        "uses_endface_truth": False,
        "forbidden_final_angle_parameters_present": False,
        "release_blocker": (
            "This six-capture same-bar audit can test fixed camera timing repeatability but cannot "
            "prove cross-bar validity or separate all local edge curvature from camera timing."
        ),
    }

    external_summary: Dict[str, object] = {
        "status": "skipped",
        "reason": "disabled_or_missing_path",
    }
    external_outputs: Dict[str, list[Dict[str, object]]] = {}
    external_root = args.external_validation.resolve()
    if not args.skip_external_validation and external_root.is_dir():
        external_geometries = []
        external_paths = []
        for group in ("head_to_tail", "tail_to_head"):
            group_paths = sorted((external_root / group).glob("*.hobj"), key=lambda path: path.name.casefold())
            if not group_paths:
                raise ValueError(f"External validation group has no HOBJ: {external_root / group}")
            external_paths.extend((path, group, fold) for fold, path in enumerate(group_paths))
        for index, (path, group, fold) in enumerate(external_paths, start=1):
            print(
                f"[{index}/{len(external_paths)}] external no-refit {group:12s} {path.name}",
                flush=True,
            )
            external_geometries.append(
                extract_capture_geometry(path, group, fold, calibration, args.endface_window_mm)
            )
        external_equations = seam_equation_rows(external_geometries)
        external_seams = fixed_offset_seam_rows(
            external_equations,
            final_offsets,
            str(final_details["model_id"]),
        )
        external_raw = [
            measure_geometry(geometry, np.zeros(4), "raw_zero_offsets")
            for geometry in external_geometries
        ]
        external_corrected = [
            measure_geometry(geometry, final_offsets, str(final_details["model_id"]))
            for geometry in external_geometries
        ]
        damaged = {name.casefold() for name in args.known_damaged_capture}
        valid_names = {
            geometry.path.name
            for geometry in external_geometries
            if geometry.path.name.casefold() not in damaged
        }
        for row in external_seams:
            row["known_damaged_excluded_from_aggregate"] = str(row["capture"]) not in valid_names
        valid_seams = [row for row in external_seams if str(row["capture"]) in valid_names]
        valid_raw = [row for row in external_raw if str(row["capture"]) in valid_names]
        valid_corrected = [row for row in external_corrected if str(row["capture"]) in valid_names]
        external_raw_eq = turnover_equivariance_rows(valid_raw)
        external_corrected_eq = turnover_equivariance_rows(valid_corrected)
        for row in external_raw_eq:
            row["measurement_state"] = "raw_external_no_refit"
        for row in external_corrected_eq:
            row["measurement_state"] = "ctb_y_sync_external_no_refit"
        external_wide, external_delta = comparison_rows(external_raw, external_corrected)
        for row in external_wide:
            row["known_damaged_excluded_from_aggregate"] = str(row["capture"]) not in valid_names
            row["validation"] = "external_fixed_model_no_refit"
        for row in external_delta:
            row["known_damaged_excluded_from_aggregate"] = str(row["capture"]) not in valid_names
            row["validation"] = "external_fixed_model_no_refit"
        external_seam_summary = capture_seam_summary(external_seams)
        for row in external_seam_summary:
            row["known_damaged_excluded_from_aggregate"] = str(row["capture"]) not in valid_names
        ext_raw_seam = finite(row["raw_signed_seam_mm"] for row in valid_seams)
        ext_corrected_seam = finite(row["corrected_signed_seam_mm"] for row in valid_seams)
        ext_raw_eq_values = finite(
            row["group_median_sum_residual_deg"] for row in external_raw_eq
        )
        ext_corrected_eq_values = finite(
            row["group_median_sum_residual_deg"] for row in external_corrected_eq
        )
        ext_raw_seam_rms = float(np.sqrt(np.mean(np.square(ext_raw_seam))))
        ext_corrected_seam_rms = float(np.sqrt(np.mean(np.square(ext_corrected_seam))))
        ext_raw_eq_rms = float(np.sqrt(np.mean(np.square(ext_raw_eq_values))))
        ext_corrected_eq_rms = float(np.sqrt(np.mean(np.square(ext_corrected_eq_values))))
        external_summary = {
            "status": "completed",
            "input_root": str(external_root),
            "capture_count": len(external_geometries),
            "aggregate_capture_count": len(valid_names),
            "known_damaged_excluded": sorted(
                geometry.path.name
                for geometry in external_geometries
                if geometry.path.name not in valid_names
            ),
            "model_refit_on_external_data": False,
            "raw_same_edge_seam_rms_mm": ext_raw_seam_rms,
            "corrected_same_edge_seam_rms_mm": ext_corrected_seam_rms,
            "same_edge_seam_improvement_percent": 100.0
            * (1.0 - ext_corrected_seam_rms / ext_raw_seam_rms),
            "raw_turnover_equivariance_rms_deg": ext_raw_eq_rms,
            "corrected_turnover_equivariance_rms_deg": ext_corrected_eq_rms,
            "turnover_equivariance_improvement_percent": 100.0
            * (1.0 - ext_corrected_eq_rms / ext_raw_eq_rms),
        }

        professional_fold_models: Dict[int, Tuple[np.ndarray, Dict[str, object]]] = {}
        professional_fold_rows = []
        professional_folds = sorted({geometry.fold for geometry in external_geometries})
        for fold in professional_folds:
            train_names = {
                geometry.path.name
                for geometry in external_geometries
                if geometry.fold != fold and geometry.path.name in valid_names
            }
            offsets, details = fit_camera_y_offsets(
                external_equations,
                train_names,
                f"professional_y_sync_fold_{fold + 1}_paired_holdout",
            )
            professional_fold_models[fold] = (offsets, details)
            item: Dict[str, object] = {
                "fold": fold,
                "model_id": details["model_id"],
                "training_capture_count": len(train_names),
                "training_capture_ids": ";".join(sorted(train_names)),
                "training_residual_rms_mm": details["training_residual_rms_mm"],
                "condition_number": details["weighted_design_condition_number"],
            }
            for obj in CAMERAS:
                item[f"camera_{obj}_y_offset_mm"] = float(offsets[obj - 1])
                item[f"camera_{obj}_y_offset_rows"] = float(offsets[obj - 1] / Y_SCALE_MM)
            professional_fold_rows.append(item)

        professional_holdout_seams = holdout_seam_rows(
            external_equations,
            professional_fold_models,
        )
        for row in professional_holdout_seams:
            row["known_damaged_excluded_from_aggregate"] = str(row["capture"]) not in valid_names
        professional_corrected = []
        for geometry in external_geometries:
            offsets, details = professional_fold_models[geometry.fold]
            professional_corrected.append(
                measure_geometry(geometry, offsets, str(details["model_id"]))
            )
        professional_valid_corrected = [
            row for row in professional_corrected if str(row["capture"]) in valid_names
        ]
        professional_corrected_eq = turnover_equivariance_rows(professional_valid_corrected)
        for row in professional_corrected_eq:
            row["measurement_state"] = "professional_paired_leave_one_fold_out_y_corrected"
        professional_wide, professional_delta = comparison_rows(
            external_raw,
            professional_corrected,
        )
        for row in professional_wide:
            row["known_damaged_excluded_from_aggregate"] = str(row["capture"]) not in valid_names
            row["validation"] = "professional_paired_leave_one_fold_out"
        for row in professional_delta:
            row["known_damaged_excluded_from_aggregate"] = str(row["capture"]) not in valid_names
            row["validation"] = "professional_paired_leave_one_fold_out"
        professional_valid_seams = [
            row
            for row in professional_holdout_seams
            if str(row["capture"]) in valid_names
        ]
        professional_raw_seam = finite(
            row["raw_signed_seam_mm"] for row in professional_valid_seams
        )
        professional_corrected_seam = finite(
            row["corrected_signed_seam_mm"] for row in professional_valid_seams
        )
        professional_corrected_eq_values = finite(
            row["group_median_sum_residual_deg"] for row in professional_corrected_eq
        )
        professional_raw_seam_rms = float(
            np.sqrt(np.mean(np.square(professional_raw_seam)))
        )
        professional_corrected_seam_rms = float(
            np.sqrt(np.mean(np.square(professional_corrected_seam)))
        )
        professional_corrected_eq_rms = float(
            np.sqrt(np.mean(np.square(professional_corrected_eq_values)))
        )
        professional_offset_stack = np.stack(
            [professional_fold_models[fold][0] for fold in professional_folds]
        )
        professional_offset_spread = np.ptp(professional_offset_stack, axis=0)
        professional_final_offsets, professional_final_details = fit_camera_y_offsets(
            external_equations,
            valid_names,
            "professional_19_valid_same_edge_y_sync_diagnostic_only",
        )
        professional_end_offsets: Dict[str, np.ndarray] = {}
        for end in ENDS:
            offsets, details = fit_camera_y_offsets(
                [row for row in external_equations if row["end"] == end],
                valid_names,
                f"professional_{end}_only_nonapplicable_diagnostic",
                minimum_equations=16,
            )
            professional_end_offsets[end] = offsets
            item = {
                "dataset": "professional_210_105",
                "end": end,
                "applied_as_correction": False,
                "diagnostic_reason": "test whether a universal higher-order camera-Y scale is identifiable",
                "residual_rms_mm": details["training_residual_rms_mm"],
            }
            for obj in CAMERAS:
                item[f"camera_{obj}_y_offset_mm"] = float(offsets[obj - 1])
            end_specific_rows.append(item)
        ctb_end_delta = ctb_end_offsets["tail"] - ctb_end_offsets["head"]
        professional_end_delta = (
            professional_end_offsets["tail"] - professional_end_offsets["head"]
        )
        end_differential_disagreement = professional_end_delta - ctb_end_delta
        professional_model = {
            "version": 12,
            "model": "endface_camera_geometry_calibration",
            "strategy": "physical_same_edge_camera_scan_row_synchronization",
            "valid": False,
            "release_readiness": {"ready": False},
            "scope": "professional_single_bar_diagnostic_only",
            "camera_y_offsets_mm": {
                str(obj): float(professional_final_offsets[obj - 1]) for obj in CAMERAS
            },
            "camera_y_offsets_rows": {
                str(obj): float(professional_final_offsets[obj - 1] / Y_SCALE_MM)
                for obj in CAMERAS
            },
            "offset_gauge_constraint": "sum(camera_y_offsets_mm)=0",
            "y_coordinate_correction": {
                "apply_to": "all_longitudinal_side_points_and_end_boundary_points",
            },
            "fit": professional_final_details,
            "uses_endface_truth": False,
            "forbidden_final_angle_parameters_present": False,
            "known_damaged_capture_excluded": sorted(
                geometry.path.name
                for geometry in external_geometries
                if geometry.path.name not in valid_names
            ),
            "paired_leave_one_fold_out": {
                "raw_same_edge_seam_rms_mm": professional_raw_seam_rms,
                "corrected_same_edge_seam_rms_mm": professional_corrected_seam_rms,
                "raw_turnover_equivariance_rms_deg": ext_raw_eq_rms,
                "corrected_turnover_equivariance_rms_deg": professional_corrected_eq_rms,
                "camera_offset_fold_spread_mm": {
                    str(obj): float(professional_offset_spread[obj - 1])
                    for obj in CAMERAS
                },
            },
            "release_blocker": (
                "Professional same-bar physical consistency is audited, but release still requires "
                "the configured camera-state gate and all v12+ package acceptance checks."
            ),
        }
        professional_model_path = (
            output_dir / "professional_endface_y_sync_model_diagnostic_only.json"
        )
        professional_model_path.parent.mkdir(parents=True, exist_ok=True)
        professional_model_path.write_text(
            json.dumps(professional_model, ensure_ascii=False, indent=2),
            encoding="utf-8",
        )
        external_summary["professional_same_bar_paired_holdout"] = {
            "valid_capture_count": len(valid_names),
            "raw_same_edge_seam_rms_mm": professional_raw_seam_rms,
            "corrected_same_edge_seam_rms_mm": professional_corrected_seam_rms,
            "same_edge_seam_improvement_percent": 100.0
            * (1.0 - professional_corrected_seam_rms / professional_raw_seam_rms),
            "raw_turnover_equivariance_rms_deg": ext_raw_eq_rms,
            "corrected_turnover_equivariance_rms_deg": professional_corrected_eq_rms,
            "turnover_equivariance_improvement_percent": 100.0
            * (1.0 - professional_corrected_eq_rms / ext_raw_eq_rms),
            "camera_offset_fold_max_spread_mm": float(
                np.max(professional_offset_spread)
            ),
            "camera_y_offsets_mm": {
                str(obj): float(professional_final_offsets[obj - 1]) for obj in CAMERAS
            },
            "difference_from_ctb_offsets_mm": {
                str(obj): float(professional_final_offsets[obj - 1] - final_offsets[obj - 1])
                for obj in CAMERAS
            },
            "candidate_model_path": str(professional_model_path),
            "candidate_model_releaseable": False,
            "higher_order_camera_y_scale_identifiable": False,
            "ctb_tail_minus_head_offset_mm": {
                str(obj): float(ctb_end_delta[obj - 1]) for obj in CAMERAS
            },
            "professional_tail_minus_head_offset_mm": {
                str(obj): float(professional_end_delta[obj - 1]) for obj in CAMERAS
            },
            "cross_bar_tail_minus_head_disagreement_mm": {
                str(obj): float(end_differential_disagreement[obj - 1])
                for obj in CAMERAS
            },
            "higher_order_rejection_reason": (
                "Head/tail differential camera offsets do not reproduce across CTB and the "
                "professional bar, so a per-camera Y scale would absorb real end-edge shape."
            ),
        }
        external_outputs = {
            "seams": external_seams,
            "seam_summary": external_seam_summary,
            "angles": external_wide,
            "angle_deltas": external_delta,
            "equivariance": external_raw_eq + external_corrected_eq,
            "professional_fold_models": professional_fold_rows,
            "professional_holdout_seams": professional_holdout_seams,
            "professional_angles": professional_wide,
            "professional_angle_deltas": professional_delta,
            "professional_equivariance": external_raw_eq + professional_corrected_eq,
        }

    output_dir.mkdir(parents=True, exist_ok=True)
    model_path = output_dir / "ctb_endface_y_sync_model_diagnostic_only.json"
    model_path.write_text(json.dumps(model, ensure_ascii=False, indent=2), encoding="utf-8")
    write_csv(output_dir / "ctb_y_sync_fold_models.csv", fold_rows)
    write_csv(output_dir / "end_specific_y_offsets_nonapplicable_diagnostic.csv", end_specific_rows)
    write_csv(output_dir / "ctb_y_sync_holdout_same_edge_seams.csv", seam_holdout)
    write_csv(output_dir / "ctb_y_sync_holdout_capture_summary.csv", seam_summary)
    write_csv(output_dir / "ctb_endface_16_angles_raw_and_y_corrected.csv", angle_wide)
    write_csv(output_dir / "ctb_endface_y_angle_deltas.csv", angle_delta)
    write_csv(
        output_dir / "ctb_endface_y_turnover_equivariance.csv",
        raw_equivariance + corrected_equivariance,
    )
    if external_outputs:
        write_csv(output_dir / "external_y_sync_same_edge_seams.csv", external_outputs["seams"])
        write_csv(
            output_dir / "external_y_sync_capture_summary.csv",
            external_outputs["seam_summary"],
        )
        write_csv(
            output_dir / "external_endface_16_angles_raw_and_y_corrected.csv",
            external_outputs["angles"],
        )
        write_csv(
            output_dir / "external_endface_y_angle_deltas.csv",
            external_outputs["angle_deltas"],
        )
        write_csv(
            output_dir / "external_endface_y_turnover_equivariance.csv",
            external_outputs["equivariance"],
        )
        write_csv(
            output_dir / "professional_y_sync_fold_models.csv",
            external_outputs["professional_fold_models"],
        )
        write_csv(
            output_dir / "professional_y_sync_holdout_same_edge_seams.csv",
            external_outputs["professional_holdout_seams"],
        )
        write_csv(
            output_dir / "professional_endface_16_angles_raw_and_y_corrected.csv",
            external_outputs["professional_angles"],
        )
        write_csv(
            output_dir / "professional_endface_y_angle_deltas.csv",
            external_outputs["professional_angle_deltas"],
        )
        write_csv(
            output_dir / "professional_endface_y_turnover_equivariance.csv",
            external_outputs["professional_equivariance"],
        )
    if fixed_external_outputs:
        write_csv(
            output_dir / "ctb_external_fixed_m1_same_edge_seams.csv",
            fixed_external_outputs["seams"],
        )
        write_csv(
            output_dir / "ctb_external_fixed_m1_turnover_equivariance.csv",
            fixed_external_outputs["equivariance"],
        )
        write_csv(
            output_dir / "ctb_external_fixed_m1_16_angles.csv",
            fixed_external_outputs["measurements"],
        )
    save_bar_svg(
        output_dir / "01_holdout_same_edge_seam_rms.svg",
        "CTB same-edge camera Y synchronization: paired holdout",
        [str(row["capture"]) for row in seam_summary],
        [float(row["raw_seam_rms_mm"]) for row in seam_summary],
        [float(row["corrected_seam_rms_mm"]) for row in seam_summary],
        "same-edge seam RMS (mm)",
    )
    raw_by_channel = {
        (str(row["physical_end_in_head_to_tail"]), str(row["local_channel_in_head_to_tail"])): abs(
            float(row["group_median_sum_residual_deg"])
        )
        for row in raw_equivariance
    }
    corrected_by_channel = {
        (str(row["physical_end_in_head_to_tail"]), str(row["local_channel_in_head_to_tail"])): abs(
            float(row["group_median_sum_residual_deg"])
        )
        for row in corrected_equivariance
    }
    labels = [f"{end}-{channel}" for end in ENDS for channel in LOCAL_CHANNELS]
    keys = [(end, channel) for end in ENDS for channel in LOCAL_CHANNELS]
    save_bar_svg(
        output_dir / "02_turnover_equivariance_raw_vs_y_corrected.svg",
        "CTB 16-angle physical-turnover equivariance",
        labels,
        [raw_by_channel[key] for key in keys],
        [corrected_by_channel[key] for key in keys],
        "abs(theta + mapped theta - 180), degrees",
    )
    summary = {
        "algorithm": "truth_free_same_edge_three_parameter_camera_y_synchronization",
        "capture_count": len(geometries),
        "equation_count": len(equations),
        "uses_endface_truth": False,
        "final_angle_correction_applied": False,
        "raw_holdout_same_edge_seam_rms_mm": raw_seam_rms,
        "corrected_holdout_same_edge_seam_rms_mm": corrected_seam_rms,
        "holdout_same_edge_seam_improvement_percent": 100.0 * (1.0 - corrected_seam_rms / raw_seam_rms),
        "fold_camera_y_offset_max_spread_mm": float(np.max(fold_spread)),
        "all_six_camera_y_offsets_mm": {str(obj): float(final_offsets[obj - 1]) for obj in CAMERAS},
        "all_six_camera_y_offsets_rows": {
            str(obj): float(final_offsets[obj - 1] / Y_SCALE_MM) for obj in CAMERAS
        },
        "raw_turnover_equivariance_rms_deg": raw_eq_rms,
        "corrected_turnover_equivariance_rms_deg": corrected_eq_rms,
        "turnover_equivariance_improvement_percent": 100.0 * (1.0 - corrected_eq_rms / raw_eq_rms),
        "angle_abs_impact_median_deg": float(np.median(angle_impacts)),
        "angle_abs_impact_p95_deg": float(np.quantile(angle_impacts, 0.95)),
        "angle_abs_impact_max_deg": float(np.max(angle_impacts)),
        "candidate_model_releaseable": False,
        "candidate_model_path": str(model_path),
        "combined_nested_audit_path": (
            str(combined_nested_audit_path) if combined_nested_audit_path else ""
        ),
        "external_fixed_professional_m1_validation": fixed_external_summary,
        "external_noncalibration_bar_validation": external_summary,
    }
    summary_path = output_dir / "ctb_endface_y_sync_summary.json"
    summary_path.write_text(json.dumps(summary, ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"Audit output: {output_dir}")
    print(
        f"Holdout seam RMS: {raw_seam_rms:.6f} -> {corrected_seam_rms:.6f} mm "
        f"({summary['holdout_same_edge_seam_improvement_percent']:.2f}% improvement)"
    )
    print(
        f"Turnover equivariance RMS: {raw_eq_rms:.6f} -> {corrected_eq_rms:.6f} deg "
        f"({summary['turnover_equivariance_improvement_percent']:.2f}% improvement)"
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
