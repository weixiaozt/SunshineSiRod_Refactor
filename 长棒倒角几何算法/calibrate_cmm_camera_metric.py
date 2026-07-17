#!/usr/bin/env python3
"""用CMM标准棒拟合每相机固定2×2局部公制度量矩阵。

这是低层相机坐标标定，不生成四角终值偏移。两组采集关系只在标定器中用于将同一
物理倒角与CMM三角形对应；运行模型不含路径分类器，每个HOBJ运行同一套固定相机链。
"""

from __future__ import annotations

import argparse
from concurrent.futures import ProcessPoolExecutor
from copy import deepcopy
import itertools
import json
import math
from pathlib import Path
import sys
from typing import Any, Mapping

import numpy as np
from scipy.optimize import least_squares


MODULE_ROOT = Path(__file__).resolve().parent
sys.path.insert(0, str(MODULE_ROOT / "src"))

from chamfer_geometry import load_calibration, measure_hobj  # noqa: E402


DEFAULT_INPUT = MODULE_ROOT.parent / "CMM机构HOBJ"
DEFAULT_BASE_MODEL = MODULE_ROOT / "models" / "chamfer_camera_metric_geometry_210_105.json"
DEFAULT_TRUTH = MODULE_ROOT / "calibration" / "cmm_chamfer_truth_reported.json"
DEFAULT_OUTPUT = MODULE_ROOT / "models" / "chamfer_camera_metric_geometry_210_105.json"
DEFAULT_DIAGNOSTICS = MODULE_ROOT / "calibration" / "cmm_camera_metric_calibration_diagnostics.json"
DEVICE_CORNERS = ("AD", "AB", "CB", "CD")
CAMERAS = ("Left", "Right", "Top", "Down")
PHYSICAL_FACE_ORDER = {
    "AB": ("A", "B"),
    "CB": ("C", "B"),
    "CD": ("C", "D"),
    "AD": ("A", "D"),
}

# 仅供CMM标定验证使用，运行时永远不读取该分组关系。
GROUP_B_DEVICE_TO_PHYSICAL = {
    "AD": ("AB", {"A": "A", "D": "B"}),
    "AB": ("AD", {"A": "A", "B": "D"}),
    "CB": ("CD", {"C": "C", "B": "D"}),
    "CD": ("CB", {"C": "C", "D": "B"}),
}


def _measure_worker(arguments: tuple[str, str, int]) -> dict[str, Any]:
    path_text, model_text, station_count = arguments
    path = Path(path_text)
    result = measure_hobj(path, load_calibration(Path(model_text)), station_count=station_count)
    compact: dict[str, Any] = {
        "name": path.name,
        "group": path.parent.name,
        "corners": {},
    }
    for corner in DEVICE_CORNERS:
        item = result["corners"][corner]
        face1, face2 = item["output_face_order"]
        rays: dict[str, list[list[float]]] = {face1: [], face2: []}
        theoretical_corners: list[list[float]] = []
        endpoints: dict[str, list[list[float]]] = {face1: [], face2: []}
        for record in result["slice_records"][corner]:
            point = np.asarray(record["raw_theoretical_corner_xz_mm"], dtype=float)
            theoretical_corners.append(point.tolist())
            for face in (face1, face2):
                endpoint = np.asarray(record[f"raw_endpoint_{face}_xz_mm"], dtype=float)
                rays[face].append((endpoint - point).tolist())
                endpoints[face].append(endpoint.tolist())
        compact["corners"][corner] = {
            "camera": item["camera"],
            "rays": rays,
            "theoretical_corners": theoretical_corners,
            "endpoints": endpoints,
        }
    return compact


def _trimmed_mean(values: np.ndarray, fraction: float = 0.10) -> float:
    data = np.sort(np.asarray(values, dtype=float))
    trim = int(math.floor(data.size * fraction)) if data.size >= 10 else 0
    used = data[trim:data.size - trim] if trim and data.size > 2 * trim else data
    return float(np.mean(used))


def _truth_candidates(payload: Mapping[str, Any]) -> dict[str, dict[str, Any]]:
    report = payload["report_as_written"]
    return {
        candidate: {
            physical_corner: deepcopy(report[report_corner])
            for physical_corner, report_corner in mapping.items()
        }
        for candidate, mapping in payload["candidate_mappings"].items()
    }


def _physical_observations(scans: list[dict[str, Any]]) -> list[dict[str, Any]]:
    output: list[dict[str, Any]] = []
    for scan in scans:
        if scan["group"] not in {"head_to_tail", "tail_to_head"}:
            raise ValueError(f"未知CMM采集组：{scan['group']}")
        group_b = scan["group"] == "tail_to_head"
        for device_corner, item in scan["corners"].items():
            if group_b:
                physical_corner, face_map = GROUP_B_DEVICE_TO_PHYSICAL[device_corner]
            else:
                physical_corner = device_corner
                face_map = {face: face for face in item["rays"]}
            output.append({
                "capture": scan["name"],
                "group": scan["group"],
                "camera": item["camera"],
                "device_corner": device_corner,
                "physical_corner": physical_corner,
                "rays": {
                    face_map[device_face]: np.asarray(values, dtype=float)
                    for device_face, values in item["rays"].items()
                },
                "theoretical_corners": np.asarray(item["theoretical_corners"], dtype=float),
                "endpoints": {
                    face_map[device_face]: np.asarray(values, dtype=float)
                    for device_face, values in item["endpoints"].items()
                },
            })
    return output


def _split_capture_names(scans: list[dict[str, Any]]) -> tuple[set[str], set[str]]:
    training: set[str] = set()
    validation: set[str] = set()
    for group in ("head_to_tail", "tail_to_head"):
        names = sorted(scan["name"] for scan in scans if scan["group"] == group)
        for index, name in enumerate(names):
            (validation if index % 3 == 2 else training).add(name)
    return training, validation


