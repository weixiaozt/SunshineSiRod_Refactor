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


FROZEN_APP = bool(getattr(sys, "frozen", False))
TOOLS_DIR = Path(getattr(sys, "_MEIPASS", Path(__file__).resolve().parent))
RUNTIME_DIR = Path(sys.executable).resolve().parent if FROZEN_APP else TOOLS_DIR
PROJECT_DIR = RUNTIME_DIR if FROZEN_APP else TOOLS_DIR.parent
DASHBOARD_DIR = TOOLS_DIR / "dashboard"
CONFIG_PATH = RUNTIME_DIR / "web_ui_config.local.json"
CONTINUOUS_STATE_PATH = RUNTIME_DIR / "dashboard_continuous_seen.local.json"
DEFAULT_CONFIG = {
    "data_root": str(RUNTIME_DIR.parent / "hobj") if FROZEN_APP else r"D:\Image_risen",
    "output_dir": str(RUNTIME_DIR.parent / "results" / "measurements") if FROZEN_APP else str(TOOLS_DIR / "results" / "measurements"),
    "script_path": str(RUNTIME_DIR.parent / "MeasureSquareRod" / "MeasureSquareRod.exe") if FROZEN_APP else str(TOOLS_DIR / "measure_square_rod_edges.py"),
    "calibration_path": str(TOOLS_DIR / "calibration" / "models" / "camera_calibration_model_210_105.json"),
    "drift_calibration_path": str(TOOLS_DIR / "calibration" / "models" / "mechanical_drift_model_210_105.json"),
    "endface_calibration_path": str(TOOLS_DIR / "calibration" / "models" / "endface_calibration_model_210_105.json"),
    "truth_csv_path": str(TOOLS_DIR / "calibration" / "truth" / "210_105.csv"),
    "step_mm": 10.0,
    "edge_offsets_mm": {"A": 0.0, "B": 0.0, "C": 0.0, "D": 0.0},
    "diagonal_offsets_mm": {"diag1": 0.0, "diag2": 0.0},
    "length_offset_mm": 0.0,
    "endface_angle_offsets_deg": {
        "head": {"A": 0.0, "B": 0.0, "C": 0.0, "D": 0.0},
        "tail": {"A": 0.0, "B": 0.0, "C": 0.0, "D": 0.0},
    },
    "continuous_measure_enabled": False,
    "continuous_scan_seconds": 10,
    "stable_file_seconds": 30,
}
CONFIG_LOCK = threading.Lock()
STATISTICS_LOCK = threading.Lock()
CONTINUOUS_STATE_LOCK = threading.Lock()
LATEST_RESULT_LOCK = threading.Lock()
LATEST_RESULT: dict[str, Any] | None = None


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
                if key == "truth_csv_path" or value not in ("", None):
                    config[key] = value
        except (OSError, json.JSONDecodeError):
            pass
    for key in ("data_root", "output_dir", "script_path", "calibration_path", "drift_calibration_path", "endface_calibration_path", "truth_csv_path"):
        config[key] = resolved(str(config.get(key, "")))
    try:
        config["step_mm"] = float(config.get("step_mm", 10.0))
    except (TypeError, ValueError):
        config["step_mm"] = 10.0
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
    endface_offsets = config.get("endface_angle_offsets_deg", {})
    config["endface_angle_offsets_deg"] = {"head": {}, "tail": {}}
    for end in ("head", "tail"):
        end_values = endface_offsets.get(end, {}) if isinstance(endface_offsets, dict) else {}
        for face in ("A", "B", "C", "D"):
            try:
                config["endface_angle_offsets_deg"][end][face] = float(end_values.get(face, 0.0))
            except (AttributeError, TypeError, ValueError):
                config["endface_angle_offsets_deg"][end][face] = 0.0
    return config


