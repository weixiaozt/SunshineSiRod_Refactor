#!/usr/bin/env python3
"""Export one-row-per-HOBJ final measurements for the virtual-bar audit algorithm.

This is an offline diagnostic exporter.  It estimates the repeatable local X/Z
camera drift field from the professional 20 HOBJ, applies that field by absolute
device scan row, and writes final aggregate values only.  It does not write a
production calibration model and does not apply final A/B/C/D answer offsets.
"""

from __future__ import annotations

import argparse
import csv
import json
import math
import sys
from pathlib import Path
from typing import Dict, Iterable, Optional

import numpy as np

try:
    from .measure_square_rod_edges import (
        HobjSource,
        OBJECT_TO_POINT,
        X_SCALE_MM,
        Y_SCALE_MM,
        calibration_for_orientation,
        cross_section_geometry,
        extract_corner_from_row,
        source_common_range,
        source_valid_ranges,
        subtract_local_drift,
    )
    from .virtual_perfect_bar_drift_audit import (
        GROUPS,
        build_drift_field,
        group_capture_paths,
        hobj_paths,
        ideal_fit_residuals,
        ideal_points,
        interpolate_shift,
        thickness_from_face_observations,
    )
except ImportError:
    sys.path.insert(0, str(Path(__file__).resolve().parent))
    from measure_square_rod_edges import (
        HobjSource,
        OBJECT_TO_POINT,
        X_SCALE_MM,
        Y_SCALE_MM,
        calibration_for_orientation,
        cross_section_geometry,
        extract_corner_from_row,
        source_common_range,
        source_valid_ranges,
        subtract_local_drift,
    )
    from virtual_perfect_bar_drift_audit import (
        GROUPS,
        build_drift_field,
        group_capture_paths,
        hobj_paths,
        ideal_fit_residuals,
        ideal_points,
        interpolate_shift,
        thickness_from_face_observations,
    )


MEASUREMENT_FIELDS = [
    "A_mm",
    "B_mm",
    "C_mm",
    "D_mm",
    "diag1_M1_M2_mm",
    "diag2_M3_M4_mm",
    "thickness_AC_height_mm",
    "thickness_BD_width_mm",
    "P1_x",
    "P1_z",
    "P2_x",
    "P2_z",
    "P3_x",
    "P3_z",
    "P4_x",
    "P4_z",
    "M1_x",
    "M1_z",
    "M2_x",
    "M2_z",
    "M3_x",
    "M3_z",
    "M4_x",
    "M4_z",
    "obj1_angle_deg",
    "obj2_angle_deg",
    "obj3_angle_deg",
    "obj4_angle_deg",
    "obj1_chamfer_mm",
    "obj2_chamfer_mm",
    "obj3_chamfer_mm",
    "obj4_chamfer_mm",
    "obj1_projection_x_mm",
    "obj1_projection_y_mm",
    "obj2_projection_x_mm",
    "obj2_projection_y_mm",
    "obj3_projection_x_mm",
    "obj3_projection_y_mm",
    "obj4_projection_x_mm",
    "obj4_projection_y_mm",
    "obj1_chamfer_face1_setback_mm",
    "obj1_chamfer_face2_setback_mm",
    "obj2_chamfer_face1_setback_mm",
    "obj2_chamfer_face2_setback_mm",
    "obj3_chamfer_face1_setback_mm",
    "obj3_chamfer_face2_setback_mm",
    "obj4_chamfer_face1_setback_mm",
    "obj4_chamfer_face2_setback_mm",
    "same_face_A_pair_angle_deg",
    "same_face_B_pair_angle_deg",
    "same_face_C_pair_angle_deg",
    "same_face_D_pair_angle_deg",
    "virtual_fit_rms_mm",
    "raw_A_mm",
    "raw_B_mm",
    "raw_C_mm",
    "raw_D_mm",
    "raw_diag1_M1_M2_mm",
    "raw_diag2_M3_M4_mm",
    "raw_thickness_AC_height_mm",
    "raw_thickness_BD_width_mm",
    "raw_P1_x",
    "raw_P1_z",
    "raw_P2_x",
    "raw_P2_z",
    "raw_P3_x",
    "raw_P3_z",
    "raw_P4_x",
    "raw_P4_z",
    "raw_M1_x",
    "raw_M1_z",
    "raw_M2_x",
    "raw_M2_z",
    "raw_M3_x",
    "raw_M3_z",
    "raw_M4_x",
    "raw_M4_z",
    "raw_virtual_fit_rms_mm",
]


