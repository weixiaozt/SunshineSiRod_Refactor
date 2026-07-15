#!/usr/bin/env python3
"""Build a physically constrained four-camera end-face model on site.

Professional truth is used only to estimate device scan-row synchronization
between the four cameras.  The generated model contains no final-angle offset
and no production orientation classifier.  Every production result is rebuilt
from that HOBJ's boundary point clouds and longitudinal side planes.
"""

from __future__ import annotations

import argparse
import csv
import json
import math
import os
import sys
from dataclasses import dataclass, field
from datetime import datetime
from pathlib import Path
from typing import Callable, Dict, Iterable, Tuple

import numpy as np

try:
    from .measure_square_rod_edges import (
        HobjSource,
        X_SCALE_MM,
        Y_SCALE_MM,
        calibration_for_orientation,
        endface_boundary_points_for_camera,
        face_to_endface_angles,
        fit_rod_axis,
        longitudinal_side_plane_normals,
        mean_cross_section_points,
        robust_plane_y_from_xz,
        same_face_relative_camera_diagnostic,
        sampled_cross_section_rows_and_edges,
        source_common_range,
        source_valid_ranges,
    )
except ImportError:  # Direct script / PyInstaller entry point.
    from measure_square_rod_edges import (
        HobjSource,
        X_SCALE_MM,
        Y_SCALE_MM,
        calibration_for_orientation,
        endface_boundary_points_for_camera,
        face_to_endface_angles,
        fit_rod_axis,
        longitudinal_side_plane_normals,
        mean_cross_section_points,
        robust_plane_y_from_xz,
        same_face_relative_camera_diagnostic,
        sampled_cross_section_rows_and_edges,
        source_common_range,
        source_valid_ranges,
    )

try:
    from .endface_wireframe_geometry import (
        LOCAL_CHANNELS as WIREFRAME_LOCAL_CHANNELS,
        mapped_turnover_channel,
        measure_wireframe_angles,
    )
except ImportError:  # Direct script / PyInstaller entry point.
    from endface_wireframe_geometry import (
        LOCAL_CHANNELS as WIREFRAME_LOCAL_CHANNELS,
        mapped_turnover_channel,
        measure_wireframe_angles,
    )


ENDS = ("head", "tail")
FACES = ("A", "B", "C", "D")
CHANNELS = tuple((end, face) for end in ENDS for face in FACES)
WIREFRAME_CHANNELS = tuple(
    (end, local_channel)
    for end in ENDS
    for local_channel in WIREFRAME_LOCAL_CHANNELS
)
TURNOVER_MARKERS = ("turnover", "tail_to_head", "reverse", "diaotou", "调头")
INDEPENDENT_VALIDATION_FOLDERS = {"physical_turnover_before", "physical_turnover_after"}
# The institution is the only public face-label authority:
# A=top, B=right, C=left, D=bottom.  The historical measurement core uses
# A=top, B=right, C=bottom, D=left, so C/D are converted internally.
INSTITUTION_TO_SOFTWARE_FACE_MAP = {"A": "A", "B": "B", "C": "D", "D": "C"}
SOFTWARE_TO_INSTITUTION_FACE_MAP = {software: report for report, software in INSTITUTION_TO_SOFTWARE_FACE_MAP.items()}


@dataclass
class TruthData:
    report_id: str
    sample_id: str
    nominal_spec: str
    head_label: str
    tail_label: str
    face_map: Dict[str, str]
    angles: Dict[str, Dict[str, float]]
    reference_edges_mm: list[float] = field(default_factory=list)
    rod_length_mm: float = math.nan


@dataclass
class PhysicalCaptureMeasurement:
    """Image-derived calibration geometry with no fitted answer channels."""

    path: Path
    orientation: str
    split: str
    rod_axis: np.ndarray
    section_points: Dict[str, Tuple[float, float]]
    side_plane_normals: Dict[str, np.ndarray]
    boundary_points: Dict[str, Dict[int, np.ndarray]]
    scan_truth: Dict[str, Dict[str, float]]
    longitudinal_rows: list[Dict[str, object]] = field(default_factory=list)
    longitudinal_profile: np.ndarray | None = None
    row_order_status: str = "not_audited"
    row_order_same_rmse_mm: float = math.nan
    row_order_reversed_rmse_mm: float = math.nan
    relative_camera_diagnostic: Dict[str, object] = field(default_factory=dict)
    camera_valid_span_rows: Dict[int, int] = field(default_factory=dict)


def _nonempty(value: object) -> str:
    return str(value or "").strip()


def read_professional_truth(
    path: Path,
    sample_id: str,
    head_label: str,
) -> TruthData:
    """Read the institution report CSV without inventing missing readings."""

    with path.open("r", encoding="utf-8-sig", newline="") as handle:
        rows = list(csv.DictReader(handle))
    endface_rows = [row for row in rows if _nonempty(row.get("record_type")).lower() == "endface_angle"]
    if not endface_rows:
        raise ValueError("Truth CSV contains no record_type=endface_angle rows")
    available_samples = sorted({_nonempty(row.get("sample_id") or row.get("bar_id")) for row in endface_rows})
    available_samples = [value for value in available_samples if value]
    if sample_id:
        selected = [row for row in endface_rows if _nonempty(row.get("sample_id") or row.get("bar_id")) == sample_id]
        if not selected:
            raise ValueError(f"Sample {sample_id!r} not found. Available: {', '.join(available_samples)}")
    elif len(available_samples) == 1:
        sample_id = available_samples[0]
        selected = endface_rows
    else:
        raise ValueError(f"Truth CSV has multiple samples; provide --sample-id. Available: {', '.join(available_samples)}")

    labels = sorted({_nonempty(row.get("location") or row.get("end")).upper() for row in selected})
    labels = [value for value in labels if value]
    head_label = head_label.strip().upper()
    if len(labels) != 2:
        raise ValueError(f"Expected exactly two physical end labels for {sample_id}; got {labels}")
    if head_label not in labels:
        raise ValueError(f"--head-label {head_label!r} must be one of the report end labels: {labels}")
    tail_label = next(label for label in labels if label != head_label)
    label_to_end = {head_label: "head", tail_label: "tail"}

    angles: Dict[str, Dict[str, float]] = {end: {} for end in ENDS}
    seen: set[tuple[str, str]] = set()
    for row in selected:
        label = _nonempty(row.get("location") or row.get("end")).upper()
        report_face = _nonempty(row.get("face")).upper()
        if label not in label_to_end or report_face not in INSTITUTION_TO_SOFTWARE_FACE_MAP:
            continue
        try:
            angle = float(row.get("value") or row.get("angle_deg") or row.get("value_deg"))
        except (TypeError, ValueError) as exc:
            raise ValueError(f"Invalid angle for {sample_id} {label}-{report_face}") from exc
        if not math.isfinite(angle):
            raise ValueError(f"Non-finite angle for {sample_id} {label}-{report_face}")
        software_face = INSTITUTION_TO_SOFTWARE_FACE_MAP[report_face]
        end = label_to_end[label]
        key = (end, software_face)
        if key in seen:
            raise ValueError(f"Duplicate professional truth for {end}-{software_face}")
        seen.add(key)
        angles[end][software_face] = angle
    missing = [f"{end}-{face}" for end, face in CHANNELS if face not in angles[end]]
    if missing:
        raise ValueError(f"Professional truth is missing: {', '.join(missing)}")
    dimension_rows = [
        row for row in rows
        if _nonempty(row.get("record_type")).lower() == "cross_section_dimension"
        and _nonempty(row.get("sample_id") or row.get("bar_id")) == sample_id
    ]
    width_1: list[float] = []
    width_2: list[float] = []
    for row in dimension_rows:
        component = _nonempty(row.get("component")).lower()
        try:
            value = float(row.get("value") or row.get("value_mm"))
        except (TypeError, ValueError):
            continue
        if math.isfinite(value):
            if component == "width_1":
                width_1.append(value)
            elif component == "width_2":
                width_2.append(value)
    if width_1 and width_2:
        long_edge = float(np.mean(np.asarray(width_1, dtype=float)))
        short_edge = float(np.mean(np.asarray(width_2, dtype=float)))
    else:
        nominal_values = [float(value) for value in truth_nominal_numbers(_nonempty(selected[0].get("nominal_spec")))]
        if len(nominal_values) < 2:
            raise ValueError("Truth CSV needs cross-section dimensions or a two-number nominal_spec")
        long_edge, short_edge = nominal_values[:2]
    length_values: list[float] = []
    for row in rows:
        if (
            _nonempty(row.get("record_type")).lower() == "length"
            and _nonempty(row.get("sample_id") or row.get("bar_id")) == sample_id
        ):
            try:
                value = float(row.get("value") or row.get("value_mm"))
            except (TypeError, ValueError):
                continue
            if math.isfinite(value) and value > 0.0:
                length_values.append(value)
    return TruthData(
        report_id=_nonempty(selected[0].get("report_id")),
        sample_id=sample_id,
        nominal_spec=_nonempty(selected[0].get("nominal_spec")),
        head_label=head_label,
        tail_label=tail_label,
        face_map=dict(INSTITUTION_TO_SOFTWARE_FACE_MAP),
        angles=angles,
        reference_edges_mm=[long_edge, short_edge, long_edge, short_edge],
        rod_length_mm=(
            float(np.mean(np.asarray(length_values, dtype=float)))
            if length_values
            else math.nan
        ),
    )


def truth_nominal_numbers(value: str) -> list[str]:
    """Extract numeric nominal dimensions without imposing a file-name format."""

    import re

    return re.findall(r"[-+]?\d+(?:\.\d+)?", value)


def capture_orientation(path: Path) -> str:
    text = str(path).casefold()
    return "turnover" if any(marker.casefold() in text for marker in TURNOVER_MARKERS) else "normal"


def discover_captures(root: Path, holdout_fraction: float) -> list[tuple[Path, str, str]]:
    paths = sorted(
        [
            path
            for path in root.rglob("*.hobj")
            if not any(part.casefold() in INDEPENDENT_VALIDATION_FOLDERS for part in path.parts)
        ],
        key=lambda item: str(item).casefold(),
    )
    if not paths:
        raise ValueError(f"No HOBJ files were found under: {root}")
    groups: Dict[str, list[Path]] = {"normal": [], "turnover": []}
    for path in paths:
        groups[capture_orientation(path)].append(path)
    missing = [name for name, values in groups.items() if not values]
    if missing:
        raise ValueError(
            "Both directions are required. Put turnover captures in a folder containing "
            "turnover, tail_to_head, reverse, diaotou, or 调头. Missing: " + ", ".join(missing)
        )
    discovered: list[tuple[Path, str, str]] = []
    for orientation, values in groups.items():
        holdout_count = max(1, int(round(len(values) * holdout_fraction)))
        if holdout_count >= len(values):
            holdout_count = 1
        split_at = len(values) - holdout_count
        for index, path in enumerate(values):
            discovered.append((path, orientation, "train" if index < split_at else "holdout"))
    return discovered


def professional_turnover_face_map() -> Dict[str, str]:
    """Map software channels after a physical end-for-end turnover.

    Institution faces are A=top, B=right, C=left, D=bottom.  A physical
    turnover keeps institution A/D and exchanges institution B/C.  Internally
    the historical software labels are A=top, B=right, C=bottom, D=left, so
    the same physical rule is A/C fixed and B/D exchanged.
    """

    return {"A": "A", "B": "D", "C": "C", "D": "B"}


def oriented_professional_truth(
    truth: Dict[str, Dict[str, float]],
    orientation: str,
) -> Dict[str, Dict[str, float]]:
    """Express physical R/L truth in the fixed HOBJ device-row coordinate.

    On-site video proves that ``tail_to_head`` is the same standard physically
    turned end-for-end about the institution A/D axis; it is not reverse camera
    travel.  HOBJ rows remain in fixed device order in both groups.  Therefore
    a turnover exchanges endpoints, exchanges institution B/C (software B/D),
    keeps A/D (software A/C), and complements the directed angle because the
    fitted end-plane normal is always oriented toward device +Y.
    """

    if orientation not in {"normal", "turnover"}:
        raise ValueError(f"Unknown calibration acquisition group: {orientation}")
    if orientation == "normal":
        return {end: dict(values) for end, values in truth.items()}
    face_map = professional_turnover_face_map()
    end_map = {"head": "tail", "tail": "head"}
    return {
        device_end: {
            device_face: 180.0 - float(truth[end_map[device_end]][face_map[device_face]])
            for device_face in FACES
        }
        for device_end in ENDS
    }


def longitudinal_profile_from_rows(rows: list[Dict[str, object]]) -> np.ndarray:
    """Return a centered four-corner X/Z trajectory without using truth."""

    usable = [row for row in rows if row.get("record") == "slice" and row.get("valid") is True]
    profile = np.asarray(
        [
            [
                [
                    float(row[f"{CAMERA_TO_SECTION_POINT[obj]}_x"]),
                    float(row[f"{CAMERA_TO_SECTION_POINT[obj]}_z"]),
                ]
                for row in usable
            ]
            for obj in (1, 2, 3, 4)
        ],
        dtype=float,
    )
    if profile.shape[1] < 5:
        raise ValueError("Calibration row-order audit needs at least five section stations")
    return profile - np.median(profile, axis=1, keepdims=True)


def audit_calibration_row_order(
    measurements: list[PhysicalCaptureMeasurement],
    *,
    minimum_decision_ratio: float = 3.0,
) -> Dict[str, object]:
    """Verify fixed HOBJ row storage independently inside each physical pose.

    A physically turned bar has a different material trajectory in device
    coordinates.  Comparing it directly with the unturned trajectory caused
    the earlier false conclusion that the scanner had reversed.  Each physical
    pose therefore gets its own image-only median reference; this audit detects
    a row-storage reversal but never decides whether the bar was turned.
    """

    references: Dict[str, np.ndarray] = {}
    for orientation in ("normal", "turnover"):
        profiles = [
            item.longitudinal_profile
            for item in measurements
            if item.orientation == orientation and item.longitudinal_profile is not None
        ]
        if len(profiles) < 3:
            raise ValueError(
                f"Row-order audit needs at least three {orientation} physical-pose captures"
            )
        references[orientation] = np.median(np.stack(profiles), axis=0)
    per_capture: list[Dict[str, object]] = []
    for item in measurements:
        reference = references[item.orientation]
        if item.longitudinal_profile is None or item.longitudinal_profile.shape != reference.shape:
            item.row_order_status = "ambiguous"
            same_rmse = math.inf
            reversed_rmse = math.inf
        else:
            same_rmse = float(
                math.sqrt(float(np.mean((item.longitudinal_profile - reference) ** 2)))
            )
            reversed_rmse = float(
                math.sqrt(
                    float(np.mean((item.longitudinal_profile[:, ::-1, :] - reference) ** 2))
                )
            )
            smaller = min(same_rmse, reversed_rmse)
            ratio = max(same_rmse, reversed_rmse) / max(smaller, 1e-12)
            if ratio < minimum_decision_ratio:
                item.row_order_status = "ambiguous"
            elif same_rmse < reversed_rmse:
                item.row_order_status = "canonical"
            else:
                item.row_order_status = "reversed"
        item.row_order_same_rmse_mm = same_rmse
        item.row_order_reversed_rmse_mm = reversed_rmse
        per_capture.append(
            {
                "capture": item.path.name,
                "acquisition_group": (
                    "head_to_tail" if item.orientation == "normal" else "tail_to_head"
                ),
                "row_order_status": item.row_order_status,
                "same_order_rmse_mm": same_rmse,
                "reversed_order_rmse_mm": reversed_rmse,
                "decision_ratio": (
                    max(same_rmse, reversed_rmse) / max(min(same_rmse, reversed_rmse), 1e-12)
                    if math.isfinite(same_rmse) and math.isfinite(reversed_rmse)
                    else 0.0
                ),
            }
        )
    return {
        "method": "image_only_within_physical_pose_centered_four_corner_longitudinal_trajectory",
        "minimum_decision_ratio": minimum_decision_ratio,
        "reference_acquisition_groups": ["head_to_tail", "tail_to_head"],
        "physical_pose_contract": (
            "head_to_tail=unturned standard; tail_to_head=same standard physically turned "
            "180 degrees about institution A/D; HOBJ rows stay in device order"
        ),
        "per_capture": per_capture,
    }


