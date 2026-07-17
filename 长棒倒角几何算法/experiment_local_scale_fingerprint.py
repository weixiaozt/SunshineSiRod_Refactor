#!/usr/bin/env python3
"""试验每相机局部射线尺度非线性，并用角位留出验证防止过拟合。"""

from __future__ import annotations

import argparse
from concurrent.futures import ProcessPoolExecutor
from copy import deepcopy
import json
import math
from pathlib import Path
import sys
from typing import Any, Mapping

import numpy as np
from scipy.optimize import least_squares


MODULE_ROOT = Path(__file__).resolve().parent
sys.path.insert(0, str(MODULE_ROOT))

import calibrate_cmm_camera_metric as cal  # noqa: E402


PARAMETER_SCALE = 0.01


def _references(
    observations: list[dict[str, Any]],
    training_names: set[str],
    matrices: Mapping[str, np.ndarray],
) -> dict[str, float]:
    output: dict[str, float] = {}
    for camera in cal.CAMERAS:
        values: list[float] = []
        for item in observations:
            if item["camera"] != camera or item["capture"] not in training_names:
                continue
            for rays in item["rays"].values():
                corrected = np.asarray(rays, dtype=float) @ matrices[camera].T
                values.extend(np.sum(corrected**2, axis=1).tolist())
        output[camera] = float(np.mean(np.asarray(values, dtype=float)))
    return output


def _calibration_delta_bounds(
    observations: list[dict[str, Any]],
    training_names: set[str],
    matrices: Mapping[str, np.ndarray],
    references: Mapping[str, float],
) -> dict[str, tuple[float, float]]:
    output: dict[str, tuple[float, float]] = {}
    for camera in cal.CAMERAS:
        values: list[float] = []
        for item in observations:
            if item["camera"] != camera or item["capture"] not in training_names:
                continue
            for rays in item["rays"].values():
                corrected = np.asarray(rays, dtype=float) @ matrices[camera].T
                values.extend((np.sum(corrected**2, axis=1) - references[camera]).tolist())
        array = np.asarray(values, dtype=float)
        output[camera] = (float(np.percentile(array, 5.0)), float(np.percentile(array, 95.0)))
    return output


def _metrics(
    matrix: np.ndarray,
    coefficient: float,
    reference_mm2: float,
    delta_bounds_mm2: tuple[float, float],
    observation: Mapping[str, Any],
) -> dict[str, float]:
    corner = observation["physical_corner"]
    face1, face2 = cal.PHYSICAL_FACE_ORDER[corner]

    def correct(rays: np.ndarray) -> np.ndarray:
        linear = np.asarray(rays, dtype=float) @ matrix.T
        squared = np.sum(linear**2, axis=1)
        delta = np.clip(
            squared - float(reference_mm2),
            float(delta_bounds_mm2[0]),
            float(delta_bounds_mm2[1]),
        )
        factors = 1.0 + float(coefficient) * delta
        return linear * factors[:, None]

    ray1 = correct(observation["rays"][face1])
    ray2 = correct(observation["rays"][face2])
    projection1 = np.linalg.norm(ray1, axis=1)
    projection2 = np.linalg.norm(ray2, axis=1)
    chord = np.linalg.norm(ray2 - ray1, axis=1)
    cosine = np.sum(ray1 * ray2, axis=1) / (projection1 * projection2)
    angle = np.degrees(np.arccos(np.clip(cosine, -1.0, 1.0)))
    return {
        f"projection_{face1}_mm": cal._trimmed_mean(projection1),
        f"projection_{face2}_mm": cal._trimmed_mean(projection2),
        "chord_mm": cal._trimmed_mean(chord),
        "corner_angle_deg": cal._trimmed_mean(angle),
    }


