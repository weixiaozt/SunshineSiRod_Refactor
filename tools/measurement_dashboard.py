#!/usr/bin/env python3
"""Local web dashboard for the square-rod measurement script.

Run from the repository root:
    python tools/measurement_dashboard.py

The dashboard deliberately reads the existing measurement CSV rather than
recalculating values in the browser.  This keeps the displayed final values
identical to the command-line measurement result.
"""

from __future__ import annotations

import csv
import json
import math
import os
import sqlite3
import subprocess
import sys
import threading
import time
from contextlib import closing
from datetime import datetime
from http import HTTPStatus
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
from typing import Any
from urllib.parse import urlparse

try:
    from .endface_wireframe_geometry import has_valid_wireframe_release_evidence
except ImportError:  # Direct script / PyInstaller entry point.
    from endface_wireframe_geometry import has_valid_wireframe_release_evidence


FROZEN_APP = bool(getattr(sys, "frozen", False))
END_FACE_ONLY = os.environ.get("SUNSHINE_ENDFACE_ONLY", "").strip().lower() in {"1", "true", "yes", "on"}
END_FACE_RAW_ONLY = END_FACE_ONLY and os.environ.get(
    "SUNSHINE_ENDFACE_RAW_ONLY", ""
).strip().lower() in {"1", "true", "yes", "on"}
TOOLS_DIR = Path(getattr(sys, "_MEIPASS", Path(__file__).resolve().parent))
RUNTIME_DIR = Path(sys.executable).resolve().parent if FROZEN_APP else TOOLS_DIR
PROJECT_DIR = RUNTIME_DIR if FROZEN_APP else TOOLS_DIR.parent
DASHBOARD_DIR = TOOLS_DIR / "dashboard"
CONFIG_PATH = RUNTIME_DIR / "web_ui_config.local.json"
CONTINUOUS_STATE_PATH = RUNTIME_DIR / "dashboard_continuous_seen.local.json"
CALIBRATION_DIR = RUNTIME_DIR.parent / "calibration" if FROZEN_APP else TOOLS_DIR / "calibration"
USER_RESULTS_DIRNAME = "user_results"
DEVELOPER_DETAILS_DIRNAME = "developer_details"
DEFAULT_CONFIG = {
    "data_root": str(RUNTIME_DIR.parent / "hobj") if FROZEN_APP else r"D:\Image_risen",
    "output_dir": str(RUNTIME_DIR.parent / "results" / "measurements") if FROZEN_APP else str(TOOLS_DIR / "results" / "measurements"),
    "script_path": str(RUNTIME_DIR.parent / "MeasureSquareRod" / "MeasureSquareRod.exe") if FROZEN_APP else str(TOOLS_DIR / "measure_square_rod_edges.py"),
    "delivery_abcd_script_path": str(RUNTIME_DIR.parent / "MeasureDeliveryGeometry" / "MeasureDeliveryGeometry.exe") if FROZEN_APP else str(TOOLS_DIR / "delivery_abcd_measure.py"),
    "delivery_abcd_calibration_path": str(CALIBRATION_DIR / "delivery_geometry" / "current_calibration.json") if FROZEN_APP else str(Path(r"D:\Project\方棒尺寸几何算法交接包_对角线弧长侧边夹角_20260715\方棒尺寸几何算法交接包_对角线弧长侧边夹角_20260715\calibration\current_calibration.json")),
    "calibration_path": str(CALIBRATION_DIR / "models" / "camera_calibration_model_210_105.json"),
    "drift_calibration_path": str(CALIBRATION_DIR / "models" / "mechanical_drift_model_210_105.json"),
    "endface_calibration_path": str(CALIBRATION_DIR / "models" / "endface_calibration_model_210_105.json"),
    "endface_measurement_mode": "raw_audit",
    "truth_csv_path": str(CALIBRATION_DIR / "truth" / "210_105.csv"),
    "step_mm": 10.0,
    "edge_offsets_mm": {"A": 0.0, "B": 0.0, "C": 0.0, "D": 0.0},
    "diagonal_offsets_mm": {"diag1": 0.0, "diag2": 0.0},
    "length_offset_mm": 0.0,
    "continuous_measure_enabled": False,
    "continuous_scan_seconds": 10,
    "stable_file_seconds": 30,
}
ENDFACE_MEASUREMENT_MODES = {"release_corrected", "raw_audit"}
CONFIG_LOCK = threading.Lock()
STATISTICS_LOCK = threading.Lock()
CONTINUOUS_STATE_LOCK = threading.Lock()
LATEST_RESULT_LOCK = threading.Lock()
RESULT_DIRECTORY_LOCK = threading.Lock()
COMPENSATION_LOG_LOCK = threading.Lock()
LATEST_RESULT: dict[str, Any] | None = None
COMPENSATION_LOG_FIELDS = [
    "changed_at",
    "item",
    "unit",
    "original_compensation",
    "new_compensation",
    "compensation_change",
    "latest_raw_measurement",
    "display_before_change",
    "display_after_change",
    "measurement_input_path",
    "source",
]


def resolved(value: str) -> str:
    """Return a normalized absolute local path, allowing an empty setting."""
    return str(Path(value).expanduser().resolve()) if value else ""


def load_config() -> dict[str, Any]:
    config = DEFAULT_CONFIG.copy()
    if CONFIG_PATH.exists():
        try:
            saved = json.loads(CONFIG_PATH.read_text(encoding="utf-8"))
            # Empty placeholders in the distributed config should not erase the
            # useful project defaults.  The optional manual-truth path is the
            # only setting where an empty value has meaning.
            for key, value in saved.items():
                # Permanent no-answer rule: silently discard the legacy
                # final-angle UI setting instead of carrying it into memory.
                if key == "endface_angle_offsets_deg":
                    continue
                if key == "truth_csv_path" or value not in ("", None):
                    config[key] = value
        except (OSError, json.JSONDecodeError):
            pass
    for key in ("data_root", "output_dir", "script_path", "delivery_abcd_script_path", "delivery_abcd_calibration_path", "calibration_path", "drift_calibration_path", "endface_calibration_path", "truth_csv_path"):
        config[key] = resolved(str(config.get(key, "")))
    try:
        config["step_mm"] = float(config.get("step_mm", 10.0))
    except (TypeError, ValueError):
        config["step_mm"] = 10.0
    mode = str(config.get("endface_measurement_mode", "release_corrected")).strip().lower()
    config["endface_measurement_mode"] = (
        mode if mode in ENDFACE_MEASUREMENT_MODES else "release_corrected"
    )
    if END_FACE_RAW_ONLY:
        config["endface_measurement_mode"] = "raw_audit"
    config["endface_raw_only"] = END_FACE_RAW_ONLY
    offsets = config.get("edge_offsets_mm", {})
    config["edge_offsets_mm"] = {}
    for edge in ("A", "B", "C", "D"):
        try:
            config["edge_offsets_mm"][edge] = float(offsets.get(edge, 0.0))
        except (AttributeError, TypeError, ValueError):
            config["edge_offsets_mm"][edge] = 0.0
    diagonal_offsets = config.get("diagonal_offsets_mm", {})
    config["diagonal_offsets_mm"] = {}
    for diagonal in ("diag1", "diag2"):
        try:
            config["diagonal_offsets_mm"][diagonal] = float(diagonal_offsets.get(diagonal, 0.0))
        except (AttributeError, TypeError, ValueError):
            config["diagonal_offsets_mm"][diagonal] = 0.0
    try:
        config["length_offset_mm"] = float(config.get("length_offset_mm", 0.0))
    except (TypeError, ValueError):
        config["length_offset_mm"] = 0.0
    return config


