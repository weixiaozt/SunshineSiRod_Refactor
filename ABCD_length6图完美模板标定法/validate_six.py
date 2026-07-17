#!/usr/bin/env python3
"""Run repeatability plus an explicit calibration-only physical-pose audit."""

from __future__ import annotations

import argparse
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parent
sys.path.insert(0, str(ROOT / "src"))

from abcd_length.calibration import discover_six_captures, load_capture_manifest  # noqa: E402
from abcd_length.validation import validate_capture_set  # noqa: E402


def main() -> int:
    parser = argparse.ArgumentParser(
        description=(
            "Validate six captures and explicit calibration pose pairs; runtime path "
            "or filename interpretation remains forbidden"
        )
    )
    parser.add_argument("--input-dir", type=Path, default=ROOT)
    parser.add_argument(
        "--model",
        type=Path,
        default=ROOT / "models" / "long_rod_template_calibrated_length_210_105.json",
    )
    parser.add_argument("--output-dir", type=Path, default=ROOT / "results" / "anonymous_six_capture_validation")
    parser.add_argument("--package-root", type=Path, default=None)
    parser.add_argument(
        "--capture-manifest",
        type=Path,
        default=ROOT / "calibration_capture_manifest.csv",
    )
    args = parser.parse_args()
    manifest_entries = load_capture_manifest(args.capture_manifest, args.input_dir)
    summary = validate_capture_set(
        discover_six_captures(args.input_dir),
        args.model,
        args.output_dir,
        args.package_root,
        manifest_entries,
    )
    print(args.output_dir / "repeatability_summary.json")
    for edge in "ABCD":
        stats = summary["edge_statistics"][edge]
        print(f"{edge}: mean={stats['mean_mm']:.6f} std={stats['std_mm']:.6f} range={stats['range_mm']:.6f} mm")
    audit = summary["explicit_calibration_pose_pair_audit"]["residual_summaries"]
    print("A/C global and B/D crossed-pose residual summaries:")
    for key, stats in audit.items():
        print(
            f"{key}: rms={stats['rms_mm']:.6f} "
            f"max_abs={stats['max_absolute_mm']:.6f} mm"
        )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