def extract_physical_captures(
    discovered: list[tuple[Path, str, str]],
    calibration: Dict[str, object],
    truth: TruthData | None,
    x_scale: float,
    y_scale: float,
    window_mm: float,
) -> list[PhysicalCaptureMeasurement]:
    """Extract immutable image geometry before any professional calibration."""

    active = calibration_for_orientation(calibration, "normal")
    transforms = {int(key): value for key, value in active["transforms"].items()}
    measurements: list[PhysicalCaptureMeasurement] = []
    for index, (path, orientation, split) in enumerate(discovered, start=1):
        print(f"[{index}/{len(discovered)}] {orientation:8s} {split:7s} {path}", flush=True)
        source = HobjSource(str(path))
        valid_ranges = source_valid_ranges(source)
        common_start, common_end = source_common_range(source, valid_ranges)
        rows, _ = sampled_cross_section_rows_and_edges(
            source,
            calibration,
            "normal",
            x_scale,
            y_scale,
            valid_ranges,
        )
        relative_camera_diagnostic = same_face_relative_camera_diagnostic(
            source,
            transforms,
            active,
            common_start,
            common_end,
            x_scale,
        )
        rod_axis = fit_rod_axis(rows)
        boundary_points = {
            end: {
                obj: endface_boundary_points_for_camera(
                    source,
                    obj,
                    end,
                    valid_ranges[obj],
                    transforms[obj],
                    active,
                    common_start,
                    x_scale,
                    y_scale,
                    window_mm,
                    common_end=common_end,
                )
                for obj in (1, 2, 3, 4)
            }
            for end in ENDS
        }
        missing = [
            f"{end}-camera{obj}"
            for end in ENDS
            for obj in (1, 2, 3, 4)
            if len(boundary_points[end][obj]) < 3
        ]
        if missing:
            raise ValueError(f"Capture {path.name} has insufficient boundary points: {', '.join(missing)}")
        measurements.append(
            PhysicalCaptureMeasurement(
                path=path,
                orientation=orientation,
                split=split,
                rod_axis=rod_axis,
                section_points=mean_cross_section_points(rows),
                side_plane_normals=longitudinal_side_plane_normals(rows),
                boundary_points=boundary_points,
                scan_truth=(
                    oriented_professional_truth(truth.angles, orientation)
                    if truth is not None
                    else {end: {face: math.nan for face in FACES} for end in ENDS}
                ),
                longitudinal_rows=rows,
                longitudinal_profile=longitudinal_profile_from_rows(rows),
                relative_camera_diagnostic=relative_camera_diagnostic,
                camera_valid_span_rows={
                    obj: int(valid_ranges[obj][1] - valid_ranges[obj][0])
                    for obj in (1, 2, 3, 4)
                },
            )
        )
    return measurements


def calibration_state_reference(
    measurements: list[PhysicalCaptureMeasurement],
    *,
    seam_median_margin_mm: float = 0.25,
    seam_span_margin_mm: float = 0.20,
    pair_angle_margin_deg: float = 0.15,
) -> Dict[str, object]:
    """Build an image-only applicability envelope for the calibrated camera state.

    This does not estimate a correction.  It records the same-face, dual-camera
    geometry observed while fitting the standard so production can refuse to
    apply scan-row synchronization after the camera rig moves.  HOBJ alone
    cannot separate camera motion from a genuinely non-planar bar, therefore a
    mismatch must retain raw geometry rather than manufacture a correction.
    """

    per_capture: list[Dict[str, object]] = []
    values: Dict[str, Dict[str, list[float]]] = {
        face: {
            "seam_median_mm": [],
            "seam_span_p05_p95_mm": [],
            "pair_angle_p90_deg": [],
        }
        for face in FACES
    }
    all_stable = True
    for item in measurements:
        diagnostic = item.relative_camera_diagnostic
        status = str(diagnostic.get("status", "missing"))
        stable = status == "relative_geometry_stable"
        all_stable = all_stable and stable
        record: Dict[str, object] = {
            "capture": item.path.name,
            "acquisition_group": (
                "head_to_tail" if item.orientation == "normal" else "tail_to_head"
            ),
            "status": status,
            "stable": stable,
            "maximum_seam_span_p05_p95_mm": diagnostic.get(
                "maximum_seam_span_p05_p95_mm", math.nan
            ),
        }
        per_face = diagnostic.get("per_face", {})
        for face in FACES:
            face_data = per_face.get(face, {}) if isinstance(per_face, dict) else {}
            for field_name in values[face]:
                try:
                    value = float(face_data[field_name])
                except (KeyError, TypeError, ValueError):
                    value = math.nan
                record[f"{face}_{field_name}"] = value
                if math.isfinite(value):
                    values[face][field_name].append(value)
        per_capture.append(record)

    face_envelopes: Dict[str, Dict[str, object]] = {}
    complete = True
    for face in FACES:
        face_values = values[face]
        if any(len(series) != len(measurements) for series in face_values.values()):
            complete = False
            face_envelopes[face] = {"valid": False}
            continue
        medians = np.asarray(face_values["seam_median_mm"], dtype=float)
        spans = np.asarray(face_values["seam_span_p05_p95_mm"], dtype=float)
        angles = np.asarray(face_values["pair_angle_p90_deg"], dtype=float)
        face_envelopes[face] = {
            "valid": True,
            "seam_median_reference_mm": float(np.median(medians)),
            "seam_median_min_mm": float(np.min(medians) - seam_median_margin_mm),
            "seam_median_max_mm": float(np.max(medians) + seam_median_margin_mm),
            "seam_span_max_mm": float(np.max(spans) + seam_span_margin_mm),
            "pair_angle_p90_max_deg": float(np.max(angles) + pair_angle_margin_deg),
        }
    return {
        "method": "same_physical_face_dual_camera_line_separation_no_truth",
        "fit_target": "image_geometry_applicability_only_no_correction",
        "all_captures_stable": bool(all_stable and complete and measurements),
        "complete": complete,
        "capture_count": len(measurements),
        "margins": {
            "seam_median_mm": seam_median_margin_mm,
            "seam_span_mm": seam_span_margin_mm,
            "pair_angle_deg": pair_angle_margin_deg,
        },
        "faces": face_envelopes,
        "per_capture": per_capture,
        "reason": (
            "Reference-only mechanical-state envelope. It never changes coordinates; "
            "a production mismatch must remain unadjusted and warned."
        ),
    }


def run_camera_state_preflight(
    input_root: Path,
    calibration: Dict[str, object],
    report_path: Path,
    x_scale: float,
) -> int:
    """Write a truth-free, correction-free mechanical-state report for HOBJ files."""

    paths = (
        [input_root]
        if input_root.is_file() and input_root.suffix.casefold() == ".hobj"
        else sorted(input_root.rglob("*.hobj"), key=lambda item: str(item).casefold())
    )
    if not paths:
        raise ValueError(f"No HOBJ files were found under: {input_root}")
    active = calibration_for_orientation(calibration, "normal")
    transforms = {int(key): value for key, value in active["transforms"].items()}
    rows: list[Dict[str, object]] = []
    for index, path in enumerate(paths, start=1):
        try:
            source = HobjSource(str(path))
            valid_ranges = source_valid_ranges(source)
            common_start, common_end = source_common_range(source, valid_ranges)
            diagnostic = same_face_relative_camera_diagnostic(
                source,
                transforms,
                active,
                common_start,
                common_end,
                x_scale,
            )
            row: Dict[str, object] = {
                "capture": path.name,
                "input_path": str(path),
                "status": diagnostic.get("status", "missing"),
                "stable": diagnostic.get("status") == "relative_geometry_stable",
                "maximum_seam_span_p05_p95_mm": diagnostic.get(
                    "maximum_seam_span_p05_p95_mm", math.nan
                ),
                "correction_applied": False,
                "uses_professional_truth": False,
                "reason": diagnostic.get("reason", ""),
            }
            per_face = diagnostic.get("per_face", {})
            for face in FACES:
                face_data = per_face.get(face, {}) if isinstance(per_face, dict) else {}
                for source_name, output_name in (
                    ("seam_median_mm", "seam_median_mm"),
                    ("seam_span_p05_p95_mm", "seam_span_mm"),
                    ("pair_angle_p90_deg", "pair_angle_p90_deg"),
                ):
                    row[f"{face}_{output_name}"] = face_data.get(source_name, "")
        except Exception as exc:
            row = {
                "capture": path.name,
                "input_path": str(path),
                "status": "diagnostic_failed",
                "stable": False,
                "maximum_seam_span_p05_p95_mm": "",
                "correction_applied": False,
                "uses_professional_truth": False,
                "reason": str(exc),
            }
        rows.append(row)
        print(
            f"[{index}/{len(paths)}] {row['status']}: {path.name}; "
            f"max_span={row['maximum_seam_span_p05_p95_mm']}"
        )
    fieldnames = [
        "capture",
        "input_path",
        "status",
        "stable",
        "maximum_seam_span_p05_p95_mm",
        "correction_applied",
        "uses_professional_truth",
        *[
            f"{face}_{field_name}"
            for face in FACES
            for field_name in (
                "seam_median_mm",
                "seam_span_mm",
                "pair_angle_p90_deg",
            )
        ],
        "reason",
    ]
    report_path.parent.mkdir(parents=True, exist_ok=True)
    with report_path.open("w", encoding="utf-8-sig", newline="") as handle:
        writer = csv.DictWriter(handle, fieldnames=fieldnames)
        writer.writeheader()
        writer.writerows({name: row.get(name, "") for name in fieldnames} for row in rows)
    all_stable = all(row.get("stable") is True for row in rows)
    print(f"Camera-state report: {report_path}")
    print(f"All captures stable: {str(all_stable).lower()}")
    return 0 if all_stable else 4


def expand_camera_y_parameters(parameters: Iterable[float]) -> np.ndarray:
    """Use camera 4 as the gauge so the four synchronization offsets sum to zero."""

    values = np.asarray(list(parameters), dtype=float)
    if values.shape != (3,) or not bool(np.all(np.isfinite(values))):
        raise ValueError("Camera synchronization requires exactly three finite gauge parameters")
    return np.r_[values, -float(np.sum(values))]


def physical_angles_for_capture(
    measurement: PhysicalCaptureMeasurement,
    camera_y_offsets_mm: Iterable[float],
) -> tuple[Dict[str, Dict[str, float]], Dict[str, object]]:
    """Rebuild the two end planes after applying physical camera Y offsets."""

    offsets = np.asarray(list(camera_y_offsets_mm), dtype=float)
    if offsets.shape != (4,) or not bool(np.all(np.isfinite(offsets))):
        raise ValueError("Four finite camera Y offsets are required")
    angles: Dict[str, Dict[str, float]] = {}
    fits: Dict[str, object] = {}
    y_offset_map = {obj: float(offsets[obj - 1]) for obj in (1, 2, 3, 4)}
    corrected_side_plane_normals = (
        longitudinal_side_plane_normals(
            measurement.longitudinal_rows,
            camera_y_offsets_mm=y_offset_map,
        )
        if measurement.longitudinal_rows
        else measurement.side_plane_normals
    )
    for end in ENDS:
        adjusted: list[np.ndarray] = []
        for obj in (1, 2, 3, 4):
            cloud = np.asarray(measurement.boundary_points[end][obj], dtype=float).copy()
            cloud[:, 1] += offsets[obj - 1]
            adjusted.append(cloud)
        fit = robust_plane_y_from_xz(np.vstack(adjusted), end, measurement.rod_axis)
        if not fit.valid:
            raise ValueError(f"Capture {measurement.path.name} {end} plane failed: {fit.reason}")
        fits[end] = fit
        angles[end] = face_to_endface_angles(
            measurement.section_points,
            fit,
            measurement.rod_axis,
            corrected_side_plane_normals,
        )
        missing = [face for face in FACES if face not in angles[end]]
        if missing:
            raise ValueError(
                f"Capture {measurement.path.name} {end} is missing angles: {', '.join(missing)}"
            )
    return angles, fits


def wireframe_angles_for_capture(
    measurement: PhysicalCaptureMeasurement,
    camera_y_offsets_mm: Iterable[float],
) -> Dict[str, object]:
    """Run the exact production 16-angle geometry on one extracted capture."""

    offsets = np.asarray(list(camera_y_offsets_mm), dtype=float)
    if offsets.shape != (4,) or not bool(np.all(np.isfinite(offsets))):
        raise ValueError("Four finite camera Y offsets are required")
    offset_map = {camera: float(offsets[camera - 1]) for camera in (1, 2, 3, 4)}
    side_normals = longitudinal_side_plane_normals(
        measurement.longitudinal_rows,
        camera_y_offsets_mm=offset_map,
    )
    return measure_wireframe_angles(
        measurement.longitudinal_rows,
        measurement.section_points,
        measurement.boundary_points,
        side_normals,
        offset_map,
    )


def wireframe_turnover_equivariance_validation(
    normal_measurements: list[PhysicalCaptureMeasurement],
    turnover_measurements: list[PhysicalCaptureMeasurement],
    camera_y_offsets_mm: Iterable[float],
    *,
    max_rmse_deg: float | None = None,
    max_error_deg: float | None = None,
    max_repeatability_range_deg: float | None = None,
) -> Dict[str, object]:
    """Validate all 16 local angles under the verified physical turnover.

    The fitted camera parameters may come from training truth, but this
    validation never reads that truth.  It compares only held-out image
    geometry using the fixed head/tail, B/C and directed-complement mapping.
    """

    if not normal_measurements or not turnover_measurements:
        raise ValueError("Wireframe turnover validation needs captures in both physical poses")
    offsets = np.asarray(list(camera_y_offsets_mm), dtype=float)
    normal_results = [
        (item, wireframe_angles_for_capture(item, offsets))
        for item in sorted(normal_measurements, key=lambda value: value.path.name.casefold())
    ]
    turnover_results = [
        (item, wireframe_angles_for_capture(item, offsets))
        for item in sorted(turnover_measurements, key=lambda value: value.path.name.casefold())
    ]
    channels: list[Dict[str, object]] = []
    group_residuals: list[float] = []
    ordinal_residuals: list[float] = []
    maximum_repeatability_range = 0.0
    for end, local_channel in WIREFRAME_CHANNELS:
        mapped_end, mapped_channel = mapped_turnover_channel(end, local_channel)
        before = np.asarray(
            [float(result["angles"][end][local_channel]) for _, result in normal_results],
            dtype=float,
        )
        after = np.asarray(
            [float(result["angles"][mapped_end][mapped_channel]) for _, result in turnover_results],
            dtype=float,
        )
        group_residual = float(np.median(before) + np.median(after) - 180.0)
        paired_count = min(len(before), len(after))
        paired = before[:paired_count] + after[:paired_count] - 180.0
        before_span = float(np.ptp(before))
        after_span = float(np.ptp(after))
        maximum_repeatability_range = max(
            maximum_repeatability_range,
            before_span,
            after_span,
        )
        group_residuals.append(group_residual)
        ordinal_residuals.extend(float(value) for value in paired)
        channels.append(
            {
                "head_to_tail_end": end,
                "head_to_tail_local_channel": local_channel,
                "tail_to_head_mapped_end": mapped_end,
                "tail_to_head_mapped_local_channel": mapped_channel,
                "expected_relation": "theta_after=180-theta_before",
                "head_to_tail_count": len(before),
                "tail_to_head_count": len(after),
                "head_to_tail_median_deg": float(np.median(before)),
                "tail_to_head_mapped_median_deg": float(np.median(after)),
                "group_median_sum_residual_deg": group_residual,
                "ordinal_pair_residuals_deg": [float(value) for value in paired],
                "ordinal_pair_rms_residual_deg": float(
                    math.sqrt(float(np.mean(np.square(paired))))
                ),
                "ordinal_pair_max_abs_residual_deg": float(np.max(np.abs(paired))),
                "head_to_tail_span_deg": before_span,
                "tail_to_head_span_deg": after_span,
            }
        )
    group_array = np.asarray(group_residuals, dtype=float)
    ordinal_array = np.asarray(ordinal_residuals, dtype=float)
    statistics = {
        "group_median_rmse_deg": float(math.sqrt(float(np.mean(np.square(group_array))))),
        "group_median_max_abs_error_deg": float(np.max(np.abs(group_array))),
        "ordinal_pair_rmse_deg": float(math.sqrt(float(np.mean(np.square(ordinal_array))))),
        "ordinal_pair_max_abs_error_deg": float(np.max(np.abs(ordinal_array))),
        "maximum_repeatability_range_deg": float(maximum_repeatability_range),
    }
    failures: list[str] = []
    validation_rmse = max(
        statistics["group_median_rmse_deg"],
        statistics["ordinal_pair_rmse_deg"],
    )
    validation_max_error = max(
        statistics["group_median_max_abs_error_deg"],
        statistics["ordinal_pair_max_abs_error_deg"],
    )
    if max_rmse_deg is not None and validation_rmse > max_rmse_deg:
        failures.append(
            f"wireframe turnover RMS {validation_rmse:.6f}° > {max_rmse_deg:.6f}°"
        )
    if max_error_deg is not None and validation_max_error > max_error_deg:
        failures.append(
            "wireframe turnover max error "
            f"{validation_max_error:.6f}° > {max_error_deg:.6f}°"
        )
    if (
        max_repeatability_range_deg is not None
        and statistics["maximum_repeatability_range_deg"] > max_repeatability_range_deg
    ):
        failures.append(
            "wireframe repeatability range "
            f"{statistics['maximum_repeatability_range_deg']:.6f}° > "
            f"{max_repeatability_range_deg:.6f}°"
        )
    limits = {
        "max_rmse_deg": max_rmse_deg,
        "max_error_deg": max_error_deg,
        "max_repeatability_range_deg": max_repeatability_range_deg,
    }
    return {
        "method": "shared_production_wireframe_16_angle_physical_turnover_no_truth_lookup",
        "shared_geometry_module": "endface_wireframe_geometry.measure_wireframe_angles",
        "uses_professional_truth": False,
        "final_angle_correction_applied": False,
        "camera_y_offsets_applied": {
            str(camera): float(offsets[camera - 1]) for camera in (1, 2, 3, 4)
        },
        "capture_counts": {
            "head_to_tail": len(normal_measurements),
            "tail_to_head": len(turnover_measurements),
        },
        "passed": not failures,
        "statistics": statistics,
        "limits": limits,
        "channels": channels,
        "failures": failures,
    }


