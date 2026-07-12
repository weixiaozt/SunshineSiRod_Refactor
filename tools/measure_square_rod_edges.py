#!/usr/bin/env python3
"""
Measure square-rod side lengths from four 3D line-scan height images.

Inputs:
  1) A HALCON .hobj containing four 3200x20000 real height images, or
  2) Four tif/tiff files in object order 1,2,3,4.

Output:
  A CSV containing per-slice lengths, diagonals, chamfer arc lengths,
  projections, perpendicularity errors, stick length, plus summary rows.

Physical mapping used here:
  Obj1 = top   -> P1
  Obj3 = left  -> P3
  Obj2 = down  -> P2
  Obj4 = right -> P4

Edges:
  A = |P3-P1|, B = |P1-P4|, C = |P2-P4|, D = |P3-P2|

Diagonals:
  diag1 = |M1-M2|, diag2 = |M3-M4|
  M1/M2/M3/M4 are chamfer midpoints, not theoretical sharp corners.
"""

from __future__ import annotations

import argparse
import csv
import json
import math
import os
import re
from dataclasses import dataclass, replace
from typing import Dict, Iterable, List, Optional, Tuple

import numpy as np
from PIL import Image

try:
    from .mechanical_drift import classify as classify_mechanical_drift
    from .mechanical_drift import local_shift as mechanical_drift_local_shift
except ImportError:  # Direct script / PyInstaller entry point.
    from mechanical_drift import classify as classify_mechanical_drift
    from mechanical_drift import local_shift as mechanical_drift_local_shift


WIDTH = 3200
HEIGHT = 20000
X_SCALE_MM = 0.015
Y_SCALE_MM = 0.04891993841
INVALID_Z = -9999.0

# HALCON object file layout seen in SunshineSiRod samples.
HOBJ_FIRST_OFFSET = 120100
HOBJ_BETWEEN_OBJECT_HEADER = 120085

# User-provided standard measurements at 20%, 50%, 80%.
DEFAULT_STANDARD = {
    "head20": {"percent": 0.20, "A": 210.06, "B": 104.00, "C": 210.03, "D": 103.98},
    "mid50": {"percent": 0.50, "A": 210.07, "B": 103.99, "C": 210.03, "D": 103.98},
    "tail80": {"percent": 0.80, "A": 210.06, "B": 104.01, "C": 210.03, "D": 103.99},
}

OBJECT_TO_POINT = {1: "P1", 3: "P3", 2: "P2", 4: "P4"}


@dataclass
class CornerResult:
    valid: bool
    vx: float = math.nan
    vz: float = math.nan
    angle_deg: float = math.nan
    chamfer_mm: float = math.nan
    projection_x_mm: float = math.nan
    projection_y_mm: float = math.nan
    chamfer_face1_setback_mm: float = math.nan
    chamfer_face2_setback_mm: float = math.nan
    chamfer_mid_x: float = math.nan
    chamfer_mid_z: float = math.nan
    line1_m: float = math.nan
    line1_b: float = math.nan
    line2_m: float = math.nan
    line2_b: float = math.nan
    face1_mid_x: float = math.nan
    face1_mid_z: float = math.nan
    face2_mid_x: float = math.nan
    face2_mid_z: float = math.nan
    seg_start_col: int = -1
    seg_end_col: int = -1
    reason: str = ""


@dataclass
class EndFaceFit:
    """Plane fit for one physical rod end in global X/Y/Z coordinates."""

    end: str
    valid: bool
    slope_x: float = math.nan
    slope_z: float = math.nan
    intercept_y: float = math.nan
    normal_x: float = math.nan
    normal_y: float = math.nan
    normal_z: float = math.nan
    verticality_deg: float = math.nan
    point_count: int = 0
    inlier_count: int = 0
    rmse_mm: float = math.nan
    reason: str = ""


class HeightSource:
    width: int
    height: int

    def row(self, obj: int, row_index: int) -> np.ndarray:
        raise NotImplementedError


@dataclass
class CalibrationCapture:
    source: HeightSource
    capture_id: str
    bar_id: str
    orientation: str = "normal"


class HobjSource(HeightSource):
    def __init__(
        self,
        path: str,
        width: int = WIDTH,
        height: int = HEIGHT,
        offsets: Optional[Dict[int, int]] = None,
    ) -> None:
        self.path = path
        self.width = width
        self.height = height
        block = width * height * 4
        if offsets is None:
            offsets = {
                obj: HOBJ_FIRST_OFFSET + (obj - 1) * (block + HOBJ_BETWEEN_OBJECT_HEADER)
                for obj in [1, 2, 3, 4]
            }
        self.offsets = offsets
        self._maps = {
            obj: np.memmap(path, dtype="<f4", mode="r", offset=offsets[obj], shape=(height, width))
            for obj in [1, 2, 3, 4]
        }

    def row(self, obj: int, row_index: int) -> np.ndarray:
        return np.asarray(self._maps[obj][row_index])


class TiffSource(HeightSource):
    def __init__(self, files: Dict[int, str]) -> None:
        self.files = files
        self._images: Dict[int, np.ndarray] = {}
        for obj, path in files.items():
            arr = np.asarray(Image.open(path), dtype=np.float32)
            if arr.ndim != 2:
                raise ValueError(f"TIFF for object {obj} is not a single-channel image: {path}")
            self._images[obj] = arr
        shapes = {arr.shape for arr in self._images.values()}
        if len(shapes) != 1:
            raise ValueError(f"TIFF image shapes are different: {shapes}")
        self.height, self.width = next(iter(shapes))

    def row(self, obj: int, row_index: int) -> np.ndarray:
        return self._images[obj][row_index]


def moving_average(values: np.ndarray, window: int = 9) -> np.ndarray:
    if len(values) < window:
        return values.copy()
    pad = window // 2
    padded = np.pad(values, (pad, pad), mode="edge")
    return np.convolve(padded, np.ones(window) / window, mode="valid")


