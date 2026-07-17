#!/usr/bin/env python3
"""Measure only A/B/C/D for one HOBJ with the independent six-capture model."""

from __future__ import annotations

import argparse
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parent
sys.path.insert(0, str(ROOT / "src"))

from abcd_length.common import write_json  # noqa: E402
from abcd_length.measurement import measure_hobj  # noqa: E402


def main() -> int:
    parser = argparse.ArgumentParser(description="Measure A/B/C/D only; path names never select orientation logic")
    parser.add_argument("--input", type=Path, required=True)
    parser.add_argument(
        "--model",
        type=Path,
        default=ROOT / "models" / "long_rod_template_calibrated_length_210_105.json",
    )
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--package-root", type=Path, default=None)
    parser.add_argument("--allow-unvalidated", action="store_true")
    args = parser.parse_args()
    result = measure_hobj(
        args.input,
        args.model,
        args.package_root,
        allow_unvalidated=args.allow_unvalidated,
    )
    write_json(args.output, result)
    print(args.output)
    print("A={A:.6f} B={B:.6f} C={C:.6f} D={D:.6f} mm".format(**result["reported_edges_mm"]))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