def physical_residuals(
    measurements: list[PhysicalCaptureMeasurement],
    gauge_parameters: Iterable[float],
) -> np.ndarray:
    offsets = expand_camera_y_parameters(gauge_parameters)
    residuals: list[float] = []
    for measurement in measurements:
        angles, _ = physical_angles_for_capture(measurement, offsets)
        residuals.extend(
            angles[end][face] - measurement.scan_truth[end][face]
            for end, face in CHANNELS
        )
    return np.asarray(residuals, dtype=float)


def physical_plane_residuals(
    measurements: list[PhysicalCaptureMeasurement],
    gauge_parameters: Iterable[float],
    *,
    equalize_capture_weight: bool = True,
) -> np.ndarray:
    """Return point-to-plane Y residuals without consulting institution truth."""

    offsets = expand_camera_y_parameters(gauge_parameters)
    residuals: list[np.ndarray] = []
    for measurement in measurements:
        for end in ENDS:
            adjusted: list[np.ndarray] = []
            for obj in (1, 2, 3, 4):
                cloud = np.asarray(measurement.boundary_points[end][obj], dtype=float).copy()
                cloud[:, 1] += offsets[obj - 1]
                adjusted.append(cloud)
            points = np.vstack(adjusted)
            fit = robust_plane_y_from_xz(points, end, measurement.rod_axis)
            if not fit.valid:
                raise ValueError(
                    f"Capture {measurement.path.name} {end} plane failed during synchronization: {fit.reason}"
                )
            values = points[:, 1] - (
                fit.slope_x * points[:, 0]
                + fit.slope_z * points[:, 2]
                + fit.intercept_y
            )
            centre = float(np.median(values))
            values = values - centre
            if equalize_capture_weight:
                values = values / math.sqrt(max(1, len(values)))
            residuals.append(values)
    return np.concatenate(residuals) if residuals else np.empty(0, dtype=float)


def residual_statistics(residuals: Iterable[float]) -> Dict[str, float]:
    values = np.asarray(list(residuals), dtype=float)
    if not values.size or not bool(np.all(np.isfinite(values))):
        return {"count": int(values.size), "rmse_deg": math.inf, "max_abs_error_deg": math.inf, "median_abs_error_deg": math.inf}
    return {
        "count": int(values.size),
        "rmse_deg": float(math.sqrt(float(np.mean(values * values)))),
        "max_abs_error_deg": float(np.max(np.abs(values))),
        "median_abs_error_deg": float(np.median(np.abs(values))),
    }


def fit_shared_camera_y_offsets(
    measurements: list[PhysicalCaptureMeasurement],
    *,
    huber_delta_mm: float = 0.08,
    max_offset_mm: float = 1.5,
    max_iterations: int = 30,
) -> tuple[np.ndarray, Dict[str, object]]:
    """Fit one shared camera synchronization state with robust Gauss-Newton.

    Only three gauge parameters are optimized.  They expand to four offsets
    whose sum is zero, so an arbitrary whole-rod translation cannot be learned.
    """

    if len(measurements) < 4:
        raise ValueError("At least four captures are required for physical synchronization fitting")
    parameters = np.zeros(3, dtype=float)
    damping = 1e-3
    history: list[Dict[str, object]] = []
    for iteration in range(max_iterations):
        residual = physical_plane_residuals(measurements, parameters)
        weights = np.minimum(1.0, huber_delta_mm / np.maximum(np.abs(residual), 1e-12))
        finite_step_mm = 0.002
        jacobian = np.column_stack(
            [
                (
                    physical_plane_residuals(
                        measurements,
                        parameters + np.eye(3, dtype=float)[column] * finite_step_mm,
                    )
                    - residual
                )
                / finite_step_mm
                for column in range(3)
            ]
        )
        system = jacobian.T @ (weights[:, None] * jacobian) + damping * np.eye(3)
        gradient = jacobian.T @ (weights * residual)
        step = np.linalg.solve(system, -gradient)
        base_loss = float(np.sum(weights * residual * residual))
        accepted = False
        for scale in (1.0, 0.5, 0.25, 0.1, 0.05, 0.01):
            candidate = np.clip(parameters + scale * step, -max_offset_mm, max_offset_mm)
            candidate_offsets = expand_camera_y_parameters(candidate)
            if float(np.max(np.abs(candidate_offsets))) > max_offset_mm:
                continue
            candidate_residual = physical_plane_residuals(measurements, candidate)
            candidate_weights = np.minimum(
                1.0,
                huber_delta_mm / np.maximum(np.abs(candidate_residual), 1e-12),
            )
            candidate_loss = float(
                np.sum(candidate_weights * candidate_residual * candidate_residual)
            )
            if candidate_loss < base_loss:
                parameters = candidate
                damping = max(1e-6, damping / 2.0)
                accepted = True
                break
        if not accepted:
            damping *= 10.0
        history.append(
            {
                "iteration": iteration,
                "camera_y_offsets_mm": [float(value) for value in expand_camera_y_parameters(parameters)],
                "weighted_plane_residual_rmse": float(
                    math.sqrt(
                        float(
                            np.mean(
                                physical_plane_residuals(measurements, parameters)
                                ** 2
                            )
                        )
                    )
                ),
            }
        )
        if float(np.linalg.norm(step)) < 1e-6:
            break
    final_residual = physical_plane_residuals(measurements, parameters)
    final_weights = np.minimum(
        1.0,
        huber_delta_mm / np.maximum(np.abs(final_residual), 1e-12),
    )
    finite_step_mm = 0.002
    final_jacobian = np.column_stack(
        [
            (
                physical_plane_residuals(
                    measurements,
                    parameters + np.eye(3, dtype=float)[column] * finite_step_mm,
                )
                - final_residual
            )
            / finite_step_mm
            for column in range(3)
        ]
    )
    weighted_jacobian = np.sqrt(final_weights)[:, None] * final_jacobian
    singular_values = np.linalg.svd(weighted_jacobian, compute_uv=False)
    condition_number = (
        float(singular_values[0] / singular_values[-1])
        if singular_values.size and singular_values[-1] > 1e-12
        else math.inf
    )
    return expand_camera_y_parameters(parameters), {
        "method": "robust_shared_camera_scan_row_synchronization",
        "fit_target": "image_point_to_end_plane_residual_only_no_institution_truth",
        "huber_delta_mm": huber_delta_mm,
        "max_offset_mm": max_offset_mm,
        "weighted_jacobian_singular_values": [float(value) for value in singular_values],
        "weighted_jacobian_condition_number": condition_number,
        "identifiability_warning": (
            "Camera-wise Y offsets that form an affine X/Z plane are partly confounded "
            "with the specimen's real end-plane tilt; professional truth must never be "
            "replaced by plane-flattening alone."
        ),
        "iterations": history,
    }


CAMERA_TO_SECTION_POINT = {1: "P1", 2: "P2", 3: "P3", 4: "P4"}


def synchronization_mode_basis(
    measurements: list[PhysicalCaptureMeasurement],
) -> Dict[str, object]:
    """Decompose four camera-Y offsets into physically identifiable modes.

    A constant four-camera offset is an arbitrary Y origin and is removed by
    the zero-sum gauge.  Two remaining modes are affine over cross-section X/Z
    and therefore change the reconstructed end-plane tilt.  The fourth mode is
    orthogonal to ``[1, X, Z]`` and represents non-coplanar camera timing.

    The non-coplanar mode is identifiable from image point-to-plane residuals.
    The two affine modes are deliberately *not* flattened from the image,
    because doing so would erase a real specimen end tilt; the professional
    standard constrains those two low-level device modes instead.
    """

    if not measurements:
        raise ValueError("Synchronization mode basis needs at least one capture")
    x = np.asarray(
        [
            np.median(
                [item.section_points[CAMERA_TO_SECTION_POINT[obj]][0] for item in measurements]
            )
            for obj in (1, 2, 3, 4)
        ],
        dtype=float,
    )
    z = np.asarray(
        [
            np.median(
                [item.section_points[CAMERA_TO_SECTION_POINT[obj]][1] for item in measurements]
            )
            for obj in (1, 2, 3, 4)
        ],
        dtype=float,
    )
    x -= float(np.mean(x))
    z -= float(np.mean(z))
    mode_x = x / float(np.linalg.norm(x))
    z_orthogonal = z - mode_x * float(mode_x @ z)
    if float(np.linalg.norm(z_orthogonal)) <= 1e-9:
        raise ValueError("Camera X/Z locations cannot identify two end-plane tilt modes")
    mode_z = z_orthogonal / float(np.linalg.norm(z_orthogonal))
    design = np.column_stack([np.ones(4, dtype=float), x, z])
    _, singular_values, vh = np.linalg.svd(design.T, full_matrices=True)
    mode_nonplanar = np.asarray(vh[-1], dtype=float)
    mode_nonplanar -= float(np.mean(mode_nonplanar))
    mode_nonplanar /= float(np.max(np.abs(mode_nonplanar)))
    orthogonality = design.T @ mode_nonplanar
    if float(np.max(np.abs(orthogonality))) > 1e-6:
        raise ValueError("Non-planar synchronization mode is not orthogonal to [1, X, Z]")
    return {
        "reference_camera_x_mm": [float(value) for value in x],
        "reference_camera_z_mm": [float(value) for value in z],
        "affine_x_mode": np.asarray(mode_x, dtype=float),
        "affine_z_mode": np.asarray(mode_z, dtype=float),
        "nonplanar_mode": np.asarray(mode_nonplanar, dtype=float),
        "cross_section_design_singular_values": [float(value) for value in singular_values],
    }


def offsets_from_physical_modes(
    basis: Dict[str, object],
    nonplanar_coefficient_mm: float,
    affine_coefficients_mm: Iterable[float],
) -> np.ndarray:
    affine = np.asarray(list(affine_coefficients_mm), dtype=float)
    if affine.shape != (2,) or not bool(np.all(np.isfinite(affine))):
        raise ValueError("Two finite affine synchronization coefficients are required")
    offsets = (
        float(nonplanar_coefficient_mm) * np.asarray(basis["nonplanar_mode"], dtype=float)
        + affine[0] * np.asarray(basis["affine_x_mode"], dtype=float)
        + affine[1] * np.asarray(basis["affine_z_mode"], dtype=float)
    )
    offsets -= float(np.mean(offsets))
    return offsets


def _huber_objective(residual: np.ndarray, delta: float) -> float:
    absolute = np.abs(np.asarray(residual, dtype=float))
    return float(
        np.sum(np.where(absolute <= delta, 0.5 * absolute * absolute, delta * (absolute - 0.5 * delta)))
    )


def fit_nonplanar_synchronization_mode(
    measurements: list[PhysicalCaptureMeasurement],
    basis: Dict[str, object],
    *,
    huber_delta_mm: float = 0.08,
    max_offset_mm: float = 1.5,
    max_iterations: int = 30,
) -> tuple[float, Dict[str, object]]:
    """Fit only the image-identifiable non-coplanar camera timing mode."""

    mode = np.asarray(basis["nonplanar_mode"], dtype=float)

    def residual_for(coefficient: float) -> np.ndarray:
        offsets = offsets_from_physical_modes(basis, coefficient, (0.0, 0.0))
        return physical_plane_residuals(measurements, offsets[:3])

    coefficient = 0.0
    damping = 1e-3
    history: list[Dict[str, float]] = []
    for iteration in range(max_iterations):
        residual = residual_for(coefficient)
        weights = np.minimum(1.0, huber_delta_mm / np.maximum(np.abs(residual), 1e-12))
        step_mm = 0.002
        jacobian = (residual_for(coefficient + step_mm) - residual) / step_mm
        denominator = float(np.sum(weights * jacobian * jacobian) + damping)
        update = -float(np.sum(weights * jacobian * residual)) / denominator
        base_loss = _huber_objective(residual, huber_delta_mm)
        accepted = False
        for scale in (1.0, 0.5, 0.25, 0.1, 0.05, 0.01):
            candidate = coefficient + scale * update
            candidate_offsets = candidate * mode
            if float(np.max(np.abs(candidate_offsets))) > max_offset_mm:
                continue
            candidate_loss = _huber_objective(residual_for(candidate), huber_delta_mm)
            if candidate_loss < base_loss:
                coefficient = candidate
                damping = max(1e-8, damping / 2.0)
                accepted = True
                break
        if not accepted:
            damping *= 10.0
        history.append(
            {
                "iteration": float(iteration),
                "coefficient_mm": float(coefficient),
                "weighted_plane_residual_rmse_mm": float(
                    math.sqrt(float(np.mean(residual_for(coefficient) ** 2)))
                ),
            }
        )
        if abs(update) < 1e-7:
            break
    final_residual = residual_for(coefficient)
    return float(coefficient), {
        "method": "robust_nonplanar_camera_timing_mode",
        "fit_target": "image_point_to_end_plane_residual_only_no_institution_truth",
        "huber_delta_mm": huber_delta_mm,
        "history": history,
        "final_plane_residual_rmse_mm": float(
            math.sqrt(float(np.mean(final_residual * final_residual)))
        ),
    }


def fit_affine_synchronization_modes_from_truth(
    measurements: list[PhysicalCaptureMeasurement],
    basis: Dict[str, object],
    nonplanar_coefficient_mm: float,
    *,
    huber_delta_deg: float = 0.15,
    max_offset_mm: float = 1.5,
    max_iterations: int = 40,
) -> tuple[np.ndarray, Dict[str, object]]:
    """Fit two shared device tilt modes, never eight answer channels."""

    parameters = np.zeros(2, dtype=float)
    damping = 1e-3

    def offsets_for(values: np.ndarray) -> np.ndarray:
        return offsets_from_physical_modes(basis, nonplanar_coefficient_mm, values)

    def residual_for(values: np.ndarray) -> np.ndarray:
        return physical_residuals(measurements, offsets_for(values)[:3])

    history: list[Dict[str, object]] = []
    for iteration in range(max_iterations):
        residual = residual_for(parameters)
        weights = np.minimum(1.0, huber_delta_deg / np.maximum(np.abs(residual), 1e-12))
        finite_step_mm = 0.002
        jacobian = np.column_stack(
            [
                (
                    residual_for(parameters + np.eye(2, dtype=float)[column] * finite_step_mm)
                    - residual
                )
                / finite_step_mm
                for column in range(2)
            ]
        )
        system = jacobian.T @ (weights[:, None] * jacobian) + damping * np.eye(2)
        gradient = jacobian.T @ (weights * residual)
        update = np.linalg.solve(system, -gradient)
        base_loss = _huber_objective(residual, huber_delta_deg)
        accepted = False
        for scale in (1.0, 0.5, 0.25, 0.1, 0.05, 0.01):
            candidate = parameters + scale * update
            candidate_offsets = offsets_for(candidate)
            if float(np.max(np.abs(candidate_offsets))) > max_offset_mm:
                continue
            if _huber_objective(residual_for(candidate), huber_delta_deg) < base_loss:
                parameters = candidate
                damping = max(1e-8, damping / 2.0)
                accepted = True
                break
        if not accepted:
            damping *= 10.0
        history.append(
            {
                "iteration": iteration,
                "affine_coefficients_mm": [float(value) for value in parameters],
                "camera_y_offsets_mm": [float(value) for value in offsets_for(parameters)],
            }
        )
        if float(np.linalg.norm(update)) < 1e-7:
            break
    final_residual = residual_for(parameters)
    final_weights = np.minimum(
        1.0, huber_delta_deg / np.maximum(np.abs(final_residual), 1e-12)
    )
    finite_step_mm = 0.002
    final_jacobian = np.column_stack(
        [
            (
                residual_for(parameters + np.eye(2, dtype=float)[column] * finite_step_mm)
                - final_residual
            )
            / finite_step_mm
            for column in range(2)
        ]
    )
    singular_values = np.linalg.svd(
        np.sqrt(final_weights)[:, None] * final_jacobian, compute_uv=False
    )
    condition_number = (
        float(singular_values[0] / singular_values[-1])
        if singular_values.size and singular_values[-1] > 1e-12
        else math.inf
    )
    return offsets_for(parameters), {
        "method": "robust_two_affine_camera_timing_modes",
        "fit_target": (
            "professional_face_angles_constrain_two_shared_affine_device_modes; "
            "no per-end or per-face output offset"
        ),
        "huber_delta_deg": huber_delta_deg,
        "affine_coefficients_mm": [float(value) for value in parameters],
        "weighted_jacobian_singular_values": [float(value) for value in singular_values],
        "weighted_jacobian_condition_number": condition_number,
        "history": history,
    }