def save_config(settings: dict[str, Any]) -> dict[str, Any]:
    original_config = load_config()
    config = load_config()
    for key in ("data_root", "output_dir", "script_path", "delivery_abcd_script_path", "delivery_abcd_calibration_path", "calibration_path", "drift_calibration_path", "endface_calibration_path", "truth_csv_path"):
        value = settings.get(key, config[key])
        if not isinstance(value, str):
            raise ValueError(f"{key} must be a path string")
        config[key] = resolved(value)
    try:
        config["step_mm"] = float(settings.get("step_mm", config["step_mm"]))
    except (TypeError, ValueError) as exc:
        raise ValueError("Slice spacing must be a number") from exc
    if config["step_mm"] <= 0:
        raise ValueError("Slice spacing must be greater than zero")
    if "endface_measurement_mode" in settings:
        mode = str(settings["endface_measurement_mode"]).strip().lower()
        if mode not in ENDFACE_MEASUREMENT_MODES:
            raise ValueError("End-face measurement mode must be release_corrected or raw_audit")
        if END_FACE_RAW_ONLY and mode != "raw_audit":
            raise ValueError("This trial package is locked to M0 Raw audit mode")
        config["endface_measurement_mode"] = mode
    if "edge_offsets_mm" in settings:
        provided = settings["edge_offsets_mm"]
        if not isinstance(provided, dict):
            raise ValueError("Manual edge compensation must be an object")
        for edge in ("A", "B", "C", "D"):
            try:
                config["edge_offsets_mm"][edge] = float(provided.get(edge, 0.0))
            except (TypeError, ValueError) as exc:
                raise ValueError(f"Manual compensation for {edge} must be a number") from exc
    if "diagonal_offsets_mm" in settings:
        provided = settings["diagonal_offsets_mm"]
        if not isinstance(provided, dict):
            raise ValueError("Manual diagonal compensation must be an object")
        for diagonal in ("diag1", "diag2"):
            try:
                config["diagonal_offsets_mm"][diagonal] = float(provided.get(diagonal, 0.0))
            except (TypeError, ValueError) as exc:
                raise ValueError(f"Manual compensation for {diagonal} must be a number") from exc
    if "length_offset_mm" in settings:
        try:
            config["length_offset_mm"] = float(settings["length_offset_mm"])
        except (TypeError, ValueError) as exc:
            raise ValueError("Manual rod-length compensation must be a number") from exc
    if "endface_angle_offsets_deg" in settings:
        raise ValueError(
            "Final end-face angle compensation is permanently disabled; "
            "end-face values must come from current-HOBJ geometry"
        )
    if "continuous_measure_enabled" in settings:
        value = settings["continuous_measure_enabled"]
        if not isinstance(value, bool):
            raise ValueError("Continuous measurement state must be true or false")
        config["continuous_measure_enabled"] = value
    CONFIG_PATH.write_text(json.dumps(config, ensure_ascii=False, indent=2), encoding="utf-8")
    if any(
        key in settings
        for key in ("edge_offsets_mm", "diagonal_offsets_mm", "length_offset_mm")
    ):
        with LATEST_RESULT_LOCK:
            latest_result = LATEST_RESULT.copy() if LATEST_RESULT else None
        append_compensation_change_log(
            Path(config["output_dir"]),
            original_config,
            config,
            latest_result,
        )
    return config


def public_config(config: dict[str, Any]) -> dict[str, Any]:
    user_results_dir, developer_details_dir = result_directories(Path(config["output_dir"]))
    return {
        **config,
        "endface_only": END_FACE_ONLY,
        "result_paths": {
            "user_results_dir": str(user_results_dir),
            "measurement_statistics_csv": str(user_results_dir / "measurement_statistics.csv"),
            "compensation_change_log_csv": str(user_results_dir / "compensation_change_log.csv"),
            "developer_details_dir": str(developer_details_dir),
        },
        "exists": {
            key: bool(value and Path(value).exists())
            for key, value in config.items()
            if key.endswith("_path") or key in {"data_root", "output_dir"}
        },
    }


def result_directories(output_root: Path) -> tuple[Path, Path]:
    return (
        output_root / USER_RESULTS_DIRNAME,
        output_root / DEVELOPER_DETAILS_DIRNAME,
    )


def prepare_result_directories(output_root: Path) -> tuple[Path, Path]:
    user_results_dir, developer_details_dir = result_directories(output_root)
    with RESULT_DIRECTORY_LOCK:
        output_root.mkdir(parents=True, exist_ok=True)
        user_results_dir.mkdir(parents=True, exist_ok=True)
        developer_details_dir.mkdir(parents=True, exist_ok=True)
        legacy_pairs = (
            (
                output_root / "measurement_statistics.csv",
                user_results_dir / "measurement_statistics.csv",
            ),
            (
                output_root / "measurement_statistics.sqlite",
                developer_details_dir / "measurement_statistics.sqlite",
            ),
        )
        for source, target in legacy_pairs:
            if source.is_file() and not target.exists():
                try:
                    os.replace(source, target)
                except OSError:
                    pass
        for pattern in ("*_measure.csv", "*_delivery_geometry.json"):
            for source in output_root.glob(pattern):
                target = developer_details_dir / source.name
                if target.exists():
                    continue
                try:
                    os.replace(source, target)
                except OSError:
                    continue
        database = developer_details_dir / "measurement_statistics.sqlite"
        if database.is_file():
            try:
                with closing(sqlite3.connect(database)) as connection:
                    table = connection.execute(
                        "SELECT 1 FROM sqlite_master WHERE type='table' AND name='measurement_statistics'"
                    ).fetchone()
                    if table:
                        rows = connection.execute(
                            "SELECT id, slice_csv_path FROM measurement_statistics"
                        ).fetchall()
                        for row_id, saved_path in rows:
                            if not saved_path:
                                continue
                            candidate = developer_details_dir / Path(saved_path).name
                            if candidate.is_file() and str(candidate) != str(saved_path):
                                connection.execute(
                                    "UPDATE measurement_statistics SET slice_csv_path=? WHERE id=?",
                                    (str(candidate), row_id),
                                )
                        connection.commit()
            except (OSError, sqlite3.Error):
                pass
    return user_results_dir, developer_details_dir


def compensation_values(config: dict[str, Any]) -> dict[str, float]:
    return {
        **{
            edge: float(config.get("edge_offsets_mm", {}).get(edge, 0.0))
            for edge in "ABCD"
        },
        "D1": float(config.get("diagonal_offsets_mm", {}).get("diag1", 0.0)),
        "D2": float(config.get("diagonal_offsets_mm", {}).get("diag2", 0.0)),
        "Length": float(config.get("length_offset_mm", 0.0)),
    }


def append_compensation_change_log(
    output_root: Path,
    original_config: dict[str, Any],
    updated_config: dict[str, Any],
    latest_result: dict[str, Any] | None,
) -> Path:
    user_results_dir, developer_details_dir = prepare_result_directories(output_root)
    output = user_results_dir / "compensation_change_log.csv"
    database = developer_details_dir / "compensation_change_log.sqlite"
    original = compensation_values(original_config)
    updated = compensation_values(updated_config)
    raw_summary = (latest_result or {}).get("raw_summary", {})
    measurement_fields = {
        "A": "A_mm",
        "B": "B_mm",
        "C": "C_mm",
        "D": "D_mm",
        "D1": "diag1_M1_M2_mm",
        "D2": "diag2_M3_M4_mm",
        "Length": "stick_length_mm",
    }
    rows: list[dict[str, str]] = []
    changed_at = datetime.now().isoformat(timespec="seconds")
    for item, original_value in original.items():
        updated_value = updated[item]
        if math.isclose(original_value, updated_value, rel_tol=0.0, abs_tol=1e-12):
            continue
        raw_value = field_number(raw_summary, measurement_fields[item])
        rows.append(
            {
                "changed_at": changed_at,
                "item": item,
                "unit": "mm",
                "original_compensation": f"{original_value:.6f}",
                "new_compensation": f"{updated_value:.6f}",
                "compensation_change": f"{updated_value - original_value:.6f}",
                "latest_raw_measurement": "" if raw_value is None else f"{raw_value:.6f}",
                "display_before_change": ""
                if raw_value is None
                else f"{raw_value + original_value:.6f}",
                "display_after_change": ""
                if raw_value is None
                else f"{raw_value + updated_value:.6f}",
                "measurement_input_path": str((latest_result or {}).get("input_path", "")),
                "source": "web_manual_compensation",
            }
        )
    if not rows:
        return output
    with COMPENSATION_LOG_LOCK:
        with closing(sqlite3.connect(database)) as connection:
            columns = ", ".join(f'"{field}" TEXT' for field in COMPENSATION_LOG_FIELDS)
            connection.execute(
                f"CREATE TABLE IF NOT EXISTS compensation_changes "
                f"(id INTEGER PRIMARY KEY AUTOINCREMENT, {columns})"
            )
            names = ", ".join(f'"{field}"' for field in COMPENSATION_LOG_FIELDS)
            placeholders = ", ".join("?" for _ in COMPENSATION_LOG_FIELDS)
            connection.executemany(
                f"INSERT INTO compensation_changes ({names}) VALUES ({placeholders})",
                [[row[field] for field in COMPENSATION_LOG_FIELDS] for row in rows],
            )
            connection.commit()
        refresh_compensation_change_log(output_root)
    return output


