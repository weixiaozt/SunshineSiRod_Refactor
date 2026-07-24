from __future__ import annotations

import json
from dataclasses import asdict, dataclass
from pathlib import Path
from typing import Any


@dataclass(frozen=True)
class ServiceConfig:
    project_root: Path
    baseline_root: Path
    runtime_root: Path
    host: str = "127.0.0.1"
    port: int = 8780
    max_pending_jobs: int = 1
    automatic_retry_limit: int = 1
    slow_warning_seconds: float = 20.0
    worker_hard_timeout_seconds: float = 60.0
    expected_rows: int = 20000
    expected_columns: int = 3200
    require_workspace_sources: bool = True

    @classmethod
    def load(cls, path: Path) -> "ServiceConfig":
        config_path = Path(path).resolve()
        payload = json.loads(config_path.read_text(encoding="utf-8"))

        def resolve_path(name: str) -> Path:
            value = Path(str(payload[name]))
            if not value.is_absolute():
                value = config_path.parent / value
            return value.resolve()

        config = cls(
            project_root=resolve_path("project_root"),
            baseline_root=resolve_path("baseline_root"),
            runtime_root=resolve_path("runtime_root"),
            host=str(payload.get("host", "127.0.0.1")),
            port=int(payload.get("port", 8780)),
            max_pending_jobs=int(payload.get("max_pending_jobs", 1)),
            automatic_retry_limit=int(payload.get("automatic_retry_limit", 1)),
            slow_warning_seconds=float(payload.get("slow_warning_seconds", 20.0)),
            worker_hard_timeout_seconds=float(
                payload.get("worker_hard_timeout_seconds", 60.0)
            ),
            expected_rows=int(payload.get("expected_rows", 20000)),
            expected_columns=int(payload.get("expected_columns", 3200)),
            require_workspace_sources=bool(
                payload.get("require_workspace_sources", True)
            ),
        )
        config.validate()
        return config

    def validate(self) -> None:
        if self.host not in {"127.0.0.1", "localhost", "::1"}:
            raise ValueError("The production API must listen on loopback only")
        if not (1 <= self.port <= 65535):
            raise ValueError("port must be in 1..65535")
        if self.max_pending_jobs < 0:
            raise ValueError("max_pending_jobs cannot be negative")
        if self.automatic_retry_limit < 0:
            raise ValueError("automatic_retry_limit cannot be negative")
        if self.slow_warning_seconds <= 0:
            raise ValueError("slow_warning_seconds must be positive")
        if self.worker_hard_timeout_seconds <= self.slow_warning_seconds:
            raise ValueError(
                "worker_hard_timeout_seconds must exceed slow_warning_seconds"
            )
        if self.expected_rows <= 0 or self.expected_columns <= 0:
            raise ValueError("expected TIFF dimensions must be positive")

    @property
    def geometry_manifest_path(self) -> Path:
        return (
            self.baseline_root
            / "calibration"
            / "delivery_geometry"
            / "current_calibration.json"
        )

    @property
    def endface_calibration_path(self) -> Path:
        return (
            self.baseline_root
            / "coworker_endface_delivery"
            / "calibration"
            / "current_calibration.json"
        )

    @property
    def package_manifest_path(self) -> Path:
        return self.baseline_root / "package_manifest.json"

    @property
    def database_path(self) -> Path:
        return self.runtime_root / "jobs.sqlite3"

    @property
    def results_root(self) -> Path:
        return self.runtime_root / "results"

    @property
    def logs_root(self) -> Path:
        return self.runtime_root / "logs"

    def worker_payload(self) -> dict[str, Any]:
        payload = asdict(self)
        for key in ("project_root", "baseline_root", "runtime_root"):
            payload[key] = str(payload[key])
        return payload
