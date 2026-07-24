from __future__ import annotations

import json
import logging
import time
from http import HTTPStatus
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
from typing import Any
from urllib.parse import urlparse

from . import __version__
from .baseline import verify_baseline
from .config import ServiceConfig
from .job_store import JobStore
from .supervisor import (
    AlgorithmSupervisor,
    QueueFullError,
    ServiceNotReadyError,
)


class ApiContext:
    def __init__(
        self,
        config: ServiceConfig,
        baseline_report: dict[str, Any],
        store: JobStore,
        supervisor: AlgorithmSupervisor,
        started_at_epoch: float,
    ) -> None:
        self.config = config
        self.baseline_report = baseline_report
        self.store = store
        self.supervisor = supervisor
        self.started_at_epoch = started_at_epoch


class ApiServer(ThreadingHTTPServer):
    daemon_threads = True
    allow_reuse_address = True

    def __init__(self, address: tuple[str, int], context: ApiContext) -> None:
        self.context = context
        super().__init__(address, RequestHandler)


class RequestHandler(BaseHTTPRequestHandler):
    server_version = "SquareRodHttpApi/0.1"
    protocol_version = "HTTP/1.1"

    def handle(self) -> None:
        try:
            super().handle()
        except (ConnectionResetError, BrokenPipeError):
            logging.getLogger("square_rod_api.http").debug(
                "客户端在响应完成前断开连接"
            )

    @property
    def context(self) -> ApiContext:
        return self.server.context  # type: ignore[attr-defined]

    def log_message(self, format: str, *args: Any) -> None:
        logging.getLogger("square_rod_api.http").info(
            "%s - %s", self.address_string(), format % args
        )

    def _json(
        self,
        payload: Any,
        status: HTTPStatus = HTTPStatus.OK,
        *,
        headers: dict[str, str] | None = None,
    ) -> None:
        data = json.dumps(payload, ensure_ascii=False).encode("utf-8")
        self.send_response(status)
        self.send_header("Content-Type", "application/json; charset=utf-8")
        self.send_header("Content-Length", str(len(data)))
        self.send_header("Cache-Control", "no-store")
        for name, value in (headers or {}).items():
            self.send_header(name, value)
        self.end_headers()
        try:
            self.wfile.write(data)
        except (BrokenPipeError, ConnectionResetError):
            pass

    def _read_json(self) -> dict[str, Any]:
        try:
            length = int(self.headers.get("Content-Length", "0"))
        except ValueError as exc:
            raise ValueError("Content-Length无效") from exc
        if length <= 0:
            raise ValueError("请求体不能为空")
        if length > 1024 * 1024:
            raise ValueError("请求JSON不能超过1 MiB；请传文件路径而不是文件内容")
        try:
            payload = json.loads(self.rfile.read(length).decode("utf-8"))
        except (UnicodeDecodeError, json.JSONDecodeError) as exc:
            raise ValueError("请求JSON无效") from exc
        if not isinstance(payload, dict):
            raise ValueError("请求JSON根节点必须是对象")
        return payload

    def do_GET(self) -> None:  # noqa: N802
        route = urlparse(self.path).path.rstrip("/") or "/"
        if route == "/api/v1/health":
            status = self.context.supervisor.status()
            self._json(
                {
                    "status": "alive",
                    "service_version": __version__,
                    "started_at_epoch": self.context.started_at_epoch,
                    "uptime_seconds": time.time() - self.context.started_at_epoch,
                    "algorithm_host": status,
                }
            )
            return
        if route == "/api/v1/ready":
            status = self.context.supervisor.status()
            ready = bool(status["ready"])
            self._json(
                {
                    "ready": ready,
                    "busy": status["busy"],
                    "state": status["state"],
                    "baseline_verified": self.context.baseline_report["verified"],
                    "fault_message": status["fault_message"],
                },
                HTTPStatus.OK if ready else HTTPStatus.SERVICE_UNAVAILABLE,
            )
            return
        if route == "/api/v1/version":
            self._json(
                {
                    "api_schema": "square_rod_measurement_api_v1",
                    "service_version": __version__,
                    "baseline": self.context.baseline_report,
                    "input_types": ["tiff_set", "hobj"],
                    "halcon_runtime_used": False,
                }
            )
            return
        if route.startswith("/api/v1/measurements/"):
            parts = route.split("/")
            if len(parts) not in {5, 6}:
                self._not_found()
                return
            job_id = parts[4]
            job = self.context.store.get(job_id)
            if job is None:
                self._json(
                    {
                        "error": {
                            "code": "JOB_NOT_FOUND",
                            "message": "测量任务不存在",
                        }
                    },
                    HTTPStatus.NOT_FOUND,
                )
                return
            if len(parts) == 6:
                if parts[5] != "audit":
                    self._not_found()
                    return
                self._send_audit(job)
                return
            self._json(self.context.store.public_payload(job))
            return
        self._not_found()

    def do_POST(self) -> None:  # noqa: N802
        route = urlparse(self.path).path.rstrip("/")
        if route != "/api/v1/measurements":
            self._not_found()
            return
        try:
            payload = self._read_json()
            self._validate_measurement_request(payload)
            job, created = self.context.supervisor.submit(payload)
            public = self.context.store.public_payload(job)
            public["duplicate_request"] = not created
            self._json(
                public,
                HTTPStatus.ACCEPTED if created else HTTPStatus.OK,
            )
        except QueueFullError as exc:
            self._json(
                {
                    "error": {
                        "code": "QUEUE_FULL",
                        "message": str(exc),
                        "retry_after_ms": 1000,
                    }
                },
                HTTPStatus.TOO_MANY_REQUESTS,
                headers={"Retry-After": "1"},
            )
        except ServiceNotReadyError as exc:
            self._json(
                {
                    "error": {
                        "code": "SERVICE_NOT_READY",
                        "message": str(exc),
                    }
                },
                HTTPStatus.SERVICE_UNAVAILABLE,
                headers={"Retry-After": "1"},
            )
        except ValueError as exc:
            self._json(
                {"error": {"code": "INVALID_REQUEST", "message": str(exc)}},
                HTTPStatus.BAD_REQUEST,
            )

    def _send_audit(self, job: dict[str, Any]) -> None:
        audit_value = job.get("audit_path")
        if not audit_value:
            self._json(
                {
                    "error": {
                        "code": "AUDIT_NOT_AVAILABLE",
                        "message": "完整算法结果尚不可用",
                    }
                },
                HTTPStatus.CONFLICT,
            )
            return
        path = Path(str(audit_value)).resolve()
        root = self.context.config.results_root.resolve()
        if root not in path.parents or not path.is_file():
            self._json(
                {
                    "error": {
                        "code": "AUDIT_NOT_AVAILABLE",
                        "message": "完整算法结果文件不存在",
                    }
                },
                HTTPStatus.NOT_FOUND,
            )
            return
        try:
            payload = json.loads(path.read_text(encoding="utf-8"))
        except (OSError, json.JSONDecodeError):
            self._json(
                {
                    "error": {
                        "code": "INVALID_AUDIT_RESULT",
                        "message": "完整算法结果文件无效",
                    }
                },
                HTTPStatus.INTERNAL_SERVER_ERROR,
            )
            return
        self._json(payload)

    @staticmethod
    def _validate_measurement_request(payload: dict[str, Any]) -> None:
        for field in ("request_id", "capture_id"):
            value = payload.get(field)
            if not isinstance(value, str) or not value.strip():
                raise ValueError(f"{field}不能为空")
            if len(value) > 128:
                raise ValueError(f"{field}不能超过128个字符")
        input_payload = payload.get("input")
        if not isinstance(input_payload, dict):
            raise ValueError("input必须是对象")
        input_type = input_payload.get("type")
        if input_type not in {"hobj", "tiff_set"}:
            raise ValueError("input.type必须是hobj或tiff_set")
        if input_type == "hobj" and not str(input_payload.get("path", "")).strip():
            raise ValueError("HOBJ请求必须提供input.path")
        if input_type == "tiff_set" and not isinstance(
            input_payload.get("cameras"), dict
        ):
            raise ValueError("TIFF请求必须提供input.cameras")

    def _not_found(self) -> None:
        self._json(
            {"error": {"code": "ROUTE_NOT_FOUND", "message": "接口不存在"}},
            HTTPStatus.NOT_FOUND,
        )


def serve(
    config: ServiceConfig,
    logger: logging.Logger,
) -> None:
    config.runtime_root.mkdir(parents=True, exist_ok=True)
    config.results_root.mkdir(parents=True, exist_ok=True)
    config.logs_root.mkdir(parents=True, exist_ok=True)
    baseline_report = verify_baseline(config)
    store = JobStore(config.database_path)
    supervisor = AlgorithmSupervisor(config, store, logger)
    supervisor.start()
    context = ApiContext(config, baseline_report, store, supervisor, time.time())
    server = ApiServer((config.host, config.port), context)
    logger.info("Square Rod HTTP API listening on http://%s:%s", config.host, config.port)
    try:
        server.serve_forever(poll_interval=0.25)
    finally:
        server.server_close()
        supervisor.stop()
        store.close()