def refresh_compensation_change_log(output_root: Path) -> bool:
    user_results_dir, developer_details_dir = prepare_result_directories(output_root)
    output = user_results_dir / "compensation_change_log.csv"
    database = developer_details_dir / "compensation_change_log.sqlite"
    if not database.is_file():
        return False
    temporary = output.with_suffix(".csv.tmp")
    try:
        with closing(sqlite3.connect(database)) as connection, temporary.open(
            "w", newline="", encoding="utf-8-sig"
        ) as handle:
            writer = csv.DictWriter(handle, fieldnames=COMPENSATION_LOG_FIELDS)
            writer.writeheader()
            rows = connection.execute(
                f"SELECT {', '.join(f'\"{field}\"' for field in COMPENSATION_LOG_FIELDS)} "
                "FROM compensation_changes ORDER BY id"
            )
            writer.writerows(
                dict(zip(COMPENSATION_LOG_FIELDS, values)) for values in rows
            )
        os.replace(temporary, output)
        return True
    except (OSError, sqlite3.Error):
        temporary.unlink(missing_ok=True)
        return False


def compensation_log_summary(config: dict[str, Any], limit: int = 12) -> dict[str, Any]:
    output_root = Path(config["output_dir"])
    user_results_dir, developer_details_dir = prepare_result_directories(output_root)
    database = developer_details_dir / "compensation_change_log.sqlite"
    output = user_results_dir / "compensation_change_log.csv"
    rows: list[dict[str, str]] = []
    if not database.is_file():
        with COMPENSATION_LOG_LOCK, closing(sqlite3.connect(database)) as connection:
            columns = ", ".join(f'"{field}" TEXT' for field in COMPENSATION_LOG_FIELDS)
            connection.execute(
                f"CREATE TABLE IF NOT EXISTS compensation_changes "
                f"(id INTEGER PRIMARY KEY AUTOINCREMENT, {columns})"
            )
            connection.commit()
            refresh_compensation_change_log(output_root)
    if database.is_file():
        try:
            with closing(sqlite3.connect(database)) as connection:
                values = connection.execute(
                    f"SELECT {', '.join(f'\"{field}\"' for field in COMPENSATION_LOG_FIELDS)} "
                    "FROM compensation_changes ORDER BY id DESC LIMIT ?",
                    (limit,),
                ).fetchall()
            rows = [dict(zip(COMPENSATION_LOG_FIELDS, value)) for value in values]
        except sqlite3.Error:
            rows = []
    return {"path": str(output), "rows": rows}


def discover_inputs(data_root: str) -> list[dict[str, str]]:
    root = Path(data_root)
    if not root.is_dir():
        return []
    files: list[dict[str, str]] = []
    try:
        for path in root.rglob("*.hobj"):
            stat = path.stat()
            files.append({
                "path": str(path),
                "name": path.name,
                "relative": str(path.relative_to(root)),
                "modified": datetime.fromtimestamp(stat.st_mtime).isoformat(timespec="seconds"),
                "kind": "hobj",
            })
        for folder in root.rglob("*"):
            if not folder.is_dir():
                continue
            names = [child.name.lower() for child in folder.iterdir() if child.is_file()]
            if all(any(name.endswith((".tif", ".tiff")) and (f"object_{obj}" in name or f"obj{obj}" in name or f"camera{obj}" in name) for name in names) for obj in (1, 2, 3, 4)):
                stat = folder.stat()
                files.append({
                    "path": str(folder),
                    "name": folder.name,
                    "relative": str(folder.relative_to(root)),
                    "modified": datetime.fromtimestamp(stat.st_mtime).isoformat(timespec="seconds"),
                    "kind": "tiff",
                })
    except OSError:
        return []
    return sorted(files, key=lambda item: item["modified"], reverse=True)


def read_csv_rows(path: Path) -> list[dict[str, str]]:
    with path.open("r", encoding="utf-8-sig", newline="") as handle:
        return list(csv.DictReader(handle))


def mean_measurement(path: Path) -> dict[str, Any]:
    rows = read_csv_rows(path)
    mean = next((row for row in rows if row.get("record") == "mean"), None)
    if not mean:
        raise ValueError("The measurement CSV does not contain a mean row")
    return {"summary": mean, "csv_path": str(path), "slice_count": sum(row.get("record") == "slice" for row in rows)}


def apply_manual_offsets(
    summary: dict[str, str],
    edge_offsets: dict[str, float],
    diagonal_offsets: dict[str, float],
    length_offset: float,
) -> dict[str, str]:
    """Apply only dimensional UI offsets; end-face answer offsets are forbidden."""
    corrected = summary.copy()
    for edge in ("A", "B", "C", "D"):
        field = f"{edge}_mm"
        value = field_number(summary, field)
        if value is not None:
            corrected[field] = f"{value + float(edge_offsets.get(edge, 0.0)):.6f}"
    for diagonal, field in (("diag1", "diag1_M1_M2_mm"), ("diag2", "diag2_M3_M4_mm")):
        value = field_number(summary, field)
        if value is not None:
            corrected[field] = f"{value + float(diagonal_offsets.get(diagonal, 0.0)):.6f}"
    length = field_number(summary, "stick_length_mm")
    if length is not None:
        corrected["stick_length_mm"] = f"{length + float(length_offset):.6f}"
    return corrected


FULL_STATISTICS_VALUE_FIELDS = [
    "measurement_valid",
    "A_mm", "B_mm", "C_mm", "D_mm",
    "delivery_abcd_method", "delivery_abcd_aggregation",
    "delivery_geometry_method", "delivery_geometry_aggregation", "delivery_length_mm",
    *[f"delivery_head_{edge}_mm" for edge in "ABCD"],
    *[f"delivery_tail_{edge}_mm" for edge in "ABCD"],
    "diag1_M1_M2_mm", "diag2_M3_M4_mm",
    "delivery_head_diag1_mm", "delivery_head_diag2_mm",
    "delivery_tail_diag1_mm", "delivery_tail_diag2_mm",
    "A_minus_C_mm", "B_minus_D_mm", "diagonal_difference_mm",
    "stick_length_mm",
    *[
        f"obj{obj}_{field}"
        for obj in (1, 2, 3, 4)
        for field in ("main_face_angle_deg", "chamfer_mm", "projection_x_mm", "projection_y_mm")
    ],
]
WIREFRAME_LOCAL_CHANNELS = (
    "A_left", "A_right", "B_top", "B_bottom",
    "C_top", "C_bottom", "D_left", "D_right",
)
ENDFACE_STATISTICS_VALUE_FIELDS = [
    *[
        f"{end}_{channel}_endface_raw_angle_deg"
        for end in ("head", "tail")
        for channel in WIREFRAME_LOCAL_CHANNELS
    ],
    *[
        f"{end}_{channel}_endface_angle_deg"
        for end in ("head", "tail")
        for channel in WIREFRAME_LOCAL_CHANNELS
    ],
    "head_endface_raw_representative_angle_deg",
    "tail_endface_raw_representative_angle_deg",
    "head_endface_representative_angle_deg",
    "tail_endface_representative_angle_deg",
    "head_endface_raw_worst_local_channel",
    "tail_endface_raw_worst_local_channel",
    "head_endface_worst_local_channel",
    "tail_endface_worst_local_channel",
    "head_endface_wireframe_rms_error_deg",
    "tail_endface_wireframe_rms_error_deg",
    "head_endface_max_dual_camera_seam_mm",
    "tail_endface_max_dual_camera_seam_mm",
    "head_endface_max_corner_closure_gap_mm",
    "tail_endface_max_corner_closure_gap_mm",
    "head_endface_wireframe_diagonal_twist_mm",
    "tail_endface_wireframe_diagonal_twist_mm",
    "endface_raw_quality_status",
    "endface_raw_quality_accepted",
    "endface_raw_quality_reason",
    "endface_raw_quality_uses_professional_truth",
    "endface_raw_quality_correction_applied",
    "endface_raw_quality_plane_rmse_limit_mm",
    "endface_raw_quality_stable_seam_span_limit_mm",
    "endface_raw_quality_warning_seam_span_limit_mm",
    "head_endface_raw_plane_rmse_mm",
    "tail_endface_raw_plane_rmse_mm",
    "endface_calibration_applicability_status",
    "endface_calibration_correction_applied",
]
STATISTICS_VALUE_FIELDS = ENDFACE_STATISTICS_VALUE_FIELDS if END_FACE_ONLY else FULL_STATISTICS_VALUE_FIELDS
STATISTICS_METADATA_FIELDS = ["measured_at", "bar_id", "capture_id", "input_path", "slice_csv_path"]
if not END_FACE_ONLY:
    STATISTICS_METADATA_FIELDS.append("slice_count")