def _projection_assignments(truth: Mapping[str, Any]):
    corners = tuple(PHYSICAL_FACE_ORDER)
    for bits in itertools.product((0, 1), repeat=len(corners)):
        assignment: dict[str, dict[str, float]] = {}
        signature: list[str] = []
        for corner, reverse in zip(corners, bits):
            pair = list(map(float, truth[corner]["projection_pair_mm"]))
            if reverse:
                pair.reverse()
            face1, face2 = PHYSICAL_FACE_ORDER[corner]
            assignment[corner] = {face1: pair[0], face2: pair[1]}
            signature.append(f"{corner}:{face1}={pair[0]:.4f},{face2}={pair[1]:.4f}")
        yield " | ".join(signature), assignment


def _matrix_from_parameters(parameters: np.ndarray) -> np.ndarray:
    # 上三角Cholesky形式覆盖所有二维对称正定度量，去掉对长度/夹角无影响的旋转自由度。
    return np.array([
        [math.exp(float(parameters[0])), float(parameters[1])],
        [0.0, math.exp(float(parameters[2]))],
    ])


def _initial_matrix(
    observations: list[dict[str, Any]],
    truth: Mapping[str, Any],
    projection_assignment: Mapping[str, Mapping[str, float]],
) -> np.ndarray:
    rows: list[np.ndarray] = []
    for observation in observations:
        corner = observation["physical_corner"]
        face1, face2 = PHYSICAL_FACE_ORDER[corner]
        ray1, ray2 = observation["rays"][face1], observation["rays"][face2]
        for vectors, target in (
            (ray1, projection_assignment[corner][face1]),
            (ray2, projection_assignment[corner][face2]),
            (ray2 - ray1, float(truth[corner]["chord_mm"])),
        ):
            design = np.column_stack([
                vectors[:, 0] ** 2,
                2.0 * vectors[:, 0] * vectors[:, 1],
                vectors[:, 1] ** 2,
            ])
            rows.append(np.mean(design, axis=0) / (target**2))
    solution, *_ = np.linalg.lstsq(np.vstack(rows), np.ones(len(rows)), rcond=None)
    metric = np.array([[solution[0], solution[1]], [solution[1], solution[2]]])
    eigenvalues, eigenvectors = np.linalg.eigh(metric)
    metric = eigenvectors @ np.diag(np.clip(eigenvalues, 0.55**2, 1.45**2)) @ eigenvectors.T
    return np.linalg.cholesky(metric).T


def _predicted_metrics(
    matrix: np.ndarray,
    observation: Mapping[str, Any],
    radial_distortion_per_mm2: float = 0.0,
    distortion_center_xz_mm: np.ndarray | None = None,
) -> dict[str, float]:
    corner = observation["physical_corner"]
    face1, face2 = PHYSICAL_FACE_ORDER[corner]
    radial = float(radial_distortion_per_mm2)
    if abs(radial) <= 1e-20:
        ray1 = observation["rays"][face1] @ matrix.T
        ray2 = observation["rays"][face2] @ matrix.T
    else:
        center = np.asarray(
            distortion_center_xz_mm if distortion_center_xz_mm is not None else [20.25, 0.0],
            dtype=float,
        )
        points = np.asarray(observation["theoretical_corners"], dtype=float)

        def transform(values: np.ndarray) -> np.ndarray:
            delta = values - center
            factors = 1.0 + radial * np.sum(delta**2, axis=1)
            warped = center + delta * factors[:, None]
            return warped @ matrix.T

        transformed_points = transform(points)
        ray1 = transform(np.asarray(observation["endpoints"][face1], dtype=float)) - transformed_points
        ray2 = transform(np.asarray(observation["endpoints"][face2], dtype=float)) - transformed_points
    projection1, projection2 = np.linalg.norm(ray1, axis=1), np.linalg.norm(ray2, axis=1)
    chord = np.linalg.norm(ray2 - ray1, axis=1)
    cosine = np.sum(ray1 * ray2, axis=1) / (projection1 * projection2)
    angle = np.degrees(np.arccos(np.clip(cosine, -1.0, 1.0)))
    return {
        f"projection_{face1}_mm": _trimmed_mean(projection1),
        f"projection_{face2}_mm": _trimmed_mean(projection2),
        "chord_mm": _trimmed_mean(chord),
        "corner_angle_deg": _trimmed_mean(angle),
    }


def _residual_vector(
    parameters: np.ndarray,
    observations: list[dict[str, Any]],
    truth: Mapping[str, Any],
    projection_assignment: Mapping[str, Mapping[str, float]],
) -> np.ndarray:
    matrix = _matrix_from_parameters(parameters)
    residuals: list[float] = []
    for observation in observations:
        corner = observation["physical_corner"]
        face1, face2 = PHYSICAL_FACE_ORDER[corner]
        predicted = _predicted_metrics(matrix, observation)
        residuals.extend([
            (predicted[f"projection_{face1}_mm"] - projection_assignment[corner][face1]) / 0.01,
            (predicted[f"projection_{face2}_mm"] - projection_assignment[corner][face2]) / 0.01,
            (predicted["chord_mm"] - float(truth[corner]["chord_mm"])) / 0.01,
            (predicted["corner_angle_deg"] - float(truth[corner]["corner_angle_deg"])) / 0.01,
        ])
    return np.asarray(residuals, dtype=float)


def _fit_matrices(
    observations: list[dict[str, Any]],
    training_names: set[str],
    truth: Mapping[str, Any],
    projection_assignment: Mapping[str, Mapping[str, float]],
) -> dict[str, np.ndarray]:
    matrices: dict[str, np.ndarray] = {}
    for camera in CAMERAS:
        selected = [item for item in observations if item["camera"] == camera and item["capture"] in training_names]
        initial = _initial_matrix(selected, truth, projection_assignment)
        parameters = np.array([math.log(initial[0, 0]), initial[0, 1], math.log(initial[1, 1])])
        fit = least_squares(
            _residual_vector,
            parameters,
            args=(selected, truth, projection_assignment),
            bounds=(
                np.array([math.log(0.55), -0.45, math.log(0.55)]),
                np.array([math.log(1.45), 0.45, math.log(1.45)]),
            ),
            max_nfev=200,
        )
        matrices[camera] = _matrix_from_parameters(fit.x)
    return matrices


