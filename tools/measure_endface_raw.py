#!/usr/bin/env python3
"""Locked M0 Raw entry point for the end-face trial package."""

from __future__ import annotations

import sys
from typing import Sequence

try:
    from .measure_square_rod_edges import main as measurement_main
except ImportError:
    from measure_square_rod_edges import main as measurement_main


PROHIBITED_OPTIONS = {
    "--drift-calibration",
    "--endface-calibration",
    "--endface-truth-csv",
    "--calibration-truth-csv",
    "--save-endface-calibration",
    "--save-calibration",
    "--standard-json",
}


def locked_raw_arguments(arguments: Sequence[str]) -> list[str]:
    values = list(arguments)
    prohibited = sorted(option for option in PROHIBITED_OPTIONS if option in values)
    if prohibited:
        raise ValueError(
            "M0 Raw package forbids correction, truth, and calibration-generation options: "
            + ", ".join(prohibited)
        )
    if "--calibration" not in values:
        raise ValueError("M0 Raw package requires an existing camera coordinate model")
    if "--orientation" in values:
        index = values.index("--orientation")
        if index + 1 >= len(values) or values[index + 1] != "normal":
            raise ValueError("M0 Raw package fixes orientation=normal in device coordinates")
    else:
        values.extend(["--orientation", "normal"])
    if "--endface-only" not in values:
        values.append("--endface-only")
    return values


def main() -> int:
    try:
        sys.argv[1:] = locked_raw_arguments(sys.argv[1:])
    except ValueError as exc:
        print(f"ERROR: {exc}", file=sys.stderr)
        return 2
    return measurement_main()


if __name__ == "__main__":
    raise SystemExit(main())