def save_config(settings: dict[str, Any]) -> dict[str, Any]:
    config = load_config()
    for key in ("data_root", "output_dir", "script_path", "calibration_path", "drift_calibration_path", "endface_calibration_path", "truth_csv_path"):
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
        provided = settings["endface_angle_offsets_deg"]
        if not isinstance(provided, dict):
            raise ValueError("End-face angle compensation must be an object")
        for end in ("head", "tail"):
            end_values = provided.get(end, {})
            if not isinstance(end_values, dict):
                raise ValueError(f"End-face compensation for {end} must be an object")
            for face in ("A", "B", "C", "D"):
                try:
                    config["endface_angle_offsets_deg"][end][face] = float(end_values.get(face, 0.0))
                except (TypeError, ValueError) as exc:
                    raise ValueError(f"End-face compensation for {end}-{face} must be a number") from exc
    if "continuous_measure_enabled" in settings:
        value = settings["continuous_measure_enabled"]
        if not isinstance(value, bool):
            raise ValueError("Continuous measurement state must be true or false")
        config["continuous_measure_enabled"] = value
    CONFIG_PATH.write_text(json.dumps(config, ensure_ascii=False, indent=2), encoding="utf-8")
    return config


def public_config(config: dict[str, Any]) -> dict[str, Any]:
    return {**config, "exists": {key: bool(value and Path(value).exists()) for key, value in config.items() if key.endswith("_path") or key in {"data_root", "output_dir"}}}


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
    endface_offsets: dict[str, dict[str, float]],
) -> dict[str, str]:
    """Return a corrected copy; source measurement CSV values are never overwritten."""
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
    for end in ("head", "tail"):
        corrected_angles: list[float] = []
        for face in ("A", "B", "C", "D"):
            field = f"{end}_{face}_endface_angle_deg"
            value = field_number(summary, field)
            if value is None:
                continue
            corrected_value = value + float(endface_offsets.get(end, {}).get(face, 0.0))
            corrected[field] = f"{corrected_value:.6f}"
            corrected_angles.append(corrected_value)
        if len(corrected_angles) == 4:
            corrected[f"{end}_endface_verticality_deg"] = f"{sum(abs(90.0 - value) for value in corrected_angles) / 4.0:.6f}"
    return corrected


STATISTICS_VALUE_FIELDS = [
    "measurement_valid", "drift_status", "drift_correction_applied", "drift_model_version",
    "drift_amplitude", "drift_confidence", "drift_fit_rmse_mm", "drift_correlation",
    "drift_camera_amplitude_spread",
    *[f"obj{obj}_{field}" for obj in (1, 2, 3, 4) for field in ("drift_x_mm", "drift_z_mm", "drift_amplitude")],
    "A_mm", "B_mm", "C_mm", "D_mm",
    "diag1_M1_M2_mm", "diag2_M3_M4_mm",
    "A_minus_C_mm", "B_minus_D_mm", "diagonal_difference_mm",
    "stick_length_mm", "head_endface_verticality_deg", "tail_endface_verticality_deg",
    *[f"{end}_{face}_endface_angle_deg" for end in ("head", "tail") for face in ("A", "B", "C", "D")],
    *[
        f"obj{obj}_{field}"
        for obj in (1, 2, 3, 4)
        for field in ("main_face_angle_deg", "chamfer_mm", "projection_x_mm", "projection_y_mm")
    ],
]


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
    output_dir: Path,
    input_path: Path,
    slice_csv_path: Path,
    slice_count: int,
    displayed: dict[str, str],
) -> Path:
    """Persist one dashboard row, then refresh the human-readable CSV when possible."""
    output = output_dir / "measurement_statistics.csv"
    database = output_dir / "measurement_statistics.sqlite"
    fields = [
        "measured_at", "bar_id", "capture_id", "input_path", "slice_csv_path", "slice_count",
        *STATISTICS_VALUE_FIELDS,
    ]
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
        refresh_statistics_csv(output_dir)
    return output


def _ensure_statistics_database(database: Path, csv_snapshot: Path, fields: list[str]) -> None:
    """Create the durable statistics store and migrate an existing CSV once."""
    if database.exists():
        with closing(sqlite3.connect(database)) as connection:
            existing = {row[1] for row in connection.execute("PRAGMA table_info(measurement_statistics)")}
            for field in fields:
                if field not in existing:
                    connection.execute(f'ALTER TABLE measurement_statistics ADD COLUMN "{field}" TEXT')
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
        connection.commit()