def _explicit_turnover_pair_observations(
    first_scan: Mapping[str, Any],
    turned_scan: Mapping[str, Any],
) -> list[dict[str, Any]]:
    """建立人工明确指定的同棒调头验证对。

    这里的分组只存在于离线标定器中。运行模型不保存路径、文件名或方向分类器，
    生产测量仍始终按固定设备角位输出。
    """

    first = deepcopy(first_scan)
    turned = deepcopy(turned_scan)
    first["group"] = "head_to_tail"
    turned["group"] = "tail_to_head"
    return _physical_observations([first, turned])


def _matrix_parameters(matrix: np.ndarray) -> np.ndarray:
    matrix = np.asarray(matrix, dtype=float)
    return np.asarray([
        math.log(float(matrix[0, 0])),
        float(matrix[0, 1]),
        math.log(float(matrix[1, 1])),
    ])


def _matrices_from_joint_parameters(parameters: np.ndarray) -> dict[str, np.ndarray]:
    return {
        camera: _matrix_from_parameters(parameters[index * 3:(index + 1) * 3])
        for index, camera in enumerate(CAMERAS)
    }


def _paired_observations_by_corner(
    observations: list[dict[str, Any]],
) -> dict[str, tuple[dict[str, Any], dict[str, Any]]]:
    grouped: dict[str, list[dict[str, Any]]] = {corner: [] for corner in PHYSICAL_FACE_ORDER}
    for observation in observations:
        grouped[observation["physical_corner"]].append(observation)
    output: dict[str, tuple[dict[str, Any], dict[str, Any]]] = {}
    for corner, items in grouped.items():
        first = [item for item in items if item["group"] == "head_to_tail"]
        turned = [item for item in items if item["group"] == "tail_to_head"]
        if len(first) != 1 or len(turned) != 1:
            raise ValueError(f"同棒调头验证对的 {corner} 角必须各有一条正向和调头观测")
        output[corner] = (first[0], turned[0])
    return output


def _pair_residual_vector(
    matrices: Mapping[str, np.ndarray],
    observations: list[dict[str, Any]],
) -> np.ndarray:
    residuals: list[float] = []
    for corner, (first, turned) in _paired_observations_by_corner(observations).items():
        face1, face2 = PHYSICAL_FACE_ORDER[corner]
        first_metrics = _predicted_metrics(matrices[first["camera"]], first)
        turned_metrics = _predicted_metrics(matrices[turned["camera"]], turned)
        for field, scale in (
            (f"projection_{face1}_mm", 0.01),
            (f"projection_{face2}_mm", 0.01),
            ("chord_mm", 0.01),
            ("corner_angle_deg", 0.01),
        ):
            residuals.append((first_metrics[field] - turned_metrics[field]) / scale)
    return np.asarray(residuals, dtype=float)


def _joint_residual_vector(
    parameters: np.ndarray,
    cmm_observations: list[dict[str, Any]],
    training_names: set[str],
    truth: Mapping[str, Any],
    projection_assignment: Mapping[str, Mapping[str, float]],
    pair_observations: list[dict[str, Any]],
    pair_weight: float,
) -> np.ndarray:
    matrices = _matrices_from_joint_parameters(parameters)
    residuals: list[np.ndarray] = []
    for camera in CAMERAS:
        selected = [
            item for item in cmm_observations
            if item["camera"] == camera and item["capture"] in training_names
        ]
        start = CAMERAS.index(camera) * 3
        residuals.append(_residual_vector(
            parameters[start:start + 3],
            selected,
            truth,
            projection_assignment,
        ))
    if pair_weight > 0.0:
        residuals.append(float(pair_weight) * _pair_residual_vector(matrices, pair_observations))
    return np.concatenate(residuals)


def _fit_joint_matrices(
    initial_matrices: Mapping[str, np.ndarray],
    cmm_observations: list[dict[str, Any]],
    training_names: set[str],
    truth: Mapping[str, Any],
    projection_assignment: Mapping[str, Mapping[str, float]],
    pair_observations: list[dict[str, Any]],
    pair_weight: float,
) -> dict[str, np.ndarray]:
    initial = np.concatenate([_matrix_parameters(initial_matrices[camera]) for camera in CAMERAS])
    lower = np.tile(np.asarray([math.log(0.55), -0.45, math.log(0.55)]), len(CAMERAS))
    upper = np.tile(np.asarray([math.log(1.45), 0.45, math.log(1.45)]), len(CAMERAS))
    fit = least_squares(
        _joint_residual_vector,
        initial,
        args=(
            cmm_observations,
            training_names,
            truth,
            projection_assignment,
            pair_observations,
            pair_weight,
        ),
        bounds=(lower, upper),
        max_nfev=350,
    )
    return _matrices_from_joint_parameters(fit.x)


