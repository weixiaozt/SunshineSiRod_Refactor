from __future__ import annotations

import faulthandler
import traceback
from multiprocessing.queues import Queue
from pathlib import Path
from typing import Any

from .algorithm_runtime import V7AlgorithmRuntime
from .config import ServiceConfig
from .input_adapters import InputValidationError


def worker_main(
    task_queue: Queue,
    event_queue: Queue,
    config_payload: dict[str, Any],
) -> None:
    faulthandler.enable()
    config = ServiceConfig(
        project_root=Path(config_payload["project_root"]),
        baseline_root=Path(config_payload["baseline_root"]),
        runtime_root=Path(config_payload["runtime_root"]),
        host=config_payload["host"],
        port=config_payload["port"],
        max_pending_jobs=config_payload["max_pending_jobs"],
        automatic_retry_limit=config_payload["automatic_retry_limit"],
        slow_warning_seconds=config_payload["slow_warning_seconds"],
        worker_hard_timeout_seconds=config_payload["worker_hard_timeout_seconds"],
        expected_rows=config_payload["expected_rows"],
        expected_columns=config_payload["expected_columns"],
        require_workspace_sources=config_payload.get("require_workspace_sources", True),
    )
    try:
        runtime = V7AlgorithmRuntime(config)
        event_queue.put({"type": "worker_ready"})
    except Exception as exc:
        event_queue.put(
            {
                "type": "worker_startup_failed",
                "message": str(exc),
                "traceback": traceback.format_exc(),
            }
        )
        return

    while True:
        try:
            task = task_queue.get()
        except (EOFError, OSError):
            return
        if task is None:
            return
        job_id = str(task["job_id"])
        event_queue.put({"type": "job_started", "job_id": job_id})

        def progress(stage: str, message: str) -> None:
            event_queue.put(
                {
                    "type": "job_progress",
                    "job_id": job_id,
                    "stage": stage,
                    "message": message,
                }
            )

        try:
            _, normalized, audit_path = runtime.measure(task, progress)
            event_queue.put(
                {
                    "type": "job_completed",
                    "job_id": job_id,
                    "measurement_status": normalized["measurement_status"],
                    "result": normalized,
                    "audit_path": str(audit_path),
                }
            )
        except InputValidationError as exc:
            event_queue.put(
                {
                    "type": "job_failed",
                    "job_id": job_id,
                    "code": exc.code,
                    "message": str(exc),
                    "traceback": traceback.format_exc(),
                    "retryable": False,
                }
            )
        except Exception as exc:
            event_queue.put(
                {
                    "type": "job_failed",
                    "job_id": job_id,
                    "code": "ALGORITHM_ERROR",
                    "message": str(exc),
                    "traceback": traceback.format_exc(),
                    "retryable": False,
                }
            )
