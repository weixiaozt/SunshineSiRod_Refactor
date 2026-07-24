from __future__ import annotations

import json
import sqlite3
import threading
import time
import uuid
from pathlib import Path
from typing import Any, Mapping


ACTIVE_STATUSES = (
    "queued",
    "starting",
    "validating_input",
    "measuring_geometry",
    "measuring_endface",
    "normalizing_result",
    "recovering",
    "slow",
)
TERMINAL_STATUSES = ("completed", "failed")


class DuplicateRequestIdError(ValueError):
    pass


class JobStore:
    def __init__(self, database_path: Path) -> None:
        self.database_path = Path(database_path)
        self.database_path.parent.mkdir(parents=True, exist_ok=True)
        self._lock = threading.RLock()
        self._connection = sqlite3.connect(
            self.database_path,
            check_same_thread=False,
            timeout=30.0,
        )
        self._connection.row_factory = sqlite3.Row
        self._initialize()

    def close(self) -> None:
        with self._lock:
            self._connection.close()

    def _initialize(self) -> None:
        with self._lock:
            self._connection.execute("PRAGMA journal_mode=WAL")
            self._connection.execute("PRAGMA synchronous=FULL")
            self._connection.execute(
                """
                CREATE TABLE IF NOT EXISTS jobs (
                    job_id TEXT PRIMARY KEY,
                    request_id TEXT NOT NULL UNIQUE,
                    capture_id TEXT NOT NULL,
                    status TEXT NOT NULL,
                    stage TEXT NOT NULL,
                    message TEXT NOT NULL,
                    measurement_status TEXT,
                    request_json TEXT NOT NULL,
                    result_json TEXT,
                    audit_path TEXT,
                    error_json TEXT,
                    warning_json TEXT,
                    retry_count INTEGER NOT NULL DEFAULT 0,
                    created_at_epoch REAL NOT NULL,
                    updated_at_epoch REAL NOT NULL,
                    started_at_epoch REAL,
                    completed_at_epoch REAL
                )
                """
            )
            self._connection.commit()

    def create(self, request_payload: Mapping[str, Any]) -> dict[str, Any]:
        now = time.time()
        job_id = uuid.uuid4().hex
        request_id = str(request_payload["request_id"])
        capture_id = str(request_payload["capture_id"])
        serialized = json.dumps(request_payload, ensure_ascii=False)
        with self._lock:
            existing = self.get_by_request_id(request_id)
            if existing is not None:
                return existing
            try:
                self._connection.execute(
                    """
                    INSERT INTO jobs (
                        job_id, request_id, capture_id, status, stage, message,
                        request_json, created_at_epoch, updated_at_epoch
                    ) VALUES (?, ?, ?, 'queued', 'queued', ?, ?, ?, ?)
                    """,
                    (
                        job_id,
                        request_id,
                        capture_id,
                        "测量任务已进入队列",
                        serialized,
                        now,
                        now,
                    ),
                )
                self._connection.commit()
            except sqlite3.IntegrityError as exc:
                raise DuplicateRequestIdError(request_id) from exc
        created = self.get(job_id)
        if created is None:
            raise RuntimeError("Failed to read newly created job")
        return created

    def get(self, job_id: str) -> dict[str, Any] | None:
        with self._lock:
            row = self._connection.execute(
                "SELECT * FROM jobs WHERE job_id = ?", (job_id,)
            ).fetchone()
        return self._decode(row)

    def get_by_request_id(self, request_id: str) -> dict[str, Any] | None:
        with self._lock:
            row = self._connection.execute(
                "SELECT * FROM jobs WHERE request_id = ?", (request_id,)
            ).fetchone()
        return self._decode(row)

    def list_active(self) -> list[dict[str, Any]]:
        placeholders = ",".join("?" for _ in ACTIVE_STATUSES)
        with self._lock:
            rows = self._connection.execute(
                f"""
                SELECT * FROM jobs
                WHERE status IN ({placeholders})
                ORDER BY created_at_epoch
                """,
                ACTIVE_STATUSES,
            ).fetchall()
        return [decoded for row in rows if (decoded := self._decode(row)) is not None]

    def update(self, job_id: str, **updates: Any) -> dict[str, Any]:
        if not updates:
            current = self.get(job_id)
            if current is None:
                raise KeyError(job_id)
            return current
        allowed = {
            "status",
            "stage",
            "message",
            "measurement_status",
            "result_json",
            "audit_path",
            "error_json",
            "warning_json",
            "retry_count",
            "started_at_epoch",
            "completed_at_epoch",
        }
        unknown = set(updates) - allowed
        if unknown:
            raise ValueError(f"Unknown job columns: {sorted(unknown)}")
        updates["updated_at_epoch"] = time.time()
        assignments = ", ".join(f"{key} = ?" for key in updates)
        values = list(updates.values()) + [job_id]
        with self._lock:
            cursor = self._connection.execute(
                f"UPDATE jobs SET {assignments} WHERE job_id = ?", values
            )
            if cursor.rowcount != 1:
                raise KeyError(job_id)
            self._connection.commit()
        current = self.get(job_id)
        if current is None:
            raise KeyError(job_id)
        return current

    def prepare_recovery(self) -> list[dict[str, Any]]:
        active = self.list_active()
        for job in active:
            self.update(
                job["job_id"],
                status="recovering",
                stage="recovering",
                message="算法宿主重启，正在恢复未完成任务",
            )
        return self.list_active()

    def public_payload(self, job: Mapping[str, Any]) -> dict[str, Any]:
        payload: dict[str, Any] = {
            "job_id": job["job_id"],
            "request_id": job["request_id"],
            "capture_id": job["capture_id"],
            "job_status": job["status"],
            "stage": job["stage"],
            "message": job["message"],
            "measurement_status": job["measurement_status"],
            "retry_count": job["retry_count"],
            "created_at_epoch": job["created_at_epoch"],
            "updated_at_epoch": job["updated_at_epoch"],
            "started_at_epoch": job["started_at_epoch"],
            "completed_at_epoch": job["completed_at_epoch"],
        }
        if job.get("result") is not None:
            payload.update(job["result"])
        if job.get("warning") is not None:
            payload["warning"] = job["warning"]
        if job.get("error") is not None:
            payload["error"] = job["error"]
        else:
            payload["error"] = None
        payload["audit_available"] = bool(job.get("audit_path"))
        return payload

    @staticmethod
    def _decode(row: sqlite3.Row | None) -> dict[str, Any] | None:
        if row is None:
            return None
        value = dict(row)
        value["request"] = json.loads(value.pop("request_json"))
        for column, output in (
            ("result_json", "result"),
            ("error_json", "error"),
            ("warning_json", "warning"),
        ):
            serialized = value.pop(column)
            value[output] = json.loads(serialized) if serialized else None
        return value