def classify_hobj_orientation(path: Path, professional_root: Path) -> tuple[str, str, str]:
    """Return drift group, camera orientation and non-image basis."""

    path_text = str(path).replace("\\", "/").casefold()
    professional_text = str(professional_root).replace("\\", "/").casefold()
    if path_text.startswith(professional_text):
        if "/tail_to_head/" in path_text:
            return "tail_to_head", "turnover", "professional_group_folder"
        return "head_to_tail", "normal", "professional_group_folder"
    if "tail_to_head" in path_text or "diaotou" in path_text or "调头" in str(path):
        return "tail_to_head", "turnover", "path_turnover_marker"
    return "head_to_tail", "normal", "path_default_normal"


def numeric_stats(values: Iterable[object]) -> Optional[Dict[str, float]]:
    cleaned: list[float] = []
    for value in values:
        try:
            number = float(value)
        except (TypeError, ValueError):
            continue
        if math.isfinite(number):
            cleaned.append(number)
    if not cleaned:
        return None
    array = np.asarray(cleaned, dtype=float)
    return {
        "mean": float(np.mean(array)),
        "std": float(np.std(array, ddof=0)),
        "min": float(np.min(array)),
        "max": float(np.max(array)),
        "range": float(np.max(array) - np.min(array)),
    }


def round_or_blank(value: object, digits: int = 6) -> object:
    try:
        number = float(value)
    except (TypeError, ValueError):
        return ""
    if not math.isfinite(number):
        return ""
    return round(number, digits)


def load_source(path: Path) -> tuple[HobjSource, Dict[int, tuple[int, int]], int, int]:
    source = HobjSource(str(path))
    valid_ranges = source_valid_ranges(source)
    common_start, common_end = source_common_range(source, valid_ranges)
    return source, valid_ranges, common_start, common_end


