#!/usr/bin/env python3
"""Build the independent 210_105 normal/abnormal mechanical drift model."""

from __future__ import annotations

import argparse
import json
from pathlib import Path

import numpy as np

from mechanical_drift import build_model
from measure_square_rod_edges import HobjSource, extract_corner_from_row, source_common_range


def capture_profile(path: Path, fractions: list[float]) -> np.ndarray:
    source = HobjSource(str(path))
    common_start, common_end = source_common_range(source)
    profile = np.empty((4, len(fractions), 2), dtype=float)
    for object_index, obj in enumerate((1, 2, 3, 4)):
        for station_index, fraction in enumerate(fractions):
            row = int(round(common_start + fraction * (common_end - common_start)))
            corner = extract_corner_from_row(source.row(obj, row))
            if not corner.valid:
                raise RuntimeError(f"{path}: obj{obj} at {fraction:.2%} is invalid: {corner.reason}")
            profile[object_index, station_index] = [corner.vx, corner.vz]
    return profile


def hobj_profiles(folder: Path, fractions: list[float]) -> dict[str, np.ndarray]:
    paths = sorted(folder.rglob("*.hobj"), key=lambda value: str(value).casefold())
    if not paths:
        raise ValueError(f"No HOBJ files were found under {folder}")
    profiles: dict[str, np.ndarray] = {}
    for path in paths:
        capture_id = str(path.relative_to(folder)).replace("\\", "/")
        profiles[capture_id] = capture_profile(path, fractions)
        print(f"Profiled {capture_id}")
    return profiles


def main() -> int:
    parser = argparse.ArgumentParser(description="Build a four-camera longitudinal mechanical drift model")
    parser.add_argument("--normal-dir", required=True, help="Folder containing normal captures of one standard bar")
    parser.add_argument("--abnormal-dir", required=True, help="Folder containing abnormal captures of that same bar")
    parser.add_argument("--output", required=True, help="Output model JSON")
    parser.add_argument("--bar-id", required=True)
    parser.add_argument("--specification", required=True)
    parser.add_argument("--orientation", choices=["normal", "turnover"], default="normal")
    args = parser.parse_args()

    fractions = [round(value, 10) for value in np.linspace(0.05, 0.95, 19)]
    normal_profiles = hobj_profiles(Path(args.normal_dir), fractions)
    abnormal_profiles = hobj_profiles(Path(args.abnormal_dir), fractions)
    model = build_model(
        normal_profiles,
        abnormal_profiles,
        fractions,
        bar_id=args.bar_id,
        specification=args.specification,
        orientation=args.orientation,
    )
    output = Path(args.output)
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(model, ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"Mechanical drift model written: {output}")
    for capture_id, diagnostics in model["training_diagnostics"].items():
        print(
            f"  {capture_id}: class={diagnostics['class']}, "
            f"amplitude={diagnostics['amplitude']:.6f}, "
            f"fit_rmse={diagnostics['fit_rmse_mm']:.6f} mm"
        )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
