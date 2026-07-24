from __future__ import annotations

import json
import sys
from pathlib import Path

import numpy as np
import pytest
import tifffile


API_ROOT = Path(__file__).resolve().parents[1]
PROJECT_ROOT = API_ROOT.parents[1]
sys.path.insert(0, str(API_ROOT))

from square_rod_api.algorithm_runtime import normalize_v7_result  # noqa: E402
from square_rod_api.baseline import verify_baseline  # noqa: E402
from square_rod_api.config import ServiceConfig  # noqa: E402
from square_rod_api.input_adapters import (  # noqa: E402
    CAMERAS,
    InputValidationError,
    open_tiff_set,
)
from square_rod_api.job_store import JobStore  # noqa: E402


def test_configured_v7_baseline_hashes_are_valid() -> None:
    config = ServiceConfig.load(API_ROOT / "api_config.local.json")
    report = verify_baseline(config)
    assert report["verified"] is True
    assert report["version"] == "2026-07-23-v7"
    assert len(report["checks"]) == 8
    assert all(check["match"] for check in report["checks"])


def test_tiff_set_is_explicit_float32_memmap(tmp_path: Path) -> None:
    paths = {}
    values = {}
    for index, camera in enumerate(CAMERAS):
        value = np.full((4, 6), index + 0.25, dtype=np.float32)
        path = tmp_path / f"{camera}.tif"
        tifffile.imwrite(path, value, compression=None, metadata=None)
        paths[camera] = str(path)
        values[camera] = value
    opened = open_tiff_set(paths, capture_id="sample", expected_shape=(4, 6))
    try:
        assert opened.provenance["type"] == "tiff_set"
        assert opened.provenance["memory_mapped"] is True
        assert opened.provenance["intermediate_hobj_written"] is False
        assert opened.provenance["halcon_runtime_used"] is False
        for camera in CAMERAS:
            assert np.array_equal(opened.images[camera], values[camera])
    finally:
        opened.close()


def test_tiff_set_rejects_missing_camera(tmp_path: Path) -> None:
    with pytest.raises(InputValidationError) as raised:
        open_tiff_set(
            {"Left": str(tmp_path / "Left.tif")},
            capture_id="bad",
            expected_shape=(4, 6),
        )
    assert raised.value.code == "INVALID_TIFF_SET"


def test_job_store_is_idempotent_and_recovers_active_jobs(tmp_path: Path) -> None:
    store = JobStore(tmp_path / "jobs.sqlite3")
    try:
        request = {
            "request_id": "request-1",
            "capture_id": "capture-1",
            "input": {"type": "hobj", "path": "D:/capture.hobj"},
        }
        first = store.create(request)
        duplicate = store.create(request)
        assert duplicate["job_id"] == first["job_id"]
        store.update(
            first["job_id"],
            status="measuring_geometry",
            stage="measuring_geometry",
            message="running",
        )
        recovered = store.prepare_recovery()
        assert len(recovered) == 1
        assert recovered[0]["status"] == "recovering"
    finally:
        store.close()


def test_normalization_preserves_unavailable_item() -> None:
    payload = {
        "measurement_status": "partial_success",
        "items": {
            "A_mm": {
                "value": 210.0,
                "status": "available",
                "reason_code": None,
                "reason": None,
                "valid_station_count": 850,
                "required_station_count": 10,
            },
            "diagonal_1_mm": {
                "value": None,
                "status": "unavailable",
                "reason_code": "upstream_endpoint_unavailable",
                "reason": "依赖倒角不可用",
                "valid_station_count": 0,
                "required_station_count": 595,
            },
        },
        "global_edges_mm": {"A": 210.0, "B": None, "C": None, "D": None},
        "raw_global_edges_mm": {"A": 209.9},
        "global_diagonals_mm": {"diag1": None, "diag2": None},
        "corner_geometry": {},
        "raw_corner_geometry": {},
        "delivered_length_mm": None,
        "raw_delivered_length_mm": None,
        "available_items": ["A_mm"],
        "unavailable_items": ["diagonal_1_mm"],
        "unavailable_reasons": {
            "diagonal_1_mm": {
                "reason_code": "upstream_endpoint_unavailable",
                "reason": "依赖倒角不可用",
            }
        },
        "warnings": ["diagonal_1_mm: 依赖倒角不可用"],
        "diagnostics": {},
        "coworker_endface_payload": {},
        "unified_measurement": {},
    }
    normalized = normalize_v7_result(payload)
    assert normalized["measurement_status"] == "partial_success"
    item = normalized["results"]["diagonals"]["diagonal_1"]
    assert item["value"] is None
    assert item["status"] == "unavailable"
    assert item["reason_code"] == "upstream_endpoint_unavailable"
