"""Shared data validation, robust statistics, and serialization helpers."""

from __future__ import annotations

import csv
import hashlib
import json
import math
from pathlib import Path
from typing import Any, Iterable, Mapping, Sequence

import numpy as np


EDGES = ("A", "B", "C", "D")
CAMERAS = ("Left", "Right", "Top", "Down")
EDGE_CAMERA_PAIRS = {
    "A": ("Top", "Right"),
    "B": ("Right", "Left"),
    "C": ("Left", "Down"),
    "D": ("Down", "Top"),
}
MODEL_NAME = "abcd_length_six_capture_perfect_rectangle"
MODEL_VERSION = 2
FINAL_MODEL_VERSION = 3
LONG_ROD_MODEL_VERSION = 4
SUPPORTED_MODEL_VERSIONS = (
    MODEL_VERSION,
    FINAL_MODEL_VERSION,
    LONG_ROD_MODEL_VERSION,
)
FORBIDDEN_ORIENTATION_KEYS = {
    "orientation",
    "orientation_detector",
    "orientation_models",
    "runtime_orientation_detection",
    "turnover",
    "reverse",
    "diaotou",
}


def file_sha256(path: Path, chunk_size: int = 8 * 1024 * 1024) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        while chunk := handle.read(chunk_size):
            digest.update(chunk)
    return digest.hexdigest()


def load_perfect_rectangle_truth(path: Path, tolerance_mm: float = 1e-6) -> dict[str, float]:
    with path.open("r", newline="", encoding="utf-8-sig") as handle:
        rows = [row for row in csv.DictReader(handle) if row.get("record_type") == "cross_section"]
    if len(rows) != 1:
        raise ValueError(f"Truth CSV must contain exactly one cross_section row: {path}")
    truth: dict[str, float] = {}
    for edge in EDGES:
        try:
            value = float(rows[0][f"{edge}_mm"])
        except (KeyError, TypeError, ValueError) as exc:
            raise ValueError(f"Truth CSV has no finite {edge}_mm") from exc
        if not math.isfinite(value) or value <= 0.0:
            raise ValueError(f"Truth CSV has invalid {edge}_mm={value!r}")
        truth[edge] = value
    if abs(truth["A"] - truth["C"]) > tolerance_mm:
        raise ValueError("Perfect-template truth requires A == C")
    if abs(truth["B"] - truth["D"]) > tolerance_mm:
        raise ValueError("Perfect-template truth requires B == D")
    return truth


def perfect_rectangle_corners(truth: Mapping[str, float]) -> dict[str, np.ndarray]:
    """Return nominal camera-corner positions in the delivered X/Z convention."""
    width = (float(truth["A"]) + float(truth["C"])) / 2.0
    height = (float(truth["B"]) + float(truth["D"])) / 2.0
    return {
        "Top": np.array([0.0, 0.0]),
        "Right": np.array([width, 0.0]),
        "Left": np.array([width, height]),
        "Down": np.array([0.0, height]),
    }


def edge_lengths(corners: Mapping[str, Sequence[float]]) -> dict[str, float]:
    output: dict[str, float] = {}
    for edge, (camera1, camera2) in EDGE_CAMERA_PAIRS.items():
        point1 = np.asarray(corners[camera1], dtype=float)
        point2 = np.asarray(corners[camera2], dtype=float)
        value = float(np.linalg.norm(point1 - point2))
        if not math.isfinite(value):
            raise ValueError(f"Non-finite {edge} edge length")
        output[edge] = value
    return output