def dashboard_display_values(summary: dict[str, str]) -> dict[str, str]:
    """Return exactly the measurement values shown on the dashboard."""
    values = {field: summary.get(field, "") for field in STATISTICS_VALUE_FIELDS}
    values["A_minus_C_mm"] = derived_difference(summary, "A_mm", "C_mm")
    values["B_minus_D_mm"] = derived_difference(summary, "B_mm", "D_mm")
    values["diagonal_difference_mm"] = derived_difference(summary, "diag1_M1_M2_mm", "diag2_M3_M4_mm")
    for obj in (1, 2, 3, 4):
        values[f"obj{obj}_main_face_angle_deg"] = summary.get(f"obj{obj}_verticality_error_deg", "")
    return values


def derived_difference(summary: dict[str, str], left: str, right: str) -> str:
    left_value = field_number(summary, left)
    right_value = field_number(summary, right)
    if left_value is None or right_value is None:
        return ""
    return f"{left_value - right_value:.6f}"


def append_measurement_statistics(
    output_root: Path,
    input_path: Path,
    slice_csv_path: Path,
    slice_count: int,
    displayed: dict[str, str],
) -> Path:
    """Persist one dashboard row, then refresh the human-readable CSV when possible."""
    user_results_dir, developer_details_dir = prepare_result_directories(output_root)
    output = user_results_dir / "measurement_statistics.csv"
    database = developer_details_dir / "measurement_statistics.sqlite"
    fields = [*STATISTICS_METADATA_FIELDS, *STATISTICS_VALUE_FIELDS]
    row = {
        "measured_at": datetime.now().isoformat(timespec="seconds"),
        "bar_id": bar_id_from_input(input_path),
        "capture_id": input_path.name if input_path.is_dir() else input_path.stem,
        "input_path": str(input_path),
        "slice_csv_path": str(slice_csv_path),
        "slice_count": slice_count,
        **dashboard_display_values(displayed),
    }
    with STATISTICS_LOCK:
        _ensure_statistics_database(database, output, fields)
        with closing(sqlite3.connect(database)) as connection:
            columns = ", ".join(f'"{field}"' for field in fields)
            placeholders = ", ".join("?" for _ in fields)
            connection.execute(
                f"INSERT INTO measurement_statistics ({columns}) VALUES ({placeholders})",
                [row[field] for field in fields],
            )
            connection.commit()
        refresh_statistics_csv(output_root)
    return output


def _ensure_statistics_database(database: Path, csv_snapshot: Path, fields: list[str]) -> None:
    """Create the durable statistics store and migrate an existing CSV once."""
    if database.exists():
        with closing(sqlite3.connect(database)) as connection:
            existing = {row[1] for row in connection.execute("PRAGMA table_info(measurement_statistics)")}
            for field in fields:
                if field not in existing:
                    connection.execute(f'ALTER TABLE measurement_statistics ADD COLUMN "{field}" TEXT')
            _migrate_endface_summary_to_direct_average(connection)
            connection.commit()
        return
    columns = ", ".join(f'"{field}" TEXT' for field in fields)
    with closing(sqlite3.connect(database)) as connection:
        connection.execute(f"CREATE TABLE measurement_statistics (id INTEGER PRIMARY KEY AUTOINCREMENT, {columns})")
        if csv_snapshot.is_file():
            try:
                with csv_snapshot.open("r", encoding="utf-8-sig", newline="") as handle:
                    existing_rows = list(csv.DictReader(handle))
            except OSError:
                existing_rows = []
            if existing_rows:
                names = ", ".join(f'"{field}"' for field in fields)
                placeholders = ", ".join("?" for _ in fields)
                connection.executemany(
                    f"INSERT INTO measurement_statistics ({names}) VALUES ({placeholders})",
                    [[row.get(field, "") for field in fields] for row in existing_rows],
                )
        _migrate_endface_summary_to_direct_average(connection)
        connection.commit()


def _migrate_endface_summary_to_direct_average(connection: sqlite3.Connection) -> None:
    """Keep all statistics rows on the current four-face arithmetic-mean definition."""
    for end in ("head", "tail"):
        angle_fields = [f"{end}_{face}_endface_angle_deg" for face in ("A", "B", "C", "D")]
        target = f"{end}_endface_verticality_deg"
        existing = {row[1] for row in connection.execute("PRAGMA table_info(measurement_statistics)")}
        if target not in existing or any(field not in existing for field in angle_fields):
            continue
        valid = " AND ".join(f'"{field}" IS NOT NULL AND "{field}" != \'\'' for field in angle_fields)
        expression = " + ".join(f'CAST("{field}" AS REAL)' for field in angle_fields)
        connection.execute(
            f'UPDATE measurement_statistics SET "{target}" = printf(\'%.6f\', ({expression}) / 4.0) '
            f'WHERE {valid}'
        )
        connection.execute(
            f'UPDATE measurement_statistics SET "{target}" = \'\' WHERE NOT ({valid})'
        )


def refresh_statistics_csv(output_root: Path) -> bool:
    """Export the durable store to its one CSV snapshot without failing a measurement on a file lock."""
    user_results_dir, developer_details_dir = prepare_result_directories(output_root)
    output = user_results_dir / "measurement_statistics.csv"
    database = developer_details_dir / "measurement_statistics.sqlite"
    if not database.is_file():
        return False
    fields = [*STATISTICS_METADATA_FIELDS, *STATISTICS_VALUE_FIELDS]
    temporary = output.with_suffix(".csv.tmp")
    try:
        with closing(sqlite3.connect(database)) as connection, temporary.open("w", newline="", encoding="utf-8") as handle:
            writer = csv.DictWriter(handle, fieldnames=fields)
            writer.writeheader()
            rows = connection.execute(
                f"SELECT {', '.join(f'\"{field}\"' for field in fields)} FROM measurement_statistics ORDER BY id"
            )
            writer.writerows(dict(zip(fields, values)) for values in rows)
        os.replace(temporary, output)
        return True
    except OSError:
        temporary.unlink(missing_ok=True)
        return False


def field_number(row: dict[str, str], field: str) -> float | None:
    try:
        value = row.get(field, "")
        return float(value) if value not in ("", None) else None
    except (TypeError, ValueError):
        return None


def summary_values_equal(
    left: object,
    right: object,
    *,
    tolerance: float = 5e-6,
) -> bool:
    """Compare CSV summary values without mistaking harmless formatting for correction."""

    try:
        return abs(float(left) - float(right)) <= tolerance
    except (TypeError, ValueError):
        return left == right


def is_usable_physical_endface_model(
    endface_data: dict[str, Any], camera_data: dict[str, Any]
) -> bool:
    """Mirror the measurement runtime's fail-closed v15 M1 model checks."""

    prohibited = {"angle_offsets_deg", "orientation_models", "orientation_detector"}
    try:
        version = int(endface_data.get("version", 0))
    except (TypeError, ValueError):
        return False
    if not (
        endface_data.get("valid") is True
        and version >= 15
        and endface_data.get("self_consistency_selected_model_level") == "M1"
        and endface_data.get("runtime_selected_model_level") == "M1"
        and endface_data.get("runtime_correction_applied") is True
        and endface_data.get("model") == "endface_camera_geometry_calibration"
        and endface_data.get("strategy")
        == "physical_decomposed_camera_scan_row_synchronization"
        and endface_data.get("runtime_orientation_detection") == "none"
        and endface_data.get("runtime_endpoint_label_contract")
        == {
            "head": "hobj_device_row_min",
            "tail": "hobj_device_row_max",
            "physical_R_L_inferred": False,
            "reason": "HOBJ contains no physical endpoint identity metadata",
        }
        and endface_data.get("report_to_software_face_map")
        == {"A": "A", "B": "B", "C": "D", "D": "C"}
        and isinstance(endface_data.get("y_coordinate_correction"), dict)
        and endface_data["y_coordinate_correction"].get("method")
        == "per_camera_scan_row_offset"
        and endface_data["y_coordinate_correction"].get("apply_to")
        == "all_longitudinal_side_points_and_end_boundary_points"
        and isinstance(endface_data.get("release_readiness"), dict)
        and endface_data["release_readiness"].get("ready") is True
        and not prohibited.intersection(endface_data)
    ):
        return False
    if not has_valid_wireframe_release_evidence(endface_data):
        return False
    offsets = endface_data.get("camera_y_offsets_mm")
    if not isinstance(offsets, dict):
        return False
    try:
        values = [float(offsets[str(obj)]) for obj in (1, 2, 3, 4)]
    except (KeyError, TypeError, ValueError):
        return False
    if any(not math.isfinite(value) or abs(value) > 1.5 for value in values):
        return False
    if abs(sum(values)) > 1e-6:
        return False
    state_reference = endface_data.get("calibration_state_reference")
    if not (
        isinstance(state_reference, dict)
        and state_reference.get("method")
        == "same_physical_face_dual_camera_line_separation_no_truth"
        and state_reference.get("fit_target")
        == "image_geometry_applicability_only_no_correction"
        and state_reference.get("all_captures_stable") is True
        and state_reference.get("complete") is True
        and isinstance(state_reference.get("faces"), dict)
    ):
        return False
    for face in "ABCD":
        envelope = state_reference["faces"].get(face)
        if not isinstance(envelope, dict) or envelope.get("valid") is not True:
            return False
        try:
            limits = [
                float(envelope[name])
                for name in (
                    "seam_median_min_mm",
                    "seam_median_max_mm",
                    "seam_span_max_mm",
                    "pair_angle_p90_max_deg",
                )
            ]
        except (KeyError, TypeError, ValueError):
            return False
        if any(not math.isfinite(value) for value in limits):
            return False
    basis = endface_data.get("physical_mode_basis")
    fit_details = endface_data.get("fit_details")
    if not isinstance(basis, dict) or not isinstance(fit_details, dict):
        return False
    try:
        mode_vectors = [
            [float(value) for value in basis[name]]
            for name in ("affine_x_mode", "affine_z_mode", "nonplanar_mode")
        ]
    except (KeyError, TypeError, ValueError):
        return False
    if any(
        len(vector) != 4
        or any(not math.isfinite(value) for value in vector)
        or abs(sum(vector)) > 1e-6
        for vector in mode_vectors
    ):
        return False
    selected_fit = fit_details.get("selected")
    if (
        not isinstance(selected_fit, dict)
        or selected_fit.get("method")
        != "single_nonplanar_camera_scan_row_synchronization"
        or selected_fit.get("model_level") != "M1"
    ):
        return False
    camera_metadata = camera_data.get("single_bar_metadata", {})
    camera_spec = (
        str(camera_metadata.get("specification", "")).strip()
        if isinstance(camera_metadata, dict)
        else ""
    )
    model_spec = str(endface_data.get("nominal_spec", "")).strip()
    return not camera_spec or model_spec == camera_spec


