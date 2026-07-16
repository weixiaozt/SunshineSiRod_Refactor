#!/usr/bin/env python3
"""CLI adapter for the isolated first-algorithm preview package."""

from __future__ import annotations

import runpy
import sys
from pathlib import Path


VALUE_OPTIONS_NOT_SUPPORTED_BY_V1 = {
    "--drift-calibration",
    "--endface-calibration",
    "--orientation",
}
FLAG_OPTIONS_NOT_SUPPORTED_BY_V1 = {
    "--endface-only",
}


def v1_arguments(arguments: list[str]) -> list[str]:
    filtered: list[str] = []
    index = 0
    while index < len(arguments):
        argument = arguments[index]
        if argument in VALUE_OPTIONS_NOT_SUPPORTED_BY_V1:
            index += 2
            continue
        if argument in FLAG_OPTIONS_NOT_SUPPORTED_BY_V1:
            index += 1
            continue
        filtered.append(argument)
        index += 1
    return filtered


def main() -> None:
    runtime_root = Path(getattr(sys, "_MEIPASS", Path(__file__).resolve().parent))
    algorithm_path = runtime_root / "v1_algorithm" / "measure_square_rod_edges_v1.py"
    if not algorithm_path.is_file():
        raise SystemExit(f"First-version algorithm source is missing: {algorithm_path}")
    sys.argv = [str(algorithm_path), *v1_arguments(sys.argv[1:])]
    runpy.run_path(str(algorithm_path), run_name="__main__")


if __name__ == "__main__":
    main()
