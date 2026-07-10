#!/usr/bin/env python3
"""
Measure square-rod side lengths from four 3D line-scan height images.

Inputs:
  1) A HALCON .hobj containing four 3200x20000 real height images, or
  2) Four tif/tiff files in object order 1,2,3,4.

Output:
  A CSV containing per-slice A/B/C/D lengths plus summary rows.

Physical mapping used here:
  Obj1 = top   -> P1
  Obj3 = left  -> P3
  Obj2 = down  -> P2
  Obj4 = right -> P4

Edges:
  A = |P3-P1|, B = |P1-P4|, C = |P2-P4|, D = |P3-P2|
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

    x = (cols - c0) * x_scale
    zs = moving_average(z, 9)
    peak = int(np.argmax(zs))

    # Fit the two main faces away from the corner/chamfer transition.
    margin = 120
    left_end = max(peak - margin, 40)
    right_start = min(peak + margin, len(x) - 40)
    left0 = max(0, int(peak * 0.08))
    left1 = left_end
    right0 = right_start
    right1 = min(len(x), right_start + max(120, int((len(x) - right_start) * 0.75)))
    if left1 - left0 < 50 or right1 - right0 < 50:
        return CornerResult(False, reason="not enough face-fit points", seg_start_col=c0, seg_end_col=c1)

    m1, b1, rmse1 = robust_line_fit(x[left0:left1], zs[left0:left1])
    m2, b2, rmse2 = robust_line_fit(x[right0:right1], zs[right0:right1])
    if abs(m1 - m2) < 1e-9:
        return CornerResult(False, reason="two fitted faces are nearly parallel", seg_start_col=c0, seg_end_col=c1)

    vx = (b2 - b1) / (m1 - m2)
    vz = m1 * vx + b1

    denom = 1.0 + m1 * m2
    if abs(denom) < 1e-9:
        angle = 90.0
    else:
        angle = abs(math.degrees(math.atan((m2 - m1) / denom)))
        if angle > 90.0:
            angle = 180.0 - angle

    # Small-chamfer estimate: find the short transition near V.
    d1 = np.abs(m1 * x - zs + b1) / math.sqrt(m1 * m1 + 1.0)
    d2 = np.abs(m2 * x - zs + b2) / math.sqrt(m2 * m2 + 1.0)
    noise = max(rmse1, rmse2)
    threshold = max(0.03, noise * 6.0, 0.06)
    near = np.abs(x - vx) <= 3.0
    transition = near & (np.minimum(d1, d2) > threshold)
    idx = np.where(transition)[0]
    chamfer = math.nan
    if idx.size > 1:
        groups: List[np.ndarray] = np.split(idx, np.where(np.diff(idx) > 1)[0] + 1)
        groups.sort(key=lambda g: abs(float(np.mean(x[g])) - vx))
        g = groups[0]
        t1, t2 = int(g[0]), int(g[-1])
        chamfer = float(math.hypot(x[t2] - x[t1], zs[t2] - zs[t1]))

    return CornerResult(
        True,
        vx=float(vx),
        vz=float(vz),
        angle_deg=float(angle),
        chamfer_mm=chamfer,
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


def build_calibration(
    source: HeightSource,
    common_start: int,
    common_end: int,
    standard: Dict[str, Dict[str, float]],
) -> Dict:
    samples: Dict[int, List[Tuple[float, float]]] = {1: [], 2: [], 3: [], 4: []}
    targets: Dict[int, List[Tuple[float, float]]] = {1: [], 2: [], 3: [], 4: []}
    calibration_rows: Dict[str, int] = {}

    for name, item in standard.items():
        row = int(round(common_start + item["percent"] * (common_end - common_start)))
        calibration_rows[name] = row
        points = target_points_from_edges(item)
        for obj in [1, 2, 3, 4]:
            corner = extract_corner_from_row(source.row(obj, row))
            if not corner.valid:
                raise RuntimeError(f"Calibration failed at {name}, object {obj}: {corner.reason}")
            samples[obj].append((corner.vx, corner.vz))
            targets[obj].append(points[OBJECT_TO_POINT[obj]])

    transforms = {}
    for obj in [1, 2, 3, 4]:
        local = np.array(samples[obj], dtype=float)
        target = np.array(targets[obj], dtype=float)
        transforms[str(obj)] = solve_affine(local, target).tolist()

    return {
        "version": 1,
        "mapping": {"1": "P1/top", "3": "P3/left", "2": "P2/down", "4": "P4/right"},
        "common_start_row": common_start,
        "common_end_row": common_end,
        "calibration_rows": calibration_rows,
        "standard": standard,
        "transforms": transforms,
    }


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


def edge_lengths(points: Dict[str, Tuple[float, float]]) -> Dict[str, float]:
    def dist(a: str, b: str) -> float:
        p = points[a]
        q = points[b]
        return math.hypot(p[0] - q[0], p[1] - q[1])

    return {
        "A": dist("P3", "P1"),
        "B": dist("P1", "P4"),
        "C": dist("P2", "P4"),
        "D": dist("P3", "P2"),
    }


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
        "P1_x",
        "P1_z",
        "P2_x",
        "P2_z",
        "P3_x",
        "P3_z",
        "P4_x",
        "P4_z",
        "obj1_angle_deg",
        "obj2_angle_deg",
        "obj3_angle_deg",
        "obj4_angle_deg",
        "obj1_chamfer_mm",
        "obj2_chamfer_mm",
        "obj3_chamfer_mm",
        "obj4_chamfer_mm",
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
    for stat in ["mean", "min", "max", "range", "std"]:
        out: Dict[str, object] = {"record": stat, "valid": True}
        for edge in ["A", "B", "C", "D"]:
            vals = np.array([float(r[f"{edge}_mm"]) for r in valid_rows], dtype=float)
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
            out[f"{edge}_mm"] = round(value, 6)
        rows.append(out)


def main() -> int:
    parser = argparse.ArgumentParser(description="Measure square-rod side lengths from HOBJ or four TIFFs.")
    parser.add_argument("--input", help="Path to .hobj file or a folder containing object_1..object_4 TIFFs")
    parser.add_argument("--tifs", nargs=4, help="Four TIFF files in object order: obj1 obj2 obj3 obj4")
    parser.add_argument("--output", required=True, help="Output CSV path")
    parser.add_argument("--calibration", help="Existing calibration JSON to reuse")
    parser.add_argument("--save-calibration", help="Save generated calibration JSON")
    parser.add_argument("--standard-json", help="Standard measurement JSON; defaults to the latest measured standard")
    parser.add_argument("--step-mm", type=float, default=10.0, help="Slice spacing in mm along rod length")
    parser.add_argument("--step-rows", type=int, help="Slice spacing in rows; overrides --step-mm")
    parser.add_argument("--ignore-end-percent", type=float, default=0.05, help="Ignore this percent at both ends")
    parser.add_argument("--width", type=int, default=WIDTH)
    parser.add_argument("--height", type=int, default=HEIGHT)
    parser.add_argument("--x-scale", type=float, default=X_SCALE_MM)
    parser.add_argument("--y-scale", type=float, default=Y_SCALE_MM)
    parser.add_argument("--hobj-offsets", help="Optional four byte offsets: obj1,obj2,obj3,obj4")
    args = parser.parse_args()

    source = detect_input(args)
    ranges = {obj: find_valid_row_range(source, obj) for obj in [1, 2, 3, 4]}
    common_start = max(r[0] for r in ranges.values())
    common_end = min(r[1] for r in ranges.values())
    if common_end <= common_start:
        raise RuntimeError(f"No common valid row range across four objects: {ranges}")

    if args.calibration:
        with open(args.calibration, "r", encoding="utf-8") as f:
            calibration = json.load(f)
    else:
        standard = load_standard(args.standard_json)
        calibration = build_calibration(source, common_start, common_end, standard)
        if args.save_calibration:
            os.makedirs(os.path.dirname(os.path.abspath(args.save_calibration)), exist_ok=True)
            with open(args.save_calibration, "w", encoding="utf-8") as f:
                json.dump(calibration, f, ensure_ascii=False, indent=2)

    transforms = {int(k): np.array(v, dtype=float) for k, v in calibration["transforms"].items()}

    margin = int(round(args.ignore_end_percent * (common_end - common_start)))
    first_row = common_start + margin
    last_row = common_end - margin
    step_rows = args.step_rows if args.step_rows else max(1, int(round(args.step_mm / args.y_scale)))

    output_rows: List[Dict[str, object]] = []
    for row_index in range(first_row, last_row + 1, step_rows):
        corners = {obj: extract_corner_from_row(source.row(obj, row_index), args.x_scale) for obj in [1, 2, 3, 4]}
        valid = all(c.valid for c in corners.values())
        record: Dict[str, object] = {
            "record": "slice",
            "row": row_index,
            "y_mm": round((row_index - common_start) * args.y_scale, 6),
            "valid": valid,
        }
        if valid:
            points: Dict[str, Tuple[float, float]] = {}
            for obj, point_name in OBJECT_TO_POINT.items():
                c = corners[obj]
                points[point_name] = apply_affine(transforms[obj], c.vx, c.vz)
            lengths = edge_lengths(points)
            for edge in ["A", "B", "C", "D"]:
                record[f"{edge}_mm"] = round(lengths[edge], 6)
            for point_name in ["P1", "P2", "P3", "P4"]:
                px, pz = points[point_name]
                record[f"{point_name}_x"] = round(px, 6)
                record[f"{point_name}_z"] = round(pz, 6)
        else:
            record["note"] = "; ".join(f"obj{obj}:{c.reason}" for obj, c in corners.items() if not c.valid)

        for obj in [1, 2, 3, 4]:
            record[f"obj{obj}_angle_deg"] = round(corners[obj].angle_deg, 6) if corners[obj].valid else ""
            record[f"obj{obj}_chamfer_mm"] = round(corners[obj].chamfer_mm, 6) if corners[obj].valid and not math.isnan(corners[obj].chamfer_mm) else ""
        output_rows.append(record)

    add_summary_rows(output_rows)
    write_csv(args.output, output_rows)
    print(f"CSV written: {args.output}")
    print(f"Common valid row range: {common_start}..{common_end}")
    print(f"Measured rows: {first_row}..{last_row}, step_rows={step_rows}")
    if not args.calibration and args.save_calibration:
        print(f"Calibration written: {args.save_calibration}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