def calibration_summary(config: dict[str, Any]) -> dict[str, Any]:
    path = Path(config["calibration_path"])
    if not path.is_file():
        return {"available": False, "message": "Calibration model file not found."}
    try:
        data = json.loads(path.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError) as exc:
        return {"available": False, "message": f"Cannot read calibration model: {exc}"}
    biases = data.get("corner_biases", {})
    endface_path = Path(config["endface_calibration_path"])
    endface_data: dict[str, Any] = {}
    if endface_path.is_file():
        try:
            endface_data = json.loads(endface_path.read_text(encoding="utf-8"))
        except (OSError, json.JSONDecodeError):
            endface_data = {}
    endface_usable = is_usable_physical_endface_model(endface_data, data)
    release_readiness = endface_data.get("release_readiness")
    if not isinstance(release_readiness, dict):
        release_readiness = {}
    endface_truth: dict[str, Any] = {}
    endface_truth_means: dict[str, float] = {}
    if isinstance(endface_truth, dict):
        for end in ("head", "tail"):
            values = endface_truth.get(end, {})
            if not isinstance(values, dict):
                continue
            try:
                angles = [float(values[face]) for face in ("A", "B", "C", "D")]
            except (KeyError, TypeError, ValueError):
                continue
            endface_truth_means[end] = sum(angles) / 4.0
    drift_path = Path(config["drift_calibration_path"])
    drift_data: dict[str, Any] = {}
    if drift_path.is_file():
        try:
            drift_data = json.loads(drift_path.read_text(encoding="utf-8"))
        except (OSError, json.JSONDecodeError):
            drift_data = {}
    return {
        "available": True,
        "path": str(path),
        "version": data.get("version", "—"),
        "model": data.get("model", "—"),
        "note": data.get("note", ""),
        "corner_biases": biases,
        "standard": data.get("standard", {}),
        "standards_by_capture": data.get("standards_by_capture", {}),
        "endface_path": str(endface_path),
        "endface_available": endface_usable,
        "endface_version": endface_data.get("version", "—"),
        "endface_model": endface_data.get("model", "—"),
        "endface_strategy": endface_data.get("strategy", ""),
        "endface_valid": endface_usable,
        "endface_rejection_reason": (
            str(release_readiness.get("reason", "")).strip()
            or "Legacy/truth-fitted/final-angle or invalid end-face model is blocked; generate a valid image-only physical v15+ M1 model with complete Y correction, 16-angle holdout evidence, and a camera-state gate."
            if endface_data and not endface_usable
            else ""
        ),
        "endface_sample_id": endface_data.get("sample_id", ""),
        "endface_nominal_spec": endface_data.get("nominal_spec", ""),
        "endface_truth_angles_deg": endface_truth,
        "endface_truth_means_deg": endface_truth_means,
        "endface_validation": endface_data.get("validation", {}),
        "endface_wireframe_validation": endface_data.get("wireframe_validation", {}),
        "endface_model_comparison": endface_data.get("model_comparison", {}),
        "endface_external_noncalibration_bar_validation": endface_data.get(
            "external_noncalibration_bar_validation", {}
        ),
        "endface_self_consistency_selected_model_level": endface_data.get(
            "self_consistency_selected_model_level", ""
        ),
        "endface_runtime_selected_model_level": endface_data.get(
            "runtime_selected_model_level", ""
        ),
        "endface_release_readiness": release_readiness,
        "endface_capture_geometry_quality": endface_data.get(
            "capture_geometry_quality", {}
        ),
        "endface_camera_state_all_stable": bool(
            isinstance(endface_data.get("calibration_state_reference"), dict)
            and endface_data["calibration_state_reference"].get("all_captures_stable")
            is True
        ),
        "endface_failures": endface_data.get("failures", []),
        "drift_path": str(drift_path),
        "drift_available": bool(drift_data),
        "drift_version": drift_data.get("version", "-"),
        "drift_model": drift_data.get("model", "-"),
        "drift_normal_capture_count": drift_data.get("normal_capture_count", 0),
        "drift_abnormal_capture_count": drift_data.get("abnormal_capture_count", 0),
        "manual_edge_offsets_mm": config["edge_offsets_mm"],
        "manual_length_offset_mm": config["length_offset_mm"],
    }


FIELD_ALIASES = {
    "A": "A_mm", "B": "B_mm", "C": "C_mm", "D": "D_mm",
    "diag1": "diag1_M1_M2_mm", "diag2": "diag2_M3_M4_mm",
    "stick_length": "stick_length_mm",
}


def truth_check(config: dict[str, Any], input_path: str, summary: dict[str, str]) -> dict[str, Any]:
    truth_path = Path(config["truth_csv_path"])
    if not truth_path.is_file():
        return {"available": False, "message": "No manual-truth CSV has been configured."}
    try:
        truth_rows = read_csv_rows(truth_path)
    except (OSError, csv.Error) as exc:
        return {"available": False, "message": f"Cannot read manual-truth CSV: {exc}"}
    input_file = Path(input_path)
    candidates = {input_file.stem.lower(), input_file.name.lower(), input_file.parent.name.lower()}

    def matches(row: dict[str, str]) -> bool:
        for key in ("bar_id", "file_name", "hobj_path", "input_path"):
            value = str(row.get(key, "")).strip()
            if value and (value.lower() in candidates or Path(value).stem.lower() in candidates or Path(value).name.lower() in candidates):
                return True
        return False

    matched_rows = [row for row in truth_rows if matches(row)]
    if not matched_rows:
        return {"available": False, "message": "No matching manual-truth row was found for this measurement."}
    direct_angles: dict[tuple[str, str], list[float]] = {}
    for row in matched_rows:
        end_text = str(row.get("end", row.get("端面", ""))).strip().lower()
        end = "head" if end_text in {"head", "头", "头部", "头端", "头部端面"} else "tail" if end_text in {"tail", "尾", "尾部", "尾端", "尾部端面"} else ""
        face = str(row.get("face", row.get("面", ""))).strip().upper()
        angle = next(
            (
                field_number(row, field)
                for field in ("angle_deg", "included_angle_deg", "value_deg", "夹角_deg", "量规角度_deg")
                if field_number(row, field) is not None
            ),
            None,
        )
        if end and face in {"A", "B", "C", "D"} and angle is not None:
            direct_angles.setdefault((end, face), []).append(angle)
    if direct_angles:
        comparisons = []
        for end in ("head", "tail"):
            for face in ("A", "B", "C", "D"):
                values = direct_angles.get((end, face), [])
                measured = field_number(summary, f"{end}_{face}_endface_angle_deg")
                if len(values) == 3 and measured is not None:
                    truth_value = sum(values) / 3.0
                    comparisons.append({"name": f"{end}-{face}", "truth": truth_value, "measured": measured, "difference": measured - truth_value})
        return {
            "available": bool(comparisons),
            "bar_id": input_file.parent.name or input_file.stem,
            "comparisons": comparisons,
            "message": "" if comparisons else "The 24-point angle CSV is incomplete for this measurement.",
        }

    matched = matched_rows[0]
    comparisons = []
    for truth_field, measured_field in FIELD_ALIASES.items():
        truth_value = field_number(matched, truth_field)
        measured = field_number(summary, measured_field)
        if truth_value is not None and measured is not None:
            comparisons.append({"name": truth_field, "truth": truth_value, "measured": measured, "difference": measured - truth_value})
    return {"available": True, "bar_id": matched.get("bar_id") or input_file.stem, "comparisons": comparisons}


