from __future__ import annotations

import json
import os
import sys
from pathlib import Path

import pytest


API_ROOT = Path(__file__).resolve().parents[1]
PROJECT_ROOT = API_ROOT.parents[1]
sys.path.insert(0, str(API_ROOT))

from square_rod_api.algorithm_runtime import V7AlgorithmRuntime  # noqa: E402
from square_rod_api.config import ServiceConfig  # noqa: E402


@pytest.mark.skipif(
    os.environ.get("SQUARE_ROD_RUN_REAL_HOBJ") != "1",
    reason="set SQUARE_ROD_RUN_REAL_HOBJ=1 for the 1 GiB V7 integration test",
)
def test_v7_reference_hobj_angles() -> None:
    config = ServiceConfig.load(API_ROOT / "api_config.local.json")
    runtime = V7AlgorithmRuntime(config)
    hobj = PROJECT_ROOT / "测试HOBJ" / "105-11" / "105_3" / "10_16.hobj"
    task = {
        "job_id": "pytest-v7-real-hobj",
        "request_id": "pytest-v7-real-hobj",
        "capture_id": "10_16",
        "specimen": "105-11",
        "scan": "10_16",
        "input": {"type": "hobj", "path": str(hobj)},
    }
    _, normalized, _ = runtime.measure(task, lambda stage, message: None)
    angles = tuple(
        normalized["results"]["corners"][str(index)]["main_face_angle_deg"]
        for index in range(1, 5)
    )
    assert angles == pytest.approx(
        (
            89.9320890542,
            89.9610264504,
            89.9135117171,
            89.9676710042,
        ),
        abs=5e-6,
    )
    assert normalized["measurement_status"] == "success"