def robust_vector_center(
    values: Sequence[Sequence[float]],
    outlier_limit_mm: float,
) -> tuple[np.ndarray, int, float]:
    array = np.asarray(values, dtype=float)
    if array.ndim != 2 or array.shape[1] != 2 or not np.all(np.isfinite(array)):
        raise ValueError("Expected finite N x 2 coordinate observations")
    if len(array) < 2:
        raise ValueError("At least two coordinate observations are required")
    center = np.median(array, axis=0)
    distances = np.linalg.norm(array - center, axis=1)
    used = array[distances <= float(outlier_limit_mm)]
    if len(used) < max(2, len(array) // 2):
        used = array
    result = np.median(used, axis=0)
    spread = float(np.max(np.linalg.norm(used - result, axis=1)))
    return result, int(len(used)), spread


def smooth_origin_curve(values: Sequence[Sequence[float]], window: int) -> np.ndarray:
    array = np.asarray(values, dtype=float)
    if array.ndim != 2 or array.shape[1] != 2 or not np.all(np.isfinite(array)):
        raise ValueError("Expected a finite N x 2 origin curve")
    window = int(window)
    if window < 1 or window % 2 == 0:
        raise ValueError("Smoothing window must be a positive odd integer")
    if window == 1:
        return array.copy()
    radius = window // 2
    padded = np.pad(array, ((radius, radius), (0, 0)), mode="edge")
    median_curve = np.vstack([
        np.median(padded[index:index + window], axis=0)
        for index in range(len(array))
    ])
    ramp = np.arange(1, radius + 2, dtype=float)
    kernel = np.concatenate([ramp, ramp[-2::-1]])
    kernel /= np.sum(kernel)
    padded_median = np.pad(median_curve, ((radius, radius), (0, 0)), mode="edge")
    return np.column_stack([
        np.convolve(padded_median[:, axis], kernel, mode="valid")
        for axis in range(2)
    ])


def aggregate_edge_records(
    records: Sequence[Mapping[str, Any]],
    end_station_count: int,
) -> dict[str, Any]:
    """Build the A/C global result and both requested B/D candidate results.

    A and C use the arithmetic mean of every valid measured station.  B and D
    retain two independent candidates: the mean of two dense end regions and
    the arithmetic mean of the whole rod.  No candidate changes face labels or
    inspects a path/name.
    """
    if len(records) < 10:
        raise ValueError("At least 10 valid stations are required")
    requested = int(end_station_count)
    if requested < 1:
        raise ValueError("end_station_count must be positive")
    used = min(requested, len(records) // 2)
    head = records[:used]
    tail = records[-used:]
    global_edges = {
        edge: float(np.mean([float(row[edge]) for row in records]))
        for edge in EDGES
    }
    head_edges = {
        edge: float(np.mean([float(row[edge]) for row in head]))
        for edge in EDGES
    }
    tail_edges = {
        edge: float(np.mean([float(row[edge]) for row in tail]))
        for edge in EDGES
    }
    head_tail_edges = {
        edge: (head_edges[edge] + tail_edges[edge]) / 2.0
        for edge in EDGES
    }
    reported_edges = {
        "A": global_edges["A"],
        "B": head_tail_edges["B"],
        "C": global_edges["C"],
        "D": head_tail_edges["D"],
    }
    return {
        "head": head_edges,
        "tail": tail_edges,
        "head_tail": head_tail_edges,
        "global": global_edges,
        "reported": reported_edges,
        "requested_end_station_count": requested,
        "used_end_station_count_per_end": used,
    }


def apply_bd_pair_relative_bias(
    records: Sequence[Mapping[str, Any]],
    k_mm: float,
) -> list[dict[str, Any]]:
    """Apply one antisymmetric device B/D pair correction before aggregation.

    This is deliberately not four independent final-edge offsets.  A/C stay
    unchanged, B receives +K/2, D receives -K/2, and therefore B+D is
    preserved for every measured station.
    """
    k_mm = float(k_mm)
    if not math.isfinite(k_mm) or abs(k_mm) > 0.05:
        raise ValueError(f"Invalid B/D pair-relative K: {k_mm!r}")
    output: list[dict[str, Any]] = []
    for source in records:
        row = dict(source)
        row["B"] = float(source["B"]) + k_mm / 2.0
        row["D"] = float(source["D"]) - k_mm / 2.0
        output.append(row)
    return output


def json_value(value: Any) -> Any:
    if isinstance(value, bool):
        return value
    if isinstance(value, Mapping):
        return {str(key): json_value(item) for key, item in value.items()}
    if isinstance(value, (list, tuple)):
        return [json_value(item) for item in value]
    if isinstance(value, np.ndarray):
        return value.tolist()
    if isinstance(value, (np.floating, float)):
        number = float(value)
        return number if math.isfinite(number) else None
    if isinstance(value, (np.integer, int)):
        return int(value)
    return value


def write_json(path: Path, payload: Mapping[str, Any]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(json_value(payload), ensure_ascii=False, indent=2), encoding="utf-8")


def write_csv(path: Path, rows: Iterable[Mapping[str, Any]]) -> None:
    materialized = list(rows)
    path.parent.mkdir(parents=True, exist_ok=True)
    keys: list[str] = []
    for row in materialized:
        for key in row:
            if key not in keys:
                keys.append(key)
    with path.open("w", newline="", encoding="utf-8-sig") as handle:
        writer = csv.DictWriter(handle, fieldnames=keys)
        writer.writeheader()
        writer.writerows(materialized)


def _walk_keys(value: Any) -> set[str]:
    keys: set[str] = set()
    if isinstance(value, Mapping):
        for key, item in value.items():
            keys.add(str(key).lower())
            keys.update(_walk_keys(item))
    elif isinstance(value, (list, tuple)):
        for item in value:
            keys.update(_walk_keys(item))
    return keys


def validate_model(payload: Mapping[str, Any], require_valid: bool = True) -> None:
    version = int(payload.get("version", 0))
    if payload.get("model") != MODEL_NAME or version not in SUPPORTED_MODEL_VERSIONS:
        raise ValueError("Unsupported ABCD six-capture model")
    if payload.get("contains_final_edge_offsets") is not False:
        raise ValueError("ABCD model must declare contains_final_edge_offsets=false")
    if payload.get("path_based_orientation_detection") is not False:
        raise ValueError("ABCD model must disable path-based orientation detection")
    forbidden = _walk_keys(payload) & FORBIDDEN_ORIENTATION_KEYS
    if forbidden:
        raise ValueError(f"ABCD model contains forbidden runtime orientation keys: {sorted(forbidden)}")
    if require_valid and payload.get("valid") is not True:
        raise ValueError("ABCD calibration model has not passed validation")
    runtime = payload.get("runtime_calibration")
    if not isinstance(runtime, Mapping):
        raise ValueError("ABCD model has no runtime_calibration")
    rows = runtime.get("rows")
    origins = runtime.get("origins")
    if not isinstance(rows, list) or len(rows) < 10 or not isinstance(origins, list) or len(origins) != len(rows):
        raise ValueError("ABCD runtime rows/origins are incomplete")
    for field in ("windows", "matrices"):
        mapping = runtime.get(field)
        if not isinstance(mapping, Mapping) or set(mapping) != set(CAMERAS):
            raise ValueError(f"ABCD runtime {field} must contain four cameras")
    if version >= FINAL_MODEL_VERSION:
        if payload.get("contains_independent_final_edge_offsets") is not False:
            raise ValueError("Final ABCD model must reject independent per-edge offsets")
        if runtime.get("y_synchronization") != "fixed_bias_interpolation":
            raise ValueError("Final ABCD model requires fixed camera-Y synchronization")
        k_mm = runtime.get("bd_pair_relative_bias_k_mm")
        if not isinstance(k_mm, (int, float)) or not math.isfinite(float(k_mm)):
            raise ValueError("Final ABCD model has no finite B/D pair-relative K")
        if abs(float(k_mm)) > 0.05:
            raise ValueError("Final ABCD B/D pair-relative K exceeds the safety bound")
        if runtime.get("bd_primary_aggregation") != "head_tail_dense_mean":
            raise ValueError("Final ABCD model requires B/D head-tail dense aggregation")
    if version >= LONG_ROD_MODEL_VERSION:
        if payload.get("algorithm_id") != "long_rod_template_calibrated_length":
            raise ValueError("Long-rod ABCD model has an unexpected algorithm id")
        if payload.get("algorithm_name_zh") != "长棒模板标定长度算法":
            raise ValueError("Long-rod ABCD model has an unexpected public name")