def wireframe_turnover_group_residuals(
    measurements: list[PhysicalCaptureMeasurement],
    camera_y_offsets_mm: Iterable[float],
) -> np.ndarray:
    """Return 16 image-only turnover residuals without angle truth."""

    validation = wireframe_turnover_equivariance_validation(
        [item for item in measurements if item.orientation == "normal"],
        [item for item in measurements if item.orientation == "turnover"],
        camera_y_offsets_mm,
    )
    return np.asarray(
        [float(row["group_median_sum_residual_deg"]) for row in validation["channels"]],
        dtype=float,
    )


def fit_affine_synchronization_modes_from_turnover_images(
    measurements: list[PhysicalCaptureMeasurement],
    basis: Dict[str, object],
    nonplanar_coefficient_mm: float,
    *,
    huber_delta_deg: float = 0.20,
    max_offset_mm: float = 1.5,
    max_iterations: int = 30,
) -> tuple[np.ndarray, Dict[str, object]]:
    """Fit two affine camera timing modes from physical-turnover HOBJ only."""

    if not any(item.orientation == "normal" for item in measurements) or not any(
        item.orientation == "turnover" for item in measurements
    ):
        raise ValueError("Image-only affine synchronization needs both physical poses")
    parameters = np.zeros(2, dtype=float)
    damping = 1e-3

    def offsets_for(values: np.ndarray) -> np.ndarray:
        return offsets_from_physical_modes(basis, nonplanar_coefficient_mm, values)

    def residual_for(values: np.ndarray) -> np.ndarray:
        return wireframe_turnover_group_residuals(measurements, offsets_for(values))

    finite_step_mm = 0.002
    base_residual = residual_for(parameters)
    jacobian = np.column_stack(
        [
            (
                residual_for(np.eye(2, dtype=float)[column] * finite_step_mm)
                - base_residual
            )
            / finite_step_mm
            for column in range(2)
        ]
    )
    history: list[Dict[str, object]] = []
    for iteration in range(max_iterations):
        linear_residual = base_residual + jacobian @ parameters
        weights = np.minimum(
            1.0,
            huber_delta_deg / np.maximum(np.abs(linear_residual), 1e-12),
        )
        system = jacobian.T @ (weights[:, None] * jacobian) + 1e-6 * np.eye(2)
        candidate = np.linalg.solve(system, -jacobian.T @ (weights * base_residual))
        candidate_offsets = offsets_for(candidate)
        maximum_offset = float(np.max(np.abs(candidate_offsets)))
        if maximum_offset > max_offset_mm:
            candidate *= max_offset_mm / maximum_offset
        history.append(
            {
                "iteration": iteration,
                "linearized_affine_coefficients_mm": [float(value) for value in candidate],
            }
        )
        if float(np.linalg.norm(candidate - parameters)) < 1e-7:
            parameters = candidate
            break
        parameters = candidate

    base_loss = _huber_objective(base_residual, huber_delta_deg)
    accepted_parameters = np.zeros(2, dtype=float)
    final_residual = base_residual
    for scale in (1.0, 0.75, 0.5, 0.25, 0.1):
        candidate = parameters * scale
        candidate_offsets = offsets_for(candidate)
        if float(np.max(np.abs(candidate_offsets))) > max_offset_mm:
            continue
        candidate_residual = residual_for(candidate)
        if _huber_objective(candidate_residual, huber_delta_deg) < base_loss:
            accepted_parameters = candidate
            final_residual = candidate_residual
            break
    parameters = accepted_parameters
    final_weights = np.minimum(
        1.0,
        huber_delta_deg / np.maximum(np.abs(final_residual), 1e-12),
    )
    singular_values = np.linalg.svd(
        np.sqrt(final_weights)[:, None] * jacobian,
        compute_uv=False,
    )
    condition_number = (
        float(singular_values[0] / singular_values[-1])
        if singular_values.size and singular_values[-1] > 1e-12
        else math.inf
    )
    return offsets_for(parameters), {
        "method": "robust_two_affine_camera_timing_modes_from_turnover_images",
        "fit_target": (
            "physical_turnover_shared_16_angle_equivariance_only_no_professional_or_manual_truth; "
            "no per-end or per-face output offset"
        ),
        "uses_professional_or_manual_endface_truth": False,
        "huber_delta_deg": huber_delta_deg,
        "affine_coefficients_mm": [float(value) for value in parameters],
        "weighted_jacobian_singular_values": [float(value) for value in singular_values],
        "weighted_jacobian_condition_number": condition_number,
        "linearization_step_mm": finite_step_mm,
        "raw_turnover_wireframe_rmse_deg": float(
            math.sqrt(float(np.mean(np.square(base_residual))))
        ),
        "final_turnover_wireframe_rmse_deg": float(
            math.sqrt(float(np.mean(np.square(final_residual))))
        ),
        "history": history,
    }


def fit_decomposed_camera_y_offsets_from_images(
    measurements: list[PhysicalCaptureMeasurement],
    basis: Dict[str, object],
) -> tuple[np.ndarray, Dict[str, object]]:
    """Fit all identifiable modes from HOBJ plane residuals and turnover only."""

    nonplanar_coefficient, nonplanar_details = fit_nonplanar_synchronization_mode(
        measurements,
        basis,
    )
    offsets, affine_details = fit_affine_synchronization_modes_from_turnover_images(
        measurements,
        basis,
        nonplanar_coefficient,
    )
    return offsets, {
        "method": "decomposed_camera_scan_row_synchronization",
        "evidence": "image_only_end_plane_residual_and_physical_turnover_equivariance",
        "uses_professional_or_manual_endface_truth": False,
        "nonplanar_mode": nonplanar_details,
        "affine_modes": affine_details,
    }


def fit_nonplanar_camera_y_offsets_from_images(
    measurements: list[PhysicalCaptureMeasurement],
    basis: Dict[str, object],
) -> tuple[np.ndarray, Dict[str, object]]:
    """Fit only the one camera-Y mode identifiable from image plane residuals."""

    coefficient, details = fit_nonplanar_synchronization_mode(measurements, basis)
    offsets = offsets_from_physical_modes(basis, coefficient, (0.0, 0.0))
    return offsets, {
        "method": "single_nonplanar_camera_scan_row_synchronization",
        "model_level": "M1",
        "evidence": "image_only_end_plane_residual_no_turnover_fit_no_endface_truth",
        "uses_professional_or_manual_endface_truth": False,
        "nonplanar_mode": details,
        "affine_modes": {
            "enabled": False,
            "reason": (
                "Affine camera-Y modes are confounded with real end-face tilt and are retained "
                "only in the non-selectable M2 diagnostic."
            ),
        },
    }


def fit_decomposed_camera_y_offsets(
    measurements: list[PhysicalCaptureMeasurement],
    basis: Dict[str, object],
) -> tuple[np.ndarray, Dict[str, object]]:
    """Fit three separated physical modes with their appropriate evidence."""

    nonplanar_coefficient, nonplanar_details = fit_nonplanar_synchronization_mode(
        measurements, basis
    )
    offsets, affine_details = fit_affine_synchronization_modes_from_truth(
        measurements, basis, nonplanar_coefficient
    )
    return offsets, {
        "method": "decomposed_camera_scan_row_synchronization",
        "nonplanar_mode": nonplanar_details,
        "affine_modes": affine_details,
    }


def paired_wireframe_cross_validation(
    measurements: list[PhysicalCaptureMeasurement],
    *,
    max_folds: int,
    max_rmse_deg: float,
    max_error_deg: float,
    max_repeatability_range_deg: float,
    max_parameter_spread_mm: float,
    fit_offsets: Callable[
        [list[PhysicalCaptureMeasurement], Dict[str, object]],
        tuple[np.ndarray, Dict[str, object]],
    ] = fit_decomposed_camera_y_offsets,
    fit_uses_professional_truth: bool = True,
) -> Dict[str, object]:
    """Use every usable capture once as truth-free 16-angle validation data."""

    groups = {
        orientation: sorted(
            [item for item in measurements if item.orientation == orientation],
            key=lambda item: item.path.name.casefold(),
        )
        for orientation in ("normal", "turnover")
    }
    fold_count = min(max_folds, len(groups["normal"]), len(groups["turnover"]))
    if fold_count < 2:
        raise ValueError("Paired wireframe cross-validation needs at least two captures per pose")
    fold_by_path: Dict[Path, int] = {}
    for values in groups.values():
        for index, item in enumerate(values):
            fold_by_path[item.path] = index % fold_count

    folds: list[Dict[str, object]] = []
    fold_offsets: list[np.ndarray] = []
    raw_group_residuals: list[float] = []
    corrected_group_residuals: list[float] = []
    raw_ordinal_residuals: list[float] = []
    corrected_ordinal_residuals: list[float] = []
    raw_predictions: Dict[str, list[Dict[str, object]]] = {
        "normal": [],
        "turnover": [],
    }
    corrected_predictions: Dict[str, list[Dict[str, object]]] = {
        "normal": [],
        "turnover": [],
    }
    for fold in range(fold_count):
        holdout = [item for item in measurements if fold_by_path[item.path] == fold]
        train = [item for item in measurements if fold_by_path[item.path] != fold]
        for orientation in ("normal", "turnover"):
            if sum(item.orientation == orientation for item in train) < 3:
                raise ValueError(
                    f"Wireframe fold {fold + 1} has fewer than three {orientation} training captures"
                )
        basis = synchronization_mode_basis(train)
        offsets, fit_details = fit_offsets(train, basis)
        fold_offsets.append(offsets)
        normal_holdout = [item for item in holdout if item.orientation == "normal"]
        turnover_holdout = [item for item in holdout if item.orientation == "turnover"]
        raw = wireframe_turnover_equivariance_validation(
            normal_holdout,
            turnover_holdout,
            np.zeros(4, dtype=float),
        )
        corrected = wireframe_turnover_equivariance_validation(
            normal_holdout,
            turnover_holdout,
            offsets,
        )
        raw_group_residuals.extend(
            float(row["group_median_sum_residual_deg"])
            for row in raw["channels"]
        )
        corrected_group_residuals.extend(
            float(row["group_median_sum_residual_deg"])
            for row in corrected["channels"]
        )
        raw_ordinal_residuals.extend(
            float(value)
            for row in raw["channels"]
            for value in row["ordinal_pair_residuals_deg"]
        )
        corrected_ordinal_residuals.extend(
            float(value)
            for row in corrected["channels"]
            for value in row["ordinal_pair_residuals_deg"]
        )
        for item in holdout:
            raw_predictions[item.orientation].append(
                wireframe_angles_for_capture(item, np.zeros(4, dtype=float))
            )
            corrected_predictions[item.orientation].append(
                wireframe_angles_for_capture(item, offsets)
            )
        folds.append(
            {
                "fold": fold + 1,
                "train_capture_count": len(train),
                "holdout_head_to_tail": [
                    item.path.name for item in normal_holdout
                ],
                "holdout_tail_to_head": [
                    item.path.name for item in turnover_holdout
                ],
                "camera_y_offsets_mm": {
                    str(camera): float(offsets[camera - 1])
                    for camera in (1, 2, 3, 4)
                },
                "fit_details": fit_details,
                "raw_validation": raw,
                "physical_corrected_validation": corrected,
            }
        )

    raw_array = np.asarray(raw_group_residuals, dtype=float)
    corrected_array = np.asarray(corrected_group_residuals, dtype=float)
    raw_ordinal_array = np.asarray(raw_ordinal_residuals, dtype=float)
    corrected_ordinal_array = np.asarray(corrected_ordinal_residuals, dtype=float)
    offset_array = np.asarray(fold_offsets, dtype=float)
    parameter_ranges = np.ptp(offset_array, axis=0)
    raw_rmse = float(math.sqrt(float(np.mean(np.square(raw_array)))))
    corrected_rmse = float(math.sqrt(float(np.mean(np.square(corrected_array)))))
    raw_ordinal_rmse = float(math.sqrt(float(np.mean(np.square(raw_ordinal_array)))))
    corrected_ordinal_rmse = float(
        math.sqrt(float(np.mean(np.square(corrected_ordinal_array))))
    )

    def maximum_prediction_range(
        predictions: Dict[str, list[Dict[str, object]]],
    ) -> float:
        maximum = 0.0
        for orientation in ("normal", "turnover"):
            for end, local_channel in WIREFRAME_CHANNELS:
                values = np.asarray(
                    [
                        float(result["angles"][end][local_channel])
                        for result in predictions[orientation]
                    ],
                    dtype=float,
                )
                maximum = max(maximum, float(np.ptp(values)))
        return maximum

    raw_max_repeatability = maximum_prediction_range(raw_predictions)
    corrected_max_repeatability = maximum_prediction_range(corrected_predictions)
    statistics = {
        "raw_group_median_rmse_deg": raw_rmse,
        "raw_group_median_max_abs_error_deg": float(np.max(np.abs(raw_array))),
        "physical_corrected_group_median_rmse_deg": corrected_rmse,
        "physical_corrected_group_median_max_abs_error_deg": float(
            np.max(np.abs(corrected_array))
        ),
        "raw_ordinal_pair_rmse_deg": raw_ordinal_rmse,
        "raw_ordinal_pair_max_abs_error_deg": float(
            np.max(np.abs(raw_ordinal_array))
        ),
        "physical_corrected_ordinal_pair_rmse_deg": corrected_ordinal_rmse,
        "physical_corrected_ordinal_pair_max_abs_error_deg": float(
            np.max(np.abs(corrected_ordinal_array))
        ),
        "raw_maximum_repeatability_range_deg": raw_max_repeatability,
        "physical_corrected_maximum_repeatability_range_deg": corrected_max_repeatability,
        "improvement_fraction": 1.0 - corrected_rmse / max(raw_rmse, 1e-12),
        "per_camera_parameter_range_mm": [float(value) for value in parameter_ranges],
        "maximum_parameter_range_mm": float(np.max(parameter_ranges)),
    }
    failures: list[str] = []
    validation_rmse = max(
        statistics["physical_corrected_group_median_rmse_deg"],
        statistics["physical_corrected_ordinal_pair_rmse_deg"],
    )
    validation_max_error = max(
        statistics["physical_corrected_group_median_max_abs_error_deg"],
        statistics["physical_corrected_ordinal_pair_max_abs_error_deg"],
    )
    if validation_rmse > max_rmse_deg:
        failures.append(
            "wireframe cross-validation RMS "
            f"{validation_rmse:.6f}° > "
            f"{max_rmse_deg:.6f}°"
        )
    if validation_max_error > max_error_deg:
        failures.append(
            "wireframe cross-validation max error "
            f"{validation_max_error:.6f}° > "
            f"{max_error_deg:.6f}°"
        )
    if (
        statistics["physical_corrected_maximum_repeatability_range_deg"]
        > max_repeatability_range_deg
    ):
        failures.append(
            "wireframe cross-validation repeatability range "
            f"{statistics['physical_corrected_maximum_repeatability_range_deg']:.6f}° > "
            f"{max_repeatability_range_deg:.6f}°"
        )
    if statistics["maximum_parameter_range_mm"] > max_parameter_spread_mm:
        failures.append(
            "wireframe cross-validation camera parameter range "
            f"{statistics['maximum_parameter_range_mm']:.6f} mm > "
            f"{max_parameter_spread_mm:.6f} mm"
        )
    if statistics["improvement_fraction"] < 0.20:
        failures.append(
            "wireframe cross-validation improves turnover RMS by only "
            f"{statistics['improvement_fraction'] * 100.0:.2f}%"
        )
    return {
        "method": "paired_capture_fold_validation_shared_16_angle_geometry",
        "fold_count": fold_count,
        "uses_each_usable_capture_as_holdout": True,
        "fit_uses_professional_truth_only_for_low_level_camera_y_parameters": bool(
            fit_uses_professional_truth
        ),
        "fit_uses_only_hobj_geometry_and_physical_turnover": bool(
            not fit_uses_professional_truth
        ),
        "validation_uses_professional_truth": False,
        "final_angle_correction_applied": False,
        "passed": not failures,
        "limits": {
            "max_rmse_deg": max_rmse_deg,
            "max_error_deg": max_error_deg,
            "max_repeatability_range_deg": max_repeatability_range_deg,
            "max_parameter_spread_mm": max_parameter_spread_mm,
            "minimum_improvement_fraction": 0.20,
        },
        "statistics": statistics,
        "folds": folds,
        "failures": failures,
    }