def ensure_under_root(path: Path, root: Path) -> None:
    try:
        path.resolve().relative_to(root.resolve())
    except ValueError as exc:
        raise ValueError("Selected input is outside the configured data directory") from exc


def bar_id_from_input(input_path: Path) -> str:
    """Use the capture folder name as the unique square-rod identifier."""
    candidate = input_path.name if input_path.is_dir() else input_path.parent.name
    safe = "".join("_" if char in '<>:"/\\|?*' else char for char in candidate).strip(" ._")
    return safe or "unknown_bar"


def input_signature(path: Path) -> list[int]:
    stat = path.stat()
    return [stat.st_mtime_ns, stat.st_size]


def read_continuous_seen() -> dict[str, list[int]]:
    if not CONTINUOUS_STATE_PATH.exists():
        return {}
    try:
        data = json.loads(CONTINUOUS_STATE_PATH.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError):
        return {}
    return data if isinstance(data, dict) else {}


def mark_continuous_capture_processed(path: Path) -> None:
    """Persist one successful manual or continuous measurement signature."""
    signature = input_signature(path)
    key = str(path.resolve())
    with CONTINUOUS_STATE_LOCK:
        seen = read_continuous_seen()
        seen[key] = signature
        CONTINUOUS_STATE_PATH.write_text(json.dumps(seen, ensure_ascii=False, indent=2), encoding="utf-8")


def build_measurement_command(
    config: dict[str, Any],
    input_path: Path,
    output_path: Path,
    *,
    endface_only: bool = END_FACE_ONLY,
) -> tuple[list[str], str]:
    """Build one explicit command without positional argument insertion."""

    mode = (
        str(config.get("endface_measurement_mode", "release_corrected"))
        if endface_only
        else "release_corrected"
    )
    if mode not in ENDFACE_MEASUREMENT_MODES:
        raise RuntimeError(f"Unsupported end-face measurement mode: {mode}")
    script_path = Path(config["script_path"])
    command = (
        [str(script_path)]
        if script_path.suffix.lower() == ".exe"
        else [sys.executable, str(script_path)]
    )
    command.extend(
        [
            "--input",
            str(input_path),
            "--calibration",
            config["calibration_path"],
        ]
    )
    if not endface_only:
        command.extend(["--drift-calibration", config["drift_calibration_path"]])
    elif mode == "release_corrected":
        command.extend(["--drift-calibration", config["drift_calibration_path"]])
        command.extend(["--endface-calibration", config["endface_calibration_path"]])
    command.extend(
        [
            "--output",
            str(output_path),
            "--overwrite",
            "--step-mm",
            str(config["step_mm"]),
        ]
    )
    if endface_only:
        command.extend(["--orientation", "normal", "--endface-only"])
    return command, mode


def build_delivery_abcd_command(
    config: dict[str, Any],
    input_path: Path,
    output_path: Path,
) -> list[str]:
    script_path = Path(config["delivery_abcd_script_path"])
    command = (
        [str(script_path)]
        if script_path.suffix.lower() == ".exe"
        else [sys.executable, str(script_path)]
    )
    command.extend(
        [
            "--input",
            str(input_path),
            "--output",
            str(output_path),
            "--calibration",
            config["delivery_abcd_calibration_path"],
        ]
    )
    return command


def merge_delivery_abcd_summary(
    summary: dict[str, str],
    payload: dict[str, Any],
) -> dict[str, str]:
    global_edges = payload.get("global_edges_mm")
    head_edges = payload.get("head_edges_mm")
    tail_edges = payload.get("tail_edges_mm")
    if not all(isinstance(value, dict) for value in (global_edges, head_edges, tail_edges)):
        raise RuntimeError("Delivered ABCD output is missing global/head/tail edge values")
    merged = summary.copy()
    for edge in "ABCD":
        try:
            global_value = float(global_edges[edge])
            head_value = float(head_edges[edge])
            tail_value = float(tail_edges[edge])
        except (KeyError, TypeError, ValueError) as exc:
            raise RuntimeError(f"Delivered ABCD output has an invalid {edge} value") from exc
        if not all(math.isfinite(value) for value in (global_value, head_value, tail_value)):
            raise RuntimeError(f"Delivered ABCD output has a non-finite {edge} value")
        merged[f"{edge}_mm"] = f"{global_value:.6f}"
        merged[f"delivery_head_{edge}_mm"] = f"{head_value:.6f}"
        merged[f"delivery_tail_{edge}_mm"] = f"{tail_value:.6f}"
    merged["delivery_abcd_method"] = str(payload.get("method", ""))
    merged["delivery_abcd_aggregation"] = str(payload.get("aggregation", ""))
    merged["delivery_abcd_calibration_path"] = str(payload.get("calibration_path", ""))
    merged["delivery_geometry_method"] = str(payload.get("method", ""))
    merged["delivery_geometry_aggregation"] = str(payload.get("aggregation", ""))

    delivered_length = payload.get("delivered_length_mm")
    if delivered_length is not None:
        try:
            delivered_length_value = float(delivered_length)
        except (TypeError, ValueError) as exc:
            raise RuntimeError("Delivered geometry output has an invalid rod length") from exc
        if not math.isfinite(delivered_length_value):
            raise RuntimeError("Delivered geometry output has a non-finite rod length")
        merged["delivery_length_mm"] = f"{delivered_length_value:.6f}"
        merged["stick_length_mm"] = f"{delivered_length_value:.6f}"

    global_diagonals = payload.get("global_diagonals_mm")
    head_diagonals = payload.get("head_diagonals_mm")
    tail_diagonals = payload.get("tail_diagonals_mm")
    if any(value is not None for value in (global_diagonals, head_diagonals, tail_diagonals)):
        if not all(isinstance(value, dict) for value in (global_diagonals, head_diagonals, tail_diagonals)):
            raise RuntimeError("Delivered geometry output is missing global/head/tail diagonal values")
        for diagonal, target in (
            ("diag1", "diag1_M1_M2_mm"),
            ("diag2", "diag2_M3_M4_mm"),
        ):
            try:
                global_value = float(global_diagonals[diagonal])
                head_value = float(head_diagonals[diagonal])
                tail_value = float(tail_diagonals[diagonal])
            except (KeyError, TypeError, ValueError) as exc:
                raise RuntimeError(f"Delivered geometry output has an invalid {diagonal} value") from exc
            if not all(math.isfinite(value) for value in (global_value, head_value, tail_value)):
                raise RuntimeError(f"Delivered geometry output has a non-finite {diagonal} value")
            merged[target] = f"{global_value:.6f}"
            merged[f"delivery_head_{diagonal}_mm"] = f"{head_value:.6f}"
            merged[f"delivery_tail_{diagonal}_mm"] = f"{tail_value:.6f}"

    corner_geometry = payload.get("corner_geometry")
    if corner_geometry is not None:
        if not isinstance(corner_geometry, dict):
            raise RuntimeError("Delivered geometry output has invalid four-corner values")
        field_map = {
            "main_face_angle_deg": "verticality_error_deg",
            "arc_length_mm": "chamfer_mm",
            "projection_1_mm": "projection_x_mm",
            "projection_2_mm": "projection_y_mm",
        }
        for obj in (1, 2, 3, 4):
            values = corner_geometry.get(str(obj))
            if not isinstance(values, dict):
                raise RuntimeError(f"Delivered geometry output is missing corner {obj}")
            for source_field, target_suffix in field_map.items():
                try:
                    value = float(values[source_field])
                except (KeyError, TypeError, ValueError) as exc:
                    raise RuntimeError(
                        f"Delivered geometry output has an invalid corner {obj} {source_field}"
                    ) from exc
                if not math.isfinite(value):
                    raise RuntimeError(
                        f"Delivered geometry output has a non-finite corner {obj} {source_field}"
                    )
                target = f"obj{obj}_{target_suffix}"
                if target_suffix == "verticality_error_deg":
                    merged[f"obj{obj}_main_face_angle_deg"] = f"{value:.6f}"
                merged[target] = f"{value:.6f}"
    return merged