def one_capture_final_row(
    path: Path,
    dataset: str,
    professional_root: Path,
    calibration: Dict[str, object],
    reference: Dict[str, np.ndarray],
    fields: Dict[str, object],
    step_mm: float,
    ignore_end_percent: float,
) -> Dict[str, object]:
    group, orientation, orientation_basis = classify_hobj_orientation(path, professional_root)
    source, _, common_start, common_end = load_source(path)
    active = calibration_for_orientation(calibration, orientation)
    transforms = {int(key): value for key, value in active["transforms"].items()}
    field = fields[group]

    margin = int(round(ignore_end_percent * (common_end - common_start)))
    first_row = common_start + margin
    last_row = common_end - margin
    step_rows = max(1, int(round(step_mm / Y_SCALE_MM)))
    rows = list(range(first_row, last_row + 1, step_rows))

    samples: list[Dict[str, float]] = []
    invalid_rows = 0
    missing_shift_rows = 0
    for row_index in rows:
        raw_corners = {
            obj: extract_corner_from_row(source.row(obj, row_index), X_SCALE_MM)
            for obj in (1, 2, 3, 4)
        }
        if not all(corner.valid for corner in raw_corners.values()):
            invalid_rows += 1
            continue

        raw_points, raw_midpoints, raw_lengths = cross_section_geometry(
            raw_corners,
            transforms,
            active,
            "free",
        )
        raw_thickness = thickness_from_face_observations(raw_corners, transforms, active)
        _, _, raw_fit_rms, _, _ = ideal_fit_residuals(raw_points, reference)

        corrected_corners = {}
        for obj, corner in raw_corners.items():
            shift = interpolate_shift(field, obj, row_index)
            if shift is None:
                missing_shift_rows += 1
                corrected_corners = {}
                break
            corrected_corners[obj] = subtract_local_drift(corner, float(shift[0]), float(shift[1]))
        if not corrected_corners:
            continue

        points, midpoints, lengths = cross_section_geometry(
            corrected_corners,
            transforms,
            active,
            "free",
        )
        thickness = thickness_from_face_observations(corrected_corners, transforms, active)
        _, _, fit_rms, _, _ = ideal_fit_residuals(points, reference)

        sample: Dict[str, float] = {
            "A_mm": float(lengths["A"]),
            "B_mm": float(lengths["B"]),
            "C_mm": float(lengths["C"]),
            "D_mm": float(lengths["D"]),
            "diag1_M1_M2_mm": float(lengths["diag1"]),
            "diag2_M3_M4_mm": float(lengths["diag2"]),
            "thickness_AC_height_mm": float(thickness.get("thickness_ac_mm", math.nan)),
            "thickness_BD_width_mm": float(thickness.get("thickness_bd_mm", math.nan)),
            "virtual_fit_rms_mm": float(fit_rms),
            "raw_A_mm": float(raw_lengths["A"]),
            "raw_B_mm": float(raw_lengths["B"]),
            "raw_C_mm": float(raw_lengths["C"]),
            "raw_D_mm": float(raw_lengths["D"]),
            "raw_diag1_M1_M2_mm": float(raw_lengths["diag1"]),
            "raw_diag2_M3_M4_mm": float(raw_lengths["diag2"]),
            "raw_thickness_AC_height_mm": float(raw_thickness.get("thickness_ac_mm", math.nan)),
            "raw_thickness_BD_width_mm": float(raw_thickness.get("thickness_bd_mm", math.nan)),
            "raw_virtual_fit_rms_mm": float(raw_fit_rms),
        }
        for point_name in ("P1", "P2", "P3", "P4"):
            px, pz = points[point_name]
            sample[f"{point_name}_x"] = float(px)
            sample[f"{point_name}_z"] = float(pz)
            raw_px, raw_pz = raw_points[point_name]
            sample[f"raw_{point_name}_x"] = float(raw_px)
            sample[f"raw_{point_name}_z"] = float(raw_pz)
        for midpoint_name in ("M1", "M2", "M3", "M4"):
            mx, mz = midpoints[midpoint_name]
            sample[f"{midpoint_name}_x"] = float(mx)
            sample[f"{midpoint_name}_z"] = float(mz)
            raw_mx, raw_mz = raw_midpoints[midpoint_name]
            sample[f"raw_{midpoint_name}_x"] = float(raw_mx)
            sample[f"raw_{midpoint_name}_z"] = float(raw_mz)
        for face in "ABCD":
            key = f"same_face_{face}_pair_angle_deg"
            sample[key] = float(thickness.get(key, math.nan))
        for obj in (1, 2, 3, 4):
            corner = corrected_corners[obj]
            sample[f"obj{obj}_angle_deg"] = float(corner.angle_deg)
            sample[f"obj{obj}_chamfer_mm"] = float(corner.chamfer_mm)
            sample[f"obj{obj}_projection_x_mm"] = float(corner.projection_x_mm)
            sample[f"obj{obj}_projection_y_mm"] = float(corner.projection_y_mm)
            sample[f"obj{obj}_chamfer_face1_setback_mm"] = float(corner.chamfer_face1_setback_mm)
            sample[f"obj{obj}_chamfer_face2_setback_mm"] = float(corner.chamfer_face2_setback_mm)
        samples.append(sample)

    output: Dict[str, object] = {
        "dataset": dataset,
        "capture": path.name,
        "relative_path": str(path),
        "algorithm": "virtual_perfect_bar_absolute_row_drift_plus_thickness_v1",
        "diagnostic_only": True,
        "orientation_used": orientation,
        "drift_field_group": group,
        "orientation_basis": orientation_basis,
        "common_start_row": common_start,
        "common_end_row": common_end,
        "stick_length_mm": round((common_end - common_start) * Y_SCALE_MM, 6),
        "first_measured_row": first_row,
        "last_measured_row": last_row,
        "step_rows": step_rows,
        "requested_slice_count": len(rows),
        "valid_slice_count": len(samples),
        "invalid_corner_rows": invalid_rows,
        "missing_drift_rows": missing_shift_rows,
        "status": "ok" if samples else "no_valid_samples",
    }
    for field_name in MEASUREMENT_FIELDS:
        stats = numeric_stats(sample.get(field_name) for sample in samples)
        if stats is None:
            output[field_name] = ""
            output[f"{field_name}_std"] = ""
            output[f"{field_name}_min"] = ""
            output[f"{field_name}_max"] = ""
            output[f"{field_name}_range"] = ""
            continue
        output[field_name] = round(stats["mean"], 6)
        output[f"{field_name}_std"] = round(stats["std"], 6)
        output[f"{field_name}_min"] = round(stats["min"], 6)
        output[f"{field_name}_max"] = round(stats["max"], 6)
        output[f"{field_name}_range"] = round(stats["range"], 6)
    return output


