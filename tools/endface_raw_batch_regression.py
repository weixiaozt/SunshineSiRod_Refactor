#!/usr/bin/env python3
"""Run the production M0 Raw end-face path over existing HOBJ datasets."""

from __future__ import annotations

import argparse
import csv
import json
import math
import re
import subprocess
import sys
from concurrent.futures import ThreadPoolExecutor, as_completed
from pathlib import Path
from typing import Dict, Iterable

import numpy as np

try:
    from .endface_wireframe_geometry import LOCAL_CHANNELS
    from .endface_wireframe_turnover_audit import turnover_equivariance_rows
except ImportError:
    from endface_wireframe_geometry import LOCAL_CHANNELS
    from endface_wireframe_turnover_audit import turnover_equivariance_rows


ROOT = Path(__file__).resolve().parent.parent
MEASURE_SCRIPT = ROOT / "tools" / "measure_square_rod_edges.py"
TURNOVER_MARKERS = ("tail_to_head", "turnover", "diaotou", "调头")
SUMMARY_FIELDS = (
    "endface_raw_quality_status",
    "endface_raw_quality_accepted",
    "endface_raw_quality_reason",
    "endface_raw_quality_uses_professional_truth",
    "endface_raw_quality_correction_applied",
    "head_endface_raw_plane_rmse_mm",
    "tail_endface_raw_plane_rmse_mm",
    "relative_camera_geometry_status",
    "relative_camera_max_seam_span_mm",
    "head_endface_raw_representative_angle_deg",
    "tail_endface_raw_representative_angle_deg",
    "head_endface_raw_worst_local_channel",
    "tail_endface_raw_worst_local_channel",
    "head_endface_raw_wireframe_rms_error_deg",
    "tail_endface_raw_wireframe_rms_error_deg",
    "head_endface_raw_max_dual_camera_seam_mm",
    "tail_endface_raw_max_dual_camera_seam_mm",
    "head_endface_max_corner_closure_gap_mm",
    "tail_endface_max_corner_closure_gap_mm",
    "head_endface_wireframe_diagonal_twist_mm",
    "tail_endface_wireframe_diagonal_twist_mm",
)


def read_mean(path: Path) -> Dict[str, str]:
    with path.open("r", encoding="utf-8-sig", newline="") as handle:
        for row in csv.DictReader(handle):
            if row.get("record") == "mean":
                return row
    raise ValueError(f"No mean row in {path}")