def replace_measurement_mean(path: Path, summary: dict[str, str]) -> None:
    rows = read_csv_rows(path)
    mean_index = next((index for index, row in enumerate(rows) if row.get("record") == "mean"), None)
    if mean_index is None:
        raise RuntimeError("Measurement CSV lost its mean row before ABCD merge")
    rows[mean_index] = summary.copy()
    fields: list[str] = []
    for row in rows:
        for field in row:
            if field not in fields:
                fields.append(field)
    temporary = path.with_suffix(path.suffix + ".tmp")
    with temporary.open("w", encoding="utf-8-sig", newline="") as handle:
        writer = csv.DictWriter(handle, fieldnames=fields)
        writer.writeheader()
        writer.writerows(rows)
    os.replace(temporary, path)


def write_delivery_measurement_summary(path: Path, summary: dict[str, str]) -> None:
    temporary = path.with_suffix(path.suffix + ".tmp")
    with temporary.open("w", encoding="utf-8-sig", newline="") as handle:
        writer = csv.DictWriter(handle, fieldnames=list(summary))
        writer.writeheader()
        writer.writerow(summary)
    os.replace(temporary, path)


def run_measurement(config: dict[str, Any], input_path: Path) -> dict[str, Any]:
    """Measure one input, preserve its slice CSV, and append global statistics."""

    mode = (
        str(config.get("endface_measurement_mode", "release_corrected"))
        if END_FACE_ONLY
        else "release_corrected"
    )
    if END_FACE_ONLY:
        required_paths = ["script_path", "calibration_path"]
        if mode == "release_corrected":
            required_paths.extend(["drift_calibration_path", "endface_calibration_path"])
    else:
        required_paths = ["delivery_abcd_script_path", "delivery_abcd_calibration_path"]
    for name in required_paths:
        if not Path(config[name]).is_file():
            raise RuntimeError(f"Configured {name.replace('_', ' ')} was not found")
    output_root = Path(config["output_dir"])
    _, developer_details_dir = prepare_result_directories(output_root)
    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    bar_id = bar_id_from_input(input_path)
    capture_id = input_path.name if input_path.is_dir() else input_path.stem
    output_path = developer_details_dir / f"{bar_id}_{capture_id}_{timestamp}_measure.csv"
    subprocess_options: dict[str, Any] = {
        "cwd": str(PROJECT_DIR),
        "capture_output": True,
        "text": True,
        "timeout": 900,
    }
    # The measurement executable is a console program. Hide its transient
    # console window when it is launched by the web dashboard on Windows.
    if os.name == "nt":
        subprocess_options["creationflags"] = subprocess.CREATE_NO_WINDOW
    delivery_abcd_payload: dict[str, Any] | None = None
    delivery_abcd_output_path: Path | None = None
    if END_FACE_ONLY:
        command, mode = build_measurement_command(config, input_path, output_path)
        completed = subprocess.run(command, **subprocess_options)
        if completed.returncode != 0:
            raise RuntimeError(
                completed.stderr.strip()
                or completed.stdout.strip()
                or "Measurement script failed"
            )
        result = mean_measurement(output_path)
        raw_summary = result["summary"]
    else:
        delivery_abcd_output_path = developer_details_dir / f"{bar_id}_{capture_id}_{timestamp}_delivery_geometry.json"
        delivery_command = build_delivery_abcd_command(
            config,
            input_path,
            delivery_abcd_output_path,
        )
        delivery_completed = subprocess.run(delivery_command, **subprocess_options)
        if delivery_completed.returncode != 0:
            raise RuntimeError(
                delivery_completed.stderr.strip()
                or delivery_completed.stdout.strip()
                or "Delivered geometry measurement failed"
            )
        try:
            delivery_abcd_payload = json.loads(
                delivery_abcd_output_path.read_text(encoding="utf-8")
            )
        except (OSError, json.JSONDecodeError) as exc:
            raise RuntimeError("Cannot read the delivered geometry result JSON") from exc
        raw_summary = merge_delivery_abcd_summary({}, delivery_abcd_payload)
        raw_summary["record"] = "mean"
        raw_summary["measurement_valid"] = "True"
        write_delivery_measurement_summary(output_path, raw_summary)
        result = {
            "summary": raw_summary,
            "csv_path": str(output_path),
            "slice_count": len(delivery_abcd_payload.get("slice_records", [])),
        }
        completed = delivery_completed
    corrected_summary = (
        raw_summary.copy()
        if END_FACE_ONLY
        else apply_manual_offsets(
            raw_summary,
            config["edge_offsets_mm"],
            config["diagonal_offsets_mm"],
            config["length_offset_mm"],
        )
    )
    result["raw_summary"] = raw_summary
    result["corrected_summary"] = corrected_summary
    result["delivery_abcd"] = {
        "applied": delivery_abcd_payload is not None,
        "method": raw_summary.get("delivery_abcd_method", ""),
        "aggregation": raw_summary.get("delivery_abcd_aggregation", ""),
        "result_json_path": str(delivery_abcd_output_path or ""),
        "head_edges_mm": delivery_abcd_payload.get("head_edges_mm", {}) if delivery_abcd_payload else {},
        "tail_edges_mm": delivery_abcd_payload.get("tail_edges_mm", {}) if delivery_abcd_payload else {},
    }
    result["delivery_geometry"] = {
        "applied": delivery_abcd_payload is not None,
        "method": raw_summary.get("delivery_geometry_method", ""),
        "aggregation": raw_summary.get("delivery_geometry_aggregation", ""),
        "result_json_path": str(delivery_abcd_output_path or ""),
        "head_edges_mm": delivery_abcd_payload.get("head_edges_mm", {}) if delivery_abcd_payload else {},
        "tail_edges_mm": delivery_abcd_payload.get("tail_edges_mm", {}) if delivery_abcd_payload else {},
        "head_diagonals_mm": delivery_abcd_payload.get("head_diagonals_mm", {}) if delivery_abcd_payload else {},
        "tail_diagonals_mm": delivery_abcd_payload.get("tail_diagonals_mm", {}) if delivery_abcd_payload else {},
        "corner_geometry": delivery_abcd_payload.get("corner_geometry", {}) if delivery_abcd_payload else {},
        "delivered_length_mm": delivery_abcd_payload.get("delivered_length_mm", "") if delivery_abcd_payload else "",
    }
    result["drift"] = {} if not END_FACE_ONLY else {
        key: raw_summary.get(key, "")
        for key in (
            "measurement_valid", "drift_status", "drift_detected", "drift_correction_applied",
            "drift_model_version", "drift_amplitude", "drift_confidence", "drift_fit_rmse_mm",
            "drift_correlation", "drift_camera_amplitude_spread", "drift_alignment_shift_fraction",
            "drift_sample_station_count", "drift_overlap_start_fraction", "drift_overlap_end_fraction",
            "drift_warning", "drift_reason",
            *[f"obj{obj}_{field}" for obj in (1, 2, 3, 4) for field in ("drift_x_mm", "drift_z_mm", "drift_amplitude")],
        )
    }
    result["relative_camera_geometry"] = {} if not END_FACE_ONLY else {
        key: raw_summary.get(key, "")
        for key in (
            "relative_camera_geometry_status",
            "relative_camera_geometry_warning",
            "relative_camera_geometry_correction_applied",
            "relative_camera_geometry_input_state",
            "relative_camera_max_seam_span_mm",
            "relative_camera_geometry_reason",
            *[
                f"relative_camera_{face}_{field}"
                for face in "ABCD"
                for field in ("seam_span_mm", "pair_angle_p90_deg")
            ],
        )
    }
    result["endface_calibration_applicability"] = {} if not END_FACE_ONLY else {
        key: raw_summary.get(key, "")
        for key in (
            "endface_calibration_status",
            "endface_calibration_applicability_status",
            "endface_calibration_correction_applied",
            "endface_calibration_applicability_reason",
            "endface_calibration_model_version",
            "endface_calibration_uncertainty_deg",
        )
    }
    result["endface_quality"] = {} if not END_FACE_ONLY else {
        key: raw_summary.get(key, "")
        for key in (
            "endface_raw_quality_status",
            "endface_raw_quality_accepted",
            "endface_raw_quality_reason",
            "endface_raw_quality_uses_professional_truth",
            "endface_raw_quality_correction_applied",
            "endface_raw_quality_plane_rmse_limit_mm",
            "endface_raw_quality_stable_seam_span_limit_mm",
            "endface_raw_quality_warning_seam_span_limit_mm",
            "head_endface_raw_plane_rmse_mm",
            "tail_endface_raw_plane_rmse_mm",
        )
    }
    if END_FACE_ONLY and mode == "raw_audit":
        correction_flags = {
            "mechanical_drift": raw_summary.get("drift_correction_applied", ""),
            "relative_camera_geometry": raw_summary.get(
                "relative_camera_geometry_correction_applied", ""
            ),
            "endface_physical": raw_summary.get(
                "endface_calibration_correction_applied", ""
            ),
        }
        if any(str(value).strip().lower() == "true" for value in correction_flags.values()):
            raise RuntimeError(
                "Raw audit mode detected an unexpected correction: "
                + ", ".join(
                    name for name, value in correction_flags.items()
                    if str(value).strip().lower() == "true"
                )
            )
        if str(
            raw_summary.get("endface_raw_quality_uses_professional_truth", "")
        ).strip().lower() != "false":
            raise RuntimeError("Raw end-face quality gate must not use professional truth")
        if str(
            raw_summary.get("endface_raw_quality_correction_applied", "")
        ).strip().lower() != "false":
            raise RuntimeError("Raw end-face quality gate must not apply corrections")
        for end in ("head", "tail"):
            for channel in WIREFRAME_LOCAL_CHANNELS:
                product = raw_summary.get(f"{end}_{channel}_endface_angle_deg", "")
                raw = raw_summary.get(f"{end}_{channel}_endface_raw_angle_deg", "")
                if not summary_values_equal(product, raw):
                    raise RuntimeError(
                        f"Raw audit mode changed {end}_{channel}: raw={raw}, product={product}"
                    )
            product_representative = raw_summary.get(
                f"{end}_endface_representative_angle_deg", ""
            )
            raw_representative = raw_summary.get(
                f"{end}_endface_raw_representative_angle_deg", ""
            )
            if not summary_values_equal(product_representative, raw_representative):
                raise RuntimeError(
                    f"Raw audit mode changed {end} representative angle: "
                    f"raw={raw_representative}, product={product_representative}"
                )
    result["statistics_csv_path"] = str(
        append_measurement_statistics(
            output_root,
            input_path,
            output_path,
            result["slice_count"],
            corrected_summary,
        )
    )
    result["input_path"] = str(input_path)
    result["bar_id"] = bar_id
    result["endpoint_label_contract"] = {
        "head": "hobj_device_row_min",
        "tail": "hobj_device_row_max",
        "physical_R_L_inferred": False,
    }
    result["endface_measurement_mode"] = mode
    result["runtime_professional_truth_lookup"] = False
    result["final_endface_angle_correction_applied"] = False
    result["algorithm_source"] = "endface_runtime" if END_FACE_ONLY else "delivered_geometry_only"
    result["native_measurement_applied"] = END_FACE_ONLY
    result["console"] = completed.stdout.strip()
    result["truth_check"] = (
        {
            "available": False,
            "message": "End-face runtime never reads institution/manual end-face truth.",
        }
        if END_FACE_ONLY
        else {
            "available": False,
            "message": "Global runtime uses only the delivered geometry package and does not read manual truth.",
        }
    )
    return result