def _evaluate_pair_consistency(
    observations: list[dict[str, Any]],
    matrices: Mapping[str, np.ndarray],
    radial_distortions: Mapping[str, float] | None = None,
    selected_corners: set[str] | None = None,
) -> dict[str, Any]:
    records: list[dict[str, Any]] = []
    normalized: list[float] = []
    for corner, (first, turned) in _paired_observations_by_corner(observations).items():
        if selected_corners is not None and corner not in selected_corners:
            continue
        face1, face2 = PHYSICAL_FACE_ORDER[corner]
        first_radial = float((radial_distortions or {}).get(first["camera"], 0.0))
        turned_radial = float((radial_distortions or {}).get(turned["camera"], 0.0))
        first_metrics = _predicted_metrics(matrices[first["camera"]], first, first_radial)
        turned_metrics = _predicted_metrics(matrices[turned["camera"]], turned, turned_radial)
        row: dict[str, Any] = {
            "physical_corner": corner,
            "first_device_corner": first["device_corner"],
            "first_camera": first["camera"],
            "turned_device_corner": turned["device_corner"],
            "turned_camera": turned["camera"],
        }
        for field, scale in (
            (f"projection_{face1}_mm", 0.01),
            (f"projection_{face2}_mm", 0.01),
            ("chord_mm", 0.01),
            ("corner_angle_deg", 0.01),
        ):
            difference = float(first_metrics[field] - turned_metrics[field])
            row[f"first_{field}"] = float(first_metrics[field])
            row[f"turned_{field}"] = float(turned_metrics[field])
            row[f"difference_{field}"] = difference
            normalized.append(difference / scale)
        records.append(row)
    vector = np.asarray(normalized, dtype=float)
    return {
        "normalized_rms": math.sqrt(float(np.mean(vector**2))),
        "normalized_mean_abs": float(np.mean(np.abs(vector))),
        "records": records,
    }


RADIAL_PARAMETER_SCALE = 1e-5


def _radial_models_from_parameters(
    parameters: np.ndarray,
) -> tuple[dict[str, np.ndarray], dict[str, float]]:
    matrices: dict[str, np.ndarray] = {}
    radials: dict[str, float] = {}
    for index, camera in enumerate(CAMERAS):
        start = index * 4
        matrices[camera] = _matrix_from_parameters(parameters[start:start + 3])
        radials[camera] = float(parameters[start + 3]) * RADIAL_PARAMETER_SCALE
    return matrices, radials


def _radial_model_residual_vector(
    matrix: np.ndarray,
    radial: float,
    observations: list[dict[str, Any]],
    truth: Mapping[str, Any],
    projection_assignment: Mapping[str, Mapping[str, float]],
) -> np.ndarray:
    residuals: list[float] = []
    for observation in observations:
        corner = observation["physical_corner"]
        face1, face2 = PHYSICAL_FACE_ORDER[corner]
        predicted = _predicted_metrics(matrix, observation, radial)
        residuals.extend([
            (predicted[f"projection_{face1}_mm"] - projection_assignment[corner][face1]) / 0.01,
            (predicted[f"projection_{face2}_mm"] - projection_assignment[corner][face2]) / 0.01,
            (predicted["chord_mm"] - float(truth[corner]["chord_mm"])) / 0.01,
            (predicted["corner_angle_deg"] - float(truth[corner]["corner_angle_deg"])) / 0.01,
        ])
    return np.asarray(residuals, dtype=float)


def _joint_radial_residual_vector(
    parameters: np.ndarray,
    cmm_observations: list[dict[str, Any]],
    training_names: set[str],
    truth: Mapping[str, Any],
    projection_assignment: Mapping[str, Mapping[str, float]],
    pair_observations: list[dict[str, Any]],
    pair_weight: float,
    pair_training_corners: set[str],
    radial_regularization: float,
) -> np.ndarray:
    matrices, radials = _radial_models_from_parameters(parameters)
    residuals: list[np.ndarray] = []
    for camera in CAMERAS:
        selected = [
            item for item in cmm_observations
            if item["camera"] == camera and item["capture"] in training_names
        ]
        residuals.append(_radial_model_residual_vector(
            matrices[camera], radials[camera], selected, truth, projection_assignment
        ))
    pair_values = _evaluate_pair_consistency(
        pair_observations, matrices, radials, pair_training_corners
    )
    pair_residuals: list[float] = []
    for record in pair_values["records"]:
        corner = record["physical_corner"]
        face1, face2 = PHYSICAL_FACE_ORDER[corner]
        for field in (
            f"projection_{face1}_mm",
            f"projection_{face2}_mm",
            "chord_mm",
            "corner_angle_deg",
        ):
            pair_residuals.append(float(record[f"difference_{field}"]) / 0.01)
    residuals.append(float(pair_weight) * np.asarray(pair_residuals, dtype=float))
    # 系数以1e-5为单位做轻微L2约束，防止缺少证据时产生夸张畸变。
    residuals.append(float(radial_regularization) * np.asarray([
        radials[camera] / RADIAL_PARAMETER_SCALE for camera in CAMERAS
    ]))
    return np.concatenate(residuals)


def _fit_joint_radial_models(
    initial_matrices: Mapping[str, np.ndarray],
    cmm_observations: list[dict[str, Any]],
    training_names: set[str],
    truth: Mapping[str, Any],
    projection_assignment: Mapping[str, Mapping[str, float]],
    pair_observations: list[dict[str, Any]],
    pair_weight: float,
    pair_training_corners: set[str],
    radial_regularization: float = 0.20,
) -> tuple[dict[str, np.ndarray], dict[str, float]]:
    initial_values: list[float] = []
    for camera in CAMERAS:
        initial_values.extend(_matrix_parameters(initial_matrices[camera]).tolist())
        initial_values.append(0.0)
    initial = np.asarray(initial_values, dtype=float)
    lower_one = np.asarray([math.log(0.55), -0.45, math.log(0.55), -5.0])
    upper_one = np.asarray([math.log(1.45), 0.45, math.log(1.45), 5.0])
    fit = least_squares(
        _joint_radial_residual_vector,
        initial,
        args=(
            cmm_observations,
            training_names,
            truth,
            projection_assignment,
            pair_observations,
            pair_weight,
            pair_training_corners,
            radial_regularization,
        ),
        bounds=(np.tile(lower_one, len(CAMERAS)), np.tile(upper_one, len(CAMERAS))),
        max_nfev=500,
    )
    return _radial_models_from_parameters(fit.x)


