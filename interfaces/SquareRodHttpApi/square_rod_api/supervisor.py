from __future__ import annotations

import json
import logging
import multiprocessing
import queue
import threading
import time
import uuid
from collections import deque
from multiprocessing.process import BaseProcess
from typing import Any, Mapping

from .config import ServiceConfig
from .job_store import JobStore
from .worker import worker_main


class ServiceNotReadyError(RuntimeError):
    pass


class QueueFullError(RuntimeError):
    pass


class AlgorithmSupervisor:
    def __init__(
        self,
        config: ServiceConfig,
        store: JobStore,
        logger: logging.Logger,
    ) -> None:
        self.config = config
        self.store = store
        self.logger = logger
        self._context = multiprocessing.get_context("spawn")
        self._lock = threading.RLock()
        self._pending: deque[dict[str, Any]] = deque()
        self._current: dict[str, Any] | None = None
        self._current_started_monotonic: float | None = None
        self._slow_warning_sent = False
        self._task_queue: Any = None
        self._event_queue: Any = None
        self._worker: BaseProcess | None = None
        self._worker_ready = False
        self._state = "starting"
        self._fault_message: str | None = None
        self._restart_failures = 0
        self._stopping = threading.Event()
        self._monitor = threading.Thread(
            target=self._monitor_loop,
            daemon=True,
            name="algorithm-supervisor",
        )

    def start(self) -> None:
        with self._lock:
            recovered = self.store.prepare_recovery()
            for job in recovered:
                self._pending.append(self._task_from_job(job))
            self._start_worker_locked()
        self._monitor.start()

    def stop(self) -> None:
        self._stopping.set()
        with self._lock:
            if self._task_queue is not None:
                try:
                    self._task_queue.put_nowait(None)
                except (queue.Full, OSError):
                    pass
            worker = self._worker
        if worker is not None:
            worker.join(timeout=5)
            if worker.is_alive():
                worker.terminate()
                worker.join(timeout=5)
        self._monitor.join(timeout=5)

    def submit(self, request_payload: Mapping[str, Any]) -> tuple[dict[str, Any], bool]:
        request_id = str(request_payload["request_id"])
        with self._lock:
            existing = self.store.get_by_request_id(request_id)
            if existing is not None:
                return existing, False
            if not self._worker_ready or self._state in {
                "starting",
                "warming_up",
                "recovering",
                "faulted",
            }:
                raise ServiceNotReadyError(self._fault_message or "算法服务尚未就绪")
            outstanding = (1 if self._current is not None else 0) + len(self._pending)
            capacity = 1 + self.config.max_pending_jobs
            if outstanding >= capacity:
                raise QueueFullError("当前测量任务队列已满")
            job = self.store.create(request_payload)
            task = self._task_from_job(job)
            self._pending.append(task)
            self._dispatch_locked()
            return self.store.get(job["job_id"]) or job, True

    def status(self) -> dict[str, Any]:
        with self._lock:
            worker = self._worker
            return {
                "state": self._state,
                "ready": self._worker_ready and self._state in {"ready", "busy"},
                "busy": self._current is not None,
                "current_job_id": (
                    str(self._current["job_id"]) if self._current else None
                ),
                "pending_job_count": len(self._pending),
                "pending_capacity": self.config.max_pending_jobs,
                "worker_pid": worker.pid if worker and worker.is_alive() else None,
                "worker_alive": bool(worker and worker.is_alive()),
                "fault_message": self._fault_message,
            }

    def _task_from_job(self, job: Mapping[str, Any]) -> dict[str, Any]:
        request = dict(job["request"])
        request["job_id"] = job["job_id"]
        request.setdefault("scan", request.get("capture_id", ""))
        request.setdefault("specimen", "")
        return request

    def _start_worker_locked(self) -> None:
        self._state = "warming_up"
        self._worker_ready = False
        self._fault_message = None
        self._task_queue = self._context.Queue(maxsize=1)
        self._event_queue = self._context.Queue()
        self._worker = self._context.Process(
            target=worker_main,
            args=(
                self._task_queue,
                self._event_queue,
                self.config.worker_payload(),
            ),
            daemon=True,
            name="square-rod-algorithm-worker",
        )
        self._worker.start()
        self.logger.info("Started algorithm worker pid=%s", self._worker.pid)

    def _dispatch_locked(self) -> None:
        if (
            not self._worker_ready
            or self._current is not None
            or not self._pending
            or self._task_queue is None
        ):
            self._state = "busy" if self._current is not None else self._state
            return
        task = self._pending.popleft()
        self._current = task
        self._current_started_monotonic = time.monotonic()
        self._slow_warning_sent = False
        self.store.update(
            str(task["job_id"]),
            status="starting",
            stage="starting",
            message="算法Worker已接收任务",
            started_at_epoch=time.time(),
            warning_json=None,
        )
        self._task_queue.put(task)
        self._state = "busy"

    def _monitor_loop(self) -> None:
        while not self._stopping.is_set():
            event = None
            event_queue = self._event_queue
            if event_queue is not None:
                try:
                    event = event_queue.get(timeout=0.2)
                except queue.Empty:
                    pass
                except (EOFError, OSError):
                    pass
            if event is not None:
                self._handle_event(event)
            with self._lock:
                if self._stopping.is_set():
                    break
                worker = self._worker
                if (
                    worker is not None
                    and not worker.is_alive()
                    and self._state != "faulted"
                ):
                    self._handle_worker_exit_locked(worker.exitcode)
                    continue
                self._check_timeout_locked()
                self._dispatch_locked()

    def _handle_event(self, event: Mapping[str, Any]) -> None:
        event_type = str(event.get("type"))
        with self._lock:
            if event_type == "worker_ready":
                self._worker_ready = True
                self._restart_failures = 0
                self._state = "ready"
                self._fault_message = None
                self.logger.info("Algorithm worker is ready")
                self._dispatch_locked()
                return
            if event_type == "worker_startup_failed":
                self._fault_message = str(event.get("message", "Worker startup failed"))
                self._state = "faulted"
                self._worker_ready = False
                self.logger.error(
                    "Algorithm worker startup failed: %s\n%s",
                    self._fault_message,
                    event.get("traceback", ""),
                )
                return

            job_id = str(event.get("job_id", ""))
            if not self._current or job_id != str(self._current["job_id"]):
                self.logger.warning("Ignoring stale worker event: %s", event)
                return
            if event_type == "job_started":
                self.store.update(
                    job_id,
                    status="starting",
                    stage="starting",
                    message="算法Worker正在启动本次测量",
                )
            elif event_type == "job_progress":
                stage = str(event["stage"])
                self.store.update(
                    job_id,
                    status=stage,
                    stage=stage,
                    message=str(event["message"]),
                )
            elif event_type == "job_completed":
                result = dict(event["result"])
                self.store.update(
                    job_id,
                    status="completed",
                    stage="completed",
                    message="测量完成",
                    measurement_status=str(event["measurement_status"]),
                    result_json=json.dumps(result, ensure_ascii=False),
                    audit_path=str(event["audit_path"]),
                    error_json=None,
                    completed_at_epoch=time.time(),
                )
                self.logger.info(
                    "Job %s completed measurement_status=%s",
                    job_id,
                    event["measurement_status"],
                )
                self._finish_current_locked()
            elif event_type == "job_failed":
                error_id = f"ERR-{time.strftime('%Y%m%d')}-{uuid.uuid4().hex[:8]}"
                error = {
                    "code": str(event.get("code", "ALGORITHM_ERROR")),
                    "message": str(event.get("message", "算法计算失败")),
                    "error_id": error_id,
                }
                self.store.update(
                    job_id,
                    status="failed",
                    stage="failed",
                    message=error["message"],
                    error_json=json.dumps(error, ensure_ascii=False),
                    completed_at_epoch=time.time(),
                )
                self.logger.error(
                    "Job %s failed error_id=%s code=%s message=%s\n%s",
                    job_id,
                    error_id,
                    error["code"],
                    error["message"],
                    event.get("traceback", ""),
                )
                self._finish_current_locked()

    def _finish_current_locked(self) -> None:
        self._current = None
        self._current_started_monotonic = None
        self._slow_warning_sent = False
        self._state = "ready"
        self._dispatch_locked()

    def _check_timeout_locked(self) -> None:
        if self._current is None or self._current_started_monotonic is None:
            return
        elapsed = time.monotonic() - self._current_started_monotonic
        job_id = str(self._current["job_id"])
        if elapsed >= self.config.slow_warning_seconds and not self._slow_warning_sent:
            warning = {
                "code": "CALCULATION_SLOW",
                "message": (
                    f"本次计算已超过{self.config.slow_warning_seconds:.0f}秒目标时间"
                ),
                "elapsed_seconds": elapsed,
            }
            self.store.update(
                job_id,
                warning_json=json.dumps(warning, ensure_ascii=False),
            )
            self._slow_warning_sent = True
            self.logger.warning("Job %s exceeded target time: %.3fs", job_id, elapsed)
        if elapsed < self.config.worker_hard_timeout_seconds:
            return
        self.logger.error("Job %s timed out after %.3fs", job_id, elapsed)
        worker = self._worker
        if worker is not None and worker.is_alive():
            worker.terminate()
            worker.join(timeout=5)
        self._recover_current_locked(
            "ALGORITHM_TIMEOUT",
            f"算法计算超过{self.config.worker_hard_timeout_seconds:.0f}秒并已终止",
        )
        self._start_worker_locked()

    def _handle_worker_exit_locked(self, exit_code: int | None) -> None:
        if self._stopping.is_set():
            return
        self.logger.error("Algorithm worker exited unexpectedly code=%s", exit_code)
        if self._current is not None:
            self._recover_current_locked(
                "ALGORITHM_WORKER_EXITED",
                f"算法Worker异常退出，退出码={exit_code}",
            )
        self._restart_failures += 1
        if self._restart_failures > 5:
            self._state = "faulted"
            self._worker_ready = False
            self._fault_message = "算法Worker连续启动失败"
            self._worker = None
            return
        self._state = "recovering"
        time.sleep(min(0.2 * self._restart_failures, 1.0))
        self._start_worker_locked()

    def _recover_current_locked(self, code: str, message: str) -> None:
        if self._current is None:
            return
        task = self._current
        job_id = str(task["job_id"])
        job = self.store.get(job_id)
        retry_count = int(job["retry_count"] if job else 0)
        if retry_count < self.config.automatic_retry_limit:
            retry_count += 1
            self.store.update(
                job_id,
                status="recovering",
                stage="recovering",
                message=f"{message}，正在自动恢复",
                retry_count=retry_count,
            )
            self._pending.appendleft(task)
            self.logger.warning(
                "Recovering job %s retry=%s/%s",
                job_id,
                retry_count,
                self.config.automatic_retry_limit,
            )
        else:
            error_id = f"ERR-{time.strftime('%Y%m%d')}-{uuid.uuid4().hex[:8]}"
            error = {"code": code, "message": message, "error_id": error_id}
            self.store.update(
                job_id,
                status="failed",
                stage="failed",
                message=message,
                error_json=json.dumps(error, ensure_ascii=False),
                completed_at_epoch=time.time(),
            )
            self.logger.error(
                "Job %s exhausted recovery attempts error_id=%s", job_id, error_id
            )
        self._current = None
        self._current_started_monotonic = None
        self._slow_warning_sent = False
