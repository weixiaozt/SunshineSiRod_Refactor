#!/usr/bin/env python3
"""Run the delivered 75-slice geometry algorithms directly against an HOBJ."""

from __future__ import annotations

import argparse
import importlib.util
import json
import math
import sys
import types
from pathlib import Path
from typing import Any

import numpy as np
import tifffile


DEFAULT_DELIVERY_ROOT = Path(
    r"D:\Project\方棒尺寸几何算法交接包_对角线弧长侧边夹角_20260715\方棒尺寸几何算法交接包_对角线弧长侧边夹角_20260715"
)
DEFAULT_CALIBRATION = DEFAULT_DELIVERY_ROOT / "calibration" / "current_calibration.json"
HOBJ_OFFSETS = (120100, 256240185, 512360270, 768480355)
HOBJ_CHANNELS = ("Left", "Right", "Top", "Down")
HOBJ_SHAPE = (20000, 3200)
CORNER_METRICS = {
    "1": {"corner": "AB", "angle": "侧面垂直度1", "arc": "弧长AB"},
    "2": {"corner": "CD", "angle": "侧面垂直度3", "arc": "弧长CD"},
    "3": {"corner": "AD", "angle": "侧面垂直度4", "arc": "弧长AD"},
    "4": {"corner": "BC", "angle": "侧面垂直度2", "arc": "弧长BD"},
}


def import_delivery_pipeline(package_root: Path):
    source_dir = package_root / "src" if (package_root / "src").is_dir() else package_root
    if source_dir.is_dir() and str(source_dir) not in sys.path:
        sys.path.insert(0, str(source_dir))
    if importlib.util.find_spec("matplotlib") is None:
        matplotlib = types.ModuleType("matplotlib")
        pyplot = types.ModuleType("matplotlib.pyplot")
        matplotlib.pyplot = pyplot
        sys.modules.setdefault("matplotlib", matplotlib)
        sys.modules.setdefault("matplotlib.pyplot", pyplot)
    try:
        import measure_all_bars_ctb_slice_calibrated as pipeline
    except ImportError as exc:
        raise RuntimeError(
            f"Cannot import the delivered ABCD algorithm from {source_dir}"
        ) from exc
    return pipeline


def load_runtime_calibration(path: Path) -> tuple[dict[str, Any], dict[str, Any]]:
    payload = json.loads(path.read_text(encoding="utf-8"))
    runtime = payload["runtime_calibration"]
    calibration = {
        "rows": np.asarray(runtime["rows"], dtype=int),
        "windows": {
            camera: {
                side: np.asarray(columns, dtype=int)
                for side, columns in sides.items()
            }
            for camera, sides in runtime["windows"].items()
        },
        "matrices": {
            camera: np.asarray(matrix, dtype=float)
            for camera, matrix in runtime["matrices"].items()
        },
        "origins": [
            {
                camera: np.asarray(origin, dtype=float)
                for camera, origin in origin_row.items()
            }
            for origin_row in runtime["origins"]
        ],
    }
    return payload, calibration


def first_existing(directory: Path, patterns: list[str]) -> Path | None:
    for pattern in patterns:
        matches = sorted(directory.glob(pattern))
        if matches:
            return matches[0]
    return None


def open_input_images(path: Path) -> tuple[dict[str, Any], list[np.memmap]]:
    if path.is_file() and path.suffix.lower() == ".hobj":
        images: dict[str, Any] = {}
        mappings: list[np.memmap] = []
        for offset, channel in zip(HOBJ_OFFSETS, HOBJ_CHANNELS):
            image = np.memmap(
                path,
                dtype=np.float32,
                mode="r",
                offset=offset,
                shape=HOBJ_SHAPE,
            )
            images[channel] = image
            mappings.append(image)
        return images, mappings
    if not path.is_dir():
        raise RuntimeError(f"Input is not an HOBJ file or TIFF directory: {path}")
    files = {
        "Left": first_existing(path, ["1-Left.tif", "1-Left.tiff", "*_1.tif", "*_1.tiff", "*Left*.tif", "*Left*.tiff"]),
        "Right": first_existing(path, ["2-Right.tif", "2-Right.tiff", "*_2.tif", "*_2.tiff", "*Right*.tif", "*Right*.tiff"]),
        "Top": first_existing(path, ["3-Top.tif", "3-Top.tiff", "*_3.tif", "*_3.tiff", "*Top*.tif", "*Top*.tiff"]),
        "Down": first_existing(path, ["4-Down.tif", "4-Down.tiff", "*_4.tif", "*_4.tiff", "*Down*.tif", "*Down*.tiff"]),
    }
    missing = [camera for camera, file_path in files.items() if file_path is None]
    if missing:
        raise RuntimeError(f"Missing delivered-algorithm TIFF channels: {', '.join(missing)}")
    return {camera: tifffile.memmap(file_path) for camera, file_path in files.items()}, []