def write_csv(path: Path, rows: list[Dict[str, object]]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
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


def main() -> int:
    parser = argparse.ArgumentParser(description="Export final values from virtual-bar diagnostic algorithm")
    parser.add_argument(
        "--professional-root",
        default=r"C:\Users\Administrator\Downloads\SunshineSiRod_Endface\calibration_hobj\210_105",
    )
    parser.add_argument(
        "--target-root",
        default=r"C:\Users\Administrator\Downloads\待测hobj",
    )
    parser.add_argument(
        "--calibration",
        default=str(Path(__file__).resolve().parent / "calibration" / "models" / "camera_calibration_model_210_105.json"),
    )
    parser.add_argument(
        "--output",
        default=str(Path(__file__).resolve().parent / "results" / "virtual_bar_final_measurements_all.csv"),
    )
    parser.add_argument("--long-edge-mm", type=float, default=210.0)
    parser.add_argument("--short-edge-mm", type=float, default=105.0)
    parser.add_argument("--drift-sample-count", type=int, default=19)
    parser.add_argument("--outlier-limit-mm", type=float, default=1.0)
    parser.add_argument("--step-mm", type=float, default=10.0)
    parser.add_argument("--ignore-end-percent", type=float, default=0.05)
    args = parser.parse_args()

    professional_root = Path(args.professional_root)
    target_root = Path(args.target_root)
    output_path = Path(args.output)
    with open(args.calibration, "r", encoding="utf-8") as handle:
        calibration = json.load(handle)
    reference = ideal_points(args.long_edge_mm, args.short_edge_mm)
    fractions = np.linspace(0.05, 0.95, max(5, int(args.drift_sample_count)))

    professional_groups = group_capture_paths(professional_root)
    fields: Dict[str, object] = {}
    for group, orientation in GROUPS:
        field, _ = build_drift_field(
            group,
            professional_groups[group],
            orientation,
            calibration,
            reference,
            fractions,
            args.outlier_limit_mm,
        )
        fields[group] = field

    rows: list[Dict[str, object]] = []
    professional_paths: list[Path] = []
    for group, _ in GROUPS:
        professional_paths.extend(professional_groups[group])
    for path in sorted(professional_paths, key=lambda item: str(item).casefold()):
        rows.append(
            one_capture_final_row(
                path,
                "professional_20",
                professional_root,
                calibration,
                reference,
                fields,
                args.step_mm,
                args.ignore_end_percent,
            )
        )
        print(f"done professional: {path.name}")

    for path in hobj_paths(target_root):
        rows.append(
            one_capture_final_row(
                path,
                "target_hobj",
                professional_root,
                calibration,
                reference,
                fields,
                args.step_mm,
                args.ignore_end_percent,
            )
        )
        print(f"done target: {path}")

    write_csv(output_path, rows)
    print(f"Final CSV written: {output_path}")
    print(f"Rows: {len(rows)}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