def image_model_candidate_acceptance(
    level: str,
    offsets: np.ndarray,
    raw_holdout: Dict[str, object],
    corrected_holdout: Dict[str, object],
    cross_validation: Dict[str, object],
    *,
    repeatability_degradation_tolerance_deg: float = 0.02,
) -> Dict[str, object]:
    """Require a candidate to improve Raw without destabilizing held-out results."""

    raw_fixed = raw_holdout["statistics"]
    corrected_fixed = corrected_holdout["statistics"]
    cross = cross_validation["statistics"]
    failures: list[str] = []
    if corrected_holdout.get("passed") is not True:
        failures.extend(str(value) for value in corrected_holdout.get("failures", []))
    if cross_validation.get("passed") is not True:
        failures.extend(str(value) for value in cross_validation.get("failures", []))
    comparisons = (
        (
            "fixed holdout RMS",
            float(corrected_fixed["ordinal_pair_rmse_deg"]),
            float(raw_fixed["ordinal_pair_rmse_deg"]),
        ),
        (
            "fixed holdout max error",
            float(corrected_fixed["ordinal_pair_max_abs_error_deg"]),
            float(raw_fixed["ordinal_pair_max_abs_error_deg"]),
        ),
        (
            "cross-validation RMS",
            float(cross["physical_corrected_ordinal_pair_rmse_deg"]),
            float(cross["raw_ordinal_pair_rmse_deg"]),
        ),
        (
            "cross-validation max error",
            float(cross["physical_corrected_ordinal_pair_max_abs_error_deg"]),
            float(cross["raw_ordinal_pair_max_abs_error_deg"]),
        ),
    )
    for label, corrected_value, raw_value in comparisons:
        if corrected_value >= raw_value:
            failures.append(
                f"{level} {label} {corrected_value:.6f} deg does not improve Raw {raw_value:.6f} deg"
            )
    raw_repeatability = float(cross["raw_maximum_repeatability_range_deg"])
    corrected_repeatability = float(
        cross["physical_corrected_maximum_repeatability_range_deg"]
    )
    repeatability_limit = raw_repeatability + repeatability_degradation_tolerance_deg
    if corrected_repeatability > repeatability_limit:
        failures.append(
            f"{level} repeatability range {corrected_repeatability:.6f} deg exceeds "
            f"Raw+tolerance {repeatability_limit:.6f} deg"
        )
    maximum_offset = float(np.max(np.abs(np.asarray(offsets, dtype=float))))
    if maximum_offset > 1.5:
        failures.append(f"{level} camera Y offset {maximum_offset:.6f} mm exceeds 1.500000 mm")
    return {
        "model_level": level,
        "selectable_for_runtime": level == "M1",
        "passed": not failures,
        "camera_y_offsets_mm": {
            str(camera): float(offsets[camera - 1]) for camera in (1, 2, 3, 4)
        },
        "fixed_blind_holdout": corrected_holdout,
        "paired_cross_validation": cross_validation,
        "raw_improvement_requirements": {
            "all_fixed_and_cross_validated_rms_and_max_error_must_improve": True,
            "maximum_repeatability_degradation_tolerance_deg": (
                repeatability_degradation_tolerance_deg
            ),
        },
        "failures": failures,
    }


def synchronization_cross_audit(
    measurements: list[PhysicalCaptureMeasurement],
    parameter_sets: Dict[str, np.ndarray],
) -> Dict[str, object]:
    """Cross-apply fitted states to both acquisition groups without refitting."""

    groups = {
        "head_to_tail": [item for item in measurements if item.orientation == "normal"],
        "tail_to_head": [item for item in measurements if item.orientation == "turnover"],
        "all": list(measurements),
    }
    audit: Dict[str, object] = {}
    for parameter_name, offsets in parameter_sets.items():
        values: Dict[str, object] = {}
        gauge = np.asarray(offsets, dtype=float)[:3]
        for group_name, group in groups.items():
            point_residual = physical_plane_residuals(
                group,
                gauge,
                equalize_capture_weight=False,
            )
            truth_residual = physical_residuals(group, gauge)
            values[group_name] = {
                "point_to_plane_rmse_mm": float(
                    math.sqrt(float(np.mean(point_residual * point_residual)))
                ),
                "truth_angle_rmse_deg": residual_statistics(truth_residual)["rmse_deg"],
                "truth_angle_max_abs_error_deg": residual_statistics(truth_residual)[
                    "max_abs_error_deg"
                ],
            }
        audit[parameter_name] = values
    return audit


def scan_angles_in_physical_standard_frame(
    scan_angles: Dict[str, Dict[str, float]],
    orientation: str,
) -> Dict[str, Dict[str, float]]:
    """Map device-row angle channels back to the standard's physical R/L frame."""

    if orientation == "normal":
        return {end: dict(values) for end, values in scan_angles.items()}
    if orientation != "turnover":
        raise ValueError(f"Unknown physical pose: {orientation}")
    face_map = professional_turnover_face_map()
    end_map = {"head": "tail", "tail": "head"}
    return {
        physical_end: {
            physical_face: 180.0
            - float(scan_angles[end_map[physical_end]][face_map[physical_face]])
            for physical_face in FACES
        }
        for physical_end in ENDS
    }


def physical_direction_spread(
    measurements: list[PhysicalCaptureMeasurement],
    offsets: np.ndarray,
) -> Dict[str, object]:
    group_means: Dict[str, Dict[str, Dict[str, float]]] = {}
    for orientation in ("normal", "turnover"):
        group = [item for item in measurements if item.orientation == orientation]
        values = {end: {face: [] for face in FACES} for end in ENDS}
        for item in group:
            scan_angles, _ = physical_angles_for_capture(item, offsets)
            physical = scan_angles_in_physical_standard_frame(
                scan_angles,
                item.orientation,
            )
            for end, face in CHANNELS:
                values[end][face].append(float(physical[end][face]))
        group_means[orientation] = {
            end: {face: float(np.mean(values[end][face])) for face in FACES}
            for end in ENDS
        }
    spreads = {
        end: {
            face: abs(group_means["normal"][end][face] - group_means["turnover"][end][face])
            for face in FACES
        }
        for end in ENDS
    }
    return {
        "physical_channel_means_deg": {
            "head_to_tail": group_means["normal"],
            "tail_to_head_mapped_back_after_physical_turnover": group_means["turnover"],
        },
        "channel_spread_deg": spreads,
        "max_channel_spread_deg": max(spreads[end][face] for end, face in CHANNELS),
    }


def validate_rigid_physical_turnover_measurements(
    before: list[PhysicalCaptureMeasurement],
    after: list[PhysicalCaptureMeasurement],
    offsets: np.ndarray,
    *,
    max_rmse_deg: float = 0.10,
    max_error_deg: float = 0.20,
    max_repeatability_range_deg: float = 0.10,
) -> Dict[str, object]:
    """Validate a real end-for-end turnover without fitting or truth lookup.

    Both groups must be acquired in the same canonical device coordinate and
    scan direction.  The second group is the same physical bar rotated 180°
    about the A/D axis: endpoints exchange, institution B/C exchange (software
    B/D), and the fixed +Y directed-angle convention gives ``theta'=180-theta``.
    """

    if len(before) < 2 or len(after) < 2:
        raise ValueError("Physical-turnover validation needs at least two holdout captures in each pose")
    groups: Dict[str, list[Dict[str, Dict[str, float]]]] = {"before": [], "after": []}
    plane_quality: list[Dict[str, object]] = []
    for group_name, captures in (("before", before), ("after", after)):
        for item in captures:
            angles, fits = physical_angles_for_capture(item, offsets)
            maximum_plane_rmse = max(float(fits[end].rmse_mm) for end in ENDS)
            plane_quality.append(
                {
                    "group": group_name,
                    "capture": item.path.name,
                    "maximum_endface_plane_rmse_mm": maximum_plane_rmse,
                }
            )
            if maximum_plane_rmse > 0.50:
                raise ValueError(
                    f"Physical-turnover capture {item.path.name} plane RMSE "
                    f"{maximum_plane_rmse:.6f} mm exceeds 0.500000 mm"
                )
            groups[group_name].append(angles)

    means = {
        group_name: {
            end: {
                face: float(np.mean([angles[end][face] for angles in group]))
                for face in FACES
            }
            for end in ENDS
        }
        for group_name, group in groups.items()
    }
    ranges = {
        group_name: {
            end: {
                face: float(np.ptp([angles[end][face] for angles in group]))
                for face in FACES
            }
            for end in ENDS
        }
        for group_name, group in groups.items()
    }
    face_map = professional_turnover_face_map()
    errors: list[float] = []
    channels: list[Dict[str, object]] = []
    for before_end, after_end in (("head", "tail"), ("tail", "head")):
        for before_face, after_face in face_map.items():
            expected = 180.0 - means["before"][before_end][before_face]
            measured = means["after"][after_end][after_face]
            error = measured - expected
            errors.append(error)
            channels.append(
                {
                    "before_end": before_end,
                    "before_software_face": before_face,
                    "after_end": after_end,
                    "after_software_face": after_face,
                    "expected_after_angle_deg": expected,
                    "measured_after_angle_deg": measured,
                    "error_deg": error,
                }
            )
    statistics = residual_statistics(errors)
    maximum_repeatability_range = max(
        ranges[group][end][face]
        for group in ("before", "after")
        for end, face in CHANNELS
    )
    failures: list[str] = []
    if statistics["rmse_deg"] > max_rmse_deg:
        failures.append(
            f"physical-turnover RMSE {statistics['rmse_deg']:.6f}° > {max_rmse_deg:.6f}°"
        )
    if statistics["max_abs_error_deg"] > max_error_deg:
        failures.append(
            f"physical-turnover max error {statistics['max_abs_error_deg']:.6f}° > "
            f"{max_error_deg:.6f}°"
        )
    if maximum_repeatability_range > max_repeatability_range_deg:
        failures.append(
            f"physical-turnover repeatability range {maximum_repeatability_range:.6f}° > "
            f"{max_repeatability_range_deg:.6f}°"
        )
    return {
        "passed": not failures,
        "method": "independent_hobj_rigid_turnover_equivariance_no_refit_no_truth_lookup",
        "directed_angle_law": "after=180-before",
        "software_face_map": face_map,
        "capture_counts": {"before": len(before), "after": len(after)},
        "group_means_deg": means,
        "group_ranges_deg": ranges,
        "maximum_repeatability_range_deg": maximum_repeatability_range,
        "statistics": statistics,
        "limits": {
            "max_rmse_deg": max_rmse_deg,
            "max_error_deg": max_error_deg,
            "max_repeatability_range_deg": max_repeatability_range_deg,
        },
        "channels": channels,
        "plane_quality": plane_quality,
        "failures": failures,
    }