def write_csv(path: Path, rows: list[Dict[str, object]]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    fieldnames: list[str] = []
    seen: set[str] = set()
    for row in rows:
        for key in row:
            if key not in seen:
                seen.add(key)
                fieldnames.append(key)
    with path.open("w", encoding="utf-8-sig", newline="") as handle:
        writer = csv.DictWriter(handle, fieldnames=fieldnames)
        writer.writeheader()
        writer.writerows(rows)


def safe_name(relative: Path) -> str:
    value = "__".join(relative.with_suffix("").parts)
    return re.sub(r"[^0-9A-Za-z._-]+", "_", value).strip("_") or "capture"


def capture_group(relative: Path) -> str:
    text = str(relative).casefold()
    return "tail_to_head" if any(marker.casefold() in text for marker in TURNOVER_MARKERS) else "head_to_tail"


def is_true(value: object) -> bool:
    return str(value).strip().casefold() == "true"


def finite_values(rows: Iterable[Dict[str, object]], field: str) -> np.ndarray:
    values = []
    for row in rows:
        try:
            value = float(row[field])
        except (KeyError, TypeError, ValueError):
            continue
        if math.isfinite(value):
            values.append(value)
    return np.asarray(values, dtype=float)


def run_capture(
    dataset: str,
    group: str,
    root: Path,
    hobj: Path,
    camera_calibration: Path,
    output_dir: Path,
    step_mm: float,
) -> Dict[str, object]:
    relative = hobj.relative_to(root)
    capture_output = output_dir / "captures" / dataset / f"{safe_name(relative)}_measure.csv"
    capture_output.parent.mkdir(parents=True, exist_ok=True)
    if not capture_output.is_file():
        command = [
            sys.executable,
            str(MEASURE_SCRIPT),
            "--input",
            str(hobj),
            "--calibration",
            str(camera_calibration),
            "--output",
            str(capture_output),
            "--overwrite",
            "--step-mm",
            str(step_mm),
            "--orientation",
            "normal",
            "--endface-only",
        ]
        completed = subprocess.run(
            command,
            cwd=ROOT,
            capture_output=True,
            text=True,
            timeout=900,
        )
        if completed.returncode != 0:
            return {
                "dataset": dataset,
                "group": group,
                "capture_id": hobj.stem,
                "relative_path": str(relative),
                "input_path": str(hobj),
                "measurement_csv": str(capture_output),
                "processing_status": "error",
                "processing_error": completed.stderr.strip() or completed.stdout.strip(),
                "uses_professional_truth": False,
                "correction_applied": False,
            }
    try:
        mean = read_mean(capture_output)
    except (OSError, ValueError) as exc:
        capture_output.unlink(missing_ok=True)
        return {
            "dataset": dataset,
            "group": group,
            "capture_id": hobj.stem,
            "relative_path": str(relative),
            "input_path": str(hobj),
            "measurement_csv": str(capture_output),
            "processing_status": "error",
            "processing_error": str(exc),
            "uses_professional_truth": False,
            "correction_applied": False,
        }
    row: Dict[str, object] = {
        "dataset": dataset,
        "group": group,
        "capture_id": hobj.stem,
        "relative_path": str(relative),
        "input_path": str(hobj),
        "measurement_csv": str(capture_output),
        "processing_status": "ok",
        "uses_professional_truth": False,
        "correction_applied": False,
    }
    for field in SUMMARY_FIELDS:
        row[field] = mean.get(field, "")
    for end in ("head", "tail"):
        for channel in LOCAL_CHANNELS:
            value = mean.get(f"{end}_{channel}_endface_raw_angle_deg", "")
            row[f"{end}_{channel}_raw_angle_deg"] = value
            row[f"{end}_{channel}_angle_deg"] = value
    return row


def dataset_summary(rows: list[Dict[str, object]]) -> Dict[str, object]:
    valid = [
        row
        for row in rows
        if row.get("processing_status") == "ok"
        and is_true(row.get("endface_raw_quality_accepted"))
    ]
    status_counts: Dict[str, int] = {}
    for row in rows:
        status = str(row.get("endface_raw_quality_status") or row.get("processing_status") or "unknown")
        status_counts[status] = status_counts.get(status, 0) + 1
    summary: Dict[str, object] = {
        "capture_count": len(rows),
        "processed_count": sum(row.get("processing_status") == "ok" for row in rows),
        "accepted_count": len(valid),
        "quality_status_counts": status_counts,
        "uses_professional_truth": False,
        "correction_applied": False,
    }
    for field in (
        "relative_camera_max_seam_span_mm",
        "head_endface_raw_plane_rmse_mm",
        "tail_endface_raw_plane_rmse_mm",
        "head_endface_raw_representative_angle_deg",
        "tail_endface_raw_representative_angle_deg",
    ):
        values = finite_values(valid, field)
        if len(values):
            summary[f"{field}_median"] = float(np.median(values))
            summary[f"{field}_range"] = float(np.ptp(values))
            summary[f"{field}_maximum"] = float(np.max(values))
    groups = {str(row.get("group")) for row in valid}
    if {"head_to_tail", "tail_to_head"}.issubset(groups):
        equivariance = turnover_equivariance_rows(valid)
        residuals = finite_values(equivariance, "group_median_sum_residual_deg")
        summary["turnover_equivariance"] = {
            "channel_count": len(equivariance),
            "group_median_rms_deg": float(np.sqrt(np.mean(np.square(residuals)))),
            "group_median_max_abs_deg": float(np.max(np.abs(residuals))),
            "uses_professional_truth": False,
            "correction_applied": False,
        }
    return summary


def parse_dataset(value: str) -> tuple[str, Path]:
    if "=" not in value:
        raise argparse.ArgumentTypeError("Dataset must be NAME=PATH")
    name, path = value.split("=", 1)
    name = name.strip()
    root = Path(path.strip()).expanduser().resolve()
    if not name or not root.is_dir():
        raise argparse.ArgumentTypeError(f"Invalid dataset: {value}")
    return name, root


def parse_ordinal_split(value: str) -> tuple[str, int]:
    if "=" not in value:
        raise argparse.ArgumentTypeError("Ordinal split must be NAME=COUNT")
    name, count_text = value.split("=", 1)
    try:
        count = int(count_text)
    except ValueError as exc:
        raise argparse.ArgumentTypeError(f"Invalid ordinal split: {value}") from exc
    if not name.strip() or count <= 0:
        raise argparse.ArgumentTypeError(f"Invalid ordinal split: {value}")
    return name.strip(), count


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--dataset", action="append", required=True, type=parse_dataset)
    parser.add_argument(
        "--ordinal-turnover-split",
        action="append",
        default=[],
        type=parse_ordinal_split,
        help="For a single-bar dataset without path markers, NAME=N assigns the first N sorted files to head_to_tail and the rest to tail_to_head.",
    )
    parser.add_argument("--camera-calibration", required=True, type=Path)
    parser.add_argument("--output-dir", required=True, type=Path)
    parser.add_argument("--step-mm", type=float, default=10.0)
    parser.add_argument("--workers", type=int, default=2)
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    camera_calibration = args.camera_calibration.resolve()
    if not camera_calibration.is_file():
        raise SystemExit(f"Camera calibration not found: {camera_calibration}")
    output_dir = args.output_dir.resolve()
    output_dir.mkdir(parents=True, exist_ok=True)
    ordinal_splits = dict(args.ordinal_turnover_split)
    unknown_splits = set(ordinal_splits) - {name for name, _ in args.dataset}
    if unknown_splits:
        raise SystemExit(f"Ordinal split references unknown datasets: {sorted(unknown_splits)}")
    jobs = []
    for name, root in args.dataset:
        paths = sorted(root.rglob("*.hobj"))
        split = ordinal_splits.get(name)
        if split is not None and split >= len(paths):
            raise SystemExit(f"Ordinal split for {name} must be smaller than {len(paths)}")
        for index, hobj in enumerate(paths):
            group = (
                ("head_to_tail" if index < split else "tail_to_head")
                if split is not None
                else capture_group(hobj.relative_to(root))
            )
            jobs.append((name, group, root, hobj))
    rows: list[Dict[str, object]] = []
    with ThreadPoolExecutor(max_workers=max(1, args.workers)) as executor:
        futures = {
            executor.submit(
                run_capture,
                name,
                group,
                root,
                hobj,
                camera_calibration,
                output_dir,
                args.step_mm,
            ): (name, hobj)
            for name, group, root, hobj in jobs
        }
        for index, future in enumerate(as_completed(futures), start=1):
            name, hobj = futures[future]
            row = future.result()
            rows.append(row)
            print(
                f"[{index}/{len(jobs)}] {name}/{hobj.name}: "
                f"{row.get('endface_raw_quality_status', row.get('processing_status'))}"
            )
    rows.sort(key=lambda row: (str(row.get("dataset")), str(row.get("relative_path"))))
    measurements_csv = output_dir / "endface_raw_batch_measurements.csv"
    write_csv(measurements_csv, rows)
    equivariance_rows: list[Dict[str, object]] = []
    summaries: Dict[str, object] = {}
    for name, _ in args.dataset:
        dataset_rows = [row for row in rows if row.get("dataset") == name]
        summaries[name] = dataset_summary(dataset_rows)
        accepted = [
            row
            for row in dataset_rows
            if row.get("processing_status") == "ok"
            and is_true(row.get("endface_raw_quality_accepted"))
        ]
        if {str(row.get("group")) for row in accepted} >= {"head_to_tail", "tail_to_head"}:
            for row in turnover_equivariance_rows(accepted):
                equivariance_rows.append({"dataset": name, **row})
    equivariance_csv = output_dir / "endface_raw_turnover_equivariance.csv"
    write_csv(equivariance_csv, equivariance_rows)
    audit = {
        "algorithm": "M0_raw_truth_free_16_local_endface_angles",
        "camera_geometry_model": str(camera_calibration),
        "dataset_count": len(args.dataset),
        "capture_count": len(rows),
        "uses_professional_truth": False,
        "mechanical_drift_model_loaded": False,
        "endface_correction_model_loaded": False,
        "final_angle_correction_applied": False,
        "quality_contract": {
            "plane_rmse_reject_limit_mm": 0.50,
            "same_face_seam_stable_limit_mm": 0.60,
            "same_face_seam_warning_limit_mm": 0.80,
            "warning_values_retained": True,
            "rejected_raw_values_retained_for_audit": True,
        },
        "datasets": summaries,
        "measurements_csv": str(measurements_csv),
        "turnover_equivariance_csv": str(equivariance_csv),
    }
    summary_json = output_dir / "endface_raw_batch_summary.json"
    summary_json.write_text(json.dumps(audit, ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"Measurements: {measurements_csv}")
    print(f"Turnover audit: {equivariance_csv}")
    print(f"Summary: {summary_json}")
    return 1 if any(row.get("processing_status") != "ok" for row in rows) else 0


if __name__ == "__main__":
    raise SystemExit(main())