def _cmm_residuals(
    coefficients: Mapping[str, float],
    references: Mapping[str, float],
    delta_bounds: Mapping[str, tuple[float, float]],
    matrices: Mapping[str, np.ndarray],
    observations: list[dict[str, Any]],
    names: set[str],
    truth: Mapping[str, Any],
    assignment: Mapping[str, Mapping[str, float]],
) -> np.ndarray:
    residuals: list[float] = []
    for item in observations:
        if item["capture"] not in names:
            continue
        corner = item["physical_corner"]
        face1, face2 = cal.PHYSICAL_FACE_ORDER[corner]
        predicted = _metrics(
            matrices[item["camera"]],
            coefficients[item["camera"]],
            references[item["camera"]],
            delta_bounds[item["camera"]],
            item,
        )
        residuals.extend([
            (predicted[f"projection_{face1}_mm"] - assignment[corner][face1]) / 0.01,
            (predicted[f"projection_{face2}_mm"] - assignment[corner][face2]) / 0.01,
            (predicted["chord_mm"] - float(truth[corner]["chord_mm"])) / 0.01,
            (predicted["corner_angle_deg"] - float(truth[corner]["corner_angle_deg"])) / 0.01,
        ])
    return np.asarray(residuals, dtype=float)


def _pair_residuals(
    coefficients: Mapping[str, float],
    references: Mapping[str, float],
    delta_bounds: Mapping[str, tuple[float, float]],
    matrices: Mapping[str, np.ndarray],
    observations: list[dict[str, Any]],
    selected_corners: set[str],
) -> tuple[np.ndarray, list[dict[str, Any]]]:
    residuals: list[float] = []
    records: list[dict[str, Any]] = []
    for corner, (first, turned) in cal._paired_observations_by_corner(observations).items():
        if corner not in selected_corners:
            continue
        face1, face2 = cal.PHYSICAL_FACE_ORDER[corner]
        first_metrics = _metrics(
            matrices[first["camera"]], coefficients[first["camera"]],
            references[first["camera"]], delta_bounds[first["camera"]], first,
        )
        turned_metrics = _metrics(
            matrices[turned["camera"]], coefficients[turned["camera"]],
            references[turned["camera"]], delta_bounds[turned["camera"]], turned,
        )
        row: dict[str, Any] = {
            "physical_corner": corner,
            "first_device_corner": first["device_corner"],
            "turned_device_corner": turned["device_corner"],
        }
        for field in (
            f"projection_{face1}_mm", f"projection_{face2}_mm", "chord_mm", "corner_angle_deg"
        ):
            difference = float(first_metrics[field] - turned_metrics[field])
            residuals.append(difference / 0.01)
            row[f"first_{field}"] = float(first_metrics[field])
            row[f"turned_{field}"] = float(turned_metrics[field])
            row[f"difference_{field}"] = difference
        records.append(row)
    return np.asarray(residuals, dtype=float), records


def _fit(
    matrices: Mapping[str, np.ndarray],
    references: Mapping[str, float],
    delta_bounds: Mapping[str, tuple[float, float]],
    cmm_observations: list[dict[str, Any]],
    training_names: set[str],
    truth: Mapping[str, Any],
    assignment: Mapping[str, Mapping[str, float]],
    pair_observations: list[dict[str, Any]],
    pair_corners: set[str],
    pair_weight: float,
    regularization: float,
) -> dict[str, float]:
    def residual(scaled: np.ndarray) -> np.ndarray:
        coefficients = {
            camera: float(scaled[index]) * PARAMETER_SCALE
            for index, camera in enumerate(cal.CAMERAS)
        }
        cmm = _cmm_residuals(
            coefficients, references, delta_bounds, matrices,
            cmm_observations, training_names, truth, assignment
        )
        pair, _ = _pair_residuals(
            coefficients, references, delta_bounds, matrices, pair_observations, pair_corners
        )
        return np.concatenate([
            cmm,
            float(pair_weight) * pair,
            float(regularization) * scaled,
        ])

    result = least_squares(
        residual,
        np.zeros(len(cal.CAMERAS), dtype=float),
        bounds=(-10.0 * np.ones(len(cal.CAMERAS)), 10.0 * np.ones(len(cal.CAMERAS))),
        max_nfev=300,
    )
    return {
        camera: float(result.x[index]) * PARAMETER_SCALE
        for index, camera in enumerate(cal.CAMERAS)
    }


def _rms(values: np.ndarray) -> float:
    return math.sqrt(float(np.mean(np.asarray(values, dtype=float) ** 2)))