def refresh_statistics_csv(output_dir: Path) -> bool:
    """Export the durable store to its one CSV snapshot without failing a measurement on a file lock."""
    output = output_dir / "measurement_statistics.csv"
    database = output_dir / "measurement_statistics.sqlite"
    if not database.is_file():
        return False
    fields = [
        "measured_at", "bar_id", "capture_id", "input_path", "slice_csv_path", "slice_count",
        *STATISTICS_VALUE_FIELDS,
    ]
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
        "endface_available": bool(endface_data),
        "endface_version": endface_data.get("version", "—"),
        "endface_model": endface_data.get("model", "—"),
        "drift_path": str(drift_path),
        "drift_available": bool(drift_data),
        "drift_version": drift_data.get("version", "-"),
        "drift_model": drift_data.get("model", "-"),
        "drift_normal_capture_count": drift_data.get("normal_capture_count", 0),
        "drift_abnormal_capture_count": drift_data.get("abnormal_capture_count", 0),
        "manual_edge_offsets_mm": config["edge_offsets_mm"],
        "manual_length_offset_mm": config["length_offset_mm"],
        "manual_endface_angle_offsets_deg": config["endface_angle_offsets_deg"],
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


def run_measurement(config: dict[str, Any], input_path: Path) -> dict[str, Any]:
    """Measure one input, preserve its slice CSV, and append global statistics."""
    for name in ("script_path", "calibration_path", "drift_calibration_path", "endface_calibration_path"):
        if not Path(config[name]).is_file():
            raise RuntimeError(f"Configured {name.replace('_', ' ')} was not found")
    output_dir = Path(config["output_dir"])
    output_dir.mkdir(parents=True, exist_ok=True)
    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    bar_id = bar_id_from_input(input_path)
    capture_id = input_path.name if input_path.is_dir() else input_path.stem
    output_path = output_dir / f"{bar_id}_{capture_id}_{timestamp}_measure.csv"
    script_path = Path(config["script_path"])
    command = ([str(script_path)] if script_path.suffix.lower() == ".exe" else [sys.executable, str(script_path)]) + [
        "--input", str(input_path),
        "--calibration", config["calibration_path"],
        "--drift-calibration", config["drift_calibration_path"],
        "--endface-calibration", config["endface_calibration_path"],
        "--output", str(output_path),
        "--overwrite", "--step-mm", str(config["step_mm"]),
    ]
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
    completed = subprocess.run(command, **subprocess_options)
    if completed.returncode != 0:
        raise RuntimeError(completed.stderr.strip() or completed.stdout.strip() or "Measurement script failed")
    result = mean_measurement(output_path)
    raw_summary = result["summary"]
    corrected_summary = apply_manual_offsets(
        raw_summary,
        config["edge_offsets_mm"],
        config["diagonal_offsets_mm"],
        config["length_offset_mm"],
        config["endface_angle_offsets_deg"],
    )
    result["raw_summary"] = raw_summary
    result["corrected_summary"] = corrected_summary
    result["drift"] = {
        key: raw_summary.get(key, "")
        for key in (
            "measurement_valid", "drift_status", "drift_detected", "drift_correction_applied",
            "drift_model_version", "drift_amplitude", "drift_confidence", "drift_fit_rmse_mm",
            "drift_correlation", "drift_camera_amplitude_spread",
            *[f"obj{obj}_{field}" for obj in (1, 2, 3, 4) for field in ("drift_x_mm", "drift_z_mm", "drift_amplitude")],
        )
    }
    result["statistics_csv_path"] = str(
        append_measurement_statistics(
            output_dir,
            input_path,
            output_path,
            result["slice_count"],
            corrected_summary,
        )
    )
    result["input_path"] = str(input_path)
    result["bar_id"] = bar_id
    result["console"] = completed.stdout.strip()
    result["truth_check"] = truth_check(config, str(input_path), raw_summary)
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