class ContinuousMeasurementMonitor(threading.Thread):
    """Runs each stable HOBJ once after the user manually enables continuous mode."""

    def __init__(self) -> None:
        super().__init__(name="hobj-continuous-monitor", daemon=True)
        self.seen = read_continuous_seen()

    def reload_seen(self) -> None:
        with CONTINUOUS_STATE_LOCK:
            self.seen = read_continuous_seen()

    def save_seen(self) -> None:
        with CONTINUOUS_STATE_LOCK:
            CONTINUOUS_STATE_PATH.write_text(json.dumps(self.seen, ensure_ascii=False, indent=2), encoding="utf-8")

    def run(self) -> None:
        while True:
            with CONFIG_LOCK:
                config = load_config()
            if not config.get("continuous_measure_enabled", False):
                time.sleep(1)
                continue
            with STATISTICS_LOCK:
                refresh_statistics_csv(Path(config["output_dir"]))
            self.reload_seen()
            root = Path(config["data_root"])
            try:
                files = sorted(root.rglob("*.hobj")) if root.is_dir() else []
            except OSError:
                files = []
            for path in files:
                with CONFIG_LOCK:
                    current_config = load_config()
                if not current_config.get("continuous_measure_enabled", False):
                    break
                try:
                    stat = path.stat()
                    signature = [stat.st_mtime_ns, stat.st_size]
                except OSError:
                    continue
                key = str(path.resolve())
                if self.seen.get(key) == signature:
                    continue
                if time.time() - stat.st_mtime < float(current_config.get("stable_file_seconds", 30)):
                    continue
                try:
                    result = run_measurement(current_config, path)
                    with LATEST_RESULT_LOCK:
                        global LATEST_RESULT
                        LATEST_RESULT = result
                    self.seen[key] = signature
                    self.save_seen()
                    print(f"Continuous measurement complete: {path}")
                except Exception as exc:  # Keep failed or incomplete files eligible for a later retry.
                    print(f"Continuous measurement pending for {path}: {exc}")
            time.sleep(max(1, int(config.get("continuous_scan_seconds", 10))))


class DashboardHandler(SimpleHTTPRequestHandler):
    def __init__(self, *args: Any, **kwargs: Any) -> None:
        super().__init__(*args, directory=str(DASHBOARD_DIR), **kwargs)

    def log_message(self, _format: str, *_args: Any) -> None:
        return

    def send_json(self, payload: Any, status: int = HTTPStatus.OK) -> None:
        data = json.dumps(payload, ensure_ascii=False).encode("utf-8")
        self.send_response(status)
        self.send_header("Content-Type", "application/json; charset=utf-8")
        self.send_header("Content-Length", str(len(data)))
        self.end_headers()
        self.wfile.write(data)

    def read_body(self) -> dict[str, Any]:
        length = int(self.headers.get("Content-Length", "0"))
        try:
            return json.loads(self.rfile.read(length).decode("utf-8")) if length else {}
        except json.JSONDecodeError as exc:
            raise ValueError("Request body must be valid JSON") from exc

    def do_GET(self) -> None:
        route = urlparse(self.path).path
        with CONFIG_LOCK:
            config = load_config()
        if route == "/api/config":
            self.send_json(public_config(config))
        elif route == "/api/inputs":
            self.send_json({"files": discover_inputs(config["data_root"]), "data_root": config["data_root"]})
        elif route == "/api/calibration":
            self.send_json(calibration_summary(config))
        elif route == "/api/compensation-log":
            self.send_json(compensation_log_summary(config))
        elif route == "/api/latest-result":
            with LATEST_RESULT_LOCK:
                result = LATEST_RESULT
            self.send_json({"available": result is not None, "result": result})
        else:
            super().do_GET()

    def do_POST(self) -> None:
        route = urlparse(self.path).path
        try:
            body = self.read_body()
            if route == "/api/config":
                with CONFIG_LOCK:
                    config = save_config(body)
                self.send_json(public_config(config))
                return
            with CONFIG_LOCK:
                config = load_config()
            if route != "/api/measure":
                self.send_json({"error": "Route not found"}, HTTPStatus.NOT_FOUND)
                return
            input_path = Path(str(body.get("input_path", "")))
            if not ((input_path.is_file() and input_path.suffix.lower() == ".hobj") or input_path.is_dir()):
                raise ValueError("Select a valid HOBJ file or a folder containing four TIFF files")
            ensure_under_root(input_path, Path(config["data_root"]))
            result = run_measurement(config, input_path)
            mark_continuous_capture_processed(input_path)
            self.send_json(result)
        except subprocess.TimeoutExpired:
            self.send_json({"error": "Measurement timed out after 15 minutes."}, HTTPStatus.GATEWAY_TIMEOUT)
        except (ValueError, RuntimeError, OSError) as exc:
            self.send_json({"error": str(exc)}, HTTPStatus.BAD_REQUEST)


def main() -> None:
    DASHBOARD_DIR.mkdir(parents=True, exist_ok=True)
    ContinuousMeasurementMonitor().start()
    port = int(os.environ.get("SUNSHINE_DASHBOARD_PORT", "8765"))
    server = ThreadingHTTPServer(("127.0.0.1", port), DashboardHandler)
    if sys.stdout is not None:
        print(f"Square Rod Dashboard is running at http://127.0.0.1:{port}")
        print("Press Ctrl+C to stop.")
    try:
        server.serve_forever()
    except KeyboardInterrupt:
        if sys.stdout is not None:
            print("\nDashboard stopped.")
    finally:
        server.server_close()


if __name__ == "__main__":
    main()
