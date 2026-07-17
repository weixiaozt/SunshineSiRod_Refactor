#!/usr/bin/env python3
"""Command-line entry point for the independent six-capture ABCD calibrator."""

from __future__ import annotations

import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parent
sys.path.insert(0, str(ROOT / "src"))

from abcd_length.calibration import main  # noqa: E402


if __name__ == "__main__":
    raise SystemExit(main())
