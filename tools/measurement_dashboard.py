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
import subprocess
import sys
import threading
import time
from datetime import datetime
from http import HTTPStatus
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
from typing import Any
from urllib.parse import urlparse


TOOLS_DIR = Path(__file__).resolve().parent
PROJECT_DIR = TOOLS_DIR.parent
DASHBOARD_DIR = TOOLS_DIR / "dashboard"
CONFIG_PATH = TOOLS_DIR / "web_ui_config.local.json"
AUTO_STATE_PATH = TOOLS_DIR / "dashboard_auto_seen.local.json"
DEFAULT_CONFIG = {
    "data_root": r"D:\Image_risen",
    "output_dir": str(TOOLS_DIR / "dashboard_results"),
    "script_path": str(TOOLS_DIR / "measure_square_rod_edges.py"),
    "calibration_path": str(TOOLS_DIR / "camera_calibration_model.json"),
    "truth_csv_path": "",
    "step_mm": 10.0,
    "edge_offsets_mm": {"A": 0.0, "B": 0.0, "C": 0.0, "D": 0.0},
    "endface_angle_offsets_deg": {
        "head": {"A": 0.0, "B": 0.0, "C": 0.0, "D": 0.0},
        "tail": {"A": 0.0, "B": 0.0, "C": 0.0, "D": 0.0},
    },
    "auto_measure_enabled": False,
    "auto_scan_seconds": 10,
    "stable_file_seconds": 30,
}
CONFIG_LOCK = threading.Lock()


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
    for key in ("data_root", "output_dir", "script_path", "calibration_path", "truth_csv_path"):
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
    for key in ("data_root", "output_dir", "script_path", "calibration_path", "truth_csv_path"):
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
    if "auto_measure_enabled" in settings:
        value = settings["auto_measure_enabled"]
        if not isinstance(value, bool):
            raise ValueError("Automatic detection state must be true or false")
        config["auto_measure_enabled"] = value
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
    endface_offsets: dict[str, dict[str, float]],
) -> dict[str, str]:
    """Return a corrected copy; source measurement CSV values are never overwritten."""
    corrected = summary.copy()
    for edge in ("A", "B", "C", "D"):
        field = f"{edge}_mm"
        value = field_number(summary, field)
        if value is not None:
            corrected[field] = f"{value + float(edge_offsets.get(edge, 0.0)):.6f}"
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