def _fixed_matrix_radial_residual_vector(
    scaled_radials: np.ndarray,
    fixed_matrices: Mapping[str, np.ndarray],
    cmm_observations: list[dict[str, Any]],
    training_names: set[str],
    truth: Mapping[str, Any],
    projection_assignment: Mapping[str, Mapping[str, float]],
    pair_observations: list[dict[str, Any]],
    pair_weight: float,
    pair_training_corners: set[str],
    radial_regularization: float,
) -> np.ndarray:
    radials = {
        camera: float(scaled_radials[index]) * RADIAL_PARAMETER_SCALE
        for index, camera in enumerate(CAMERAS)
    }
    residuals: list[np.ndarray] = []
    for camera in CAMERAS:
        selected = [
            item for item in cmm_observations
            if item["camera"] == camera and item["capture"] in training_names
        ]
        residuals.append(_radial_model_residual_vector(
            fixed_matrices[camera], radials[camera], selected, truth, projection_assignment
        ))
    pair_values = _evaluate_pair_consistency(
        pair_observations, fixed_matrices, radials, pair_training_corners
    )
    pair_residuals: list[float] = []
    for record in pair_values["records"]:
        corner = record["physical_corner"]
        face1, face2 = PHYSICAL_FACE_ORDER[corner]
        for field in (
            f"projection_{face1}_mm",
            f"projection_{face2}_mm",
            "chord_mm",
            "corner_angle_deg",
        ):
            pair_residuals.append(float(record[f"difference_{field}"]) / 0.01)
    residuals.append(float(pair_weight) * np.asarray(pair_residuals, dtype=float))
    residuals.append(float(radial_regularization) * np.asarray(scaled_radials, dtype=float))
    return np.concatenate(residuals)


def _fit_fixed_matrix_radials(
    fixed_matrices: Mapping[str, np.ndarray],
    cmm_observations: list[dict[str, Any]],
    training_names: set[str],
    truth: Mapping[str, Any],
    projection_assignment: Mapping[str, Mapping[str, float]],
    pair_observations: list[dict[str, Any]],
    pair_weight: float,
    pair_training_corners: set[str],
    radial_regularization: float = 0.20,
) -> dict[str, float]:
    fit = least_squares(
        _fixed_matrix_radial_residual_vector,
        np.zeros(len(CAMERAS), dtype=float),
        args=(
            fixed_matrices,
            cmm_observations,
            training_names,
            truth,
            projection_assignment,
            pair_observations,
            pair_weight,
            pair_training_corners,
            radial_regularization,
        ),
        bounds=(-5.0 * np.ones(len(CAMERAS)), 5.0 * np.ones(len(CAMERAS))),
        max_nfev=300,
    )
    return {
        camera: float(fit.x[index]) * RADIAL_PARAMETER_SCALE
        for index, camera in enumerate(CAMERAS)
    }


def _evaluate_radial_models(
    observations: list[dict[str, Any]],
    capture_names: set[str],
    matrices: Mapping[str, np.ndarray],
    radials: Mapping[str, float],
    truth: Mapping[str, Any],
    projection_assignment: Mapping[str, Mapping[str, float]],
) -> dict[str, Any]:
    records: list[dict[str, Any]] = []
    normalized: list[float] = []
    for observation in observations:
        if observation["capture"] not in capture_names:
            continue
        corner = observation["physical_corner"]
        face1, face2 = PHYSICAL_FACE_ORDER[corner]
        predicted = _predicted_metrics(
            matrices[observation["camera"]], observation, radials[observation["camera"]]
        )
        row = {
            "capture": observation["capture"],
            "group": observation["group"],
            "camera": observation["camera"],
            "device_corner": observation["device_corner"],
            "physical_corner": corner,
        }
        for field, target, scale in (
            (f"projection_{face1}_mm", projection_assignment[corner][face1], 0.01),
            (f"projection_{face2}_mm", projection_assignment[corner][face2], 0.01),
            ("chord_mm", float(truth[corner]["chord_mm"]), 0.01),
            ("corner_angle_deg", float(truth[corner]["corner_angle_deg"]), 0.01),
        ):
            error = float(predicted[field] - target)
            row[field] = float(predicted[field])
            row[f"{field}_target"] = float(target)
            row[f"{field}_error"] = error
            normalized.append(error / scale)
        records.append(row)
    vector = np.asarray(normalized, dtype=float)
    return {
        "normalized_rms": math.sqrt(float(np.mean(vector**2))),
        "normalized_mean_abs": float(np.mean(np.abs(vector))),
        "records": records,
    }


def _evaluate(
    observations: list[dict[str, Any]],
    capture_names: set[str],
    matrices: Mapping[str, np.ndarray],
    truth: Mapping[str, Any],
    projection_assignment: Mapping[str, Mapping[str, float]],
) -> dict[str, Any]:
    records: list[dict[str, Any]] = []
    normalized: list[float] = []
    for observation in observations:
        if observation["capture"] not in capture_names:
            continue
        corner = observation["physical_corner"]
        face1, face2 = PHYSICAL_FACE_ORDER[corner]
        predicted = _predicted_metrics(matrices[observation["camera"]], observation)
        row = {
            "capture": observation["capture"], "group": observation["group"],
            "camera": observation["camera"], "device_corner": observation["device_corner"],
            "physical_corner": corner,
        }
        for field, target, scale in (
            (f"projection_{face1}_mm", projection_assignment[corner][face1], 0.01),
            (f"projection_{face2}_mm", projection_assignment[corner][face2], 0.01),
            ("chord_mm", float(truth[corner]["chord_mm"]), 0.01),
            ("corner_angle_deg", float(truth[corner]["corner_angle_deg"]), 0.01),
        ):
            error = float(predicted[field] - target)
            row[field], row[f"{field}_target"], row[f"{field}_error"] = float(predicted[field]), float(target), error
            normalized.append(error / scale)
        records.append(row)
    vector = np.asarray(normalized, dtype=float)
    return {
        "normalized_rms": math.sqrt(float(np.mean(vector**2))),
        "normalized_mean_abs": float(np.mean(np.abs(vector))),
        "records": records,
    }