def build_physical_model(
    measurements: list[PhysicalCaptureMeasurement],
    truth: TruthData,
    camera_path: Path,
    truth_path: Path,
    y_scale: float,
    max_direction_spread: float,
    max_validation_rmse: float,
    max_validation_error: float,
    max_direction_parameter_spread_mm: float = 0.25,
    max_loo_parameter_range_mm: float = 0.10,
    max_turnover_rmse_deg: float = 0.10,
    max_turnover_error_deg: float = 0.20,
    max_turnover_repeatability_range_deg: float = 0.10,
    max_wireframe_turnover_rmse_deg: float = 0.30,
    max_wireframe_turnover_error_deg: float = 0.60,
    max_wireframe_repeatability_range_deg: float = 0.20,
    max_wireframe_cv_parameter_spread_mm: float = 0.15,
    wireframe_cross_validation_folds: int = 5,
) -> tuple[Dict[str, object], list[Dict[str, object]]]:
    """Build v13 from separated physical modes with 16-angle safety gates."""

    row_order_audit = audit_calibration_row_order(measurements)
    capture_quality: list[Dict[str, object]] = []
    for item in measurements:
        _, raw_fits = physical_angles_for_capture(
            item,
            np.zeros(4, dtype=float),
        )
        maximum_plane_rmse = max(float(raw_fits[end].rmse_mm) for end in ENDS)
        reasons: list[str] = []
        if item.row_order_status != "canonical":
            reasons.append(
                f"HOBJ spatial row order is {item.row_order_status}; "
                f"same={item.row_order_same_rmse_mm:.6f} mm, "
                f"reversed={item.row_order_reversed_rmse_mm:.6f} mm"
            )
        if maximum_plane_rmse > 0.50:
            reasons.append(
                f"raw end-face boundary plane RMSE {maximum_plane_rmse:.6f} mm > 0.500000 mm"
            )
        relative_status = str(
            item.relative_camera_diagnostic.get("status", "missing")
        )
        excluded = bool(reasons)
        if excluded:
            item.split = "excluded_geometry"
        capture_quality.append(
            {
                "capture": item.path.name,
                "orientation": item.orientation,
                "maximum_raw_endface_plane_rmse_mm": maximum_plane_rmse,
                "row_order_status": item.row_order_status,
                "row_order_same_rmse_mm": item.row_order_same_rmse_mm,
                "row_order_reversed_rmse_mm": item.row_order_reversed_rmse_mm,
                "relative_camera_geometry_status": relative_status,
                "relative_camera_max_seam_span_mm": item.relative_camera_diagnostic.get(
                    "maximum_seam_span_p05_p95_mm", math.nan
                ),
                "excluded": excluded,
                "reason": "; ".join(reasons),
            }
        )
    usable = [item for item in measurements if item.split != "excluded_geometry"]
    train = [item for item in usable if item.split == "train"]
    holdout = [item for item in usable if item.split == "holdout"]
    if not train or not holdout:
        raise ValueError("Both training and holdout captures are required")
    for orientation in ("normal", "turnover"):
        if sum(item.orientation == orientation for item in train) < 3:
            raise ValueError(f"Too few usable {orientation} training captures after geometry QC")
    state_reference = calibration_state_reference(usable)
    basis = synchronization_mode_basis(train)
    offsets, optimizer = fit_decomposed_camera_y_offsets(train, basis)
    orientation_offsets: Dict[str, np.ndarray] = {}
    orientation_optimizers: Dict[str, Dict[str, object]] = {}
    for orientation in ("normal", "turnover"):
        group_train = [item for item in train if item.orientation == orientation]
        group_offsets, group_optimizer = fit_decomposed_camera_y_offsets(
            group_train, basis
        )
        orientation_offsets[orientation] = group_offsets
        orientation_optimizers[orientation] = group_optimizer
    orientation_offset_spread = float(
        np.max(np.abs(orientation_offsets["normal"] - orientation_offsets["turnover"]))
    )
    cross_audit = synchronization_cross_audit(
        train,
        {
            "zero": np.zeros(4, dtype=float),
            "shared": offsets,
            "head_to_tail_only": orientation_offsets["normal"],
            "tail_to_head_only": orientation_offsets["turnover"],
        },
    )
    zero_parameters = np.zeros(3, dtype=float)
    fitted_parameters = offsets[:3]
    raw_train = residual_statistics(
        physical_residuals(train, zero_parameters)
    )
    raw_holdout = residual_statistics(
        physical_residuals(holdout, zero_parameters)
    )
    corrected_train = residual_statistics(
        physical_residuals(train, fitted_parameters)
    )
    corrected_holdout = residual_statistics(
        physical_residuals(holdout, fitted_parameters)
    )
    direction = physical_direction_spread(usable, offsets)
    turnover_holdout_validation = validate_rigid_physical_turnover_measurements(
        [item for item in holdout if item.orientation == "normal"],
        [item for item in holdout if item.orientation == "turnover"],
        offsets,
        max_rmse_deg=max_turnover_rmse_deg,
        max_error_deg=max_turnover_error_deg,
        max_repeatability_range_deg=max_turnover_repeatability_range_deg,
    )
    wireframe_holdout_validation = wireframe_turnover_equivariance_validation(
        [item for item in holdout if item.orientation == "normal"],
        [item for item in holdout if item.orientation == "turnover"],
        offsets,
        max_rmse_deg=max_wireframe_turnover_rmse_deg,
        max_error_deg=max_wireframe_turnover_error_deg,
        max_repeatability_range_deg=max_wireframe_repeatability_range_deg,
    )
    wireframe_cross_validation = paired_wireframe_cross_validation(
        usable,
        max_folds=wireframe_cross_validation_folds,
        max_rmse_deg=max_wireframe_turnover_rmse_deg,
        max_error_deg=max_wireframe_turnover_error_deg,
        max_repeatability_range_deg=max_wireframe_repeatability_range_deg,
        max_parameter_spread_mm=max_wireframe_cv_parameter_spread_mm,
    )
    loo_offsets: list[np.ndarray] = []
    loo_details: list[Dict[str, object]] = []
    for excluded in train:
        fitted, details = fit_decomposed_camera_y_offsets(
            [item for item in train if item is not excluded], basis
        )
        loo_offsets.append(fitted)
        loo_details.append(
            {
                "excluded_capture": excluded.path.name,
                "camera_y_offsets_mm": [float(value) for value in fitted],
                "nonplanar_coefficient_mm": float(
                    details["nonplanar_mode"]["history"][-1]["coefficient_mm"]
                ),
            }
        )
    loo_array = np.asarray(loo_offsets, dtype=float)
    loo_parameter_ranges = np.ptp(loo_array, axis=0)
    maximum_loo_parameter_range = float(np.max(loo_parameter_ranges))
    failures: list[str] = []
    capture_mean_spans = [
        float(np.mean(list(item.camera_valid_span_rows.values())))
        for item in usable
        if len(item.camera_valid_span_rows) == 4
    ]
    median_span_rows = (
        float(np.median(np.asarray(capture_mean_spans, dtype=float)))
        if capture_mean_spans
        else math.nan
    )
    implied_y_scale = (
        float(truth.rod_length_mm / median_span_rows)
        if math.isfinite(truth.rod_length_mm)
        and truth.rod_length_mm > 0.0
        and math.isfinite(median_span_rows)
        and median_span_rows > 0.0
        else math.nan
    )
    y_scale_relative_difference = (
        abs(implied_y_scale / y_scale - 1.0)
        if math.isfinite(implied_y_scale) and y_scale > 0.0
        else math.nan
    )
    if not math.isfinite(y_scale_relative_difference):
        failures.append(
            "professional rod length or four-camera valid-row spans are unavailable; "
            "the Y row-pitch audit cannot be completed"
        )
    elif y_scale_relative_difference > 0.005:
        failures.append(
            f"configured Y scale {y_scale:.12f} mm/row differs from the CMM-length "
            f"implied scale {implied_y_scale:.12f} mm/row by "
            f"{y_scale_relative_difference * 100.0:.4f}% > 0.5000%"
        )
    if state_reference.get("all_captures_stable") is not True:
        unstable = [
            str(item.path.name)
            for item in usable
            if item.relative_camera_diagnostic.get("status")
            != "relative_geometry_stable"
        ]
        failures.append(
            "calibration camera state is not image-geometrically stable in every usable capture; "
            "HOBJ alone cannot distinguish rig motion from true face non-planarity, so physical "
            "camera synchronization cannot be released. Affected captures: "
            + ", ".join(unstable)
        )
    if corrected_holdout["rmse_deg"] > max_validation_rmse:
        failures.append(
            f"physical holdout RMSE {corrected_holdout['rmse_deg']:.6f}° > "
            f"{max_validation_rmse:.6f}°"
        )
    if corrected_holdout["max_abs_error_deg"] > max_validation_error:
        failures.append(
            f"physical holdout max error {corrected_holdout['max_abs_error_deg']:.6f}° > "
            f"{max_validation_error:.6f}°"
        )
    if direction["max_channel_spread_deg"] > max_direction_spread:
        failures.append(
            f"head_to_tail/tail_to_head physical-channel spread {direction['max_channel_spread_deg']:.6f}° > "
            f"{max_direction_spread:.6f}°"
        )
    improvement = 1.0 - corrected_holdout["rmse_deg"] / max(raw_holdout["rmse_deg"], 1e-12)
    if improvement < 0.20:
        failures.append(
            f"physical synchronization improves holdout RMSE by only {improvement * 100.0:.2f}%"
        )
    if float(np.max(np.abs(offsets))) > 1.5:
        failures.append("a camera synchronization offset exceeds the physical 1.5 mm limit")
    if orientation_offset_spread > max_direction_parameter_spread_mm:
        failures.append(
            f"head_to_tail/tail_to_head camera synchronization parameter spread "
            f"{orientation_offset_spread:.6f} mm > "
            f"{max_direction_parameter_spread_mm:.6f} mm"
        )
    if maximum_loo_parameter_range > max_loo_parameter_range_mm:
        failures.append(
            f"leave-one-capture-out camera parameter range "
            f"{maximum_loo_parameter_range:.6f} mm > "
            f"{max_loo_parameter_range_mm:.6f} mm"
        )
    if not turnover_holdout_validation["passed"]:
        failures.extend(
            "holdout " + failure
            for failure in turnover_holdout_validation["failures"]
        )
    if not wireframe_holdout_validation["passed"]:
        failures.extend(
            "wireframe holdout " + failure
            for failure in wireframe_holdout_validation["failures"]
        )
    if not wireframe_cross_validation["passed"]:
        failures.extend(
            "wireframe cross-validation " + failure
            for failure in wireframe_cross_validation["failures"]
        )

    diagnostics: list[Dict[str, object]] = []
    max_plane_rmse = 0.0
    for item in measurements:
        raw_angles, raw_fits = physical_angles_for_capture(
            item,
            np.zeros(4, dtype=float),
        )
        corrected_angles, corrected_fits = physical_angles_for_capture(
            item,
            offsets,
        )
        raw_wireframe: Dict[str, object] | None = None
        corrected_wireframe: Dict[str, object] | None = None
        wireframe_error = ""
        try:
            raw_wireframe = wireframe_angles_for_capture(
                item,
                np.zeros(4, dtype=float),
            )
            corrected_wireframe = wireframe_angles_for_capture(item, offsets)
        except ValueError as exc:
            wireframe_error = str(exc)
            if item.split != "excluded_geometry":
                failures.append(
                    f"usable capture {item.path.name} wireframe reconstruction failed: {exc}"
                )
        for end in ENDS:
            if item.split != "excluded_geometry":
                max_plane_rmse = max(
                    max_plane_rmse,
                    float(raw_fits[end].rmse_mm),
                    float(corrected_fits[end].rmse_mm),
                )
            for face in FACES:
                diagnostics.append(
                    {
                        "capture": item.path.name,
                        "input_path": str(item.path),
                        "orientation": item.orientation,
                        "acquisition_group": (
                            "head_to_tail" if item.orientation == "normal" else "tail_to_head"
                        ),
                        "row_order_status": item.row_order_status,
                        "split": item.split,
                        "channel_end": end,
                        "channel_face": SOFTWARE_TO_INSTITUTION_FACE_MAP[face],
                        "software_channel_face": face,
                        "raw_angle_deg": f"{raw_angles[end][face]:.9f}",
                        "physical_corrected_angle_deg": f"{corrected_angles[end][face]:.9f}",
                        "mapped_truth_deg": f"{item.scan_truth[end][face]:.9f}",
                        "error_deg": f"{corrected_angles[end][face] - item.scan_truth[end][face]:.9f}",
                        "raw_endface_plane_rmse_mm": f"{raw_fits[end].rmse_mm:.9f}",
                        "corrected_endface_plane_rmse_mm": f"{corrected_fits[end].rmse_mm:.9f}",
                        "relative_camera_geometry_status": item.relative_camera_diagnostic.get(
                            "status", "missing"
                        ),
                        "relative_camera_max_seam_span_mm": item.relative_camera_diagnostic.get(
                            "maximum_seam_span_p05_p95_mm", math.nan
                        ),
                    }
                )
        if raw_wireframe is None or corrected_wireframe is None:
            diagnostics.append(
                {
                    "capture": item.path.name,
                    "input_path": str(item.path),
                    "orientation": item.orientation,
                    "acquisition_group": (
                        "head_to_tail" if item.orientation == "normal" else "tail_to_head"
                    ),
                    "split": item.split,
                    "diagnostic_kind": "wireframe_reconstruction_failure",
                    "uses_professional_truth": False,
                    "final_angle_correction_applied": False,
                    "reason": wireframe_error,
                }
            )
            continue
        for end, local_channel in WIREFRAME_CHANNELS:
            raw_summary = raw_wireframe["summaries"][end]
            corrected_summary = corrected_wireframe["summaries"][end]
            raw_shape = raw_wireframe["diagnostics"][end]
            corrected_shape = corrected_wireframe["diagnostics"][end]
            diagnostics.append(
                {
                    "capture": item.path.name,
                    "input_path": str(item.path),
                    "orientation": item.orientation,
                    "acquisition_group": (
                        "head_to_tail" if item.orientation == "normal" else "tail_to_head"
                    ),
                    "row_order_status": item.row_order_status,
                    "split": item.split,
                    "diagnostic_kind": "wireframe_local_angle",
                    "channel_end": end,
                    "local_channel": local_channel,
                    "raw_angle_deg": f"{raw_wireframe['angles'][end][local_channel]:.9f}",
                    "physical_corrected_angle_deg": (
                        f"{corrected_wireframe['angles'][end][local_channel]:.9f}"
                    ),
                    "raw_representative_angle_deg": f"{raw_summary['worst_local_angle_deg']:.9f}",
                    "physical_corrected_representative_angle_deg": (
                        f"{corrected_summary['worst_local_angle_deg']:.9f}"
                    ),
                    "raw_worst_local_channel": raw_summary["worst_local_channel"],
                    "physical_corrected_worst_local_channel": corrected_summary[
                        "worst_local_channel"
                    ],
                    "raw_max_dual_camera_seam_mm": (
                        f"{raw_shape['max_dual_camera_seam_mid_mm']:.9f}"
                    ),
                    "physical_corrected_max_dual_camera_seam_mm": (
                        f"{corrected_shape['max_dual_camera_seam_mid_mm']:.9f}"
                    ),
                    "raw_max_corner_closure_gap_mm": (
                        f"{raw_shape['max_corner_closure_gap_mm']:.9f}"
                    ),
                    "physical_corrected_max_corner_closure_gap_mm": (
                        f"{corrected_shape['max_corner_closure_gap_mm']:.9f}"
                    ),
                    "uses_professional_truth": False,
                    "final_angle_correction_applied": False,
                }
            )
    if max_plane_rmse > 0.50:
        failures.append(
            f"usable end-face boundary plane RMSE reaches {max_plane_rmse:.6f} mm > 0.500000 mm"
        )
    counts = {
        orientation: {
            split: sum(item.orientation == orientation and item.split == split for item in measurements)
            for split in ("train", "holdout", "excluded_geometry")
        }
        for orientation in ("normal", "turnover")
    }
    model: Dict[str, object] = {
        "version": 13,
        "model": "endface_camera_geometry_calibration",
        "strategy": "physical_decomposed_camera_scan_row_synchronization",
        "valid": not failures,
        "created_at": datetime.now().isoformat(timespec="seconds"),
        "note": (
            "Four camera scan-row offsets are decomposed into one non-planar image-only mode "
            "and two affine device modes constrained by professional truth. The same per-camera "
            "Y synchronization offsets are applied to all longitudinal side points and all "
            "end-boundary points, across both ends and both physical bar poses. "
            "There are no final face-angle offsets, no output clamping, and no runtime truth or "
            "orientation lookup. Head/tail mean canonical HOBJ device-row minimum/maximum, not "
            "motor travel start/end; physical turnover must appear naturally. A same-face, "
            "dual-camera image envelope blocks the physical correction when the current rig "
            "state no longer matches calibration; it never creates a correction. The production "
            "16-angle wireframe geometry is also used unchanged for blind holdout and paired-fold "
            "physical-turnover validation."
        ),
        "source_report_id": truth.report_id,
        "source_truth_csv": str(truth_path.resolve()),
        "source_camera_calibration": str(camera_path.resolve()),
        "sample_id": truth.sample_id,
        "nominal_spec": truth.nominal_spec,
        # This maps institution labels only while fitting the known standard;
        # it is not a production orientation lookup.
        "professional_truth_end_mapping": {truth.head_label: "head", truth.tail_label: "tail"},
        "head_tail_definition": "canonical_hobj_device_row_min_and_row_max",
        "runtime_endpoint_label_contract": {
            "head": "hobj_device_row_min",
            "tail": "hobj_device_row_max",
            "physical_R_L_inferred": False,
            "reason": "HOBJ contains no physical endpoint identity metadata",
        },
        "runtime_orientation_detection": "none",
        "report_to_software_face_map": dict(INSTITUTION_TO_SOFTWARE_FACE_MAP),
        "institution_report_face_layout": {"top": "A", "bottom": "D", "left": "C", "right": "B"},
        "camera_y_offsets_mm": {
            str(obj): float(offsets[obj - 1]) for obj in (1, 2, 3, 4)
        },
        "camera_y_offsets_rows": {
            str(obj): float(offsets[obj - 1] / y_scale) for obj in (1, 2, 3, 4)
        },
        "y_axis_scale_audit": {
            "method": "cmm_rod_length_vs_median_four_camera_valid_row_span_check_only",
            "configured_y_scale_mm_per_row": y_scale,
            "professional_rod_length_mm": truth.rod_length_mm,
            "median_four_camera_span_rows": median_span_rows,
            "implied_y_scale_mm_per_row": implied_y_scale,
            "relative_difference_fraction": y_scale_relative_difference,
            "maximum_relative_difference_fraction": 0.005,
            "correction_applied": False,
            "reason": (
                "The report length is an independent scale audit. It must not silently refit "
                "Y because end slope and boundary visibility also affect observed row spans."
            ),
        },
        "y_coordinate_correction": {
            "method": "per_camera_scan_row_offset",
            "apply_to": "all_longitudinal_side_points_and_end_boundary_points",
        },
        "offset_gauge_constraint": "sum(camera_y_offsets_mm)=0",
        "physical_mode_basis": {
            key: (
                [float(value) for value in np.asarray(basis[key], dtype=float)]
                if key in {"affine_x_mode", "affine_z_mode", "nonplanar_mode"}
                else basis[key]
            )
            for key in basis
        },
        "prohibited_fields_absent": {
            "angle_offsets_deg": True,
            "orientation_models": True,
            "orientation_detector": True,
        },
        "professional_truth_angles_deg": {
            end: {
                report_face: truth.angles[end][software_face]
                for report_face, software_face in INSTITUTION_TO_SOFTWARE_FACE_MAP.items()
            }
            for end in ENDS
        },
        "software_truth_angles_deg": truth.angles,
        "capture_counts": {
            "head_to_tail": counts["normal"],
            "tail_to_head": counts["turnover"],
        },
        "capture_geometry_quality": {
            "method": "image_only_row_order_and_raw_endface_boundary_plane_rmse",
            "max_plane_rmse_mm": 0.50,
            "row_order_audit": row_order_audit,
            "per_capture": capture_quality,
            "excluded_captures": [item for item in capture_quality if item["excluded"]],
        },
        "calibration_state_reference": state_reference,
        "fit_details": {
            "shared": optimizer,
            "orientation_stability_audit": {
                "head_to_tail_camera_y_offsets_mm": [
                    float(value) for value in orientation_offsets["normal"]
                ],
                "tail_to_head_camera_y_offsets_mm": [
                    float(value) for value in orientation_offsets["turnover"]
                ],
                "maximum_parameter_spread_mm": orientation_offset_spread,
                "limit_mm": max_direction_parameter_spread_mm,
                "head_to_tail_optimizer": orientation_optimizers["normal"],
                "tail_to_head_optimizer": orientation_optimizers["turnover"],
                "cross_application": cross_audit,
            },
            "leave_one_capture_out_stability": {
                "per_capture": loo_details,
                "per_camera_parameter_range_mm": [
                    float(value) for value in loo_parameter_ranges
                ],
                "maximum_parameter_range_mm": maximum_loo_parameter_range,
                "limit_mm": max_loo_parameter_range_mm,
            },
        },
        "direction_equivariance": direction,
        "physical_turnover_hobj_validation": turnover_holdout_validation,
        "wireframe_validation": {
            "method": "shared_production_16_angle_holdout_and_paired_cross_validation",
            "shared_geometry_module": "endface_wireframe_geometry.measure_wireframe_angles",
            "fixed_blind_holdout": wireframe_holdout_validation,
            "paired_cross_validation": wireframe_cross_validation,
            "passed": bool(
                wireframe_holdout_validation["passed"]
                and wireframe_cross_validation["passed"]
            ),
        },
        "runtime_output_contract": {
            "local_angles_per_end": 8,
            "total_local_angles": 16,
            "representative_value_per_end": "actual_local_angle_farthest_from_90_not_average",
            "raw_and_physical_corrected_retained": True,
        },
        "validation": {
            "raw_train": raw_train,
            "raw_holdout": raw_holdout,
            "physical_corrected_train": corrected_train,
            "physical_corrected_holdout": corrected_holdout,
            "holdout_improvement_fraction": improvement,
            "maximum_endface_plane_rmse_mm": max_plane_rmse,
            "calibration_uncertainty_rmse_deg": corrected_holdout["rmse_deg"],
            "wireframe_cross_validation_uncertainty_rmse_deg": wireframe_cross_validation[
                "statistics"
            ]["physical_corrected_group_median_rmse_deg"],
            "limits": {
                "max_direction_spread_deg": max_direction_spread,
                "max_validation_rmse_deg": max_validation_rmse,
                "max_validation_error_deg": max_validation_error,
                "max_camera_y_offset_mm": 1.5,
                "max_endface_plane_rmse_mm": 0.50,
                "max_direction_parameter_spread_mm": max_direction_parameter_spread_mm,
                "max_loo_parameter_range_mm": max_loo_parameter_range_mm,
            },
        },
        "four_angle_mean_warning": (
            "The arithmetic mean is geometrically near 90 degrees for opposite face pairs and "
            "must not be used as the primary perpendicularity quality metric."
        ),
        "release_readiness": {
            "ready": bool(
                not failures
                and turnover_holdout_validation["passed"]
                and wireframe_holdout_validation["passed"]
                and wireframe_cross_validation["passed"]
            ),
            "reason": (
                "All image-state, Y-scale, calibration, blind holdout, and video-verified physical "
                "turnover HOBJ gates pass."
                if not failures and turnover_holdout_validation["passed"]
                else "; ".join(failures)
            ),
            "calibration_holdout_passed": not failures,
            "image_row_order_audited": True,
            "synthetic_rigid_turnover_unit_test_required": True,
            "verified_physical_turnover_hobj_passed": bool(
                turnover_holdout_validation["passed"]
            ),
            "wireframe_16_angle_holdout_passed": bool(
                wireframe_holdout_validation["passed"]
            ),
            "wireframe_16_angle_cross_validation_passed": bool(
                wireframe_cross_validation["passed"]
            ),
            "physical_turnover_evidence": (
                "On-site video verified that tail_to_head is the same standard physically turned "
                "end-for-end; the last two captures in each pose are blind holdout data."
            ),
        },
        "failures": failures,
    }
    return model, diagnostics


