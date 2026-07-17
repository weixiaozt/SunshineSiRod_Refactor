"""Narrow adapter to the coworker-delivered low-level corner extraction code."""

from __future__ import annotations

import importlib
import sys
from pathlib import Path
from typing import Any

import numpy as np


HOBJ_OFFSETS = (120100, 256240185, 512360270, 768480355)
HOBJ_CHANNELS = ("Left", "Right", "Top", "Down")
HOBJ_SHAPE = (20000, 3200)


def repository_root() -> Path:
    return Path(__file__).resolve().parents[3]


def default_vendor_root() -> Path:
    base = repository_root() / "同事交付包代码" / "方棒尺寸几何算法交接包_对角线弧长侧边夹角_20260715"
    matches = sorted(base.glob("*/src/measure_all_bars_ctb_slice_calibrated.py"))
    if not matches:
        raise FileNotFoundError(f"Coworker delivery source was not found below {base}")
    return matches[0].parent.parent


def import_vendor_pipeline(package_root: Path | None = None) -> Any:
    root = Path(package_root) if package_root else default_vendor_root()
    source = root / "src" if (root / "src").is_dir() else root
    required = source / "measure_all_bars_ctb_slice_calibrated.py"
    if not required.is_file():
        raise FileNotFoundError(f"Missing coworker delivery pipeline: {required}")
    source_text = str(source.resolve())
    if source_text not in sys.path:
        sys.path.insert(0, source_text)
    return importlib.import_module("measure_all_bars_ctb_slice_calibrated")


def open_hobj(path: Path) -> tuple[dict[str, np.memmap], list[np.memmap]]:
    path = Path(path)
    if not path.is_file() or path.suffix.lower() != ".hobj":
        raise ValueError(f"Input must be an HOBJ file: {path}")
    images: dict[str, np.memmap] = {}
    mappings: list[np.memmap] = []
    for offset, channel in zip(HOBJ_OFFSETS, HOBJ_CHANNELS):
        image = np.memmap(path, dtype=np.float32, mode="r", offset=offset, shape=HOBJ_SHAPE)
        images[channel] = image
        mappings.append(image)
    return images, mappings


def close_hobj(images: dict[str, Any], mappings: list[np.memmap]) -> None:
    images.clear()
    mappings.clear()