def write_dashboard_summary(
    path: Path,
    raw: dict[str, str],
    corrected: dict[str, str],
    edge_offsets: dict[str, float],
    endface_offsets: dict[str, dict[str, float]],
) -> Path:
    output = path.with_name(f"{path.stem}_dashboard_summary.csv")
    fields = [
        "record", "A_mm", "B_mm", "C_mm", "D_mm",
        "diag1_M1_M2_mm", "diag2_M3_M4_mm", "stick_length_mm",
        "head_endface_verticality_deg", "tail_endface_verticality_deg",
        *[f"{end}_{face}_endface_angle_deg" for end in ("head", "tail") for face in ("A", "B", "C", "D")],
    ]
    with output.open("w", newline="", encoding="utf-8-sig") as handle:
        writer = csv.DictWriter(handle, fieldnames=fields)
        writer.writeheader()
        writer.writerow({"record": "raw_mean", **{key: raw.get(key, "") for key in fields if key != "record"}})
        writer.writerow({"record": "corrected_mean", **{key: corrected.get(key, "") for key in fields if key != "record"}})
        writer.writerow({"record": "manual_edge_offsets_mm", "A_mm": edge_offsets["A"], "B_mm": edge_offsets["B"], "C_mm": edge_offsets["C"], "D_mm": edge_offsets["D"]})
        writer.writerow({
            "record": "manual_endface_offsets_deg",
            **{
                f"{end}_{face}_endface_angle_deg": endface_offsets[end][face]
                for end in ("head", "tail") for face in ("A", "B", "C", "D")
            },
        })
    return output


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
    return {
        "available": True,
        "path": str(path),
        "version": data.get("version", "—"),
        "model": data.get("model", "—"),
        "note": data.get("note", ""),
        "corner_biases": biases,
        "standard": data.get("standard", {}),
        "manual_edge_offsets_mm": config["edge_offsets_mm"],
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


def run_auto_measurement(config: dict[str, Any], input_path: Path) -> None:
    """Measure one already-stable HOBJ using the same output naming as the UI."""
    if not Path(config["script_path"]).is_file() or not Path(config["calibration_path"]).is_file():
        raise RuntimeError("Measurement script or calibration model is unavailable")
    output_dir = Path(config["output_dir"])
    output_dir.mkdir(parents=True, exist_ok=True)
    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    bar_id = bar_id_from_input(input_path)
    output_path = output_dir / f"{bar_id}_{input_path.stem}_{timestamp}_measure.csv"
    command = [
        sys.executable, config["script_path"], "--input", str(input_path),
        "--calibration", config["calibration_path"], "--output", str(output_path),
        "--overwrite", "--step-mm", str(config["step_mm"]),
    ]
    completed = subprocess.run(command, cwd=str(PROJECT_DIR), capture_output=True, text=True, timeout=900)
    if completed.returncode != 0:
        raise RuntimeError(completed.stderr.strip() or completed.stdout.strip() or "Automatic measurement failed")
    measured = mean_measurement(output_path)
    write_dashboard_summary(
        output_path,
        measured["summary"],
        apply_manual_offsets(
            measured["summary"],
            config["edge_offsets_mm"],
            config["endface_angle_offsets_deg"],
        ),
        config["edge_offsets_mm"],
        config["endface_angle_offsets_deg"],
    )


class AutoMeasurementMonitor(threading.Thread):
    """Watches new, stable HOBJ files without reprocessing the initial archive."""

    def __init__(self) -> None:
        super().__init__(name="hobj-auto-monitor", daemon=True)
        self.seen: dict[str, list[int]] = {}
        if AUTO_STATE_PATH.exists():
            try:
                self.seen = json.loads(AUTO_STATE_PATH.read_text(encoding="utf-8"))
            except (OSError, json.JSONDecodeError):
                self.seen = {}
        self.initialized = AUTO_STATE_PATH.exists()

    def save_seen(self) -> None:
        AUTO_STATE_PATH.write_text(json.dumps(self.seen, ensure_ascii=False, indent=2), encoding="utf-8")

    def run(self) -> None:
        while True:
            with CONFIG_LOCK:
                config = load_config()
            root = Path(config["data_root"])
            changed = False
            if config.get("auto_measure_enabled", True) and root.is_dir():
                try:
                    files = list(root.rglob("*.hobj"))
                except OSError:
                    files = []
                if not self.initialized:
                    for path in files:
                        stat = path.stat()
                        self.seen[str(path.resolve())] = [stat.st_mtime_ns, stat.st_size]
                    self.initialized = True
                    changed = True
                else:
                    for path in files:
                        try:
                            stat = path.stat()
                        except OSError:
                            continue
                        key = str(path.resolve())
                        signature = [stat.st_mtime_ns, stat.st_size]
                        if self.seen.get(key) == signature:
                            continue
                        if time.time() - stat.st_mtime < float(config.get("stable_file_seconds", 30)):
                            continue
                        try:
                            run_auto_measurement(config, path)
                            self.seen[key] = signature
                            changed = True
                            print(f"Auto measured: {path}")
                        except Exception as exc:  # Keep it pending for a later retry after the file changes.
                            print(f"Auto measurement pending for {path}: {exc}")
            if changed:
                self.save_seen()
            time.sleep(max(3, int(config.get("auto_scan_seconds", 10))))


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
            for name in ("script_path", "calibration_path"):
                if not Path(config[name]).is_file():
                    raise ValueError(f"Configured {name.replace('_', ' ')} was not found")
            output_dir = Path(config["output_dir"])
            output_dir.mkdir(parents=True, exist_ok=True)
            timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
            bar_id = bar_id_from_input(input_path)
            capture_id = input_path.name if input_path.is_dir() else input_path.stem
            output_path = output_dir / f"{bar_id}_{capture_id}_{timestamp}_measure.csv"
            command = [
                sys.executable, config["script_path"], "--input", str(input_path),
                "--calibration", config["calibration_path"], "--output", str(output_path),
                "--overwrite", "--step-mm", str(config["step_mm"]),
            ]
            completed = subprocess.run(command, cwd=str(PROJECT_DIR), capture_output=True, text=True, timeout=900)
            if completed.returncode != 0:
                raise RuntimeError(completed.stderr.strip() or completed.stdout.strip() or "Measurement script failed")
            result = mean_measurement(output_path)
            raw_summary = result["summary"]
            corrected_summary = apply_manual_offsets(
                raw_summary,
                config["edge_offsets_mm"],
                config["endface_angle_offsets_deg"],
            )
            result["raw_summary"] = raw_summary
            result["corrected_summary"] = corrected_summary
            result["dashboard_summary_path"] = str(
                write_dashboard_summary(
                    output_path,
                    raw_summary,
                    corrected_summary,
                    config["edge_offsets_mm"],
                    config["endface_angle_offsets_deg"],
                )
            )
            result["input_path"] = str(input_path)
            result["bar_id"] = bar_id
            result["console"] = completed.stdout.strip()
            result["truth_check"] = truth_check(config, str(input_path), result["summary"])
            self.send_json(result)
        except subprocess.TimeoutExpired:
            self.send_json({"error": "Measurement timed out after 15 minutes."}, HTTPStatus.GATEWAY_TIMEOUT)
        except (ValueError, RuntimeError, OSError) as exc:
            self.send_json({"error": str(exc)}, HTTPStatus.BAD_REQUEST)


def main() -> None:
    DASHBOARD_DIR.mkdir(parents=True, exist_ok=True)
    AutoMeasurementMonitor().start()
    server = ThreadingHTTPServer(("127.0.0.1", 8765), DashboardHandler)
    print("Square Rod Dashboard is running at http://127.0.0.1:8765")
    print("Press Ctrl+C to stop.")
    try:
        server.serve_forever()
    except KeyboardInterrupt:
        print("\nDashboard stopped.")
    finally:
        server.server_close()


if __name__ == "__main__":
    main()