def build_image_self_consistency_model(
    measurements: list[PhysicalCaptureMeasurement],
    camera_path: Path,
    camera_calibration: Dict[str, object],
    *,
    max_wireframe_turnover_rmse_deg: float = 0.30,
    max_wireframe_turnover_error_deg: float = 0.60,
    max_wireframe_repeatability_range_deg: float = 0.20,
    max_wireframe_cv_parameter_spread_mm: float = 0.15,
    wireframe_cross_validation_folds: int = 5,
) -> tuple[Dict[str, object], list[Dict[str, object]]]:
    """Build a v15 nested M0/M1/M2 audit using HOBJ geometry only."""

    row_order_audit = audit_calibration_row_order(measurements)
    capture_quality: list[Dict[str, object]] = []
    for item in measurements:
        _, raw_fits = physical_angles_for_capture(item, np.zeros(4, dtype=float))
        maximum_plane_rmse = max(float(raw_fits[end].rmse_mm) for end in ENDS)
        reasons: list[str] = []
        if item.row_order_status != "canonical":
            reasons.append(
                f"HOBJ spatial row order is {item.row_order_status}; "
                f"same={item.row_order_same_rmse_mm:.6f} mm, "
                f"reversed={item.row_order_reversed_rmse_mm:.6f} mm"
            )
        if maximum_plane_rmse > 0.50:
            reasons.append(
                f"raw end-face boundary plane RMSE {maximum_plane_rmse:.6f} mm > 0.500000 mm"
            )
        excluded = bool(reasons)
        if excluded:
            item.split = "excluded_geometry"
        capture_quality.append(
            {
                "capture": item.path.name,
                "orientation": item.orientation,
                "maximum_raw_endface_plane_rmse_mm": maximum_plane_rmse,
                "row_order_status": item.row_order_status,
                "row_order_same_rmse_mm": item.row_order_same_rmse_mm,
                "row_order_reversed_rmse_mm": item.row_order_reversed_rmse_mm,
                "relative_camera_geometry_status": item.relative_camera_diagnostic.get(
                    "status", "missing"
                ),
                "relative_camera_max_seam_span_mm": item.relative_camera_diagnostic.get(
                    "maximum_seam_span_p05_p95_mm", math.nan
                ),
                "excluded": excluded,
                "reason": "; ".join(reasons),
            }
        )

    usable = [item for item in measurements if item.split != "excluded_geometry"]
    train = [item for item in usable if item.split == "train"]
    holdout = [item for item in usable if item.split == "holdout"]
    for label, values in (("training", train), ("holdout", holdout)):
        for orientation in ("normal", "turnover"):
            minimum = 3 if label == "training" else 2
            count = sum(item.orientation == orientation for item in values)
            if count < minimum:
                raise ValueError(
                    f"Image self-consistency needs at least {minimum} {orientation} {label} captures; got {count}"
                )

    state_reference = calibration_state_reference(usable)
    basis = synchronization_mode_basis(train)
    m1_offsets, m1_optimizer = fit_nonplanar_camera_y_offsets_from_images(train, basis)
    m2_offsets, m2_optimizer = fit_decomposed_camera_y_offsets_from_images(train, basis)
    raw_holdout_validation = wireframe_turnover_equivariance_validation(
        [item for item in holdout if item.orientation == "normal"],
        [item for item in holdout if item.orientation == "turnover"],
        np.zeros(4, dtype=float),
    )
    m1_holdout_validation = wireframe_turnover_equivariance_validation(
        [item for item in holdout if item.orientation == "normal"],
        [item for item in holdout if item.orientation == "turnover"],
        m1_offsets,
        max_rmse_deg=max_wireframe_turnover_rmse_deg,
        max_error_deg=max_wireframe_turnover_error_deg,
        max_repeatability_range_deg=max_wireframe_repeatability_range_deg,
    )
    m1_cross_validation = paired_wireframe_cross_validation(
        usable,
        max_folds=wireframe_cross_validation_folds,
        max_rmse_deg=max_wireframe_turnover_rmse_deg,
        max_error_deg=max_wireframe_turnover_error_deg,
        max_repeatability_range_deg=max_wireframe_repeatability_range_deg,
        max_parameter_spread_mm=max_wireframe_cv_parameter_spread_mm,
        fit_offsets=fit_nonplanar_camera_y_offsets_from_images,
        fit_uses_professional_truth=False,
    )
    m2_holdout_validation = wireframe_turnover_equivariance_validation(
        [item for item in holdout if item.orientation == "normal"],
        [item for item in holdout if item.orientation == "turnover"],
        m2_offsets,
        max_rmse_deg=max_wireframe_turnover_rmse_deg,
        max_error_deg=max_wireframe_turnover_error_deg,
        max_repeatability_range_deg=max_wireframe_repeatability_range_deg,
    )
    m2_cross_validation = paired_wireframe_cross_validation(
        usable,
        max_folds=wireframe_cross_validation_folds,
        max_rmse_deg=max_wireframe_turnover_rmse_deg,
        max_error_deg=max_wireframe_turnover_error_deg,
        max_repeatability_range_deg=max_wireframe_repeatability_range_deg,
        max_parameter_spread_mm=max_wireframe_cv_parameter_spread_mm,
        fit_offsets=fit_decomposed_camera_y_offsets_from_images,
        fit_uses_professional_truth=False,
    )
    m1_candidate = image_model_candidate_acceptance(
        "M1",
        m1_offsets,
        raw_holdout_validation,
        m1_holdout_validation,
        m1_cross_validation,
    )
    m2_candidate = image_model_candidate_acceptance(
        "M2",
        m2_offsets,
        raw_holdout_validation,
        m2_holdout_validation,
        m2_cross_validation,
    )
    m2_candidate["selectable_for_runtime"] = False
    m2_candidate["nonselection_reason"] = (
        "M2 contains two affine camera-Y modes confounded with real end-face tilt; it is diagnostic only."
    )
    self_consistency_selected_level = "M1" if m1_candidate["passed"] else "M0"
    runtime_selected_level = (
        "M1"
        if self_consistency_selected_level == "M1"
        and state_reference.get("all_captures_stable") is True
        else "M0"
    )
    offsets = m1_offsets if runtime_selected_level == "M1" else np.zeros(4, dtype=float)
    optimizer = m1_optimizer if runtime_selected_level == "M1" else {
        "method": "raw_zero_camera_y_offsets",
        "model_level": "M0",
        "evidence": "no_endface_correction_applied",
        "uses_professional_or_manual_endface_truth": False,
    }

    failures: list[str] = []
    if state_reference.get("all_captures_stable") is not True:
        unstable = [
            item.path.name
            for item in usable
            if item.relative_camera_diagnostic.get("status") != "relative_geometry_stable"
        ]
        failures.append(
            "calibration camera state is not image-geometrically stable in every usable capture; "
            "HOBJ alone cannot distinguish rig motion from true face non-planarity, so physical "
            "camera synchronization cannot be released. Affected captures: "
            + ", ".join(unstable)
        )
    if not m1_candidate["passed"]:
        failures.extend(
            "M1 selection gate " + failure for failure in m1_candidate["failures"]
        )

    diagnostics: list[Dict[str, object]] = []
    for item in measurements:
        try:
            raw_wireframe = wireframe_angles_for_capture(item, np.zeros(4, dtype=float))
            m1_wireframe = wireframe_angles_for_capture(item, m1_offsets)
            m2_wireframe = wireframe_angles_for_capture(item, m2_offsets)
            runtime_wireframe = wireframe_angles_for_capture(item, offsets)
        except ValueError as exc:
            diagnostics.append(
                {
                    "diagnostic_kind": "wireframe_capture_error",
                    "capture": item.path.name,
                    "input_path": str(item.path),
                    "orientation": item.orientation,
                    "split": item.split,
                    "error": str(exc),
                    "uses_professional_or_manual_endface_truth": False,
                    "final_angle_correction_applied": False,
                }
            )
            if item.split != "excluded_geometry":
                failures.append(
                    f"usable capture {item.path.name} wireframe reconstruction failed: {exc}"
                )
            continue
        for end, local_channel in WIREFRAME_CHANNELS:
            raw_summary = raw_wireframe["summaries"][end]
            m1_summary = m1_wireframe["summaries"][end]
            m2_summary = m2_wireframe["summaries"][end]
            runtime_summary = runtime_wireframe["summaries"][end]
            raw_diagnostics = raw_wireframe["diagnostics"][end]
            m1_diagnostics = m1_wireframe["diagnostics"][end]
            m2_diagnostics = m2_wireframe["diagnostics"][end]
            runtime_diagnostics = runtime_wireframe["diagnostics"][end]
            diagnostics.append(
                {
                    "diagnostic_kind": "wireframe_local_angle",
                    "capture": item.path.name,
                    "input_path": str(item.path),
                    "orientation": item.orientation,
                    "acquisition_group": (
                        "head_to_tail" if item.orientation == "normal" else "tail_to_head"
                    ),
                    "row_order_status": item.row_order_status,
                    "split": item.split,
                    "channel_end": end,
                    "local_channel": local_channel,
                    "raw_angle_deg": f"{raw_wireframe['angles'][end][local_channel]:.9f}",
                    "m1_candidate_angle_deg": f"{m1_wireframe['angles'][end][local_channel]:.9f}",
                    "m2_diagnostic_angle_deg": f"{m2_wireframe['angles'][end][local_channel]:.9f}",
                    "runtime_selected_angle_deg": f"{runtime_wireframe['angles'][end][local_channel]:.9f}",
                    "runtime_selected_model_level": runtime_selected_level,
                    "raw_representative_angle_deg": f"{raw_summary['worst_local_angle_deg']:.9f}",
                    "m1_candidate_representative_angle_deg": f"{m1_summary['worst_local_angle_deg']:.9f}",
                    "m2_diagnostic_representative_angle_deg": f"{m2_summary['worst_local_angle_deg']:.9f}",
                    "runtime_selected_representative_angle_deg": f"{runtime_summary['worst_local_angle_deg']:.9f}",
                    "raw_max_dual_camera_seam_mm": f"{raw_diagnostics['max_dual_camera_seam_mid_mm']:.9f}",
                    "m1_candidate_max_dual_camera_seam_mm": f"{m1_diagnostics['max_dual_camera_seam_mid_mm']:.9f}",
                    "m2_diagnostic_max_dual_camera_seam_mm": f"{m2_diagnostics['max_dual_camera_seam_mid_mm']:.9f}",
                    "runtime_selected_max_dual_camera_seam_mm": f"{runtime_diagnostics['max_dual_camera_seam_mid_mm']:.9f}",
                    "uses_professional_or_manual_endface_truth": False,
                    "final_angle_correction_applied": False,
                }
            )

    metadata = camera_calibration.get("single_bar_metadata", {})
    nominal_spec = (
        str(metadata.get("specification", "")).strip()
        if isinstance(metadata, dict)
        else ""
    )
    readiness = bool(
        not failures
        and runtime_selected_level == "M1"
        and m1_candidate["passed"]
    )
    model: Dict[str, object] = {
        "version": 15,
        "model": "endface_camera_geometry_calibration",
        "strategy": "physical_decomposed_camera_scan_row_synchronization",
        "valid": readiness,
        "created_at": datetime.now().isoformat(timespec="seconds"),
        "note": (
            "Nested image-only M0/M1/M2 audit. M0 is unadjusted Raw. M1 fits only the single "
            "non-planar camera-Y mode identifiable from HOBJ plane residuals and is the only "
            "selectable correction. M2 adds two turnover-fitted affine modes but is diagnostic "
            "only because those modes can absorb real end-face tilt. Institution/manual end-face "
            "values are never loaded or used."
        ),
        "source_camera_calibration": str(camera_path.resolve()),
        "nominal_spec": nominal_spec,
        "institution_or_manual_endface_truth_usage": {
            "loaded": False,
            "used_for_fit": False,
            "used_for_validation": False,
            "used_for_runtime": False,
            "reason": "End-face truth is considered too coarse for the local 16-angle geometry.",
        },
        "head_tail_definition": "canonical_hobj_device_row_min_and_row_max",
        "runtime_endpoint_label_contract": {
            "head": "hobj_device_row_min",
            "tail": "hobj_device_row_max",
            "physical_R_L_inferred": False,
            "reason": "HOBJ contains no physical endpoint identity metadata",
        },
        "runtime_orientation_detection": "none",
        "self_consistency_selected_model_level": self_consistency_selected_level,
        "runtime_selected_model_level": runtime_selected_level,
        "runtime_correction_applied": runtime_selected_level == "M1",
        "report_to_software_face_map": dict(INSTITUTION_TO_SOFTWARE_FACE_MAP),
        "institution_report_face_layout": {
            "top": "A",
            "bottom": "D",
            "left": "C",
            "right": "B",
        },
        "camera_y_offsets_mm": {
            str(camera): float(offsets[camera - 1]) for camera in (1, 2, 3, 4)
        },
        "camera_y_offsets_rows": {
            str(camera): float(offsets[camera - 1] / Y_SCALE_MM)
            for camera in (1, 2, 3, 4)
        },
        "y_coordinate_correction": {
            "method": "per_camera_scan_row_offset",
            "apply_to": "all_longitudinal_side_points_and_end_boundary_points",
        },
        "offset_gauge_constraint": "sum(camera_y_offsets_mm)=0",
        "physical_mode_basis": {
            key: (
                [float(value) for value in np.asarray(basis[key], dtype=float)]
                if key in {"affine_x_mode", "affine_z_mode", "nonplanar_mode"}
                else basis[key]
            )
            for key in basis
        },
        "prohibited_fields_absent": {
            "angle_offsets_deg": True,
            "orientation_models": True,
            "orientation_detector": True,
        },
        "capture_counts": {
            orientation: {
                split: sum(
                    item.orientation == orientation and item.split == split
                    for item in measurements
                )
                for split in ("train", "holdout", "excluded_geometry")
            }
            for orientation in ("normal", "turnover")
        },
        "capture_geometry_quality": {
            "method": "image_only_row_order_and_raw_endface_boundary_plane_rmse",
            "max_plane_rmse_mm": 0.50,
            "row_order_audit": row_order_audit,
            "per_capture": capture_quality,
            "excluded_captures": [item for item in capture_quality if item["excluded"]],
        },
        "calibration_state_reference": state_reference,
        "model_comparison": {
            "selection_policy": (
                "M0 Raw is the immutable baseline; M1 must improve Raw on fixed holdout and "
                "paired cross-validation without destabilizing repeatability; M2 is never "
                "runtime-selectable."
            ),
            "M0": {
                "model_level": "M0",
                "description": "Raw unadjusted 16-angle wireframe",
                "camera_y_offsets_mm": {str(camera): 0.0 for camera in (1, 2, 3, 4)},
                "fixed_blind_holdout": raw_holdout_validation,
                "runtime_fallback": True,
            },
            "M1": m1_candidate,
            "M2": m2_candidate,
        },
        "fit_details": {
            "selected": optimizer,
            "M1": m1_optimizer,
            "M2": m2_optimizer,
        },
        "wireframe_validation": {
            "method": "shared_production_16_angle_holdout_and_paired_cross_validation",
            "shared_geometry_module": "endface_wireframe_geometry.measure_wireframe_angles",
            "fit_evidence": "image_only_physical_turnover_no_endface_truth",
            "selected_model_level": "M1",
            "fixed_blind_holdout": m1_holdout_validation,
            "raw_fixed_blind_holdout": raw_holdout_validation,
            "paired_cross_validation": m1_cross_validation,
            "passed": bool(m1_candidate["passed"]),
        },
        "runtime_output_contract": {
            "local_angles_per_end": 8,
            "total_local_angles": 16,
            "representative_value_per_end": "actual_local_angle_farthest_from_90_not_average",
            "raw_and_physical_corrected_retained": True,
        },
        "validation": {
            "method": "image_only_self_consistency_no_absolute_accuracy_claim",
            "wireframe_cross_validation_uncertainty_rmse_deg": m1_cross_validation[
                "statistics"
            ]["physical_corrected_ordinal_pair_rmse_deg"],
            "absolute_accuracy_claimed": False,
        },
        "release_readiness": {
            "ready": readiness,
            "reason": (
                "All image-only state, identifiability, fixed holdout, and paired cross-validation gates pass."
                if readiness
                else "; ".join(failures)
            ),
            "selected_model_level": runtime_selected_level,
            "calibration_holdout_passed": bool(m1_holdout_validation["passed"]),
            "image_row_order_audited": True,
            "verified_physical_turnover_hobj_passed": bool(
                m1_holdout_validation["passed"]
            ),
            "wireframe_16_angle_holdout_passed": bool(
                m1_holdout_validation["passed"]
            ),
            "wireframe_16_angle_cross_validation_passed": bool(
                m1_cross_validation["passed"]
            ),
            "professional_or_manual_endface_truth_used": False,
            "physical_turnover_evidence": (
                "On-site video verified that tail_to_head is the same standard physically turned "
                "end-for-end; the last two captures in each pose are blind holdout data."
            ),
        },
        "failures": failures,
    }
    return model, diagnostics