def main() -> int:
    parser = argparse.ArgumentParser(description="局部尺度非线性相机指纹试验")
    parser.add_argument("--input", type=Path, default=MODULE_ROOT.parent / "CMM机构HOBJ")
    parser.add_argument("--base-model", type=Path,
                        default=MODULE_ROOT / "models" / "chamfer_camera_metric_geometry_210_105.json")
    parser.add_argument("--truth", type=Path,
                        default=MODULE_ROOT / "calibration" / "cmm_chamfer_truth_reported.json")
    parser.add_argument("--base-diagnostics", type=Path,
                        default=MODULE_ROOT / "calibration" / "cmm_camera_metric_calibration_diagnostics.json")
    parser.add_argument("--consistency-first", type=Path, required=True)
    parser.add_argument("--consistency-turned", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--diagnostics", type=Path, required=True)
    parser.add_argument("--stations", type=int, default=180)
    parser.add_argument("--consistency-stations", type=int, default=360)
    parser.add_argument("--weights", nargs="*", type=float, default=[0.5, 1.0, 1.5, 2.0])
    parser.add_argument("--regularization", type=float, default=0.15)
    parser.add_argument("--workers", type=int, default=3)
    args = parser.parse_args()

    base_model = cal.load_calibration(args.base_model)
    identity = deepcopy(base_model)
    for config in identity["hobj_channels"].values():
        config["metric_transform_2x2"] = [[1.0, 0.0], [0.0, 1.0]]
        config["metric_radial_distortion_per_mm2"] = 0.0
        config["metric_local_ray_scale_per_mm2"] = 0.0
    temporary = args.output.with_name("_temporary_local_scale_identity.json")
    temporary.parent.mkdir(parents=True, exist_ok=True)
    temporary.write_text(json.dumps(identity, ensure_ascii=False, indent=2), encoding="utf-8")
    try:
        paths = sorted(
            path for path in args.input.glob("*/*.hobj") if path.name != "14_16.hobj"
        )
        if len(paths) != 19:
            raise RuntimeError(f"排除14_16后应有19张CMM图，实际{len(paths)}张")
        jobs = [(str(path), str(temporary), int(args.stations)) for path in paths]
        with ProcessPoolExecutor(max_workers=int(args.workers)) as executor:
            scans = list(executor.map(cal._measure_worker, jobs))
        pair_jobs = [
            (str(args.consistency_first.resolve()), str(temporary), int(args.consistency_stations)),
            (str(args.consistency_turned.resolve()), str(temporary), int(args.consistency_stations)),
        ]
        with ProcessPoolExecutor(max_workers=2) as executor:
            pair_scans = list(executor.map(cal._measure_worker, pair_jobs))
    finally:
        temporary.unlink(missing_ok=True)

    observations = cal._physical_observations(scans)
    pair_observations = cal._explicit_turnover_pair_observations(pair_scans[0], pair_scans[1])
    training_names, validation_names = cal._split_capture_names(scans)
    truth_payload = json.loads(args.truth.read_text(encoding="utf-8"))
    candidate_name = base_model["camera_metric_calibration"]["selected_truth_candidate"]
    truth = cal._truth_candidates(truth_payload)[candidate_name]
    assignment = json.loads(args.base_diagnostics.read_text(encoding="utf-8"))[
        "projection_face_values_mm"
    ]
    matrices = {
        camera: np.asarray(base_model["hobj_channels"][camera]["metric_transform_2x2"], dtype=float)
        for camera in cal.CAMERAS
    }
    references = _references(observations, training_names, matrices)
    delta_bounds = _calibration_delta_bounds(
        observations, training_names, matrices, references
    )
    zeros = {camera: 0.0 for camera in cal.CAMERAS}
    baseline_cmm = _rms(_cmm_residuals(
        zeros, references, delta_bounds, matrices,
        observations, validation_names, truth, assignment
    ))
    all_corners = set(cal.PHYSICAL_FACE_ORDER)
    baseline_pair_values, baseline_pair_records = _pair_residuals(
        zeros, references, delta_bounds, matrices, pair_observations, all_corners
    )
    baseline_pair = _rms(baseline_pair_values)

    rankings: list[dict[str, Any]] = []
    for weight in args.weights:
        holdouts: list[dict[str, Any]] = []
        for holdout in cal.PHYSICAL_FACE_ORDER:
            coefficients = _fit(
                matrices, references, delta_bounds,
                observations, training_names, truth, assignment,
                pair_observations, all_corners - {holdout}, weight, args.regularization,
            )
            cmm_rms = _rms(_cmm_residuals(
                coefficients, references, delta_bounds, matrices,
                observations, validation_names, truth, assignment
            ))
            pair_values, pair_records = _pair_residuals(
                coefficients, references, delta_bounds, matrices, pair_observations, {holdout}
            )
            holdouts.append({
                "corner": holdout,
                "cmm_validation_rms": cmm_rms,
                "holdout_pair_rms": _rms(pair_values),
                "coefficients": coefficients,
                "records": pair_records,
            })
        cv_cmm = _rms(np.asarray([item["cmm_validation_rms"] for item in holdouts]))
        cv_pair = _rms(np.asarray([item["holdout_pair_rms"] for item in holdouts]))
        rankings.append({
            "weight": float(weight),
            "cv_cmm_rms": cv_cmm,
            "cv_holdout_pair_rms": cv_pair,
            "cv_combined_rms": math.sqrt((cv_cmm**2 + cv_pair**2) / 2.0),
            "passes_cmm_guard": all(
                item["cmm_validation_rms"] <= baseline_cmm * 1.05 for item in holdouts
            ),
            "holdouts": holdouts,
        })
    accepted = [item for item in rankings if item["passes_cmm_guard"]]
    if not accepted:
        raise RuntimeError("所有局部尺度候选均未通过CMM留出门槛")
    choice = min(accepted, key=lambda item: item["cv_combined_rms"])
    coefficients = _fit(
        matrices, references, delta_bounds,
        observations, training_names, truth, assignment,
        pair_observations, all_corners, choice["weight"], args.regularization,
    )
    final_cmm = _rms(_cmm_residuals(
        coefficients, references, delta_bounds, matrices,
        observations, validation_names, truth, assignment
    ))
    final_pair_values, final_pair_records = _pair_residuals(
        coefficients, references, delta_bounds, matrices, pair_observations, all_corners
    )
    final_pair = _rms(final_pair_values)

    candidate = deepcopy(base_model)
    for camera in cal.CAMERAS:
        config = candidate["hobj_channels"][camera]
        config["metric_radial_distortion_per_mm2"] = 0.0
        config["metric_distortion_center_xz_mm"] = [20.25, 0.0]
        config["metric_local_ray_scale_per_mm2"] = coefficients[camera]
        config["metric_local_ray_reference_mm2"] = references[camera]
        config["metric_local_ray_delta_bounds_mm2"] = list(delta_bounds[camera])
    candidate["camera_metric_calibration"]["local_scale_nonlinearity_candidate"] = True
    candidate["camera_metric_calibration"]["runtime_path_classification"] = False
    args.output.write_text(json.dumps(candidate, ensure_ascii=False, indent=2), encoding="utf-8")
    diagnostics = {
        "method": "fixed_linear_camera_metric_plus_local_ray_scale_nonlinearity",
        "linear_matrices_frozen": True,
        "runtime_orientation_inference": False,
        "baseline_cmm_validation_rms": baseline_cmm,
        "baseline_pair_rms": baseline_pair,
        "baseline_pair_records": baseline_pair_records,
        "references_mm2": references,
        "delta_bounds_mm2": delta_bounds,
        "selected_weight": choice["weight"],
        "coefficients_per_mm2": coefficients,
        "final_cmm_validation_rms": final_cmm,
        "final_pair_rms": final_pair,
        "final_pair_records": final_pair_records,
        "rankings": sorted(rankings, key=lambda item: item["cv_combined_rms"]),
    }
    args.diagnostics.write_text(json.dumps(diagnostics, ensure_ascii=False, indent=2), encoding="utf-8")
    print(json.dumps({
        "output": str(args.output.resolve()),
        "diagnostics": str(args.diagnostics.resolve()),
        "baseline_cmm_validation_rms": baseline_cmm,
        "baseline_pair_rms": baseline_pair,
        "selected_weight": choice["weight"],
        "cv_cmm_rms": choice["cv_cmm_rms"],
        "cv_holdout_pair_rms": choice["cv_holdout_pair_rms"],
        "coefficients_per_mm2": coefficients,
        "references_mm2": references,
        "delta_bounds_mm2": delta_bounds,
        "final_cmm_validation_rms": final_cmm,
        "final_pair_rms": final_pair,
        "final_pair_records": final_pair_records,
    }, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