def json_value(value: Any) -> Any:
    if isinstance(value, dict):
        return {str(key): json_value(item) for key, item in value.items()}
    if isinstance(value, list):
        return [json_value(item) for item in value]
    if isinstance(value, np.ndarray):
        return value.tolist()
    if isinstance(value, (np.floating, float)):
        number = float(value)
        return number if math.isfinite(number) else None
    if isinstance(value, (np.integer, int)):
        return int(value)
    return value


def metric_number(metrics: dict[str, Any], key: str) -> float:
    try:
        value = float(metrics[key])
    except (KeyError, TypeError, ValueError) as exc:
        raise RuntimeError(f"Delivered geometry result is missing a valid metric: {key}") from exc
    if not math.isfinite(value):
        raise RuntimeError(f"Delivered geometry metric is not finite: {key}")
    return value


def measure(
    input_path: Path,
    calibration_path: Path,
    package_root: Path,
) -> dict[str, Any]:
    pipeline = import_delivery_pipeline(package_root)
    calibration_payload, calibration = load_runtime_calibration(calibration_path)
    images, mappings = open_input_images(input_path)
    original_memmap = pipeline.tifffile.memmap
    pipeline.tifffile.memmap = lambda source: source if isinstance(source, np.ndarray) else original_memmap(source)
    try:
        scan_input = pipeline.ScanInput(input_path.parent.name, input_path.stem, images)
        result = pipeline.measure_scan(scan_input, calibration)
    finally:
        pipeline.tifffile.memmap = original_memmap
        images.clear()
        mappings.clear()

    metrics = result["metrics"]
    global_edges: dict[str, float] = {}
    head_edges: dict[str, float] = {}
    tail_edges: dict[str, float] = {}
    for edge in "ABCD":
        head = metric_number(metrics, f"头-边长{edge}")
        tail = metric_number(metrics, f"尾-边长{edge}")
        head_edges[edge] = head
        tail_edges[edge] = tail
        global_edges[edge] = (head + tail) / 2.0

    head_diagonals = {
        "diag1": metric_number(metrics, "头-对角线1"),
        "diag2": metric_number(metrics, "头-对角线2"),
    }
    tail_diagonals = {
        "diag1": metric_number(metrics, "尾-对角线1"),
        "diag2": metric_number(metrics, "尾-对角线2"),
    }
    global_diagonals = {
        diagonal: (head_diagonals[diagonal] + tail_diagonals[diagonal]) / 2.0
        for diagonal in ("diag1", "diag2")
    }
    corner_geometry = {}
    for number, definition in CORNER_METRICS.items():
        corner = definition["corner"]
        corner_geometry[number] = {
            "corner": corner,
            "main_face_angle_deg": metric_number(metrics, definition["angle"]),
            "arc_length_mm": metric_number(metrics, definition["arc"]),
            "projection_1_mm": metric_number(metrics, f"弧长投影{corner}-1"),
            "projection_2_mm": metric_number(metrics, f"弧长投影{corner}-2"),
        }
    return json_value(
        {
            "method": "delivered_ctb_75_slice_geometry_20260715",
            "aggregation": (
                "edges_and_diagonals_mean_of_head5_tail5;"
                "arc_projection_and_main_face_angles_mean_of_all75"
            ),
            "input": str(input_path),
            "calibration_path": str(calibration_path),
            "calibration_status": calibration_payload.get("status", ""),
            "delivered_length_mm": metric_number(metrics, "长度"),
            "global_edges_mm": global_edges,
            "head_edges_mm": head_edges,
            "tail_edges_mm": tail_edges,
            "global_diagonals_mm": global_diagonals,
            "head_diagonals_mm": head_diagonals,
            "tail_diagonals_mm": tail_diagonals,
            "corner_geometry": corner_geometry,
            "slice_records": result["slice_records"],
            "diagnostics": result["diagnostics"],
        }
    )


def main() -> int:
    parser = argparse.ArgumentParser(
        description=(
            "Measure A/B/C/D, diagonals, arc lengths, projections, and four main-face "
            "angles using the delivered 75-slice geometry algorithms."
        )
    )
    parser.add_argument("--input", required=True, help="HOBJ file or four-TIFF directory")
    parser.add_argument("--output", required=True, help="Output JSON path")
    parser.add_argument("--calibration", default=str(DEFAULT_CALIBRATION), help="Delivered current_calibration.json")
    parser.add_argument("--package-root", default=str(DEFAULT_DELIVERY_ROOT), help="Delivered package root; ignored when modules are bundled")
    args = parser.parse_args()

    output_path = Path(args.output).resolve()
    output_path.parent.mkdir(parents=True, exist_ok=True)
    result = measure(
        Path(args.input).resolve(),
        Path(args.calibration).resolve(),
        Path(args.package_root).resolve(),
    )
    output_path.write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding="utf-8")
    print(output_path)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
