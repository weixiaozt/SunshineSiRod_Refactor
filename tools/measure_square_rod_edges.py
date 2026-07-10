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
from dataclasses import dataclass
from typing import Dict, Iterable, List, Optional, Tuple

import numpy as np
from PIL import Image


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
    projection_z_mm: float = math.nan
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


class HeightSource:
    width: int
    height: int

    def row(self, obj: int, row_index: int) -> np.ndarray:
        raise NotImplementedError


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
    projection_z = math.nan
    chamfer_mid_x = math.nan
    chamfer_mid_z = math.nan
    if idx.size > 1:
        groups: List[np.ndarray] = np.split(idx, np.where(np.diff(idx) > 1)[0] + 1)
        groups.sort(key=lambda g: abs(float(np.mean(x[g])) - vx))
        g = groups[0]
        t1, t2 = int(g[0]), int(g[-1])
        chamfer = float(math.hypot(x[t2] - x[t1], zs[t2] - zs[t1]))
        projection_x = float(abs(x[t2] - x[t1]))
        projection_z = float(abs(zs[t2] - zs[t1]))
        chamfer_mid_x = float((x[t1] + x[t2]) * 0.5)
        chamfer_mid_z = float((zs[t1] + zs[t2]) * 0.5)

    return CornerResult(
        True,
        vx=float(vx),
        vz=float(vz),
        angle_deg=float(angle),
        chamfer_mm=chamfer,
        projection_x_mm=projection_x,
        projection_z_mm=projection_z,
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
    if orientation == "swap_bd":
        out["B"], out["D"] = out["D"], out["B"]
    return out


def source_common_range(source: HeightSource) -> Tuple[int, int]:
    ranges = {obj: find_valid_row_range(source, obj) for obj in [1, 2, 3, 4]}
    common_start = max(r[0] for r in ranges.values())
    common_end = min(r[1] for r in ranges.values())
    if common_end <= common_start:
        raise RuntimeError(f"No common valid row range across four objects: {ranges}")
    return common_start, common_end


def calibration_hobjs_from_folder(folder: str) -> List[str]:
    hobjs = [
        os.path.join(folder, name)
        for name in os.listdir(folder)
        if name.lower().endswith(".hobj")
    ]
    if not hobjs:
        raise ValueError(f"No .hobj files found in calibration folder: {folder}")
    return sorted(hobjs, key=lambda p: (is_turnover_capture(p), os.path.basename(p)))


def is_turnover_capture(path: str) -> bool:
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


def average_orthogonal_matrices(matrices: List[np.ndarray]) -> np.ndarray:
    mean = np.mean(np.stack(matrices), axis=0)
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


def build_calibration_from_captures(
    captures: List[Tuple[HeightSource, str, str]],
    standard: Dict[str, Dict[str, float]],
) -> Dict:
    samples: Dict[int, List[Tuple[CornerResult, Tuple[float, float]]]] = {1: [], 2: [], 3: [], 4: []}
    calibration_records: List[Tuple[Dict[int, CornerResult], Dict[str, Tuple[float, float]]]] = []
    calibration_rows: Dict[str, Dict[str, int]] = {}
    common_ranges: Dict[str, List[int]] = {}

    for source, capture_name, orientation in captures:
        common_start, common_end = source_common_range(source)
        common_ranges[capture_name] = [common_start, common_end]
        calibration_rows[capture_name] = {}
        for name, item in standard.items():
            row = int(round(common_start + item["percent"] * (common_end - common_start)))
            calibration_rows[capture_name][name] = row
            points = target_points_from_edges(orientation_edges(item, orientation))
            record_corners: Dict[int, CornerResult] = {}
            for obj in [1, 2, 3, 4]:
                corner = extract_corner_from_row(source.row(obj, row))
                if not corner.valid:
                    raise RuntimeError(f"Calibration failed at {capture_name}/{name}, object {obj}: {corner.reason}")
                samples[obj].append((corner, points[OBJECT_TO_POINT[obj]]))
                record_corners[obj] = corner
            calibration_records.append((record_corners, points))

    transforms = {}
    for obj in [1, 2, 3, 4]:
        matrices: List[np.ndarray] = []
        direction_errors: List[float] = []
        for corner, _ in samples[obj]:
            matrix, error = solve_direction_matrix(local_face_directions(corner), EXPECTED_FACE_DIRECTIONS[obj])
            matrices.append(matrix)
            direction_errors.append(error)
        matrix = average_orthogonal_matrices(matrices)
        translations = []
        for corner, target in samples[obj]:
            mapped = matrix @ np.array([corner.vx, corner.vz], dtype=float)
            translations.append((target[0] - mapped[0], target[1] - mapped[1]))
        tx = float(np.mean([t[0] for t in translations]))
        tz = float(np.mean([t[1] for t in translations]))
        transforms[str(obj)] = {
            "matrix": matrix.tolist(),
            "translation": [tx, tz],
            "direction_error_mean": float(np.mean(direction_errors)),
        }

    transform_objects = {int(k): v for k, v in transforms.items()}
    residuals: Dict[str, List[Tuple[float, float]]] = {p: [] for p in ["P1", "P2", "P3", "P4"]}
    for record_corners, target_points in calibration_records:
        raw_points = {
            OBJECT_TO_POINT[obj]: apply_transform(transform_objects[obj], record_corners[obj].vx, record_corners[obj].vz)
            for obj in [1, 2, 3, 4]
        }
        reconstructed = reconstruct_points_from_global_sides(record_corners, transform_objects, raw_points)
        for point_name in ["P1", "P2", "P3", "P4"]:
            target = target_points[point_name]
            actual = reconstructed[point_name]
            residuals[point_name].append((target[0] - actual[0], target[1] - actual[1]))
    corner_biases = {
        point_name: [
            float(np.mean([r[0] for r in values])),
            float(np.mean([r[1] for r in values])),
        ]
        for point_name, values in residuals.items()
    }

    return {
        "version": 6,
        "model": "camera_oriented_transform_with_corner_bias",
        "note": "Calibration fixes camera-to-global orientation/translation and per-corner residual bias from standard-bar captures. Default measurement is free four-corner geometry: A/C and B/D are not forced or range-limited.",
        "mapping": {"1": "P1/top", "3": "P3/left", "2": "P2/down", "4": "P4/right"},
        "common_ranges": common_ranges,
        "calibration_rows": calibration_rows,
        "standard": standard,
        "transforms": transforms,
        "corner_biases": corner_biases,
    }


def build_calibration(
    source: HeightSource,
    common_start: int,
    common_end: int,
    standard: Dict[str, Dict[str, float]],
) -> Dict:
    return build_calibration_from_captures([(source, "single", "normal")], standard)


def load_standard(path: Optional[str]) -> Dict[str, Dict[str, float]]:
    if not path:
        return DEFAULT_STANDARD
    with open(path, "r", encoding="utf-8") as f:
        data = json.load(f)
    return data


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
        path = os.path.join(os.path.dirname(os.path.abspath(__file__)), f"{stem}_measure.csv")
    return path if args.overwrite else unique_path(path)


def write_csv(path: str, rows: List[Dict[str, object]]) -> None:
    os.makedirs(os.path.dirname(os.path.abspath(path)), exist_ok=True)
    fieldnames = [
        "record",
        "row",
        "y_mm",
        "valid",
        "A_mm",
        "B_mm",
        "C_mm",
        "D_mm",
        "diag1_M1_M2_mm",
        "diag2_M3_M4_mm",
        "stick_length_mm",
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
        "obj1_verticality_error_deg",
        "obj2_verticality_error_deg",
        "obj3_verticality_error_deg",
        "obj4_verticality_error_deg",
        "obj1_chamfer_mm",
        "obj2_chamfer_mm",
        "obj3_chamfer_mm",
        "obj4_chamfer_mm",
        "obj1_projection_x_mm",
        "obj1_projection_z_mm",
        "obj2_projection_x_mm",
        "obj2_projection_z_mm",
        "obj3_projection_x_mm",
        "obj3_projection_z_mm",
        "obj4_projection_x_mm",
        "obj4_projection_z_mm",
        "note",
    ]
    with open(path, "w", newline="", encoding="utf-8-sig") as f:
        writer = csv.DictWriter(f, fieldnames=fieldnames)
        writer.writeheader()
        writer.writerows(rows)


def add_summary_rows(rows: List[Dict[str, object]]) -> None:
    valid_rows = [r for r in rows if r.get("record") == "slice" and r.get("valid") is True]
    if not valid_rows:
        return
    skip_columns = {"record", "row", "y_mm", "valid", "note"}
    numeric_columns = [
        key
        for key in valid_rows[0].keys()
        if key not in skip_columns
        and any(r.get(key) not in ("", None) for r in valid_rows)
    ]
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
        rows.append(out)


def main() -> int:
    parser = argparse.ArgumentParser(description="Measure square-rod side lengths from HOBJ or four TIFFs.")
    parser.add_argument("--input", help="Path to .hobj file or a folder containing object_1..object_4 TIFFs")
    parser.add_argument("--tifs", nargs=4, help="Four TIFF files in object order: obj1 obj2 obj3 obj4")
    parser.add_argument("--output", help="Output CSV path or directory. Defaults to tools/<hobj_name>_measure.csv")
    parser.add_argument("--overwrite", action="store_true", help="Allow overwriting an existing CSV")
    parser.add_argument("--calibration", help="Existing calibration JSON to reuse")
    parser.add_argument("--save-calibration", help="Save generated calibration JSON")
    parser.add_argument("--standard-json", help="Standard measurement JSON; defaults to the latest measured standard")
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

    if not args.input and not args.tifs:
        parser.print_help()
        return 2
    if not args.calibration and not args.save_calibration:
        raise ValueError(
            "Provide --calibration for measurement. Use --save-calibration only when the input is the standard calibration bar."
        )

    source: Optional[HeightSource] = None
    common_start = 0
    common_end = 0

    if args.calibration:
        source = detect_input(args)
        common_start, common_end = source_common_range(source)
        with open(args.calibration, "r", encoding="utf-8") as f:
            calibration = json.load(f)
        print(
            "Using calibration: "
            f"{args.calibration} "
            f"(version={calibration.get('version')}, model={calibration.get('model', 'unknown')})"
        )
    else:
        standard = load_standard(args.standard_json)
        if args.input and os.path.isdir(args.input):
            hobj_paths = calibration_hobjs_from_folder(args.input)
            captures = []
            for path in hobj_paths:
                name = os.path.splitext(os.path.basename(path))[0]
                orientation = "swap_bd" if is_turnover_capture(path) else "normal"
                captures.append((HobjSource(path, args.width, args.height), name, orientation))
            calibration = build_calibration_from_captures(captures, standard)
            # Use the first calibration source to produce an immediate CSV sanity check.
            source = captures[0][0]
            common_start, common_end = source_common_range(source)
            print("Calibration captures:")
            for _, name, orientation in captures:
                print(f"  {name}: orientation={orientation}")
        else:
            source = detect_input(args)
            common_start, common_end = source_common_range(source)
            calibration = build_calibration(source, common_start, common_end, standard)
        if args.save_calibration:
            os.makedirs(os.path.dirname(os.path.abspath(args.save_calibration)), exist_ok=True)
            with open(args.save_calibration, "w", encoding="utf-8") as f:
                json.dump(calibration, f, ensure_ascii=False, indent=2)

    transforms = {int(k): v for k, v in calibration["transforms"].items()}
    if source is None:
        raise RuntimeError("No measurement source was loaded")

    margin = int(round(args.ignore_end_percent * (common_end - common_start)))
    first_row = common_start + margin
    last_row = common_end - margin
    step_rows = args.step_rows if args.step_rows else max(1, int(round(args.step_mm / args.y_scale)))

    stick_length_mm = round((common_end - common_start) * args.y_scale, 6)
    output_rows: List[Dict[str, object]] = []
    for row_index in range(first_row, last_row + 1, step_rows):
        corners = {obj: extract_corner_from_row(source.row(obj, row_index), args.x_scale) for obj in [1, 2, 3, 4]}
        valid = all(c.valid for c in corners.values())
        record: Dict[str, object] = {
            "record": "slice",
            "row": row_index,
            "y_mm": round((row_index - common_start) * args.y_scale, 6),
            "valid": valid,
            "stick_length_mm": stick_length_mm,
        }
        if valid:
            points: Dict[str, Tuple[float, float]] = {}
            for obj, point_name in OBJECT_TO_POINT.items():
                c = corners[obj]
                points[point_name] = apply_transform(transforms[obj], c.vx, c.vz)
            points = reconstruct_points_from_global_sides(corners, transforms, points)
            points = apply_corner_biases(points, calibration)
            free_points = points
            if args.geometry_mode == "rectangular":
                points = fuse_rectangular_cross_section(points)
            point_deltas = {
                name: (points[name][0] - free_points[name][0], points[name][1] - free_points[name][1])
                for name in ["P1", "P2", "P3", "P4"]
            }
            lengths = edge_lengths(points)
            for edge in ["A", "B", "C", "D"]:
                record[f"{edge}_mm"] = round(lengths[edge], 6)
            for point_name in ["P1", "P2", "P3", "P4"]:
                px, pz = points[point_name]
                record[f"{point_name}_x"] = round(px, 6)
                record[f"{point_name}_z"] = round(pz, 6)
            midpoints: Dict[str, Tuple[float, float]] = {}
            for obj, point_name in OBJECT_TO_POINT.items():
                corner = corners[obj]
                midpoint_name = "M" + point_name[1:]
                if math.isnan(corner.chamfer_mid_x) or math.isnan(corner.chamfer_mid_z):
                    # If no chamfer segment is found, fall back to the theoretical corner.
                    midpoints[midpoint_name] = points[point_name]
                else:
                    raw_midpoint = apply_transform(
                        transforms[obj],
                        corner.chamfer_mid_x,
                        corner.chamfer_mid_z,
                    )
                    delta = point_deltas[point_name]
                    midpoints[midpoint_name] = (
                        raw_midpoint[0] + delta[0],
                        raw_midpoint[1] + delta[1],
                    )
            record["diag1_M1_M2_mm"] = round(two_point_distance(midpoints, "M1", "M2"), 6)
            record["diag2_M3_M4_mm"] = round(two_point_distance(midpoints, "M3", "M4"), 6)
            for midpoint_name in ["M1", "M2", "M3", "M4"]:
                mx, mz = midpoints[midpoint_name]
                record[f"{midpoint_name}_x"] = round(mx, 6)
                record[f"{midpoint_name}_z"] = round(mz, 6)
        else:
            record["note"] = "; ".join(f"obj{obj}:{c.reason}" for obj, c in corners.items() if not c.valid)

        for obj in [1, 2, 3, 4]:
            record[f"obj{obj}_angle_deg"] = round(corners[obj].angle_deg, 6) if corners[obj].valid else ""
            record[f"obj{obj}_verticality_error_deg"] = round(abs(90.0 - corners[obj].angle_deg), 6) if corners[obj].valid else ""
            record[f"obj{obj}_chamfer_mm"] = round(corners[obj].chamfer_mm, 6) if corners[obj].valid and not math.isnan(corners[obj].chamfer_mm) else ""
            record[f"obj{obj}_projection_x_mm"] = round(corners[obj].projection_x_mm, 6) if corners[obj].valid and not math.isnan(corners[obj].projection_x_mm) else ""
            record[f"obj{obj}_projection_z_mm"] = round(corners[obj].projection_z_mm, 6) if corners[obj].valid and not math.isnan(corners[obj].projection_z_mm) else ""
        output_rows.append(record)

    add_summary_rows(output_rows)
    output_path = resolve_output_path(args)
    write_csv(output_path, output_rows)
    print(f"CSV written: {output_path}")
    print(f"Common valid row range: {common_start}..{common_end}")
    print(f"Measured rows: {first_row}..{last_row}, step_rows={step_rows}")
    if not args.calibration and args.save_calibration:
        print(f"Calibration written: {args.save_calibration}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