def main() -> int:
    parser = argparse.ArgumentParser(description="拟合四相机倒角局部二维公制度量")
    parser.add_argument("--input", type=Path, default=DEFAULT_INPUT)
    parser.add_argument("--base-model", type=Path, default=DEFAULT_BASE_MODEL)
    parser.add_argument("--truth", type=Path, default=DEFAULT_TRUTH)
    parser.add_argument("--output", type=Path, default=DEFAULT_OUTPUT)
    parser.add_argument("--diagnostics", type=Path, default=DEFAULT_DIAGNOSTICS)
    parser.add_argument("--stations", type=int, default=180)
    parser.add_argument("--workers", type=int, default=3)
    parser.add_argument("--exclude", nargs="*", default=["14_16.hobj"])
    parser.add_argument("--consistency-first", type=Path, default=None,
                        help="可选：已人工确认同棒调头验证对的第一张HOBJ")
    parser.add_argument("--consistency-turned", type=Path, default=None,
                        help="可选：上述同一根棒物理调头后的HOBJ")
    parser.add_argument("--consistency-stations", type=int, default=360)
    parser.add_argument(
        "--consistency-weight-grid",
        nargs="*",
        type=float,
        default=[0.15, 0.25, 0.40, 0.60, 0.85, 1.20],
        help="同棒调头等值约束权重候选；只用于离线选模",
    )
    parser.add_argument(
        "--radial-cross-validation",
        action="store_true",
        help="试验相机局部一阶径向畸变，并对四个物理角逐一留出盲测",
    )
    parser.add_argument("--radial-regularization", type=float, default=0.20)
    parser.add_argument(
        "--freeze-linear-matrices",
        action="store_true",
        help="径向试验中冻结既有2×2矩阵，只识别四个小量畸变系数",
    )
    args = parser.parse_args()

    if (args.consistency_first is None) != (args.consistency_turned is None):
        parser.error("--consistency-first 和 --consistency-turned 必须成对提供")

    base_model = load_calibration(args.base_model)
    frozen_base_matrices = {
        camera: np.asarray(
            base_model["hobj_channels"][camera].get("metric_transform_2x2", np.eye(2)),
            dtype=float,
        )
        for camera in CAMERAS
    }
    raw_model = deepcopy(base_model)
    for config in raw_model["hobj_channels"].values():
        config["metric_transform_2x2"] = [[1.0, 0.0], [0.0, 1.0]]
    temporary = args.output.with_name("_temporary_identity_metric_model.json")
    temporary.write_text(json.dumps(raw_model, ensure_ascii=False, indent=2), encoding="utf-8")
    try:
        excluded = set(args.exclude)
        paths = sorted(path for path in args.input.glob("*/*.hobj") if path.name not in excluded)
        if len(paths) != 19:
            raise RuntimeError(f"排除异常图后应为19张，实际为{len(paths)}张")
        jobs = [(str(path), str(temporary), int(args.stations)) for path in paths]
        with ProcessPoolExecutor(max_workers=int(args.workers)) as executor:
            scans = list(executor.map(_measure_worker, jobs))
        consistency_scans: list[dict[str, Any]] = []
        if args.consistency_first is not None and args.consistency_turned is not None:
            pair_jobs = [
                (str(args.consistency_first.resolve()), str(temporary), int(args.consistency_stations)),
                (str(args.consistency_turned.resolve()), str(temporary), int(args.consistency_stations)),
            ]
            with ProcessPoolExecutor(max_workers=2) as executor:
                consistency_scans = list(executor.map(_measure_worker, pair_jobs))
    finally:
        temporary.unlink(missing_ok=True)

    observations = _physical_observations(scans)
    training_names, validation_names = _split_capture_names(scans)
    truth_payload = json.loads(args.truth.read_text(encoding="utf-8"))
    candidates = _truth_candidates(truth_payload)
    rankings: list[dict[str, Any]] = []
    fitted: dict[tuple[str, str], tuple[dict[str, np.ndarray], dict[str, dict[str, float]]]] = {}
    for candidate_name, truth in candidates.items():
        for assignment_name, assignment in _projection_assignments(truth):
            matrices = _fit_matrices(observations, training_names, truth, assignment)
            validation = _evaluate(observations, validation_names, matrices, truth, assignment)
            rankings.append({
                "truth_candidate": candidate_name,
                "projection_assignment": assignment_name,
                "validation_normalized_rms": validation["normalized_rms"],
                "validation_normalized_mean_abs": validation["normalized_mean_abs"],
            })
            fitted[(candidate_name, assignment_name)] = (matrices, assignment)
    rankings.sort(key=lambda item: item["validation_normalized_rms"])
    winner = rankings[0]
    matrices, assignment = fitted[(winner["truth_candidate"], winner["projection_assignment"])]
    selected_truth = candidates[winner["truth_candidate"]]

    identity = {camera: np.eye(2) for camera in matrices}
    raw_validation = _evaluate(observations, validation_names, identity, selected_truth, assignment)
    corrected_training = _evaluate(observations, training_names, matrices, selected_truth, assignment)
    corrected_validation = _evaluate(observations, validation_names, matrices, selected_truth, assignment)
    if args.freeze_linear_matrices:
        matrices = {camera: matrix.copy() for camera, matrix in frozen_base_matrices.items()}
        corrected_training = _evaluate(
            observations, training_names, matrices, selected_truth, assignment
        )
        corrected_validation = _evaluate(
            observations, validation_names, matrices, selected_truth, assignment
        )
    baseline_matrices = {camera: matrix.copy() for camera, matrix in matrices.items()}
    baseline_cmm_validation_rms = float(corrected_validation["normalized_rms"])
    radial_distortions = {camera: 0.0 for camera in CAMERAS}

    consistency_diagnostics: dict[str, Any] | None = None
    if consistency_scans:
        pair_observations = _explicit_turnover_pair_observations(
            consistency_scans[0], consistency_scans[1]
        )
        baseline_pair = _evaluate_pair_consistency(pair_observations, matrices)
        candidates_with_pair: list[dict[str, Any]] = []
        candidate_matrices: dict[float, dict[str, np.ndarray]] = {}
        maximum_cmm_rms = corrected_validation["normalized_rms"] * 1.10
        for pair_weight in args.consistency_weight_grid:
            if pair_weight <= 0.0:
                raise ValueError("同棒调头一致性权重必须大于0")
            trial_matrices = _fit_joint_matrices(
                matrices,
                observations,
                training_names,
                selected_truth,
                assignment,
                pair_observations,
                float(pair_weight),
            )
            trial_cmm = _evaluate(
                observations, validation_names, trial_matrices, selected_truth, assignment
            )
            trial_pair = _evaluate_pair_consistency(pair_observations, trial_matrices)
            combined = math.sqrt(
                (trial_cmm["normalized_rms"] ** 2 + trial_pair["normalized_rms"] ** 2) / 2.0
            )
            candidates_with_pair.append({
                "pair_weight": float(pair_weight),
                "cmm_validation_normalized_rms": trial_cmm["normalized_rms"],
                "pair_normalized_rms": trial_pair["normalized_rms"],
                "combined_normalized_rms": combined,
                "passes_cmm_guard": trial_cmm["normalized_rms"] <= maximum_cmm_rms,
                "pair_records": trial_pair["records"],
            })
            candidate_matrices[float(pair_weight)] = trial_matrices
        acceptable = [item for item in candidates_with_pair if item["passes_cmm_guard"]]
        if not acceptable:
            raise RuntimeError("所有同棒调头联合优化候选均使CMM留出误差恶化超过10%")
        selected_pair = min(acceptable, key=lambda item: item["combined_normalized_rms"])
        if selected_pair["pair_normalized_rms"] >= baseline_pair["normalized_rms"]:
            raise RuntimeError("同棒调头联合优化未改善外部验证对，拒绝写入候选模型")
        matrices = candidate_matrices[selected_pair["pair_weight"]]
        corrected_training = _evaluate(
            observations, training_names, matrices, selected_truth, assignment
        )
        corrected_validation = _evaluate(
            observations, validation_names, matrices, selected_truth, assignment
        )
        consistency_diagnostics = {
            "explicit_pair_only_in_calibrator": True,
            "runtime_orientation_inference": False,
            "first_capture": str(args.consistency_first.resolve()),
            "turned_capture": str(args.consistency_turned.resolve()),
            "station_count_per_capture": int(args.consistency_stations),
            "baseline_pair_normalized_rms": baseline_pair["normalized_rms"],
            "baseline_pair_records": baseline_pair["records"],
            "cmm_validation_guard_max_normalized_rms": maximum_cmm_rms,
            "selected_pair_weight": selected_pair["pair_weight"],
            "selected_pair_normalized_rms": selected_pair["pair_normalized_rms"],
            "candidate_rankings": sorted(
                candidates_with_pair, key=lambda item: item["combined_normalized_rms"]
            ),
        }

        if args.radial_cross_validation:
            all_pair_corners = set(PHYSICAL_FACE_ORDER)
            radial_rankings: list[dict[str, Any]] = []
            for pair_weight in args.consistency_weight_grid:
                holdout_records: list[dict[str, Any]] = []
                for holdout_corner in PHYSICAL_FACE_ORDER:
                    if args.freeze_linear_matrices:
                        trial_matrices = baseline_matrices
                        trial_radials = _fit_fixed_matrix_radials(
                            baseline_matrices,
                            observations,
                            training_names,
                            selected_truth,
                            assignment,
                            pair_observations,
                            float(pair_weight),
                            all_pair_corners - {holdout_corner},
                            float(args.radial_regularization),
                        )
                    else:
                        trial_matrices, trial_radials = _fit_joint_radial_models(
                            baseline_matrices,
                            observations,
                            training_names,
                            selected_truth,
                            assignment,
                            pair_observations,
                            float(pair_weight),
                            all_pair_corners - {holdout_corner},
                            float(args.radial_regularization),
                        )
                    trial_cmm = _evaluate_radial_models(
                        observations,
                        validation_names,
                        trial_matrices,
                        trial_radials,
                        selected_truth,
                        assignment,
                    )
                    holdout_pair = _evaluate_pair_consistency(
                        pair_observations,
                        trial_matrices,
                        trial_radials,
                        {holdout_corner},
                    )
                    holdout_records.append({
                        "holdout_corner": holdout_corner,
                        "cmm_validation_normalized_rms": trial_cmm["normalized_rms"],
                        "holdout_pair_normalized_rms": holdout_pair["normalized_rms"],
                        "holdout_pair_records": holdout_pair["records"],
                        "radial_distortion_per_mm2": trial_radials,
                    })
                cmm_rms = math.sqrt(float(np.mean([
                    item["cmm_validation_normalized_rms"] ** 2 for item in holdout_records
                ])))
                pair_rms = math.sqrt(float(np.mean([
                    item["holdout_pair_normalized_rms"] ** 2 for item in holdout_records
                ])))
                radial_rankings.append({
                    "pair_weight": float(pair_weight),
                    "cross_validated_cmm_rms": cmm_rms,
                    "cross_validated_holdout_pair_rms": pair_rms,
                    "cross_validated_combined_rms": math.sqrt((cmm_rms**2 + pair_rms**2) / 2.0),
                    "passes_cmm_guard": all(
                        item["cmm_validation_normalized_rms"] <= maximum_cmm_rms
                        for item in holdout_records
                    ),
                    "holdouts": holdout_records,
                })
            accepted_radial = [item for item in radial_rankings if item["passes_cmm_guard"]]
            if not accepted_radial:
                raise RuntimeError("径向指纹模型的所有留出候选均未通过CMM误差门槛")
            radial_choice = min(
                accepted_radial, key=lambda item: item["cross_validated_combined_rms"]
            )
            if args.freeze_linear_matrices:
                full_matrices = baseline_matrices
                full_radials = _fit_fixed_matrix_radials(
                    baseline_matrices,
                    observations,
                    training_names,
                    selected_truth,
                    assignment,
                    pair_observations,
                    float(radial_choice["pair_weight"]),
                    all_pair_corners,
                    float(args.radial_regularization),
                )
            else:
                full_matrices, full_radials = _fit_joint_radial_models(
                    baseline_matrices,
                    observations,
                    training_names,
                    selected_truth,
                    assignment,
                    pair_observations,
                    float(radial_choice["pair_weight"]),
                    all_pair_corners,
                    float(args.radial_regularization),
                )
            radial_cmm = _evaluate_radial_models(
                observations,
                validation_names,
                full_matrices,
                full_radials,
                selected_truth,
                assignment,
            )
            radial_pair = _evaluate_pair_consistency(
                pair_observations, full_matrices, full_radials
            )
            if radial_cmm["normalized_rms"] > maximum_cmm_rms:
                raise RuntimeError("径向指纹完整拟合未通过CMM误差门槛")
            if radial_pair["normalized_rms"] >= baseline_pair["normalized_rms"]:
                raise RuntimeError("径向指纹完整拟合未改善105-4一致性")
            matrices = full_matrices
            radial_distortions = full_radials
            corrected_training = _evaluate_radial_models(
                observations,
                training_names,
                matrices,
                radial_distortions,
                selected_truth,
                assignment,
            )
            corrected_validation = radial_cmm
            consistency_diagnostics["radial_cross_validation"] = {
                "model": "fixed_camera_linear_metric_plus_first_order_radial_distortion",
                "linear_matrices_frozen": bool(args.freeze_linear_matrices),
                "distortion_center_xz_mm": [20.25, 0.0],
                "radial_regularization": float(args.radial_regularization),
                "baseline_cmm_validation_normalized_rms": baseline_cmm_validation_rms,
                "selected_pair_weight": radial_choice["pair_weight"],
                "selected_cross_validated_holdout_pair_rms": radial_choice[
                    "cross_validated_holdout_pair_rms"
                ],
                "selected_cross_validated_cmm_rms": radial_choice[
                    "cross_validated_cmm_rms"
                ],
                "full_pair_normalized_rms": radial_pair["normalized_rms"],
                "full_pair_records": radial_pair["records"],
                "full_cmm_validation_normalized_rms": radial_cmm["normalized_rms"],
                "radial_distortion_per_mm2": radial_distortions,
                "rankings": sorted(
                    radial_rankings,
                    key=lambda item: item["cross_validated_combined_rms"],
                ),
            }

    output = deepcopy(base_model)
    output["version"] = 2
    output["model"] = "long_rod_chamfer_four_camera_fingerprint_metric_correction"
    output["algorithm_name"] = "长棒倒角几何·四相机指纹度量修正算法"
    output["description"] = "四相机指纹二维公制度量修正；不含16个终值补偿或运行时方向分类。"
    for camera, matrix in matrices.items():
        output["hobj_channels"][camera]["metric_transform_2x2"] = matrix.tolist()
        output["hobj_channels"][camera]["metric_radial_distortion_per_mm2"] = float(
            radial_distortions[camera]
        )
        output["hobj_channels"][camera]["metric_distortion_center_xz_mm"] = [20.25, 0.0]
    output["camera_metric_calibration"] = {
        "method": "fixed_per_camera_2d_metric_transform_from_P_T1_T2_geometry",
        "uses_final_value_offsets": False,
        "runtime_path_classification": False,
        "selected_truth_candidate": winner["truth_candidate"],
        "cmm_cb_cd_note": truth_payload["status"],
        "corrected_validation_normalized_rms": corrected_validation["normalized_rms"],
        "explicit_same_bar_consistency_used": consistency_diagnostics is not None,
        "diagnostics_file": args.diagnostics.name,
    }
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(output, ensure_ascii=False, indent=2), encoding="utf-8")
    diagnostics = {
        "method": "fixed_per_camera_2d_metric_transform_from_P_T1_T2_geometry",
        "runtime_model": str(args.output.resolve()),
        "truth_source": str(args.truth.resolve()),
        "selected_truth_candidate": winner["truth_candidate"],
        "cmm_cb_cd_note": truth_payload["status"],
        "excluded_captures": list(args.exclude),
        "training_captures": sorted(training_names),
        "validation_captures": sorted(validation_names),
        "station_count_per_capture": int(args.stations),
        "projection_face_values_mm": assignment,
        "matrices": {camera: matrix.tolist() for camera, matrix in matrices.items()},
        "radial_distortion_per_mm2": radial_distortions,
        "raw_validation_normalized_rms": raw_validation["normalized_rms"],
        "corrected_training_normalized_rms": corrected_training["normalized_rms"],
        "corrected_validation_normalized_rms": corrected_validation["normalized_rms"],
        "candidate_rankings": rankings,
        "validation_records": corrected_validation["records"],
        "same_bar_turnover_consistency": consistency_diagnostics,
    }
    args.diagnostics.parent.mkdir(parents=True, exist_ok=True)
    args.diagnostics.write_text(json.dumps(diagnostics, ensure_ascii=False, indent=2), encoding="utf-8")
    print(json.dumps({
        "output": str(args.output.resolve()),
        "diagnostics": str(args.diagnostics.resolve()),
        "selected_truth_candidate": winner["truth_candidate"],
        "projection_assignment": assignment,
        "matrices": {camera: matrix.tolist() for camera, matrix in matrices.items()},
        "raw_validation_normalized_rms": raw_validation["normalized_rms"],
        "corrected_training_normalized_rms": corrected_training["normalized_rms"],
        "corrected_validation_normalized_rms": corrected_validation["normalized_rms"],
        "top_candidates": rankings[:4],
        "same_bar_turnover_consistency": consistency_diagnostics,
    }, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
