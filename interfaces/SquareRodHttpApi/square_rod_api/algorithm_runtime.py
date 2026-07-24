from __future__ import annotations

import json
import os
import sys
import time
from pathlib import Path
from typing import Any, Callable, Mapping

from .config import ServiceConfig
from .input_adapters import InputValidationError, open_measurement_input


ProgressCallback = Callable[[str, str], None]


def _configure_source_paths(project_root: Path) -> None:
    source_roots = (
        project_root / "ABCD_length6图完美模板标定法" / "src",
        project_root / "长棒倒角几何算法" / "src",
        project_root / "长棒棒长计算算法" / "src",
        project_root / "四角独立角度算法" / "src",
        project_root
        / "release"
        / "Square Rod Measurement0720_V4"
        / "coworker_endface_delivery"
        / "src",
        project_root / "tools",
    )
    for source in reversed(source_roots):
        text = str(source.resolve())
        if text not in sys.path:
            sys.path.insert(0, text)


class V7AlgorithmRuntime:
    """Direct-array runtime using the exact V7 algorithm modules and models."""

    def __init__(self, config: ServiceConfig) -> None:
        self.config = config
        _configure_source_paths(config.project_root)
        import rebuilt_delivery_geometry_measure as rebuilt_geometry
        import unified_square_rod_measure as unified

        self.rebuilt_geometry = rebuilt_geometry
        self.unified = unified
        # Preload and re-verify immutable models before the worker reports ready.
        self.rebuilt_geometry.load_manifest(config.geometry_manifest_path)
        self.unified.endface.load_runtime_calibration(config.endface_calibration_path)

    def measure(
        self,
        task: Mapping[str, Any],
        progress: ProgressCallback,
    ) -> tuple[dict[str, Any], dict[str, Any], Path]:
        capture_id = str(task["capture_id"])
        opened = None
        started = time.perf_counter()
        try:
            progress("validating_input", "正在验证并映射四相机输入")
            opened = open_measurement_input(
                task["input"],
                capture_id=capture_id,
                expected_shape=(
                    self.config.expected_rows,
                    self.config.expected_columns,
                ),
            )
            mapped_seconds = time.perf_counter() - started

            progress("measuring_geometry", "正在计算V7尺寸几何")
            geometry_started = time.perf_counter()
            geometry = self.rebuilt_geometry.measure(
                opened.trace_path,
                self.config.geometry_manifest_path,
                shared_images=opened.images,
                shared_mappings=opened.mappings,
            )
            geometry_seconds = time.perf_counter() - geometry_started

            progress("measuring_endface", "正在计算V7端面结果")
            endface_started = time.perf_counter()
            endface_payload = self.unified.measure_endface_from_shared_images(
                opened.images,
                self.config.endface_calibration_path,
                str(task.get("specimen", "")),
                str(task.get("scan", capture_id)),
            )
            endface_seconds = time.perf_counter() - endface_started

            progress("normalizing_result", "正在整理测量结果")
            geometry["coworker_endface_payload"] = endface_payload
            geometry["unified_measurement"] = {
                "version": 2,
                "single_process": True,
                "single_input_open": True,
                "shared_camera_arrays": True,
                "input_type": opened.provenance["type"],
                "intermediate_hobj_written": False,
                "halcon_runtime_used": False,
                "parallel_geometry_endface": False,
                "input_mapping_seconds": mapped_seconds,
                "geometry_seconds": geometry_seconds,
                "endface_seconds": endface_seconds,
                "total_wall_seconds": time.perf_counter() - started,
            }
            geometry["input_traceability"] = opened.provenance
            serializable = self.unified._json_value(geometry)
            output_path = self._write_audit_result(str(task["job_id"]), serializable)
            normalized = normalize_v7_result(serializable)
            return serializable, normalized, output_path
        finally:
            if opened is not None:
                opened.close()

    def _write_audit_result(self, job_id: str, payload: Mapping[str, Any]) -> Path:
        result_dir = self.config.results_root / job_id
        result_dir.mkdir(parents=True, exist_ok=True)
        output_path = result_dir / "algorithm_result.json"
        temporary = result_dir / "algorithm_result.json.writing"
        temporary.write_text(
            json.dumps(payload, ensure_ascii=False, indent=2) + "\n",
            encoding="utf-8",
        )
        os.replace(temporary, output_path)
        return output_path


def _item(value: Any, *, raw_value: Any = None) -> dict[str, Any]:
    if value is None:
        return {
            "value": None,
            "raw_value": raw_value,
            "status": "unavailable",
            "reason_code": "VALUE_UNAVAILABLE",
            "reason": "V7未返回可用值",
        }
    return {
        "value": value,
        "raw_value": raw_value,
        "status": "available",
        "reason_code": None,
        "reason": None,
    }

def normalize_v7_result(payload: Mapping[str, Any]) -> dict[str, Any]:
    """Keep V7 per-item availability while adding stable C# convenience groups."""

    items = dict(payload.get("items", {}))
    final_edges = payload.get("global_edges_mm", {})
    raw_edges = payload.get("raw_global_edges_mm", {})
    edges = {
        edge: {
            **dict(items.get(f"{edge}_mm", _item(final_edges.get(edge)))),
            "raw_value": raw_edges.get(edge),
        }
        for edge in "ABCD"
    }
    diagonal_items = {
        "diagonal_1": dict(
            items.get(
                "diagonal_1_mm",
                _item(payload.get("global_diagonals_mm", {}).get("diag1")),
            )
        ),
        "diagonal_2": dict(
            items.get(
                "diagonal_2_mm",
                _item(payload.get("global_diagonals_mm", {}).get("diag2")),
            )
        ),
    }
    corners: dict[str, Any] = {}
    formal_corners = payload.get("corner_geometry", {})
    raw_corners = payload.get("raw_corner_geometry", {})
    for index in ("1", "2", "3", "4"):
        formal = dict(formal_corners.get(index, {}))
        raw = dict(raw_corners.get(index, {}))
        formal["raw"] = raw
        corners[index] = formal

    rod_length = {
        "raw": dict(
            items.get(
                "raw_length_mm", _item(payload.get("raw_delivered_length_mm"))
            )
        ),
        "calibrated": dict(
            items.get(
                "calibrated_length_mm", _item(payload.get("delivered_length_mm"))
            )
        ),
    }
    endface_payload = payload.get("coworker_endface_payload", {})
    return {
        "measurement_status": payload.get("measurement_status", "failed"),
        "results": {
            "edges": edges,
            "diagonals": diagonal_items,
            "corners": corners,
            "rod_length": rod_length,
            "endface": {
                "scan_metrics": endface_payload.get("scan_metrics", {}),
                "raw_angles": endface_payload.get(
                    "measured_side_end_plane_angles_raw", {}
                ),
            },
            "items": items,
        },
        "available_items": list(payload.get("available_items", [])),
        "unavailable_items": list(payload.get("unavailable_items", [])),
        "unavailable_reasons": dict(payload.get("unavailable_reasons", {})),
        "warnings": list(payload.get("warnings", [])),
        "traceability": {
            "method": payload.get("method"),
            "aggregation": payload.get("aggregation"),
            "input": payload.get("input_traceability", {}),
            "model_audit": payload.get("diagnostics", {}).get("model_audit", {}),
            "path_or_filename_orientation_used": payload.get("diagnostics", {}).get(
                "path_or_filename_orientation_used", False
            ),
        },
        "timing": dict(payload.get("unified_measurement", {})),
    }