def write_diagnostics(path: Path, rows: list[Dict[str, object]]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    if not rows:
        return
    fieldnames: list[str] = []
    for row in rows:
        for field_name in row:
            if field_name not in fieldnames:
                fieldnames.append(field_name)
    with path.open("w", encoding="utf-8-sig", newline="") as handle:
        writer = csv.DictWriter(handle, fieldnames=fieldnames)
        writer.writeheader()
        writer.writerows(rows)


def main() -> int:
    parser = argparse.ArgumentParser(description="Offline physical end-face camera model generator")
    parser.add_argument("--input", required=True, help="Root folder containing normal and turnover HOBJ subfolders")
    parser.add_argument(
        "--calibration-mode",
        choices=("image_self_consistency", "legacy_truth_audit"),
        default="image_self_consistency",
        help="Default is pure HOBJ physical-turnover self-consistency; legacy truth mode is audit-only",
    )
    parser.add_argument(
        "--truth-csv",
        help="Institution report CSV; ignored by image_self_consistency and accepted only by legacy_truth_audit",
    )
    parser.add_argument("--camera-calibration", required=True, help="Existing camera coordinate calibration JSON")
    parser.add_argument("--output-model", help="Output end-face model JSON")
    parser.add_argument(
        "--camera-state-only",
        action="store_true",
        help="Only write a truth-free image-geometry stability report; never fit a model",
    )
    parser.add_argument(
        "--camera-state-report-csv",
        help="Output CSV for --camera-state-only",
    )
    parser.add_argument("--sample-id", default="", help="Institution sample ID; optional for a one-sample CSV")
    parser.add_argument("--head-label", default="R", help="Report end label treated as physical head (fixed default: R)")
    parser.add_argument("--holdout-fraction", type=float, default=0.20)
    parser.add_argument("--min-captures-per-direction", type=int, default=5)
    parser.add_argument("--expected-captures-per-direction", type=int, default=0)
    parser.add_argument("--endface-window-mm", type=float, default=15.0)
    parser.add_argument("--x-scale", type=float, default=X_SCALE_MM)
    parser.add_argument("--y-scale", type=float, default=Y_SCALE_MM)
    parser.add_argument("--max-direction-spread-deg", type=float, default=0.10)
    parser.add_argument("--max-validation-rmse-deg", type=float, default=0.20)
    parser.add_argument("--max-validation-error-deg", type=float, default=0.35)
    parser.add_argument("--max-direction-parameter-spread-mm", type=float, default=0.25)
    parser.add_argument("--max-loo-parameter-range-mm", type=float, default=0.10)
    parser.add_argument(
        "--physical-turnover-before",
        help="Independent HOBJ folder: standard bar before a verified physical end-for-end turnover",
    )
    parser.add_argument(
        "--physical-turnover-after",
        help="Independent HOBJ folder: same bar after verified 180-degree turnover about A/D",
    )
    parser.add_argument("--max-turnover-rmse-deg", type=float, default=0.10)
    parser.add_argument("--max-turnover-error-deg", type=float, default=0.20)
    parser.add_argument("--max-turnover-repeatability-range-deg", type=float, default=0.10)
    parser.add_argument("--max-wireframe-turnover-rmse-deg", type=float, default=0.30)
    parser.add_argument("--max-wireframe-turnover-error-deg", type=float, default=0.60)
    parser.add_argument("--max-wireframe-repeatability-range-deg", type=float, default=0.20)
    parser.add_argument("--max-wireframe-cv-parameter-spread-mm", type=float, default=0.15)
    parser.add_argument("--wireframe-cross-validation-folds", type=int, default=5)
    parser.add_argument("--diagnostics-csv", help="Optional per-capture diagnostic CSV")
    args = parser.parse_args()

    input_root = Path(args.input).expanduser().resolve()
    camera_path = Path(args.camera_calibration).expanduser().resolve()
    for path, description in ((input_root, "HOBJ root"), (camera_path, "camera calibration")):
        if not path.exists():
            raise ValueError(f"{description} not found: {path}")
    calibration = json.loads(camera_path.read_text(encoding="utf-8"))
    if args.camera_state_only:
        report_path = Path(
            args.camera_state_report_csv
            or (input_root / "camera_state_preflight.csv")
        ).expanduser().resolve()
        return run_camera_state_preflight(
            input_root,
            calibration,
            report_path,
            args.x_scale,
        )
    if not args.output_model:
        raise ValueError("--output-model is required unless --camera-state-only is used")
    output_path = Path(args.output_model).expanduser().resolve()
    # A failed or interrupted recalibration must never leave a previous model
    # looking current and usable.
    output_path.unlink(missing_ok=True)
    if not 0.0 < args.holdout_fraction < 0.5:
        raise ValueError("--holdout-fraction must be between 0 and 0.5")
    discovered = discover_captures(input_root, args.holdout_fraction)
    direction_counts = {name: sum(orientation == name for _, orientation, _ in discovered) for name in ("normal", "turnover")}
    for orientation, count in direction_counts.items():
        if args.expected_captures_per_direction and count != args.expected_captures_per_direction:
            raise ValueError(
                f"{orientation} has {count} captures; exactly {args.expected_captures_per_direction} are required"
            )
        if count < args.min_captures_per_direction:
            raise ValueError(
                f"{orientation} has {count} captures; at least {args.min_captures_per_direction} are required"
            )
    if args.calibration_mode == "image_self_consistency":
        if args.truth_csv:
            print(
                "Ignoring --truth-csv: image_self_consistency never loads institution/manual end-face truth.",
                flush=True,
            )
        measurements = extract_physical_captures(
            discovered,
            calibration,
            None,
            args.x_scale,
            args.y_scale,
            args.endface_window_mm,
        )
        model, diagnostics = build_image_self_consistency_model(
            measurements,
            camera_path,
            calibration,
            max_wireframe_turnover_rmse_deg=args.max_wireframe_turnover_rmse_deg,
            max_wireframe_turnover_error_deg=args.max_wireframe_turnover_error_deg,
            max_wireframe_repeatability_range_deg=args.max_wireframe_repeatability_range_deg,
            max_wireframe_cv_parameter_spread_mm=args.max_wireframe_cv_parameter_spread_mm,
            wireframe_cross_validation_folds=args.wireframe_cross_validation_folds,
        )
        diagnostics_path = (
            Path(args.diagnostics_csv).expanduser().resolve()
            if args.diagnostics_csv
            else output_path.with_name(output_path.stem + "_diagnostics.csv")
        )
        write_diagnostics(diagnostics_path, diagnostics)
        if not model["valid"]:
            rejected_path = output_path.with_name(output_path.stem + "_rejected.json")
            rejected_path.parent.mkdir(parents=True, exist_ok=True)
            rejected_path.write_text(
                json.dumps(model, ensure_ascii=False, indent=2),
                encoding="utf-8",
            )
            print("Image-only self-consistency calibration rejected:", file=sys.stderr)
            for failure in model["failures"]:
                print(f"  - {failure}", file=sys.stderr)
            print(f"Rejected-model audit: {rejected_path}", file=sys.stderr)
            print(f"Diagnostics: {diagnostics_path}", file=sys.stderr)
            return 3
        output_path.parent.mkdir(parents=True, exist_ok=True)
        output_path.write_text(
            json.dumps(model, ensure_ascii=False, indent=2),
            encoding="utf-8",
        )
        print(f"Image-only model written: {output_path}")
        print(f"Diagnostics: {diagnostics_path}")
        return 0

    if not args.truth_csv:
        raise ValueError("--truth-csv is required for --calibration-mode legacy_truth_audit")
    truth_path = Path(args.truth_csv).expanduser().resolve()
    if not truth_path.exists():
        raise ValueError(f"truth CSV not found: {truth_path}")
    if args.head_label.strip().upper() != "R":
        raise ValueError("Professional end mapping is fixed: R=head and L=tail")
    truth = read_professional_truth(truth_path, args.sample_id, args.head_label)
    measurements = extract_physical_captures(
        discovered,
        calibration,
        truth,
        args.x_scale,
        args.y_scale,
        args.endface_window_mm,
    )
    model, diagnostics = build_physical_model(
        measurements,
        truth,
        camera_path,
        truth_path,
        args.y_scale,
        args.max_direction_spread_deg,
        args.max_validation_rmse_deg,
        args.max_validation_error_deg,
        args.max_direction_parameter_spread_mm,
        args.max_loo_parameter_range_mm,
        args.max_turnover_rmse_deg,
        args.max_turnover_error_deg,
        args.max_turnover_repeatability_range_deg,
        args.max_wireframe_turnover_rmse_deg,
        args.max_wireframe_turnover_error_deg,
        args.max_wireframe_repeatability_range_deg,
        args.max_wireframe_cv_parameter_spread_mm,
        args.wireframe_cross_validation_folds,
    )
    if bool(args.physical_turnover_before) != bool(args.physical_turnover_after):
        raise ValueError(
            "Provide both --physical-turnover-before and --physical-turnover-after, or neither"
        )
    if args.physical_turnover_before and args.physical_turnover_after:
        validation_groups: Dict[str, list[PhysicalCaptureMeasurement]] = {}
        for group_name, raw_path in (
            ("before", args.physical_turnover_before),
            ("after", args.physical_turnover_after),
        ):
            group_path = Path(raw_path).expanduser().resolve()
            if not group_path.is_dir():
                raise ValueError(f"Physical-turnover {group_name} folder not found: {group_path}")
            paths = sorted(group_path.rglob("*.hobj"), key=lambda item: str(item).casefold())
            if len(paths) < 3:
                raise ValueError(
                    f"Physical-turnover {group_name} needs at least three HOBJ files; got {len(paths)}"
                )
            validation_groups[group_name] = extract_physical_captures(
                [(path, "normal", "turnover_validation") for path in paths],
                calibration,
                truth,
                args.x_scale,
                args.y_scale,
                args.endface_window_mm,
            )
        offsets = np.asarray(
            [float(model["camera_y_offsets_mm"][str(obj)]) for obj in (1, 2, 3, 4)],
            dtype=float,
        )
        turnover_validation = validate_rigid_physical_turnover_measurements(
            validation_groups["before"],
            validation_groups["after"],
            offsets,
            max_rmse_deg=args.max_turnover_rmse_deg,
            max_error_deg=args.max_turnover_error_deg,
            max_repeatability_range_deg=args.max_turnover_repeatability_range_deg,
        )
        model["physical_turnover_hobj_validation"] = turnover_validation
        base_readiness = dict(model["release_readiness"])
        independent_ready = bool(model["valid"] and turnover_validation["passed"])
        combined_failures = list(model.get("failures", [])) + list(
            turnover_validation["failures"]
        )
        model["release_readiness"] = {
            **base_readiness,
            "ready": independent_ready,
            "reason": (
                "All calibration, row-order, holdout and independent physical-turnover HOBJ gates pass."
                if independent_ready
                else "; ".join(combined_failures)
            ),
            "calibration_holdout_passed": bool(model["valid"]),
            "image_row_order_audited": True,
            "synthetic_rigid_turnover_unit_test_required": True,
            "verified_physical_turnover_hobj_passed": bool(turnover_validation["passed"]),
            "before_path": str(Path(args.physical_turnover_before).expanduser().resolve()),
            "after_path": str(Path(args.physical_turnover_after).expanduser().resolve()),
        }
        if not turnover_validation["passed"]:
            model["failures"].extend(turnover_validation["failures"])
            model["valid"] = False
    diagnostics_path = Path(args.diagnostics_csv).expanduser().resolve() if args.diagnostics_csv else output_path.with_name(output_path.stem + "_diagnostics.csv")
    write_diagnostics(diagnostics_path, diagnostics)
    if not model["valid"]:
        rejected_path = output_path.with_name(output_path.stem + "_rejected.json")
        rejected_path.parent.mkdir(parents=True, exist_ok=True)
        rejected_path.write_text(json.dumps(model, ensure_ascii=False, indent=2), encoding="utf-8")
        print("Calibration rejected:", file=sys.stderr)
        for failure in model["failures"]:
            print(f"  - {failure}", file=sys.stderr)
        print(f"Rejected-model audit: {rejected_path}", file=sys.stderr)
        print(f"Diagnostics: {diagnostics_path}", file=sys.stderr)
        return 3
    output_path.parent.mkdir(parents=True, exist_ok=True)
    output_path.write_text(json.dumps(model, ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"Model written: {output_path}")
    print(f"Diagnostics: {diagnostics_path}")
    excluded = model["capture_geometry_quality"]["excluded_captures"]
    if excluded:
        print("Geometry-QC exclusions (independent of professional truth):")
        for details in excluded:
            print(f"  - {details['capture']}: {details['reason']}")
    validation = model["validation"]
    print(
        "Physical holdout RMSE: "
        f"{validation['physical_corrected_holdout']['rmse_deg']:.6f} deg"
    )
    print(
        "Physical holdout max error: "
        f"{validation['physical_corrected_holdout']['max_abs_error_deg']:.6f} deg"
    )
    print(
        "Shared camera Y offsets (mm): "
        + ", ".join(
            f"C{obj}={model['camera_y_offsets_mm'][str(obj)]:.6f}"
            for obj in (1, 2, 3, 4)
        )
    )
    if not model["release_readiness"]["ready"]:
        print("Engineering model only; production/Web/package release remains blocked:")
        print(f"  {model['release_readiness']['reason']}")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as exc:
        print(f"ERROR: {exc}", file=sys.stderr)
        raise SystemExit(2)