def robust_line_fit(x: np.ndarray, z: np.ndarray) -> Tuple[float, float, float]:
    m, b = np.polyfit(x, z, 1)
    for _ in range(2):
        residual = z - (m * x + b)
        sigma = np.median(np.abs(residual - np.median(residual))) * 1.4826 + 1e-9
        keep = np.abs(residual) < 3.0 * sigma
        if keep.sum() > max(20, len(x) // 3):
            m, b = np.polyfit(x[keep], z[keep], 1)
    rmse = float(np.sqrt(np.mean((z - (m * x + b)) ** 2)))
    return float(m), float(b), rmse


def extract_corner_from_row(row_values: np.ndarray, x_scale: float = X_SCALE_MM) -> CornerResult:
    valid_cols = np.where(
        (row_values > INVALID_Z)
        & np.isfinite(row_values)
        & (np.abs(row_values) > 1e-6)
    )[0]
    if valid_cols.size < 100:
        return CornerResult(False, reason="too few valid points")

    c0, c1 = int(valid_cols[0]), int(valid_cols[-1])
    cols = np.arange(c0, c1 + 1)
    z = row_values[c0 : c1 + 1].astype(float)
    keep = (z > INVALID_Z) & np.isfinite(z) & (np.abs(z) > 1e-6)
    cols = cols[keep]
    z = z[keep]
    if len(z) < 100:
        return CornerResult(False, reason="too few valid points after cleanup")

    # Use absolute sensor-column coordinates. A relative origin tied to the
    # visible segment would hide real part-size shifts and make the calibration
    # behave like a single-spec correction.
    x = cols * x_scale
    zs = moving_average(z, 9)
    peak = int(np.argmax(zs))

    # Fit the two main faces from the outer parts of the profile. This keeps
    # both small and large chamfers out of the face fit; P is then the
    # intersection of the two main-face extensions.
    left0 = max(0, int(peak * 0.05))
    left1 = max(left0 + 1, int(peak * 0.62))
    right0 = min(len(x) - 1, peak + int((len(x) - peak) * 0.38))
    right1 = min(len(x), peak + int((len(x) - peak) * 0.95))
    if left1 - left0 < 50 or right1 - right0 < 50:
        margin = 180
        left0 = max(0, int(peak * 0.05))
        left1 = max(40, peak - margin)
        right0 = min(len(x) - 40, peak + margin)
        right1 = min(len(x), max(right0 + 1, peak + int((len(x) - peak) * 0.95)))
    if left1 - left0 < 50 or right1 - right0 < 50:
        return CornerResult(False, reason="not enough face-fit points", seg_start_col=c0, seg_end_col=c1)

    m1, b1, rmse1 = robust_line_fit(x[left0:left1], zs[left0:left1])
    m2, b2, rmse2 = robust_line_fit(x[right0:right1], zs[right0:right1])
    if abs(m1 - m2) < 1e-9:
        return CornerResult(False, reason="two fitted faces are nearly parallel", seg_start_col=c0, seg_end_col=c1)

    vx = (b2 - b1) / (m1 - m2)
    vz = m1 * vx + b1
    face1_mid_x = float(np.mean(x[left0:left1]))
    face1_mid_z = float(m1 * face1_mid_x + b1)
    face2_mid_x = float(np.mean(x[right0:right1]))
    face2_mid_z = float(m2 * face2_mid_x + b2)

    denom = 1.0 + m1 * m2
    if abs(denom) < 1e-9:
        angle = 90.0
    else:
        angle = abs(math.degrees(math.atan((m2 - m1) / denom)))
        if angle > 90.0:
            angle = 180.0 - angle

    # Chamfer estimate: find the transition near V, then fit/use its endpoints.
    d1 = np.abs(m1 * x - zs + b1) / math.sqrt(m1 * m1 + 1.0)
    d2 = np.abs(m2 * x - zs + b2) / math.sqrt(m2 * m2 + 1.0)
    noise = max(rmse1, rmse2)
    threshold = max(0.03, noise * 6.0, 0.06)
    near = np.abs(x - vx) <= 15.0
    transition = near & (np.minimum(d1, d2) > threshold)
    idx = np.where(transition)[0]
    chamfer = math.nan
    projection_x = math.nan
    projection_y = math.nan
    chamfer_face1_setback = math.nan
    chamfer_face2_setback = math.nan
    chamfer_mid_x = math.nan
    chamfer_mid_z = math.nan
    if idx.size > 1:
        groups: List[np.ndarray] = np.split(idx, np.where(np.diff(idx) > 1)[0] + 1)
        groups.sort(key=lambda g: abs(float(np.mean(x[g])) - vx))
        g = groups[0]
        t1, t2 = int(g[0]), int(g[-1])
        t1x, t1z = float(x[t1]), float(zs[t1])
        t2x, t2z = float(x[t2]), float(zs[t2])

        # The transition mask deliberately excludes samples close to either
        # main face, so its first/last raw samples are not the physical
        # chamfer endpoints.  Fit the chamfer line and intersect it with both
        # main-face fits instead.  This removes threshold-dependent length
        # bias while retaining the raw endpoints as a safe fallback.
        if g.size >= 6:
            mc, bc, _ = robust_line_fit(x[g], zs[g])
            denom1 = m1 - mc
            denom2 = m2 - mc
            if abs(denom1) > 1e-9 and abs(denom2) > 1e-9:
                candidate1_x = (bc - b1) / denom1
                candidate2_x = (bc - b2) / denom2
                candidate1_z = m1 * candidate1_x + b1
                candidate2_z = m2 * candidate2_x + b2
                values = (candidate1_x, candidate1_z, candidate2_x, candidate2_z)
                if all(math.isfinite(value) for value in values):
                    t1x, t1z = float(candidate1_x), float(candidate1_z)
                    t2x, t2z = float(candidate2_x), float(candidate2_z)

        chamfer_face1_setback = float(math.hypot(t1x - vx, t1z - vz))
        chamfer_face2_setback = float(math.hypot(t2x - vx, t2z - vz))
        chamfer = float(math.hypot(t2x - t1x, t2z - t1z))
        # Physical chamfer projections use the two main faces as the local
        # triangle axes: X runs from P to T2 along L2, Y from P to T1 along L1.
        projection_x = chamfer_face2_setback
        projection_y = chamfer_face1_setback
        chamfer_mid_x = float((t1x + t2x) * 0.5)
        chamfer_mid_z = float((t1z + t2z) * 0.5)

    return CornerResult(
        True,
        vx=float(vx),
        vz=float(vz),
        angle_deg=float(angle),
        chamfer_mm=chamfer,
        projection_x_mm=projection_x,
        projection_y_mm=projection_y,
        chamfer_face1_setback_mm=chamfer_face1_setback,
        chamfer_face2_setback_mm=chamfer_face2_setback,
        chamfer_mid_x=chamfer_mid_x,
        chamfer_mid_z=chamfer_mid_z,
        line1_m=m1,
        line1_b=b1,
        line2_m=m2,
        line2_b=b2,
        face1_mid_x=face1_mid_x,
        face1_mid_z=face1_mid_z,
        face2_mid_x=face2_mid_x,
        face2_mid_z=face2_mid_z,
        seg_start_col=c0,
        seg_end_col=c1,
    )


def sample_local_corner_profile(
    source: HeightSource,
    common_start: int,
    common_end: int,
    fractions: Iterable[float],
    x_scale: float = X_SCALE_MM,
    reference_start_row: Optional[int] = None,
    reference_end_row: Optional[int] = None,
) -> np.ndarray:
    """Sample the four raw local corner vertices used for drift classification."""
    stations = [float(value) for value in fractions]
    profile = np.empty((4, len(stations), 2), dtype=float)
    for object_index, obj in enumerate((1, 2, 3, 4)):
        for station_index, fraction in enumerate(stations):
            if reference_start_row is not None and reference_end_row is not None:
                row = int(round(reference_start_row + fraction * (reference_end_row - reference_start_row)))
            else:
                row = int(round(common_start + fraction * (common_end - common_start)))
            corner = extract_corner_from_row(source.row(obj, row), x_scale)
            if not corner.valid:
                raise RuntimeError(
                    f"Drift detection failed at obj{obj}, {fraction:.2%}: {corner.reason}"
                )
            profile[object_index, station_index] = [corner.vx, corner.vz]
    return profile


def drift_fraction_for_row(
    model: Dict[str, object], row: int, common_start: int, common_end: int
) -> float:
    """Map a measurement row into the drift model's longitudinal coordinate."""
    reference_start = model.get("reference_common_start_row")
    reference_end = model.get("reference_common_end_row")
    if reference_start is not None and reference_end is not None and int(reference_end) > int(reference_start):
        return (float(row) - float(reference_start)) / (float(reference_end) - float(reference_start))
    return (float(row) - float(common_start)) / (float(common_end) - float(common_start))


def subtract_local_drift(corner: CornerResult, shift_x: float, shift_z: float) -> CornerResult:
    """Translate one complete local corner observation before camera calibration."""
    if not corner.valid:
        return corner
    values = {
        "vx": corner.vx - shift_x,
        "vz": corner.vz - shift_z,
        "face1_mid_x": corner.face1_mid_x - shift_x,
        "face1_mid_z": corner.face1_mid_z - shift_z,
        "face2_mid_x": corner.face2_mid_x - shift_x,
        "face2_mid_z": corner.face2_mid_z - shift_z,
        "line1_b": corner.line1_b + corner.line1_m * shift_x - shift_z,
        "line2_b": corner.line2_b + corner.line2_m * shift_x - shift_z,
    }
    if math.isfinite(corner.chamfer_mid_x):
        values["chamfer_mid_x"] = corner.chamfer_mid_x - shift_x
    if math.isfinite(corner.chamfer_mid_z):
        values["chamfer_mid_z"] = corner.chamfer_mid_z - shift_z
    return replace(corner, **values)


def find_valid_row_range(source: HeightSource, obj: int, min_points: int = 100, step: int = 1) -> Tuple[int, int]:
    good: List[int] = []
    for r in range(0, source.height, step):
        row = source.row(obj, r)
        count = int(np.count_nonzero((row > INVALID_Z) & np.isfinite(row) & (np.abs(row) > 1e-6)))
        if count >= min_points:
            good.append(r)
    if not good:
        raise RuntimeError(f"Object {obj} has no valid rows")
    return good[0], good[-1]


def representative_corner_from_row_range(
    source: HeightSource,
    obj: int,
    first_row: int,
    last_row: int,
    sample_count: int = 9,
) -> Tuple[CornerResult, int]:
    """Return the valid corner closest to the range's robust median corner."""

    if last_row < first_row:
        raise ValueError(f"Invalid calibration row range: {first_row}..{last_row}")
    rows = sorted(set(int(round(v)) for v in np.linspace(first_row, last_row, num=max(1, sample_count))))
    candidates = [(row, extract_corner_from_row(source.row(obj, row))) for row in rows]
    candidates = [(row, corner) for row, corner in candidates if corner.valid]
    if not candidates:
        raise RuntimeError(f"Calibration failed in rows {first_row}..{last_row}, object {obj}: no valid corner")
    if len(candidates) == 1:
        return candidates[0][1], candidates[0][0]
    median_vx = float(np.median([corner.vx for _, corner in candidates]))
    median_vz = float(np.median([corner.vz for _, corner in candidates]))
    row, corner = min(candidates, key=lambda item: math.hypot(item[1].vx - median_vx, item[1].vz - median_vz))
    return corner, row


def circle_intersections(
    c0: Tuple[float, float],
    r0: float,
    c1: Tuple[float, float],
    r1: float,
) -> List[Tuple[float, float]]:
    x0, y0 = c0
    x1, y1 = c1
    dx = x1 - x0
    dy = y1 - y0
    d = math.hypot(dx, dy)
    if d <= 1e-9:
        raise ValueError("circle centers overlap")
    a = (r0 * r0 - r1 * r1 + d * d) / (2.0 * d)
    h2 = r0 * r0 - a * a
    h = math.sqrt(max(0.0, h2))
    xm = x0 + a * dx / d
    ym = y0 + a * dy / d
    rx = -dy * h / d
    ry = dx * h / d
    return [(xm + rx, ym + ry), (xm - rx, ym - ry)]


def target_points_from_edges(edges: Dict[str, float]) -> Dict[str, Tuple[float, float]]:
    # Anchor P2 at origin, P4 on +X, P3 on +Z. P1 is solved from A and B.
    a = edges["A"]
    b = edges["B"]
    c = edges["C"]
    d = edges["D"]
    p2 = (0.0, 0.0)
    p4 = (c, 0.0)
    p3 = (0.0, d)
    candidates = circle_intersections(p3, a, p4, b)
    p1 = max(candidates, key=lambda p: p[1])
    return {"P1": p1, "P2": p2, "P3": p3, "P4": p4}


def orientation_edges(edges: Dict[str, float], orientation: str) -> Dict[str, float]:
    out = dict(edges)
    if orientation in {"swap_bd", "turnover"}:
        out["B"], out["D"] = out["D"], out["B"]
    return out


def oriented_standard_pairs(
    standard: Dict[str, Dict[str, float]],
    orientation: str,
) -> List[Tuple[str, Dict[str, float], str, Dict[str, float]]]:
    """Pair current capture windows with physical truth windows.

    A turnover reverses the rod's longitudinal direction.  The first current
    window therefore represents the last physical truth window, while the
    middle window remains the middle one.  The returned tuple is
    ``(source_name, source_window, truth_name, truth_values)``.
    """

    ordered = sorted(standard.items(), key=lambda item: float(item[1]["percent"]))
    truth_ordered = list(reversed(ordered)) if orientation in {"swap_bd", "turnover"} else ordered
    return [
        (source_name, source_item, truth_name, truth_item)
        for (source_name, source_item), (truth_name, truth_item) in zip(ordered, truth_ordered)
    ]


def source_valid_ranges(source: HeightSource) -> Dict[int, Tuple[int, int]]:
    return {obj: find_valid_row_range(source, obj) for obj in [1, 2, 3, 4]}


def source_common_range(
    source: HeightSource,
    ranges: Optional[Dict[int, Tuple[int, int]]] = None,
) -> Tuple[int, int]:
    if ranges is None:
        ranges = source_valid_ranges(source)
    common_start = max(r[0] for r in ranges.values())
    common_end = min(r[1] for r in ranges.values())
    if common_end <= common_start:
        raise RuntimeError(f"No common valid row range across four objects: {ranges}")
    return common_start, common_end


def normalize_bar_id(value: str) -> str:
    """Normalize operator-entered bar IDs without changing their core text."""

    text = os.path.splitext(os.path.basename(str(value).strip()))[0]
    text = re.sub(r"\s*_\s*", "_", text)
    text = re.sub(r"\s+", " ", text)
    return text


def strip_turnover_marker(value: str) -> str:
    """Return the physical bar ID from a normal or turnover folder name."""

    text = str(value).strip()
    # Operators label reversal folders with either the Chinese term or its
    # pinyin. Keep the remaining suffix (for example ``_3``) intact.
    text = re.sub(r"(?i)(调头|diaotou)", "", text)
    text = re.sub(r"\s+", " ", text).strip()
    return normalize_bar_id(text)


def bar_id_key(value: str) -> str:
    return normalize_bar_id(value).casefold()


def calibration_hobjs_from_folder(folder: str) -> List[Tuple[str, str, str]]:
    """Discover calibration captures as ``(path, bar_id, capture_id)``.

    Preferred layout::

        root/<bar_id>/<capture>.hobj

    Multiple HOBJ files in one child folder are repeat captures of the same
    physical bar.  Flat ``root/<bar_id>.hobj`` input remains supported for
    older calibration archives.
    """

    root = os.path.abspath(folder)
    found: List[Tuple[str, str, str]] = []
    for current, _, names in os.walk(root):
        for name in names:
            if not name.lower().endswith(".hobj"):
                continue
            path = os.path.join(current, name)
            relative_parent = os.path.relpath(current, root)
            raw_bar_id = os.path.splitext(name)[0] if relative_parent == "." else os.path.basename(current)
            turnover = is_turnover_capture(path)
            bar_id = strip_turnover_marker(raw_bar_id) if turnover else normalize_bar_id(raw_bar_id)
            capture_stem = os.path.splitext(name)[0]
            capture_id = capture_stem if relative_parent == "." else f"{bar_id}/{'turnover/' if turnover else ''}{capture_stem}"
            found.append((path, bar_id, capture_id))
    if not found:
        raise ValueError(f"No .hobj files found in calibration folder: {folder}")
    return sorted(found, key=lambda item: (bar_id_key(item[1]), is_turnover_capture(item[0]), item[2].casefold()))


def is_turnover_capture(path: str) -> bool:
    # The marker is normally on the parent folder, not the HOBJ file name.
    # Keep mojibake fallbacks because some legacy terminals display Chinese
    # folder names using the wrong code page.
    full_path = os.fspath(path).casefold()
    if "调头" in full_path or "diaotou" in full_path or "璋冨ご" in full_path or "鐠嬪啫銇" in full_path:
        return True
    name = os.path.basename(path).lower()
    # Keep the mojibake fallback because some terminals show the Chinese file
    # name with the wrong code page, while Python still sees the real Unicode.
    return "调头" in name or "diaotou" in name or "璋冨ご" in name


def solve_affine(local: np.ndarray, target: np.ndarray) -> np.ndarray:
    # [X, Z]^T = matrix(2x3) * [vx, vz, 1]^T
    design = np.column_stack([local[:, 0], local[:, 1], np.ones(len(local))])
    mx, *_ = np.linalg.lstsq(design, target[:, 0], rcond=None)
    mz, *_ = np.linalg.lstsq(design, target[:, 1], rcond=None)
    return np.vstack([mx, mz])


def apply_affine(matrix: np.ndarray, vx: float, vz: float) -> Tuple[float, float]:
    v = np.array([vx, vz, 1.0])
    out = matrix @ v
    return float(out[0]), float(out[1])


def normalize_pi(angle: float) -> float:
    return (angle + math.pi / 2.0) % math.pi - math.pi / 2.0


def average_pi_angles(angles: Iterable[float]) -> float:
    values = list(angles)
    if not values:
        return 0.0
    s = sum(math.sin(2.0 * a) for a in values)
    c = sum(math.cos(2.0 * a) for a in values)
    return normalize_pi(0.5 * math.atan2(s, c))


def rotation_from_main_faces(corner: CornerResult) -> float:
    theta1 = math.atan(corner.line1_m)
    theta2 = math.atan(corner.line2_m)
    assignments = [(0.0, math.pi / 2.0), (math.pi / 2.0, 0.0)]
    best_phi = 0.0
    best_error = float("inf")
    for target1, target2 in assignments:
        phi = average_pi_angles([
            normalize_pi(target1 - theta1),
            normalize_pi(target2 - theta2),
        ])
        error = abs(normalize_pi(theta1 + phi - target1)) + abs(normalize_pi(theta2 + phi - target2))
        if error < best_error:
            best_error = error
            best_phi = phi
    return normalize_pi(best_phi)


def rotate_point(phi: float, x: float, z: float) -> Tuple[float, float]:
    c = math.cos(phi)
    s = math.sin(phi)
    return c * x - s * z, s * x + c * z


def apply_transform(transform: object, vx: float, vz: float) -> Tuple[float, float]:
    if isinstance(transform, dict):
        if "matrix" in transform:
            matrix = np.array(transform["matrix"], dtype=float)
            offset = np.array(transform["translation"], dtype=float)
            out = matrix @ np.array([vx, vz], dtype=float) + offset
            return float(out[0]), float(out[1])
        phi = float(transform["rotation_rad"])
        rx, rz = rotate_point(phi, vx, vz)
        return rx + float(transform["tx"]), rz + float(transform["tz"])
    return apply_affine(np.array(transform, dtype=float), vx, vz)


EXPECTED_FACE_DIRECTIONS = {
    # Global X is right, global Z is up. Directions point away from each corner
    # along its two adjacent main faces.
    1: [(-1.0, 0.0), (0.0, -1.0)],  # P1 top-right: top face left, right face down
    2: [(1.0, 0.0), (0.0, 1.0)],    # P2 bottom-left: bottom face right, left face up
    3: [(1.0, 0.0), (0.0, -1.0)],   # P3 top-left: top face right, left face down
    4: [(-1.0, 0.0), (0.0, 1.0)],   # P4 bottom-right: bottom face left, right face up
}

CORNER_SIDE_DIRECTIONS = {
    1: [("A", (-1.0, 0.0)), ("B", (0.0, -1.0))],
    2: [("C", (1.0, 0.0)), ("D", (0.0, 1.0))],
    3: [("A", (1.0, 0.0)), ("D", (0.0, -1.0))],
    4: [("C", (-1.0, 0.0)), ("B", (0.0, 1.0))],
}


def unit_vector(v: Tuple[float, float]) -> np.ndarray:
    arr = np.array(v, dtype=float)
    norm = float(np.linalg.norm(arr))
    if norm < 1e-9:
        raise ValueError("zero-length direction vector")
    return arr / norm


def local_face_directions(corner: CornerResult) -> List[np.ndarray]:
    v1 = (
        corner.face1_mid_x - corner.vx,
        corner.face1_mid_z - corner.vz,
    )
    v2 = (
        corner.face2_mid_x - corner.vx,
        corner.face2_mid_z - corner.vz,
    )
    return [unit_vector(v1), unit_vector(v2)]


def solve_direction_matrix(local_dirs: List[np.ndarray], target_dirs: List[Tuple[float, float]]) -> Tuple[np.ndarray, float]:
    best_matrix: Optional[np.ndarray] = None
    best_error = float("inf")
    target_options = [
        [unit_vector(target_dirs[0]), unit_vector(target_dirs[1])],
        [unit_vector(target_dirs[1]), unit_vector(target_dirs[0])],
    ]
    local = np.vstack(local_dirs)
    for target in target_options:
        global_dirs = np.vstack(target)
        h = local.T @ global_dirs
        u, _, vt = np.linalg.svd(h)
        matrix = vt.T @ u.T
        mapped = (matrix @ local.T).T
        error = float(np.sum(np.linalg.norm(mapped - global_dirs, axis=1)))
        if error < best_error:
            best_error = error
            best_matrix = matrix
    if best_matrix is None:
        raise RuntimeError("failed to solve direction matrix")
    return best_matrix, best_error


def average_orthogonal_matrices(
    matrices: List[np.ndarray],
    weights: Optional[List[float]] = None,
) -> np.ndarray:
    stack = np.stack(matrices)
    mean = np.mean(stack, axis=0) if weights is None else np.average(stack, axis=0, weights=np.asarray(weights, dtype=float))
    u, _, vt = np.linalg.svd(mean)
    return u @ vt


def fit_2d_line(points: List[Tuple[float, float]]) -> Tuple[float, float, float]:
    arr = np.array(points, dtype=float)
    centroid = np.mean(arr, axis=0)
    _, _, vt = np.linalg.svd(arr - centroid)
    direction = vt[0]
    normal = np.array([-direction[1], direction[0]], dtype=float)
    norm = float(np.linalg.norm(normal))
    if norm < 1e-9:
        raise RuntimeError("cannot fit line from degenerate points")
    normal /= norm
    c = -float(normal @ centroid)
    return float(normal[0]), float(normal[1]), c


def intersect_2d_lines(l1: Tuple[float, float, float], l2: Tuple[float, float, float]) -> Tuple[float, float]:
    a1, b1, c1 = l1
    a2, b2, c2 = l2
    det = a1 * b2 - a2 * b1
    if abs(det) < 1e-9:
        raise RuntimeError("global side lines are nearly parallel")
    x = (b1 * c2 - b2 * c1) / det
    z = (c1 * a2 - c2 * a1) / det
    return float(x), float(z)


def assign_corner_faces_to_sides(
    obj: int,
    corner_point: Tuple[float, float],
    face_points: List[Tuple[float, float]],
) -> Dict[str, Tuple[float, float]]:
    vectors = [unit_vector((p[0] - corner_point[0], p[1] - corner_point[1])) for p in face_points]
    sides = CORNER_SIDE_DIRECTIONS[obj]
    # Try both assignments and keep the one whose transformed face directions
    # best match the physical side directions at that corner.
    candidates = [
        {sides[0][0]: 0, sides[1][0]: 1},
        {sides[0][0]: 1, sides[1][0]: 0},
    ]
    best = candidates[0]
    best_score = -float("inf")
    for candidate in candidates:
        score = 0.0
        for side_name, expected in sides:
            score += float(vectors[candidate[side_name]] @ unit_vector(expected))
        if score > best_score:
            best_score = score
            best = candidate
    return {side: face_points[index] for side, index in best.items()}


def reconstruct_points_from_global_sides(
    corners: Dict[int, CornerResult],
    transforms: Dict[int, object],
    raw_points: Dict[str, Tuple[float, float]],
) -> Dict[str, Tuple[float, float]]:
    side_points: Dict[str, List[Tuple[float, float]]] = {"A": [], "B": [], "C": [], "D": []}
    for obj, point_name in OBJECT_TO_POINT.items():
        corner = corners[obj]
        corner_point = raw_points[point_name]
        f1 = apply_transform(transforms[obj], corner.face1_mid_x, corner.face1_mid_z)
        f2 = apply_transform(transforms[obj], corner.face2_mid_x, corner.face2_mid_z)
        assigned = assign_corner_faces_to_sides(obj, corner_point, [f1, f2])
        for side_name, face_point in assigned.items():
            side_points[side_name].append(corner_point)
            side_points[side_name].append(face_point)

    lines = {side: fit_2d_line(points) for side, points in side_points.items()}
    return {
        "P1": intersect_2d_lines(lines["A"], lines["B"]),
        "P2": intersect_2d_lines(lines["C"], lines["D"]),
        "P3": intersect_2d_lines(lines["A"], lines["D"]),
        "P4": intersect_2d_lines(lines["B"], lines["C"]),
    }


def fuse_rectangular_cross_section(points: Dict[str, Tuple[float, float]]) -> Dict[str, Tuple[float, float]]:
    # Four independent corner cameras carry small alignment residuals. For the
    # bar cross-section, fuse opposite camera observations into one global
    # rectangular section instead of letting one corner dominate one edge.
    left_x = (points["P2"][0] + points["P3"][0]) * 0.5
    right_x = (points["P1"][0] + points["P4"][0]) * 0.5
    bottom_z = (points["P2"][1] + points["P4"][1]) * 0.5
    top_z = (points["P1"][1] + points["P3"][1]) * 0.5
    return {
        "P1": (right_x, top_z),
        "P2": (left_x, bottom_z),
        "P3": (left_x, top_z),
        "P4": (right_x, bottom_z),
    }


def apply_corner_biases(
    points: Dict[str, Tuple[float, float]],
    calibration: Dict,
) -> Dict[str, Tuple[float, float]]:
    biases = calibration.get("corner_biases", {})
    if not biases:
        return points
    corrected = dict(points)
    for point_name, point in points.items():
        bias = biases.get(point_name)
        if bias:
            corrected[point_name] = (point[0] + float(bias[0]), point[1] + float(bias[1]))
    return corrected


def _build_calibration_core(
    captures: List[CalibrationCapture],
    standard: Dict[str, Dict[str, float]],
    standards_by_bar: Optional[Dict[str, Dict[str, Dict[str, float]]]] = None,
) -> Dict:
    samples: Dict[int, List[Tuple[CornerResult, Tuple[float, float], float]]] = {1: [], 2: [], 3: [], 4: []}
    calibration_records: List[Tuple[Dict[int, CornerResult], Dict[str, Tuple[float, float]], float]] = []
    calibration_rows: Dict[str, Dict[str, Dict[str, object]]] = {}
    common_ranges: Dict[str, List[int]] = {}
    captures_per_bar: Dict[str, int] = {}
    for capture in captures:
        key = bar_id_key(capture.bar_id)
        captures_per_bar[key] = captures_per_bar.get(key, 0) + 1

    used_standards: Dict[str, Dict[str, Dict[str, float]]] = {}
    capture_metadata: Dict[str, Dict[str, object]] = {}
    for capture in captures:
        source = capture.source
        capture_name = capture.capture_id
        bar_key = bar_id_key(capture.bar_id)
        orientation = capture.orientation
        capture_standard = standard
        if standards_by_bar is not None:
            try:
                capture_standard = standards_by_bar[bar_key]
            except KeyError as exc:
                available = ", ".join(sorted(standards_by_bar))
                raise ValueError(
                    f"No cross_section truth for calibration bar folder '{capture.bar_id}'. "
                    f"CSV bar_id must equal the folder name. Available: {available}"
                ) from exc
        paired_standards = oriented_standard_pairs(capture_standard, orientation)
        used_standards[capture_name] = {
            source_name: truth_item
            for source_name, _, _, truth_item in paired_standards
        }
        capture_metadata[capture_name] = {
            "bar_id": capture.bar_id,
            "orientation": orientation,
            "repeat_count_for_bar": captures_per_bar[bar_key],
        }
        sample_weight = 1.0 / (captures_per_bar[bar_key] * len(paired_standards))
        common_start, common_end = source_common_range(source)
        common_ranges[capture_name] = [common_start, common_end]
        calibration_rows[capture_name] = {}
        for source_name, source_item, truth_name, truth_item in paired_standards:
            start_percent = float(source_item.get("start_percent", source_item["percent"]))
            end_percent = float(source_item.get("end_percent", source_item["percent"]))
            first_row = int(round(common_start + start_percent * (common_end - common_start)))
            last_row = int(round(common_start + end_percent * (common_end - common_start)))
            row_info: Dict[str, object] = {
                "start_row": first_row,
                "end_row": last_row,
                "source_range": source_name,
                "truth_range": truth_name,
                "representative_rows": {},
            }
            calibration_rows[capture_name][source_name] = row_info
            points = target_points_from_edges(orientation_edges(truth_item, orientation))
            record_corners: Dict[int, CornerResult] = {}
            for obj in [1, 2, 3, 4]:
                corner, representative_row = representative_corner_from_row_range(source, obj, first_row, last_row)
                row_info["representative_rows"][str(obj)] = representative_row
                samples[obj].append((corner, points[OBJECT_TO_POINT[obj]], sample_weight))
                record_corners[obj] = corner
            calibration_records.append((record_corners, points, sample_weight))

    transforms = {}
    for obj in [1, 2, 3, 4]:
        matrices: List[np.ndarray] = []
        direction_errors: List[float] = []
        matrix_weights: List[float] = []
        for corner, _, weight in samples[obj]:
            matrix, error = solve_direction_matrix(local_face_directions(corner), EXPECTED_FACE_DIRECTIONS[obj])
            matrices.append(matrix)
            direction_errors.append(error)
            matrix_weights.append(weight)
        matrix = average_orthogonal_matrices(matrices, matrix_weights)
        translations = []
        translation_weights = []
        for corner, target, weight in samples[obj]:
            mapped = matrix @ np.array([corner.vx, corner.vz], dtype=float)
            translations.append((target[0] - mapped[0], target[1] - mapped[1]))
            translation_weights.append(weight)
        tx = float(np.average([t[0] for t in translations], weights=translation_weights))
        tz = float(np.average([t[1] for t in translations], weights=translation_weights))
        transforms[str(obj)] = {
            "matrix": matrix.tolist(),
            "translation": [tx, tz],
            "direction_error_mean": float(np.average(direction_errors, weights=matrix_weights)),
        }

    transform_objects = {int(k): v for k, v in transforms.items()}
    residuals: Dict[str, List[Tuple[float, float, float]]] = {p: [] for p in ["P1", "P2", "P3", "P4"]}
    for record_corners, target_points, weight in calibration_records:
        raw_points = {
            OBJECT_TO_POINT[obj]: apply_transform(transform_objects[obj], record_corners[obj].vx, record_corners[obj].vz)
            for obj in [1, 2, 3, 4]
        }
        reconstructed = reconstruct_points_from_global_sides(record_corners, transform_objects, raw_points)
        for point_name in ["P1", "P2", "P3", "P4"]:
            target = target_points[point_name]
            actual = reconstructed[point_name]
            residuals[point_name].append((target[0] - actual[0], target[1] - actual[1], weight))
    corner_biases = {
        point_name: [
            float(np.average([r[0] for r in values], weights=[r[2] for r in values])),
            float(np.average([r[1] for r in values], weights=[r[2] for r in values])),
        ]
        for point_name, values in residuals.items()
    }

    captured_bar_ids = sorted({capture.bar_id for capture in captures}, key=str.casefold)
    unused_truth_bar_ids = []
    if standards_by_bar is not None:
        captured_keys = {bar_id_key(bar) for bar in captured_bar_ids}
        unused_truth_bar_ids = sorted(
            [bar_id for bar_id in standards_by_bar if bar_id not in captured_keys],
            key=str.casefold,
        )

    return {
        "version": 6,
        "model": "camera_oriented_transform_with_corner_bias",
        "note": "Calibration fixes camera-to-global orientation/translation and per-corner residual bias from standard-bar captures. Default measurement is free four-corner geometry: A/C and B/D are not forced or range-limited.",
        "mapping": {"1": "P1/top", "3": "P3/left", "2": "P2/down", "4": "P4/right"},
        "common_ranges": common_ranges,
        "calibration_rows": calibration_rows,
        "standard": standard,
        "standards_by_capture": used_standards,
        "capture_metadata": capture_metadata,
        "captured_bar_ids": captured_bar_ids,
        "unused_truth_bar_ids": unused_truth_bar_ids,
        "bar_weighting": "equal_per_bar_then_equal_per_repeat_and_position",
        "transforms": transforms,
        "corner_biases": corner_biases,
    }


def build_calibration_from_captures(
    captures: List[CalibrationCapture],
    standard: Dict[str, Dict[str, float]],
    standards_by_bar: Optional[Dict[str, Dict[str, Dict[str, float]]]] = None,
) -> Dict:
    """Build a calibration with optional normal/turnover submodels.

    The default top-level transforms remain the normal-direction transforms so
    existing production calls are backward compatible.  When reversal captures
    are available, an explicit ``orientation_models`` map preserves the camera
    geometry of each direction instead of averaging two reflected states.
    """

    base = _build_calibration_core(captures, standard, standards_by_bar)
    by_orientation: Dict[str, List[CalibrationCapture]] = {}
    for capture in captures:
        key = "turnover" if capture.orientation in {"swap_bd", "turnover"} else "normal"
        by_orientation.setdefault(key, []).append(capture)
    if set(by_orientation) == {"normal", "turnover"}:
        normal_model = _build_calibration_core(by_orientation["normal"], standard, standards_by_bar)
        turnover_model = _build_calibration_core(by_orientation["turnover"], standard, standards_by_bar)
        base.update({
            "version": 7,
            "model": "camera_oriented_transform_with_orientation_models",
            "note": "Free four-corner measurement with separate normal and turnover camera transforms. Turnover reverses longitudinal truth windows and swaps B/D physical faces; A/C remain unchanged.",
            "transforms": normal_model["transforms"],
            "corner_biases": normal_model["corner_biases"],
            "orientation_models": {
                "normal": normal_model,
                "turnover": turnover_model,
            },
        })
    return base


def calibration_for_orientation(calibration: Dict, orientation: str) -> Dict:
    """Select the direction-specific calibration when the model provides it."""

    models = calibration.get("orientation_models")
    key = "turnover" if orientation in {"swap_bd", "turnover"} else "normal"
    if isinstance(models, dict) and isinstance(models.get(key), dict):
        return models[key]
    return calibration


def build_calibration(
    source: HeightSource,
    common_start: int,
    common_end: int,
    standard: Dict[str, Dict[str, float]],
) -> Dict:
    return build_calibration_from_captures(
        [CalibrationCapture(source=source, capture_id="single", bar_id="single")],
        standard,
    )


def load_standard(path: Optional[str]) -> Dict[str, Dict[str, float]]:
    if not path:
        return DEFAULT_STANDARD
    with open(path, "r", encoding="utf-8") as f:
        data = json.load(f)
    return data


def load_standard_truth_csv(path: str) -> Dict[str, Dict[str, Dict[str, float]]]:
    """Read cross-section truth from the unified manual-calibration CSV.

    The file also contains end-face rows.  Only rows with
    ``record_type=cross_section`` are consumed here; their position may be a
    fraction (0.2) or a percent (20).
    """

    with open(path, "r", encoding="utf-8-sig", newline="") as handle:
        rows = list(csv.DictReader(handle))
    standards_by_bar: Dict[str, Dict[str, Dict[str, float]]] = {}
    for row in rows:
        record_type = str(row.get("record_type", "")).strip().lower()
        if record_type != "cross_section":
            continue
        bar_id = normalize_bar_id(str(row.get("bar_id", "")).strip())
        if not bar_id:
            raise ValueError("cross_section rows need a bar_id matching the HOBJ file name without .hobj")
        raw_percent = str(row.get("position_percent", "")).strip().replace("%", "")
        parts = raw_percent.split("-")
        if len(parts) not in {1, 2}:
            raise ValueError(f"cross_section position_percent must be a point or range such as 25 or 15-25: {raw_percent}")
        try:
            start_percent = float(parts[0])
            end_percent = float(parts[-1])
        except ValueError as exc:
            raise ValueError("cross_section rows need a numeric position_percent or range") from exc
        if start_percent > 1.0:
            start_percent /= 100.0
        if end_percent > 1.0:
            end_percent /= 100.0
        if not 0.0 < start_percent <= end_percent < 1.0:
            raise ValueError(f"cross_section position_percent must stay between 0 and 100: {raw_percent}")
        percent = (start_percent + end_percent) * 0.5
        edges: Dict[str, float] = {"percent": percent, "start_percent": start_percent, "end_percent": end_percent}
        for edge in ["A", "B", "C", "D"]:
            raw_value = str(row.get(f"{edge}_mm", row.get(edge, ""))).strip()
            try:
                edges[edge] = float(raw_value)
            except ValueError as exc:
                raise ValueError(
                    f"cross_section position {raw_percent} is missing a numeric {edge}_mm value"
                ) from exc
        name = f"range{start_percent * 100:g}_{end_percent * 100:g}"
        bar_standard = standards_by_bar.setdefault(bar_id_key(bar_id), {})
        if name in bar_standard:
            raise ValueError(
                f"Duplicate cross_section truth row for bar_id={bar_id}, position_percent={raw_percent}"
            )
        bar_standard[name] = edges
    if not standards_by_bar:
        raise ValueError("No record_type=cross_section rows were found in the calibration truth CSV")
    for bar_id, standard in standards_by_bar.items():
        if len(standard) < 3:
            raise ValueError(
                f"bar_id={bar_id} needs at least three cross_section truth rows at known longitudinal positions"
            )
        standards_by_bar[bar_id] = dict(sorted(standard.items(), key=lambda item: item[1]["percent"]))
    return standards_by_bar


def standard_for_bar(
    standards_by_bar: Dict[str, Dict[str, Dict[str, float]]],
    bar_id: str,
) -> Dict[str, Dict[str, float]]:
    try:
        return standards_by_bar[bar_id_key(bar_id)]
    except KeyError as exc:
        available = ", ".join(sorted(standards_by_bar))
        raise ValueError(
            f"No cross_section truth for '{bar_id}'. CSV bar_id must equal the HOBJ file name without .hobj. "
            f"Available: {available}"
        ) from exc


def detect_input(args: argparse.Namespace) -> HeightSource:
    if args.tifs:
        if len(args.tifs) != 4:
            raise ValueError("--tifs must provide four files in object order: obj1 obj2 obj3 obj4")
        return TiffSource({1: args.tifs[0], 2: args.tifs[1], 3: args.tifs[2], 4: args.tifs[3]})

    if not args.input:
        raise ValueError("Provide --input HOBJ/folder or --tifs four files")

    path = args.input
    if os.path.isfile(path) and path.lower().endswith(".hobj"):
        offsets = None
        if args.hobj_offsets:
            values = [int(v.strip()) for v in args.hobj_offsets.split(",")]
            if len(values) != 4:
                raise ValueError("--hobj-offsets must contain four comma-separated byte offsets")
            offsets = {1: values[0], 2: values[1], 3: values[2], 4: values[3]}
        return HobjSource(path, args.width, args.height, offsets)

    if os.path.isdir(path):
        files: Dict[int, str] = {}
        names = os.listdir(path)
        for obj in [1, 2, 3, 4]:
            candidates = [
                n for n in names
                if n.lower().endswith((".tif", ".tiff"))
                and (f"object_{obj}" in n.lower() or f"obj{obj}" in n.lower() or f"camera{obj}" in n.lower())
            ]
            if not candidates:
                raise ValueError(f"Cannot find TIFF for object {obj} in {path}")
            files[obj] = os.path.join(path, sorted(candidates)[0])
        return TiffSource(files)

    raise ValueError(f"Unsupported input: {path}")


def two_point_distance(points: Dict[str, Tuple[float, float]], a: str, b: str) -> float:
    p = points[a]
    q = points[b]
    return math.hypot(p[0] - q[0], p[1] - q[1])


def edge_lengths(points: Dict[str, Tuple[float, float]]) -> Dict[str, float]:
    def dist(a: str, b: str) -> float:
        return two_point_distance(points, a, b)

    return {
        "A": dist("P3", "P1"),
        "B": dist("P1", "P4"),
        "C": dist("P2", "P4"),
        "D": dist("P3", "P2"),
    }


def cross_section_geometry(
    corners: Dict[int, CornerResult],
    transforms: Dict[int, object],
    calibration: Dict,
    geometry_mode: str,
) -> Tuple[
    Dict[str, Tuple[float, float]],
    Dict[str, Tuple[float, float]],
    Dict[str, float],
]:
    """Transform one four-camera slice and return corners, chamfer midpoints and lengths."""
    points = {
        point_name: apply_transform(transforms[obj], corners[obj].vx, corners[obj].vz)
        for obj, point_name in OBJECT_TO_POINT.items()
    }
    points = reconstruct_points_from_global_sides(corners, transforms, points)
    points = apply_corner_biases(points, calibration)
    free_points = points
    if geometry_mode == "rectangular":
        points = fuse_rectangular_cross_section(points)
    point_deltas = {
        name: (points[name][0] - free_points[name][0], points[name][1] - free_points[name][1])
        for name in ("P1", "P2", "P3", "P4")
    }
    midpoints: Dict[str, Tuple[float, float]] = {}
    for obj, point_name in OBJECT_TO_POINT.items():
        corner = corners[obj]
        midpoint_name = "M" + point_name[1:]
        if math.isnan(corner.chamfer_mid_x) or math.isnan(corner.chamfer_mid_z):
            midpoints[midpoint_name] = points[point_name]
        else:
            midpoint = apply_transform(transforms[obj], corner.chamfer_mid_x, corner.chamfer_mid_z)
            delta = point_deltas[point_name]
            midpoints[midpoint_name] = (midpoint[0] + delta[0], midpoint[1] + delta[1])
    lengths = edge_lengths(points)
    lengths["diag1"] = two_point_distance(midpoints, "M1", "M2")
    lengths["diag2"] = two_point_distance(midpoints, "M3", "M4")
    return points, midpoints, lengths


def normalized_axis(axis: np.ndarray) -> np.ndarray:
    norm = float(np.linalg.norm(axis))
    if norm <= 1e-12:
        return np.array([0.0, 1.0, 0.0], dtype=float)
    out = np.asarray(axis, dtype=float) / norm
    return out if out[1] >= 0.0 else -out


def robust_plane_y_from_xz(
    points_xyz: np.ndarray,
    end: str,
    rod_axis: Optional[np.ndarray] = None,
) -> EndFaceFit:
    """Fit Y = slope_x * X + slope_z * Z + intercept using robust MAD rejection."""

    arr = np.asarray(points_xyz, dtype=float)
    if arr.ndim != 2 or arr.shape[1] != 3:
        return EndFaceFit(end=end, valid=False, reason="end-face points must be an N x 3 array")
    arr = arr[np.all(np.isfinite(arr), axis=1)]
    if len(arr) < 12:
        return EndFaceFit(end=end, valid=False, point_count=len(arr), reason="fewer than 12 usable end-face points")

    design = np.column_stack([arr[:, 0], arr[:, 2], np.ones(len(arr))])
    target_y = arr[:, 1]
    keep = np.ones(len(arr), dtype=bool)
    coef = np.zeros(3, dtype=float)
    for _ in range(6):
        if int(keep.sum()) < 12:
            break
        coef, *_ = np.linalg.lstsq(design[keep], target_y[keep], rcond=None)
        residual = target_y - design @ coef
        centre = float(np.median(residual[keep]))
        mad = float(np.median(np.abs(residual[keep] - centre)))
        sigma = 1.4826 * mad
        threshold = max(0.015, 3.5 * sigma)
        updated = np.abs(residual - centre) <= threshold
        if int(updated.sum()) < max(12, len(arr) // 4):
            break
        if np.array_equal(updated, keep):
            keep = updated
            break
        keep = updated

    if int(keep.sum()) < 12:
        return EndFaceFit(end=end, valid=False, point_count=len(arr), inlier_count=int(keep.sum()), reason="plane fit retained too few inliers")

    coef, *_ = np.linalg.lstsq(design[keep], target_y[keep], rcond=None)
    residual = target_y[keep] - design[keep] @ coef
    rmse = float(np.sqrt(np.mean(residual * residual)))
    slope_x, slope_z, intercept = (float(v) for v in coef)
    normal = normalized_axis(np.array([-slope_x, 1.0, -slope_z], dtype=float))
    axis = normalized_axis(np.array([0.0, 1.0, 0.0], dtype=float) if rod_axis is None else rod_axis)
    cosine = float(np.clip(abs(normal @ axis), 0.0, 1.0))
    verticality = math.degrees(math.acos(cosine))
    return EndFaceFit(
        end=end,
        valid=True,
        slope_x=slope_x,
        slope_z=slope_z,
        intercept_y=intercept,
        normal_x=float(normal[0]),
        normal_y=float(normal[1]),
        normal_z=float(normal[2]),
        verticality_deg=verticality,
        point_count=len(arr),
        inlier_count=int(keep.sum()),
        rmse_mm=rmse,
    )


def fit_rod_axis(rows: List[Dict[str, object]]) -> np.ndarray:
    """Fit the actual rod centre trajectory instead of assuming it follows the scanner Y axis."""

    centres: List[Tuple[float, float, float]] = []
    for row in rows:
        if row.get("record") != "slice" or row.get("valid") is not True:
            continue
        try:
            y = float(row["y_mm"])
            x = float(np.mean([float(row[f"P{i}_x"]) for i in [1, 2, 3, 4]]))
            z = float(np.mean([float(row[f"P{i}_z"]) for i in [1, 2, 3, 4]]))
        except (KeyError, TypeError, ValueError):
            continue
        centres.append((x, y, z))
    if len(centres) < 3:
        return np.array([0.0, 1.0, 0.0], dtype=float)

    arr = np.array(centres, dtype=float)
    y = arr[:, 1]
    dx_dy, _ = np.polyfit(y, arr[:, 0], 1)
    dz_dy, _ = np.polyfit(y, arr[:, 2], 1)
    for _ in range(3):
        predicted_x = dx_dy * y + float(np.median(arr[:, 0] - dx_dy * y))
        predicted_z = dz_dy * y + float(np.median(arr[:, 2] - dz_dy * y))
        residual = np.hypot(arr[:, 0] - predicted_x, arr[:, 2] - predicted_z)
        med = float(np.median(residual))
        sigma = 1.4826 * float(np.median(np.abs(residual - med))) + 1e-9
        keep = residual <= med + 3.5 * sigma
        if int(keep.sum()) < 3:
            break
        dx_dy, _ = np.polyfit(y[keep], arr[keep, 0], 1)
        dz_dy, _ = np.polyfit(y[keep], arr[keep, 2], 1)
    return normalized_axis(np.array([float(dx_dy), 1.0, float(dz_dy)], dtype=float))


def endface_boundary_points_for_camera(
    source: HeightSource,
    obj: int,
    end: str,
    valid_range: Tuple[int, int],
    transform: object,
    calibration: Dict,
    common_start: int,
    x_scale: float,
    y_scale: float,
    window_mm: float,
    min_run_rows: int = 3,
    column_step: int = 4,
    common_end: Optional[int] = None,
    drift_model: Optional[Dict[str, object]] = None,
    drift_amplitude: float = 0.0,
    drift_alignment_shift_fraction: float = 0.0,
) -> np.ndarray:
    """Extract the first/last continuous material boundary for one camera."""

    row0, row1 = valid_range
    window_rows = max(12, int(round(window_mm / y_scale)))
    if end == "head":
        block_start = row0
        block_end = min(row1, row0 + window_rows - 1)
    elif end == "tail":
        block_start = max(row0, row1 - window_rows + 1)
        block_end = row1
    else:
        raise ValueError(f"Unknown end: {end}")
    if block_end - block_start + 1 < min_run_rows:
        return np.empty((0, 3), dtype=float)

    block = np.stack([source.row(obj, row) for row in range(block_start, block_end + 1)]).astype(float, copy=False)
    valid = (block > INVALID_Z) & np.isfinite(block) & (np.abs(block) > 1e-6)
    runs = valid.copy()
    if end == "head":
        for offset in range(1, min_run_rows):
            runs[:-offset] &= valid[offset:]
            runs[-offset:] = False
    else:
        for offset in range(1, min_run_rows):
            runs[offset:] &= valid[:-offset]
            runs[:offset] = False

    candidate_cols = np.flatnonzero(np.any(runs, axis=0))[:: max(1, column_step)]
    if candidate_cols.size == 0:
        return np.empty((0, 3), dtype=float)
    bias = calibration.get("corner_biases", {}).get(OBJECT_TO_POINT[obj], [0.0, 0.0])
    points: List[Tuple[float, float, float]] = []
    for col in candidate_cols:
        available = np.flatnonzero(runs[:, col])
        if available.size == 0:
            continue
        local_row = int(available[0] if end == "head" else available[-1])
        if end == "head":
            inside = slice(local_row, min(len(block), local_row + 6))
        else:
            inside = slice(max(0, local_row - 5), local_row + 1)
        z_values = block[inside, col]
        z_values = z_values[(z_values > INVALID_Z) & np.isfinite(z_values) & (np.abs(z_values) > 1e-6)]
        if z_values.size < min_run_rows:
            continue
        local_x = float(col) * x_scale
        local_z = float(np.median(z_values))
        absolute_row = block_start + local_row
        if drift_model is not None and common_end is not None and common_end > common_start:
            fraction = drift_fraction_for_row(drift_model, absolute_row, common_start, common_end)
            shift_x, shift_z = mechanical_drift_local_shift(
                drift_model, obj, fraction + drift_alignment_shift_fraction, drift_amplitude
            )
            local_x -= shift_x
            local_z -= shift_z
        global_x, global_z = apply_transform(transform, local_x, local_z)
        global_x += float(bias[0])
        global_z += float(bias[1])
        global_y = float(block_start + local_row - common_start) * y_scale
        points.append((global_x, global_y, global_z))
    return np.asarray(points, dtype=float) if points else np.empty((0, 3), dtype=float)


def fit_endfaces_from_source(
    source: HeightSource,
    valid_ranges: Dict[int, Tuple[int, int]],
    transforms: Dict[int, object],
    calibration: Dict,
    common_start: int,
    rod_axis: np.ndarray,
    x_scale: float,
    y_scale: float,
    window_mm: float,
    common_end: Optional[int] = None,
    drift_model: Optional[Dict[str, object]] = None,
    drift_amplitude: float = 0.0,
    drift_alignment_shift_fraction: float = 0.0,
) -> Dict[str, EndFaceFit]:
    results: Dict[str, EndFaceFit] = {}
    for end in ["head", "tail"]:
        clouds = [
            endface_boundary_points_for_camera(
                source,
                obj,
                end,
                valid_ranges[obj],
                transforms[obj],
                calibration,
                common_start,
                x_scale,
                y_scale,
                window_mm,
                common_end=common_end,
                drift_model=drift_model,
                drift_amplitude=drift_amplitude,
                drift_alignment_shift_fraction=drift_alignment_shift_fraction,
            )
            for obj in [1, 2, 3, 4]
        ]
        nonempty = [cloud for cloud in clouds if len(cloud)]
        if not nonempty:
            results[end] = EndFaceFit(end=end, valid=False, reason="no continuous end boundary was found")
            continue
        results[end] = robust_plane_y_from_xz(np.vstack(nonempty), end, rod_axis)
    return results


def mean_cross_section_points(rows: List[Dict[str, object]]) -> Dict[str, Tuple[float, float]]:
    points: Dict[str, Tuple[float, float]] = {}
    for point_name in ["P1", "P2", "P3", "P4"]:
        values: List[Tuple[float, float]] = []
        for row in rows:
            if row.get("record") != "slice" or row.get("valid") is not True:
                continue
            try:
                values.append((float(row[f"{point_name}_x"]), float(row[f"{point_name}_z"])))
            except (KeyError, TypeError, ValueError):
                continue
        if not values:
            raise ValueError(f"Cannot determine mean cross-section point {point_name}")
        arr = np.asarray(values, dtype=float)
        points[point_name] = (float(np.median(arr[:, 0])), float(np.median(arr[:, 1])))
    return points


def face_to_endface_angles(
    section_points: Dict[str, Tuple[float, float]],
    endface_fit: EndFaceFit,
    rod_axis: np.ndarray,
) -> Dict[str, float]:
    """Return the included angle between each actual side plane and one end plane."""

    if not endface_fit.valid:
        return {}
    face_ends = {
        "A": ("P3", "P1"),
        "B": ("P1", "P4"),
        "C": ("P2", "P4"),
        "D": ("P3", "P2"),
    }
    axis = normalized_axis(rod_axis)
    end_normal = normalized_axis(
        np.array([endface_fit.normal_x, endface_fit.normal_y, endface_fit.normal_z], dtype=float)
    )
    angles: Dict[str, float] = {}
    for face, (start_name, finish_name) in face_ends.items():
        start = section_points[start_name]
        finish = section_points[finish_name]
        edge_direction = np.array([finish[0] - start[0], 0.0, finish[1] - start[1]], dtype=float)
        side_normal = np.cross(edge_direction, axis)
        norm = float(np.linalg.norm(side_normal))
        if norm <= 1e-12:
            continue
        side_normal /= norm
        cosine = float(np.clip(abs(side_normal @ end_normal), 0.0, 1.0))
        angles[face] = math.degrees(math.acos(cosine))
    return angles


def mean_face_endface_angle(face_angles: Dict[str, float]) -> float:
    """Return the arithmetic mean of the four face-to-end-face angles."""
    values = [face_angles[face] for face in ["A", "B", "C", "D"] if face in face_angles]
    return float(np.mean(values)) if len(values) == 4 else math.nan


def measure_endface_angles_for_capture(
    capture: CalibrationCapture,
    calibration: Dict,
    x_scale: float,
    y_scale: float,
    window_mm: float,
) -> Dict[str, Dict[str, float]]:
    """Measure the eight raw face-to-end angles needed by end-face calibration."""

    source = capture.source
    valid_ranges = source_valid_ranges(source)
    common_start, common_end = source_common_range(source, valid_ranges)
    active_calibration = calibration_for_orientation(calibration, capture.orientation)
    transforms = {int(key): value for key, value in active_calibration["transforms"].items()}
    rows: List[Dict[str, object]] = []
    for fraction in np.linspace(0.1, 0.9, 9):
        row_index = int(round(common_start + float(fraction) * (common_end - common_start)))
        corners = {obj: extract_corner_from_row(source.row(obj, row_index), x_scale) for obj in [1, 2, 3, 4]}
        if not all(corner.valid for corner in corners.values()):
            continue
        raw_points = {
            OBJECT_TO_POINT[obj]: apply_transform(transforms[obj], corners[obj].vx, corners[obj].vz)
            for obj in [1, 2, 3, 4]
        }
        points = reconstruct_points_from_global_sides(corners, transforms, raw_points)
        points = apply_corner_biases(points, active_calibration)
        record: Dict[str, object] = {
            "record": "slice",
            "valid": True,
            "y_mm": (row_index - common_start) * y_scale,
        }
        for point_name, (x, z) in points.items():
            record[f"{point_name}_x"] = x
            record[f"{point_name}_z"] = z
        rows.append(record)
    if len(rows) < 3:
        raise ValueError(f"Calibration capture {capture.capture_id} has fewer than three usable section rows")
    rod_axis = fit_rod_axis(rows)
    endfaces = fit_endfaces_from_source(
        source,
        valid_ranges,
        transforms,
        active_calibration,
        common_start,
        rod_axis,
        x_scale,
        y_scale,
        window_mm,
    )
    section_points = mean_cross_section_points(rows)
    return {
        end: face_to_endface_angles(section_points, endfaces[end], rod_axis)
        for end in ["head", "tail"]
    }


def oriented_endface_truth(
    truth: Dict[str, Dict[str, float]],
    orientation: str,
) -> Dict[str, Dict[str, float]]:
    """Express physical manual end-face truth in the current capture labels."""

    if orientation not in {"swap_bd", "turnover"}:
        return {end: dict(values) for end, values in truth.items()}
    end_map = {"head": "tail", "tail": "head"}
    face_map = {"A": "A", "B": "D", "C": "C", "D": "B"}
    return {
        end: {
            face: truth[end_map[end]][face_map[face]]
            for face in ["A", "B", "C", "D"]
        }
        for end in ["head", "tail"]
    }


def _build_balanced_endface_angle_calibration_core(
    captures: List[CalibrationCapture],
    calibration: Dict,
    truth_csv_path: str,
    x_scale: float,
    y_scale: float,
    window_mm: float,
) -> Dict[str, object]:
    """Fit eight offsets with equal bar weight and equal repeat weight within each bar."""

    raw_by_bar: Dict[str, List[Tuple[str, Dict[str, Dict[str, float]]]]] = {}
    display_bar_ids: Dict[str, str] = {}
    for capture in captures:
        bar_key = bar_id_key(capture.bar_id)
        display_bar_ids[bar_key] = capture.bar_id
        raw = measure_endface_angles_for_capture(capture, calibration, x_scale, y_scale, window_mm)
        raw_by_bar.setdefault(bar_key, []).append((capture.capture_id, raw))

    per_bar: Dict[str, Dict[str, object]] = {}
    bar_offsets: List[Dict[str, Dict[str, float]]] = []
    for bar_key, measurements in raw_by_bar.items():
        bar_id = display_bar_ids[bar_key]
        truth = oriented_endface_truth(
            read_manual_endface_angle_truth(truth_csv_path, bar_id),
            captures[0].orientation,
        )
        if not truth:
            raise ValueError(f"No direct end-face angle truth was found for calibration bar {bar_id}")
        mean_raw: Dict[str, Dict[str, float]] = {"head": {}, "tail": {}}
        offsets: Dict[str, Dict[str, float]] = {"head": {}, "tail": {}}
        for end in ["head", "tail"]:
            for face in ["A", "B", "C", "D"]:
                values = [angles[end][face] for _, angles in measurements if face in angles.get(end, {})]
                if len(values) != len(measurements):
                    raise ValueError(f"Missing raw angle {end}-{face} in one or more captures for bar {bar_id}")
                mean_raw[end][face] = float(np.mean(np.asarray(values, dtype=float)))
                offsets[end][face] = truth[end][face] - mean_raw[end][face]
        bar_offsets.append(offsets)
        per_bar[bar_id] = {
            "capture_ids": [capture_id for capture_id, _ in measurements],
            "capture_count": len(measurements),
            "mean_raw_angles_deg": mean_raw,
            "manual_truth_angles_deg": truth,
            "angle_offsets_deg": offsets,
        }

    combined: Dict[str, Dict[str, float]] = {"head": {}, "tail": {}}
    for end in ["head", "tail"]:
        for face in ["A", "B", "C", "D"]:
            combined[end][face] = float(np.mean([offsets[end][face] for offsets in bar_offsets]))
    captured_bar_ids = sorted({capture.bar_id for capture in captures}, key=str.casefold)
    truth_bar_ids = read_manual_truth_bar_ids(truth_csv_path, "endface_angle")
    captured_keys = {bar_id_key(bar_id) for bar_id in captured_bar_ids}
    unused_truth_bar_ids = [
        bar_id
        for key, bar_id in sorted(truth_bar_ids.items(), key=lambda item: item[1].casefold())
        if key not in captured_keys
    ]
    return {
        "version": 3,
        "model": "endface_face_angle_offset",
        "note": "Each bar is weighted equally; repeat HOBJ captures are averaged within their parent bar folder first.",
        "bar_weighting": "equal_per_bar_then_equal_per_repeat",
        "captured_bar_ids": captured_bar_ids,
        "unused_truth_bar_ids": unused_truth_bar_ids,
        "angle_offsets_deg": combined,
        "per_bar": per_bar,
    }


def build_balanced_endface_angle_calibration_model(
    captures: List[CalibrationCapture],
    calibration: Dict,
    truth_csv_path: str,
    x_scale: float,
    y_scale: float,
    window_mm: float,
) -> Dict[str, object]:
    """Build normal/turnover end-face offsets without mixing physical ends."""

    by_orientation: Dict[str, List[CalibrationCapture]] = {}
    for capture in captures:
        key = "turnover" if capture.orientation in {"swap_bd", "turnover"} else "normal"
        by_orientation.setdefault(key, []).append(capture)
    if set(by_orientation) != {"normal", "turnover"}:
        return _build_balanced_endface_angle_calibration_core(
            captures, calibration, truth_csv_path, x_scale, y_scale, window_mm
        )
    normal_model = _build_balanced_endface_angle_calibration_core(
        by_orientation["normal"], calibration, truth_csv_path, x_scale, y_scale, window_mm
    )
    turnover_model = _build_balanced_endface_angle_calibration_core(
        by_orientation["turnover"], calibration, truth_csv_path, x_scale, y_scale, window_mm
    )
    normal_model.update({
        "version": 4,
        "model": "endface_face_angle_offset_by_orientation",
        "note": "Separate normal and turnover end-face offsets. Turnover swaps head/tail and B/D; A/C remain unchanged.",
        "orientation_models": {
            "normal": normal_model.copy(),
            "turnover": turnover_model,
        },
    })
    return normal_model


def endface_calibration_for_orientation(model: Optional[Dict[str, object]], orientation: str) -> Optional[Dict[str, object]]:
    if not model:
        return model
    models = model.get("orientation_models")
    key = "turnover" if orientation in {"swap_bd", "turnover"} else "normal"
    if isinstance(models, dict) and isinstance(models.get(key), dict):
        return models[key]
    return model


def manual_position_fraction(row: Dict[str, str], ordinal: int) -> float:
    raw_fraction = str(row.get("position_fraction", "")).strip()
    raw_percent = str(row.get("position_percent", "")).strip()
    raw_position = str(row.get("position", "")).strip().lower()
    if raw_fraction:
        value = float(raw_fraction)
        if 0.0 <= value <= 1.0:
            return value
    if raw_percent:
        value = float(raw_percent)
        if 0.0 <= value <= 100.0:
            return value / 100.0
    position_aliases = {
        "1": 0.25,
        "p1": 0.25,
        "first": 0.25,
        "2": 0.50,
        "p2": 0.50,
        "middle": 0.50,
        "mid": 0.50,
        "3": 0.75,
        "p3": 0.75,
        "last": 0.75,
    }
    if raw_position in position_aliases:
        return position_aliases[raw_position]
    if raw_position:
        value = float(raw_position)
        if 0.0 <= value <= 1.0:
            return value
    return [0.25, 0.50, 0.75][min(max(ordinal, 0), 2)]


def normalize_end_name(value: str) -> str:
    text = value.strip().lower()
    if text in {"head", "front", "start", "头", "头部", "头端", "头部端面"}:
        return "head"
    if text in {"tail", "rear", "end", "尾", "尾部", "尾端", "尾部端面"}:
        return "tail"
    return text


def first_float(row: Dict[str, str], names: Iterable[str]) -> Optional[float]:
    for name in names:
        raw = row.get(name, "")
        if raw not in ("", None):
            try:
                value = float(raw)
            except (TypeError, ValueError):
                continue
            if math.isfinite(value):
                return value
    return None


def read_manual_truth_bar_ids(path: str, record_type: Optional[str] = None) -> Dict[str, str]:
    with open(path, "r", encoding="utf-8-sig", newline="") as handle:
        rows = list(csv.DictReader(handle))
    bar_ids: Dict[str, str] = {}
    for row in rows:
        if record_type and str(row.get("record_type", "")).strip().lower() != record_type:
            continue
        bar_id = normalize_bar_id(str(row.get("bar_id", "")).strip())
        if bar_id:
            bar_ids[bar_id_key(bar_id)] = bar_id
    return bar_ids


def read_manual_endface_truth(
    path: str,
    section_points: Dict[str, Tuple[float, float]],
    bar_id: str,
) -> Dict[str, EndFaceFit]:
    """Read the user's 4 faces x 3 positions x 2 ends signed gauge readings."""

    with open(path, "r", encoding="utf-8-sig", newline="") as handle:
        rows = list(csv.DictReader(handle))
    target_ids = {bar_id_key(bar_id)}
    matching = [
        row
        for row in rows
        if not str(row.get("bar_id", "")).strip()
        or bar_id_key(str(row.get("bar_id", "")).strip()) in target_ids
    ]
    if not matching:
        raise ValueError(f"No end-face truth rows match bar_id={bar_id}")

    face_ends = {
        "A": ("P3", "P1"),
        "B": ("P1", "P4"),
        "C": ("P2", "P4"),
        "D": ("P3", "P2"),
    }
    grouped: Dict[str, List[Tuple[float, float, float]]] = {"head": [], "tail": []}
    counts: Dict[Tuple[str, str], int] = {}
    for row in matching:
        end = normalize_end_name(str(row.get("end", row.get("端面", ""))))
        face = str(row.get("face", row.get("面", ""))).strip().upper()
        if end not in grouped or face not in face_ends:
            continue
        deviation = first_float(
            row,
            ["deviation_mm", "reading_mm", "value_mm", "perpendicularity_mm", "偏差_mm", "量规读数_mm"],
        )
        if deviation is None:
            continue
        key = (end, face)
        ordinal = counts.get(key, 0)
        counts[key] = ordinal + 1
        x = first_float(row, ["x_mm", "X_mm", "x", "X"])
        z = first_float(row, ["z_mm", "Z_mm", "z", "Z"])
        if x is None or z is None:
            fraction = manual_position_fraction(row, ordinal)
            start_name, end_name = face_ends[face]
            start = section_points[start_name]
            finish = section_points[end_name]
            x = start[0] + fraction * (finish[0] - start[0])
            z = start[1] + fraction * (finish[1] - start[1])
        grouped[end].append((float(x), float(deviation), float(z)))

    fits: Dict[str, EndFaceFit] = {}
    for end in ["head", "tail"]:
        missing = [face for face in ["A", "B", "C", "D"] if counts.get((end, face), 0) < 3]
        if missing:
            fits[end] = EndFaceFit(
                end=end,
                valid=False,
                point_count=len(grouped[end]),
                reason=f"manual truth needs three readings on faces: {','.join(missing)}",
            )
            continue
        fits[end] = robust_plane_y_from_xz(np.asarray(grouped[end], dtype=float), end)
    return fits


def read_manual_endface_angle_truth(path: str, bar_id: str) -> Dict[str, Dict[str, float]]:
    """Average three direct gauge angle readings for each end/face pair."""

    with open(path, "r", encoding="utf-8-sig", newline="") as handle:
        rows = list(csv.DictReader(handle))
    target_ids = {bar_id_key(bar_id)}
    grouped: Dict[str, Dict[str, List[float]]] = {
        end: {face: [] for face in ["A", "B", "C", "D"]}
        for end in ["head", "tail"]
    }
    for row in rows:
        row_bar = str(row.get("bar_id", "")).strip()
        if row_bar and row_bar not in target_ids:
            row_bar = bar_id_key(row_bar)
        if row_bar and row_bar not in target_ids:
            continue
        end = normalize_end_name(str(row.get("end", row.get("端面", ""))))
        face = str(row.get("face", row.get("面", ""))).strip().upper()
        if end not in grouped or face not in grouped[end]:
            continue
        angle = first_float(row, ["angle_deg", "included_angle_deg", "value_deg", "夹角_deg", "量规角度_deg"])
        if angle is not None:
            grouped[end][face].append(angle)

    if not any(grouped[end][face] for end in grouped for face in grouped[end]):
        return {}
    means: Dict[str, Dict[str, float]] = {"head": {}, "tail": {}}
    for end in ["head", "tail"]:
        for face in ["A", "B", "C", "D"]:
            values = grouped[end][face]
            if len(values) != 3:
                raise ValueError(f"Manual angle truth requires exactly three readings for {end}-{face}; got {len(values)}")
            means[end][face] = float(np.mean(np.asarray(values, dtype=float)))
    return means


def build_endface_face_angle_calibration_model(
    raw_angles: Dict[str, Dict[str, float]],
    truth_angles: Dict[str, Dict[str, float]],
    source_name: str,
) -> Dict[str, object]:
    offsets: Dict[str, Dict[str, float]] = {"head": {}, "tail": {}}
    for end in ["head", "tail"]:
        for face in ["A", "B", "C", "D"]:
            if face not in raw_angles.get(end, {}) or face not in truth_angles.get(end, {}):
                raise ValueError(f"Cannot calibrate missing end-face angle {end}-{face}")
            offsets[end][face] = truth_angles[end][face] - raw_angles[end][face]
    return {
        "version": 2,
        "model": "endface_face_angle_offset",
        "source": source_name,
        "note": "Each offset equals the mean of three direct gauge angles minus the raw visual face-to-end angle.",
        "angle_offsets_deg": offsets,
        "manual_truth_angles_deg": truth_angles,
    }


def apply_endface_face_angle_calibration_model(
    raw_angles: Dict[str, Dict[str, float]],
    model: Optional[Dict[str, object]],
) -> Dict[str, Dict[str, float]]:
    corrected = {end: dict(values) for end, values in raw_angles.items()}
    if not model or model.get("model") != "endface_face_angle_offset":
        return corrected
    offsets = model.get("angle_offsets_deg")
    if not isinstance(offsets, dict):
        return corrected
    for end in ["head", "tail"]:
        end_offsets = offsets.get(end, {})
        if not isinstance(end_offsets, dict):
            continue
        for face in ["A", "B", "C", "D"]:
            if face in corrected.get(end, {}):
                corrected[end][face] += float(end_offsets.get(face, 0.0))
    return corrected


def rod_axis_slopes(rod_axis: np.ndarray) -> Tuple[float, float]:
    axis = normalized_axis(rod_axis)
    if abs(float(axis[1])) <= 1e-12:
        return 0.0, 0.0
    return float(axis[0] / axis[1]), float(axis[2] / axis[1])


def build_endface_calibration_model(
    raw_fits: Dict[str, EndFaceFit],
    truth_fits: Dict[str, EndFaceFit],
    rod_axis: np.ndarray,
    source_name: str,
) -> Dict[str, object]:
    axis_x, axis_z = rod_axis_slopes(rod_axis)
    ends: Dict[str, Dict[str, float]] = {}
    for end in ["head", "tail"]:
        raw = raw_fits[end]
        truth = truth_fits[end]
        if not raw.valid or not truth.valid:
            raise ValueError(f"Cannot calibrate {end}: raw={raw.reason or 'ok'}, truth={truth.reason or 'ok'}")
        raw_residual_x = raw.slope_x + axis_x
        raw_residual_z = raw.slope_z + axis_z
        ends[end] = {
            "slope_offset_x": truth.slope_x - raw_residual_x,
            "slope_offset_z": truth.slope_z - raw_residual_z,
            "raw_residual_slope_x": raw_residual_x,
            "raw_residual_slope_z": raw_residual_z,
            "truth_slope_x": truth.slope_x,
            "truth_slope_z": truth.slope_z,
            "truth_verticality_deg": truth.verticality_deg,
        }
    return {
        "version": 1,
        "model": "endface_plane_slope_offset",
        "source": source_name,
        "note": "Offsets are fitted from signed 4-face x 3-position manual readings. Raw visual values remain in the measurement CSV.",
        "ends": ends,
    }


def apply_endface_calibration_model(
    raw_fit: EndFaceFit,
    rod_axis: np.ndarray,
    model: Optional[Dict[str, object]],
) -> EndFaceFit:
    if not raw_fit.valid or not model:
        return raw_fit
    ends = model.get("ends")
    if not isinstance(ends, dict):
        return raw_fit
    end_model = ends.get(raw_fit.end)
    if not isinstance(end_model, dict):
        return raw_fit
    axis_x, axis_z = rod_axis_slopes(rod_axis)
    residual_x = raw_fit.slope_x + axis_x + float(end_model.get("slope_offset_x", 0.0))
    residual_z = raw_fit.slope_z + axis_z + float(end_model.get("slope_offset_z", 0.0))
    corrected_slope_x = residual_x - axis_x
    corrected_slope_z = residual_z - axis_z
    normal = normalized_axis(np.array([-corrected_slope_x, 1.0, -corrected_slope_z], dtype=float))
    axis = normalized_axis(rod_axis)
    cosine = float(np.clip(abs(normal @ axis), 0.0, 1.0))
    return EndFaceFit(
        end=raw_fit.end,
        valid=True,
        slope_x=corrected_slope_x,
        slope_z=corrected_slope_z,
        intercept_y=raw_fit.intercept_y,
        normal_x=float(normal[0]),
        normal_y=float(normal[1]),
        normal_z=float(normal[2]),
        verticality_deg=math.degrees(math.acos(cosine)),
        point_count=raw_fit.point_count,
        inlier_count=raw_fit.inlier_count,
        rmse_mm=raw_fit.rmse_mm,
    )


def input_stem(args: argparse.Namespace) -> str:
    if args.input and os.path.isfile(args.input):
        return os.path.splitext(os.path.basename(args.input))[0]
    if args.input and os.path.isdir(args.input):
        return os.path.basename(os.path.abspath(args.input))
    if args.tifs:
        first = os.path.splitext(os.path.basename(args.tifs[0]))[0]
        folder = os.path.basename(os.path.dirname(os.path.abspath(args.tifs[0])))
        return folder or first or "tif_input"
    return "measurement"


def unique_path(path: str) -> str:
    if not os.path.exists(path):
        return path
    root, ext = os.path.splitext(path)
    for index in range(2, 10000):
        candidate = f"{root}_{index:03d}{ext}"
        if not os.path.exists(candidate):
            return candidate
    raise RuntimeError(f"Cannot find a free output file name for {path}")


def resolve_output_path(args: argparse.Namespace) -> str:
    stem = input_stem(args)
    if args.output:
        output = args.output
        if output.endswith(("\\", "/")) or os.path.isdir(output):
            path = os.path.join(output, f"{stem}_measure.csv")
        else:
            path = output
    else:
        path = os.path.join(
            os.path.dirname(os.path.abspath(__file__)),
            "results",
            "measurements",
            f"{stem}_measure.csv",
        )
    return path if args.overwrite else unique_path(path)


def write_csv(path: str, rows: List[Dict[str, object]]) -> None:
    os.makedirs(os.path.dirname(os.path.abspath(path)), exist_ok=True)
    fieldnames = [
        "record",
        "row",
        "y_mm",
        "valid",
        "measurement_valid",
        "drift_status",
        "drift_detected",
        "drift_correction_applied",
        "drift_model_version",
        "drift_amplitude",
        "drift_confidence",
        "drift_fit_rmse_mm",
        "drift_correlation",
        "drift_camera_amplitude_spread",
        "drift_alignment_shift_fraction",
        "drift_sample_station_count",
        "drift_overlap_start_fraction",
        "drift_overlap_end_fraction",
        "drift_warning",
        "drift_reason",
        "drift_raw_A_mm",
        "drift_raw_B_mm",
        "drift_raw_C_mm",
        "drift_raw_D_mm",
        "drift_raw_diag1_M1_M2_mm",
        "drift_raw_diag2_M3_M4_mm",
        "A_mm",
        "B_mm",
        "C_mm",
        "D_mm",
        "diag1_M1_M2_mm",
        "diag2_M3_M4_mm",
        "stick_length_mm",
        "drift_raw_head_endface_plane_verticality_deg",
        "drift_raw_tail_endface_plane_verticality_deg",
        "drift_raw_head_endface_verticality_deg",
        "drift_raw_tail_endface_verticality_deg",
        "drift_raw_head_A_endface_angle_deg",
        "drift_raw_head_B_endface_angle_deg",
        "drift_raw_head_C_endface_angle_deg",
        "drift_raw_head_D_endface_angle_deg",
        "drift_raw_tail_A_endface_angle_deg",
        "drift_raw_tail_B_endface_angle_deg",
        "drift_raw_tail_C_endface_angle_deg",
        "drift_raw_tail_D_endface_angle_deg",
        "head_endface_raw_verticality_deg",
        "tail_endface_raw_verticality_deg",
        "head_endface_plane_verticality_deg",
        "tail_endface_plane_verticality_deg",
        "head_endface_verticality_deg",
        "tail_endface_verticality_deg",
        "head_A_endface_raw_angle_deg",
        "head_B_endface_raw_angle_deg",
        "head_C_endface_raw_angle_deg",
        "head_D_endface_raw_angle_deg",
        "tail_A_endface_raw_angle_deg",
        "tail_B_endface_raw_angle_deg",
        "tail_C_endface_raw_angle_deg",
        "tail_D_endface_raw_angle_deg",
        "head_A_endface_angle_deg",
        "head_B_endface_angle_deg",
        "head_C_endface_angle_deg",
        "head_D_endface_angle_deg",
        "tail_A_endface_angle_deg",
        "tail_B_endface_angle_deg",
        "tail_C_endface_angle_deg",
        "tail_D_endface_angle_deg",
        "head_A_endface_truth_angle_deg",
        "head_B_endface_truth_angle_deg",
        "head_C_endface_truth_angle_deg",
        "head_D_endface_truth_angle_deg",
        "tail_A_endface_truth_angle_deg",
        "tail_B_endface_truth_angle_deg",
        "tail_C_endface_truth_angle_deg",
        "tail_D_endface_truth_angle_deg",
        "head_A_endface_difference_deg",
        "head_B_endface_difference_deg",
        "head_C_endface_difference_deg",
        "head_D_endface_difference_deg",
        "tail_A_endface_difference_deg",
        "tail_B_endface_difference_deg",
        "tail_C_endface_difference_deg",
        "tail_D_endface_difference_deg",
        "head_endface_truth_deg",
        "tail_endface_truth_deg",
        "head_endface_difference_deg",
        "tail_endface_difference_deg",
        "P1_x",
        "P1_z",
        "P2_x",
        "P2_z",
        "P3_x",
        "P3_z",
        "P4_x",
        "P4_z",
        "drift_raw_P1_x",
        "drift_raw_P1_z",
        "drift_raw_P2_x",
        "drift_raw_P2_z",
        "drift_raw_P3_x",
        "drift_raw_P3_z",
        "drift_raw_P4_x",
        "drift_raw_P4_z",
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
        "obj1_verticality_error_deg",
        "obj2_verticality_error_deg",
        "obj3_verticality_error_deg",
        "obj4_verticality_error_deg",
        "obj1_chamfer_mm",
        "obj2_chamfer_mm",
        "obj3_chamfer_mm",
        "obj4_chamfer_mm",
        "obj1_projection_x_mm",
        "obj1_projection_y_mm",
        "obj1_chamfer_face1_setback_mm",
        "obj1_chamfer_face2_setback_mm",
        "obj2_projection_x_mm",
        "obj2_projection_y_mm",
        "obj2_chamfer_face1_setback_mm",
        "obj2_chamfer_face2_setback_mm",
        "obj3_projection_x_mm",
        "obj3_projection_y_mm",
        "obj3_chamfer_face1_setback_mm",
        "obj3_chamfer_face2_setback_mm",
        "obj4_projection_x_mm",
        "obj4_projection_y_mm",
        "obj4_chamfer_face1_setback_mm",
        "obj4_chamfer_face2_setback_mm",
        "obj1_drift_x_mm",
        "obj1_drift_z_mm",
        "obj1_drift_amplitude",
        "obj2_drift_x_mm",
        "obj2_drift_z_mm",
        "obj2_drift_amplitude",
        "obj3_drift_x_mm",
        "obj3_drift_z_mm",
        "obj3_drift_amplitude",
        "obj4_drift_x_mm",
        "obj4_drift_z_mm",
        "obj4_drift_amplitude",
        "endface",
        "endface_fit_kind",
        "endface_slope_x",
        "endface_slope_z",
        "endface_intercept_y",
        "endface_normal_x",
        "endface_normal_y",
        "endface_normal_z",
        "endface_point_count",
        "endface_inlier_count",
        "endface_rmse_mm",
        "note",
    ]
    with open(path, "w", newline="", encoding="utf-8-sig") as f:
        writer = csv.DictWriter(f, fieldnames=fieldnames)
        writer.writeheader()
        writer.writerows(rows)


def endface_fit_record(fit: EndFaceFit, fit_kind: str) -> Dict[str, object]:
    record: Dict[str, object] = {
        "record": "endface_fit",
        "valid": fit.valid,
        "endface": fit.end,
        "endface_fit_kind": fit_kind,
        "endface_point_count": fit.point_count,
        "endface_inlier_count": fit.inlier_count,
        "note": fit.reason,
    }
    if fit.valid:
        record.update(
            {
                "endface_slope_x": round(fit.slope_x, 9),
                "endface_slope_z": round(fit.slope_z, 9),
                "endface_intercept_y": round(fit.intercept_y, 6),
                "endface_normal_x": round(fit.normal_x, 9),
                "endface_normal_y": round(fit.normal_y, 9),
                "endface_normal_z": round(fit.normal_z, 9),
                "endface_rmse_mm": round(fit.rmse_mm, 6),
            }
        )
        record[f"{fit.end}_endface_plane_verticality_deg"] = round(fit.verticality_deg, 6)
    return record


def add_summary_rows(rows: List[Dict[str, object]]) -> None:
    valid_rows = [r for r in rows if r.get("record") == "slice" and r.get("valid") is True]
    if not valid_rows:
        return
    metadata_columns = {
        "record", "row", "y_mm", "valid", "measurement_valid", "note",
        "drift_status", "drift_detected", "drift_correction_applied", "drift_model_version",
        "drift_warning", "drift_reason",
    }
    numeric_columns: List[str] = []
    for key in valid_rows[0]:
        if key in metadata_columns:
            continue
        values = [row.get(key) for row in valid_rows if row.get(key) not in ("", None)]
        if not values:
            continue
        try:
            [float(value) for value in values]
        except (TypeError, ValueError):
            continue
        numeric_columns.append(key)
    for stat in ["mean", "min", "max", "range", "std"]:
        out: Dict[str, object] = {"record": stat, "valid": True}
        for column in numeric_columns:
            raw_values = [r.get(column) for r in valid_rows if r.get(column) not in ("", None)]
            vals = np.array([float(v) for v in raw_values], dtype=float)
            vals = vals[np.isfinite(vals)]
            if vals.size == 0:
                continue
            if stat == "mean":
                value = float(np.mean(vals))
            elif stat == "min":
                value = float(np.min(vals))
            elif stat == "max":
                value = float(np.max(vals))
            elif stat == "range":
                value = float(np.max(vals) - np.min(vals))
            else:
                value = float(np.std(vals, ddof=0))
            out[column] = round(value, 6)
        if stat == "mean":
            for key in metadata_columns - {"record", "row", "y_mm", "valid", "note"}:
                out[key] = valid_rows[0].get(key, "")
        rows.append(out)


def main() -> int:
    parser = argparse.ArgumentParser(description="Measure square-rod side lengths from HOBJ or four TIFFs.")
    parser.add_argument("--input", help="Path to .hobj file or a folder containing object_1..object_4 TIFFs")
    parser.add_argument("--tifs", nargs=4, help="Four TIFF files in object order: obj1 obj2 obj3 obj4")
    parser.add_argument("--output", help="Output CSV path or directory. Defaults to tools/results/measurements/<hobj_name>_measure.csv")
    parser.add_argument("--overwrite", action="store_true", help="Allow overwriting an existing CSV")
    parser.add_argument("--calibration", help="Existing calibration JSON to reuse")
    parser.add_argument("--drift-calibration", help="Independent four-camera mechanical drift model JSON")
    parser.add_argument("--save-calibration", help="Save generated calibration JSON")
    parser.add_argument(
        "--orientation",
        choices=["auto", "normal", "turnover"],
        default="auto",
        help="Rod direction for measurement. auto recognises a 调头/diaotou path marker; production paths without a marker default to normal.",
    )
    parser.add_argument("--standard-json", help="Standard measurement JSON; defaults to the latest measured standard")
    parser.add_argument(
        "--calibration-truth-csv",
        help="Unified manual calibration CSV: cross_section A/B/C/D rows plus optional endface_angle rows",
    )
    parser.add_argument("--endface-truth-csv", help="Manual end-face truth CSV: direct angles or signed 24-point readings")
    parser.add_argument("--endface-calibration", help="Existing end-face slope-offset calibration JSON")
    parser.add_argument("--save-endface-calibration", help="Create an end-face calibration JSON from this input and manual truth CSV")
    parser.add_argument("--endface-window-mm", type=float, default=15.0, help="Longitudinal window used to find each physical end boundary")
    parser.add_argument("--step-mm", type=float, default=10.0, help="Slice spacing in mm along rod length")
    parser.add_argument("--step-rows", type=int, help="Slice spacing in rows; overrides --step-mm")
    parser.add_argument("--ignore-end-percent", type=float, default=0.05, help="Ignore this percent at both ends")
    parser.add_argument(
        "--geometry-mode",
        choices=["free", "rectangular"],
        default="free",
        help="Cross-section mode. free is the real four-corner measurement; rectangular is only a diagnostic comparison that forces opposite sides equal.",
    )
    parser.add_argument("--width", type=int, default=WIDTH)
    parser.add_argument("--height", type=int, default=HEIGHT)
    parser.add_argument("--x-scale", type=float, default=X_SCALE_MM)
    parser.add_argument("--y-scale", type=float, default=Y_SCALE_MM)
    parser.add_argument("--hobj-offsets", help="Optional four byte offsets: obj1,obj2,obj3,obj4")
    args = parser.parse_args()

    measurement_orientation = args.orientation
    if measurement_orientation == "auto":
        measurement_orientation = "turnover" if args.input and is_turnover_capture(args.input) else "normal"

    if not args.input and not args.tifs:
        parser.print_help()
        return 2
    if not args.calibration and not args.save_calibration:
        raise ValueError(
            "Provide --calibration for measurement. Use --save-calibration only when the input is the standard calibration bar."
        )
    if args.standard_json and args.calibration_truth_csv:
        raise ValueError("Use only one of --standard-json or --calibration-truth-csv")
    truth_csv_path = args.endface_truth_csv or args.calibration_truth_csv
    if args.save_endface_calibration and not truth_csv_path:
        raise ValueError("--save-endface-calibration requires --endface-truth-csv or --calibration-truth-csv")
    if args.endface_window_mm <= 0.0:
        raise ValueError("--endface-window-mm must be positive")

    source: Optional[HeightSource] = None
    common_start = 0
    common_end = 0
    valid_ranges: Dict[int, Tuple[int, int]] = {}
    calibration_captures: List[CalibrationCapture] = []

    if args.calibration:
        with open(args.calibration, "r", encoding="utf-8") as f:
            calibration = json.load(f)
        if args.input and os.path.isdir(args.input) and args.save_endface_calibration:
            for path, bar_id, capture_id in calibration_hobjs_from_folder(args.input):
                calibration_captures.append(
                    CalibrationCapture(
                        source=HobjSource(path, args.width, args.height),
                        capture_id=capture_id,
                        bar_id=bar_id,
                        orientation="turnover" if is_turnover_capture(path) else "normal",
                    )
                )
            source = calibration_captures[0].source
        else:
            source = detect_input(args)
        valid_ranges = source_valid_ranges(source)
        common_start, common_end = source_common_range(source, valid_ranges)
        print(
            "Using calibration: "
            f"{args.calibration} "
            f"(version={calibration.get('version')}, model={calibration.get('model', 'unknown')})"
        )
        if calibration_captures:
            print("End-face calibration captures:")
            for capture in calibration_captures:
                print(f"  {capture.capture_id}: bar_id={capture.bar_id}")
    else:
        standards_by_bar = load_standard_truth_csv(args.calibration_truth_csv) if args.calibration_truth_csv else None
        standard = load_standard(args.standard_json)
        if args.input and os.path.isdir(args.input):
            discovered = calibration_hobjs_from_folder(args.input)
            for path, bar_id, capture_id in discovered:
                orientation = "turnover" if is_turnover_capture(path) else "normal"
                calibration_captures.append(
                    CalibrationCapture(
                        source=HobjSource(path, args.width, args.height),
                        capture_id=capture_id,
                        bar_id=bar_id,
                        orientation=orientation,
                    )
                )
            if standards_by_bar is not None:
                standard = standard_for_bar(standards_by_bar, calibration_captures[0].bar_id)
            calibration = build_calibration_from_captures(calibration_captures, standard, standards_by_bar)
            # Use the first calibration source to produce an immediate CSV sanity check.
            source = calibration_captures[0].source
            valid_ranges = source_valid_ranges(source)
            common_start, common_end = source_common_range(source, valid_ranges)
            print("Calibration captures:")
            for capture in calibration_captures:
                print(
                    f"  {capture.capture_id}: bar_id={capture.bar_id}, "
                    f"orientation={capture.orientation}"
                )
            for missing_bar in calibration.get("unused_truth_bar_ids", []):
                print(f"  SKIP truth bar without HOBJ folder: {missing_bar}")
        else:
            source = detect_input(args)
            valid_ranges = source_valid_ranges(source)
            common_start, common_end = source_common_range(source, valid_ranges)
            if standards_by_bar is not None:
                standard = standard_for_bar(standards_by_bar, input_stem(args))
            calibration = build_calibration(source, common_start, common_end, standard)
        if args.save_calibration:
            os.makedirs(os.path.dirname(os.path.abspath(args.save_calibration)), exist_ok=True)
            with open(args.save_calibration, "w", encoding="utf-8") as f:
                json.dump(calibration, f, ensure_ascii=False, indent=2)

    if calibration_captures:
        measurement_orientation = calibration_captures[0].orientation
    active_calibration = calibration_for_orientation(calibration, measurement_orientation)
    transforms = {int(k): v for k, v in active_calibration["transforms"].items()}
    if source is None:
        raise RuntimeError("No measurement source was loaded")
    if not valid_ranges:
        valid_ranges = source_valid_ranges(source)

    drift_model: Optional[Dict[str, object]] = None
    drift_result: Dict[str, object] = {
        "status": "not_configured",
        "detected": False,
        "correction_applied": False,
        "valid": True,
        "confidence": 1.0,
        "model_version": "",
        "amplitude": 0.0,
        "fit_rmse_mm": 0.0,
        "normal_rmse_mm": 0.0,
        "correlation": 0.0,
        "camera_amplitude_spread": 0.0,
        "alignment_shift_fraction": 0.0,
        "sample_station_count": 0,
        "overlap_start_fraction": 0.0,
        "overlap_end_fraction": 0.0,
        "warning": False,
        "reason": "drift_model_not_configured",
        "per_camera_amplitude": {str(obj): 0.0 for obj in (1, 2, 3, 4)},
    }
    if args.drift_calibration:
        with open(args.drift_calibration, "r", encoding="utf-8") as handle:
            loaded_drift_model = json.load(handle)
        if loaded_drift_model.get("model") != "four_camera_longitudinal_local_drift":
            raise ValueError(f"Unsupported mechanical drift model: {loaded_drift_model.get('model')}")
        model_orientation = str(loaded_drift_model.get("orientation", "normal"))
        if model_orientation != measurement_orientation:
            drift_result["status"] = "not_applicable_orientation"
            drift_result["model_version"] = int(loaded_drift_model.get("version", 0))
            drift_result["reason"] = "drift_model_orientation_not_applicable"
        else:
            drift_model = loaded_drift_model
            model_fractions = [float(value) for value in drift_model["sample_fractions"]]
            reference_start = drift_model.get("reference_common_start_row")
            reference_end = drift_model.get("reference_common_end_row")
            if reference_start is not None and reference_end is not None and int(reference_end) > int(reference_start):
                reference_start = int(reference_start)
                reference_end = int(reference_end)
                selected_fractions = [
                    fraction
                    for fraction in model_fractions
                    if common_start <= reference_start + fraction * (reference_end - reference_start) <= common_end
                ]
            else:
                reference_start = None
                reference_end = None
                selected_fractions = model_fractions
            if len(selected_fractions) < 5:
                drift_result.update({
                    "status": "unmatched_unadjusted",
                    "valid": True,
                    "warning": True,
                    "reason": "insufficient_absolute_scan_overlap",
                    "model_version": int(drift_model.get("version", 0)),
                    "sample_station_count": len(selected_fractions),
                })
            else:
                profile = sample_local_corner_profile(
                    source,
                    common_start,
                    common_end,
                    selected_fractions,
                    args.x_scale,
                    reference_start,
                    reference_end,
                )
                drift_result = classify_mechanical_drift(drift_model, profile, selected_fractions)
                drift_result["sample_station_count"] = len(selected_fractions)
                drift_result["overlap_start_fraction"] = selected_fractions[0]
                drift_result["overlap_end_fraction"] = selected_fractions[-1]
            print(
                "Mechanical drift: "
                f"status={drift_result['status']}, amplitude={drift_result['amplitude']:.6f}, "
                f"correlation={drift_result['correlation']:.6f}, "
                f"fit_rmse={drift_result['fit_rmse_mm']:.6f} mm"
            )

    margin = int(round(args.ignore_end_percent * (common_end - common_start)))
    first_row = common_start + margin
    last_row = common_end - margin
    step_rows = args.step_rows if args.step_rows else max(1, int(round(args.step_mm / args.y_scale)))

    stick_length_mm = round((common_end - common_start) * args.y_scale, 6)
    output_rows: List[Dict[str, object]] = []
    raw_geometry_rows: List[Dict[str, object]] = []
    measurement_valid = bool(drift_result.get("valid", True))
    drift_amplitude = float(drift_result.get("amplitude", 0.0))
    drift_alignment_shift = float(drift_result.get("alignment_shift_fraction", 0.0))
    apply_drift = bool(drift_result.get("correction_applied", False)) and drift_model is not None
    for row_index in range(first_row, last_row + 1, step_rows):
        raw_corners = {obj: extract_corner_from_row(source.row(obj, row_index), args.x_scale) for obj in [1, 2, 3, 4]}
        valid = all(c.valid for c in raw_corners.values())
        fraction = (
            drift_fraction_for_row(drift_model, row_index, common_start, common_end)
            if drift_model is not None
            else (row_index - common_start) / (common_end - common_start)
        )
        record: Dict[str, object] = {
            "record": "slice",
            "row": row_index,
            "y_mm": round((row_index - common_start) * args.y_scale, 6),
            "valid": valid,
            "measurement_valid": valid and measurement_valid,
            "stick_length_mm": stick_length_mm,
            "drift_status": drift_result.get("status", "not_configured"),
            "drift_detected": bool(drift_result.get("detected", False)),
            "drift_correction_applied": apply_drift,
            "drift_model_version": drift_result.get("model_version", ""),
            "drift_amplitude": round(drift_amplitude, 6),
            "drift_confidence": round(float(drift_result.get("confidence", 0.0)), 6),
            "drift_fit_rmse_mm": round(float(drift_result.get("fit_rmse_mm", 0.0)), 6),
            "drift_correlation": round(float(drift_result.get("correlation", 0.0)), 6),
            "drift_camera_amplitude_spread": round(float(drift_result.get("camera_amplitude_spread", 0.0)), 6),
            "drift_alignment_shift_fraction": round(drift_alignment_shift, 6),
            "drift_sample_station_count": int(drift_result.get("sample_station_count", 0)),
            "drift_overlap_start_fraction": round(float(drift_result.get("overlap_start_fraction", 0.0)), 6),
            "drift_overlap_end_fraction": round(float(drift_result.get("overlap_end_fraction", 0.0)), 6),
            "drift_warning": bool(drift_result.get("warning", False)),
            "drift_reason": str(drift_result.get("reason", "")),
        }
        raw_geometry_record: Dict[str, object] = {
            "record": "slice",
            "row": row_index,
            "y_mm": record["y_mm"],
            "valid": valid,
        }
        if valid:
            raw_points, raw_midpoints, raw_lengths = cross_section_geometry(
                raw_corners, transforms, active_calibration, args.geometry_mode
            )
            for edge in ["A", "B", "C", "D"]:
                record[f"drift_raw_{edge}_mm"] = round(raw_lengths[edge], 6)
            for point_name in ["P1", "P2", "P3", "P4"]:
                px, pz = raw_points[point_name]
                record[f"drift_raw_{point_name}_x"] = round(px, 6)
                record[f"drift_raw_{point_name}_z"] = round(pz, 6)
                raw_geometry_record[f"{point_name}_x"] = px
                raw_geometry_record[f"{point_name}_z"] = pz
            record["drift_raw_diag1_M1_M2_mm"] = round(raw_lengths["diag1"], 6)
            record["drift_raw_diag2_M3_M4_mm"] = round(raw_lengths["diag2"], 6)

            corrected_corners: Dict[int, CornerResult] = {}
            for obj, point_name in OBJECT_TO_POINT.items():
                if drift_model is not None:
                    shift_x, shift_z = mechanical_drift_local_shift(
                        drift_model, obj, fraction + drift_alignment_shift, drift_amplitude
                    )
                else:
                    shift_x, shift_z = 0.0, 0.0
                record[f"obj{obj}_drift_x_mm"] = round(shift_x, 6)
                record[f"obj{obj}_drift_z_mm"] = round(shift_z, 6)
                camera_amplitude = drift_result.get("per_camera_amplitude", {}).get(str(obj), 0.0)
                record[f"obj{obj}_drift_amplitude"] = round(float(camera_amplitude), 6)
                corrected_corners[obj] = (
                    subtract_local_drift(raw_corners[obj], shift_x, shift_z)
                    if apply_drift
                    else raw_corners[obj]
                )

            if measurement_valid:
                points, midpoints, lengths = cross_section_geometry(
                    corrected_corners, transforms, active_calibration, args.geometry_mode
                )
                for edge in ["A", "B", "C", "D"]:
                    record[f"{edge}_mm"] = round(lengths[edge], 6)
                for point_name in ["P1", "P2", "P3", "P4"]:
                    px, pz = points[point_name]
                    record[f"{point_name}_x"] = round(px, 6)
                    record[f"{point_name}_z"] = round(pz, 6)
                record["diag1_M1_M2_mm"] = round(lengths["diag1"], 6)
                record["diag2_M3_M4_mm"] = round(lengths["diag2"], 6)
                for midpoint_name in ["M1", "M2", "M3", "M4"]:
                    mx, mz = midpoints[midpoint_name]
                    record[f"{midpoint_name}_x"] = round(mx, 6)
                    record[f"{midpoint_name}_z"] = round(mz, 6)
        else:
            record["note"] = "; ".join(f"obj{obj}:{c.reason}" for obj, c in raw_corners.items() if not c.valid)

        for obj in [1, 2, 3, 4]:
            corner = raw_corners[obj]
            record[f"obj{obj}_angle_deg"] = round(corner.angle_deg, 6) if corner.valid else ""
            record[f"obj{obj}_verticality_error_deg"] = round(corner.angle_deg, 6) if corner.valid else ""
            record[f"obj{obj}_chamfer_mm"] = round(corner.chamfer_mm, 6) if corner.valid and not math.isnan(corner.chamfer_mm) else ""
            record[f"obj{obj}_projection_x_mm"] = round(corner.projection_x_mm, 6) if corner.valid and not math.isnan(corner.projection_x_mm) else ""
            record[f"obj{obj}_projection_y_mm"] = round(corner.projection_y_mm, 6) if corner.valid and not math.isnan(corner.projection_y_mm) else ""
            record[f"obj{obj}_chamfer_face1_setback_mm"] = round(corner.chamfer_face1_setback_mm, 6) if corner.valid and not math.isnan(corner.chamfer_face1_setback_mm) else ""
            record[f"obj{obj}_chamfer_face2_setback_mm"] = round(corner.chamfer_face2_setback_mm, 6) if corner.valid and not math.isnan(corner.chamfer_face2_setback_mm) else ""
        output_rows.append(record)
        raw_geometry_rows.append(raw_geometry_record)

    raw_rod_axis = fit_rod_axis(raw_geometry_rows)
    pre_drift_endfaces = fit_endfaces_from_source(
        source,
        valid_ranges,
        transforms,
        active_calibration,
        common_start,
        raw_rod_axis,
        args.x_scale,
        args.y_scale,
        args.endface_window_mm,
    )
    rod_axis = fit_rod_axis(output_rows) if measurement_valid else raw_rod_axis
    raw_endfaces = fit_endfaces_from_source(
        source,
        valid_ranges,
        transforms,
        active_calibration,
        common_start,
        rod_axis,
        args.x_scale,
        args.y_scale,
        args.endface_window_mm,
        common_end=common_end,
        drift_model=drift_model if apply_drift else None,
        drift_amplitude=drift_amplitude,
        drift_alignment_shift_fraction=drift_alignment_shift,
    )

    endface_model: Optional[Dict[str, object]] = None
    if args.endface_calibration:
        with open(args.endface_calibration, "r", encoding="utf-8") as handle:
            loaded_model = json.load(handle)
        if loaded_model.get("model") not in {"endface_plane_slope_offset", "endface_face_angle_offset", "endface_face_angle_offset_by_orientation"}:
            raise ValueError(f"Unsupported end-face calibration model: {loaded_model.get('model')}")
        endface_model = endface_calibration_for_orientation(loaded_model, measurement_orientation)

    pre_drift_section_points = mean_cross_section_points(raw_geometry_rows)
    pre_drift_face_angles = {
        end: face_to_endface_angles(pre_drift_section_points, pre_drift_endfaces[end], raw_rod_axis)
        for end in ["head", "tail"]
    }
    section_points = mean_cross_section_points(output_rows) if measurement_valid else pre_drift_section_points
    raw_face_angles = {
        end: face_to_endface_angles(section_points, raw_endfaces[end], rod_axis)
        for end in ["head", "tail"]
    }
    truth_endfaces: Dict[str, EndFaceFit] = {}
    truth_face_angles: Dict[str, Dict[str, float]] = {}
    truth_bar_id = (
        calibration_captures[0].bar_id
        if calibration_captures
        else os.path.splitext(os.path.basename(getattr(source, "path", "")))[0] or input_stem(args)
    )
    if truth_csv_path:
        truth_face_angles = read_manual_endface_angle_truth(truth_csv_path, truth_bar_id)
        if not truth_face_angles:
            truth_endfaces = read_manual_endface_truth(truth_csv_path, section_points, truth_bar_id)

    if args.save_endface_calibration:
        if calibration_captures and truth_face_angles:
            endface_model = build_balanced_endface_angle_calibration_model(
                calibration_captures,
                calibration,
                truth_csv_path,
                args.x_scale,
                args.y_scale,
                args.endface_window_mm,
            )
        elif truth_face_angles:
            endface_model = build_endface_face_angle_calibration_model(
                raw_face_angles,
                truth_face_angles,
                truth_bar_id,
            )
        else:
            endface_model = build_endface_calibration_model(
                raw_endfaces,
                truth_endfaces,
                rod_axis,
                truth_bar_id,
            )
        os.makedirs(os.path.dirname(os.path.abspath(args.save_endface_calibration)), exist_ok=True)
        with open(args.save_endface_calibration, "w", encoding="utf-8") as handle:
            json.dump(endface_model, handle, ensure_ascii=False, indent=2)

    corrected_endfaces = {
        end: apply_endface_calibration_model(raw_endfaces[end], rod_axis, endface_model)
        for end in ["head", "tail"]
    }
    corrected_face_angles = {
        end: face_to_endface_angles(section_points, corrected_endfaces[end], rod_axis)
        for end in ["head", "tail"]
    }
    corrected_face_angles = apply_endface_face_angle_calibration_model(corrected_face_angles, endface_model)
    for row in output_rows:
        if row.get("record") != "slice":
            continue
        for end in ["head", "tail"]:
            pre_drift_fit = pre_drift_endfaces[end]
            raw_fit = raw_endfaces[end]
            corrected_fit = corrected_endfaces[end]
            truth_fit = truth_endfaces.get(end)
            if pre_drift_fit.valid:
                row[f"drift_raw_{end}_endface_plane_verticality_deg"] = round(pre_drift_fit.verticality_deg, 6)
            pre_drift_error = mean_face_endface_angle(pre_drift_face_angles[end])
            if math.isfinite(pre_drift_error):
                row[f"drift_raw_{end}_endface_verticality_deg"] = round(pre_drift_error, 6)
            for face, angle in pre_drift_face_angles[end].items():
                row[f"drift_raw_{end}_{face}_endface_angle_deg"] = round(angle, 6)
            if raw_fit.valid:
                row[f"{end}_endface_plane_verticality_deg"] = round(raw_fit.verticality_deg, 6)
            raw_error = mean_face_endface_angle(raw_face_angles[end])
            if math.isfinite(raw_error):
                row[f"{end}_endface_raw_verticality_deg"] = round(raw_error, 6)
            for face, angle in raw_face_angles[end].items():
                row[f"{end}_{face}_endface_raw_angle_deg"] = round(angle, 6)
            if corrected_fit.valid and measurement_valid:
                corrected_error = mean_face_endface_angle(corrected_face_angles[end])
                if math.isfinite(corrected_error):
                    row[f"{end}_endface_verticality_deg"] = round(corrected_error, 6)
            if measurement_valid:
                for face, angle in corrected_face_angles[end].items():
                    row[f"{end}_{face}_endface_angle_deg"] = round(angle, 6)
            if end in truth_face_angles and measurement_valid:
                truth_error = mean_face_endface_angle(truth_face_angles[end])
                if math.isfinite(truth_error):
                    row[f"{end}_endface_truth_deg"] = round(truth_error, 6)
                    corrected_error = mean_face_endface_angle(corrected_face_angles[end])
                    if math.isfinite(corrected_error):
                        row[f"{end}_endface_difference_deg"] = round(corrected_error - truth_error, 6)
                for face, truth_angle in truth_face_angles[end].items():
                    row[f"{end}_{face}_endface_truth_angle_deg"] = round(truth_angle, 6)
                    if face in corrected_face_angles[end]:
                        row[f"{end}_{face}_endface_difference_deg"] = round(
                            corrected_face_angles[end][face] - truth_angle,
                            6,
                        )
            if truth_fit and truth_fit.valid and measurement_valid:
                row[f"{end}_endface_truth_deg"] = round(truth_fit.verticality_deg, 6)
                if corrected_fit.valid:
                    row[f"{end}_endface_difference_deg"] = round(
                        corrected_fit.verticality_deg - truth_fit.verticality_deg,
                        6,
                    )

    add_summary_rows(output_rows)
    for end in ["head", "tail"]:
        output_rows.append(endface_fit_record(raw_endfaces[end], "raw_visual"))
        if endface_model and endface_model.get("model") == "endface_plane_slope_offset":
            output_rows.append(endface_fit_record(corrected_endfaces[end], "corrected_visual"))
        if end in truth_endfaces:
            output_rows.append(endface_fit_record(truth_endfaces[end], "manual_truth"))
    output_path = resolve_output_path(args)
    write_csv(output_path, output_rows)
    print(f"CSV written: {output_path}")
    print(f"Common valid row range: {common_start}..{common_end}")
    print(f"Measured rows: {first_row}..{last_row}, step_rows={step_rows}")
    if not args.calibration and args.save_calibration:
        print(f"Calibration written: {args.save_calibration}")
    if args.save_endface_calibration:
        print(f"End-face calibration written: {args.save_endface_calibration}")
    for end in ["head", "tail"]:
        raw_fit = raw_endfaces[end]
        if raw_fit.valid:
            print(
                f"{end.capitalize()} end-face raw plane/axis deviation: {raw_fit.verticality_deg:.6f} deg, "
                f"points={raw_fit.inlier_count}/{raw_fit.point_count}, rmse={raw_fit.rmse_mm:.6f} mm"
            )
        else:
            print(f"{end.capitalize()} end-face raw unavailable: {raw_fit.reason}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
