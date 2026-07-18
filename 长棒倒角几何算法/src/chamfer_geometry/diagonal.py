"""基于四个实体倒角端点的长棒截面对角线测量。

定义固定为：

* 对角线1：AB 倒角的 A 侧端点（角点2）到 CD 倒角的 C 侧端点（角点6）；
* 对角线2：AD 倒角的 A 侧端点（角点1）到 CB/BC 倒角的 C 侧端点（角点5）。

每个端点先由倒角几何 v3 在单相机局部坐标中实测，再用长棒模板长度模型中的
四相机全局矩阵、随扫描位置变化的原点曲线和固定 Y 偏移对齐到同一 X/Z 截面。
运行时不读取文件名中的正反、调头或转向提示，也不应用对角线终值偏移。
"""

from __future__ import annotations

import hashlib
import json
import math
from pathlib import Path
from typing import Any, Mapping

import numpy as np

from .core import extract_corner_geometry
from .measurement import (
    REQUIRED_CORNERS,
    _camera_profile,
    _metric_geometry,
    _open_hobj,
    _row_bounds,
    _trimmed_statistics,
)


ALGORITHM_NAME = "长棒实体倒角端点全局对角线算法"
ALGORITHM_ID = "long_rod_physical_chamfer_endpoint_global_diagonal"
ALGORITHM_VERSION = 1

CAMERAS = ("Top", "Right", "Left", "Down")

# corner/face 精确指向用户定义的八个实体倒角端点。
ENDPOINTS: dict[str, dict[str, str]] = {
    "point1": {"corner": "AD", "face": "A", "camera": "Top"},
    "point2": {"corner": "AB", "face": "A", "camera": "Right"},
    "point5": {"corner": "CB", "face": "C", "camera": "Left"},
    "point6": {"corner": "CD", "face": "C", "camera": "Down"},
}

DIAGONALS: dict[str, tuple[str, str]] = {
    "diagonal_1": ("point2", "point6"),
    "diagonal_2": ("point1", "point5"),
}


def _sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with Path(path).open("rb") as handle:
        for block in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def load_global_coordinate_calibration(path: Path) -> dict[str, Any]:
    """读取长棒模板长度模型中可追溯的低层四相机全局坐标参数。"""

    path = Path(path)
    payload = json.loads(path.read_text(encoding="utf-8"))
    if payload.get("algorithm_id") != "long_rod_template_calibrated_length":
        raise ValueError("全局坐标模型必须来自长棒模板标定长度算法")
    if payload.get("valid") is not True or payload.get("release_ready") is not True:
        raise ValueError("长棒模板长度模型尚未标记为 valid/release_ready")
    if payload.get("path_based_orientation_detection") is not False:
        raise ValueError("全局坐标模型不得使用路径方向识别")
    if payload.get("orientation_mapping_applied") is not False:
        raise ValueError("全局坐标模型不得在运行时执行调头映射")
    if payload.get("contains_final_edge_offsets") is not False:
        raise ValueError("全局坐标模型不得包含独立边长终值偏移")

    runtime = payload.get("runtime_calibration", {})
    rows = np.asarray(runtime.get("rows", []), dtype=int)
    origins = runtime.get("origins", [])
    matrices = runtime.get("matrices", {})
    biases = runtime.get("camera_y_bias_mm_by_camera", {})
    if rows.ndim != 1 or rows.size < 10 or len(origins) != rows.size:
        raise ValueError("全局坐标模型的 rows/origins 不完整")
    if np.any(np.diff(rows) <= 0):
        raise ValueError("全局坐标模型的 rows 必须严格递增")
    if set(matrices) != set(CAMERAS) or set(biases) != set(CAMERAS):
        raise ValueError("全局坐标模型必须包含四相机矩阵和固定 Y 偏移")
    if runtime.get("y_synchronization") != "fixed_bias_interpolation":
        raise ValueError("对角线第一版要求 fixed_bias_interpolation 固定 Y 同步")
    if float(runtime.get("y_scale_mm_per_row", 0.0)) <= 0.0:
        raise ValueError("扫描行 Y 公制比例无效")
    for camera in CAMERAS:
        matrix = np.asarray(matrices[camera], dtype=float)
        if matrix.shape != (2, 2) or not np.all(np.isfinite(matrix)):
            raise ValueError(f"{camera} 的全局坐标矩阵无效")
        for index, origin in enumerate(origins):
            point = np.asarray(origin[camera], dtype=float)
            if point.shape != (2,) or not np.all(np.isfinite(point)):
                raise ValueError(f"{camera} 的第 {index} 个全局原点无效")
    return payload


def _raw_reversed_point_to_vendor_local(
    point_xz_mm: np.ndarray,
    camera_config: Mapping[str, Any],
    parameters: Mapping[str, Any],
) -> np.ndarray:
    """把倒角模块的镜像裁剪坐标还原成交付坐标链使用的原始列坐标。"""

    start = int(parameters.get("reversed_crop_start_pixel", 500))
    stop = int(parameters.get("reversed_crop_stop_pixel", 3200))
    scale = float(camera_config["local_x_scale_mm_per_pixel"])
    # 原图3200列先镜像，再取[500:3200]；裁剪坐标0对应原图列2699。
    reversed_origin_mm = float(stop - start - 1) * scale
    point = np.asarray(point_xz_mm, dtype=float)
    return np.array([reversed_origin_mm - point[0], point[1]], dtype=float)


def _global_endpoint(
    geometry: Mapping[str, Any],
    metric_geometry: Mapping[str, Any],
    *,
    face: str,
    corner_config: Mapping[str, Any],
    camera_config: Mapping[str, Any],
    parameters: Mapping[str, Any],
    global_matrix: np.ndarray,
    global_origin: np.ndarray,
) -> np.ndarray:
    """把一个实测倒角端点变换到公共 X/Z 坐标。

    全局位置以原始轮廓拟合出的理论尖角为锚点；倒角 v3 的公制度量只修正从理论
    尖角到端点的射线。这样既保留四相机全局外参，又不会把局部矩阵对绝对坐标原点
    的选择误当成跨相机平移。
    """

    left_face = str(corner_config["reversed_profile_left_face"])
    right_face = str(corner_config["reversed_profile_right_face"])
    if face == left_face:
        endpoint_key = "left_endpoint_xz_mm"
    elif face == right_face:
        endpoint_key = "right_endpoint_xz_mm"
    else:
        raise ValueError(f"{corner_config} 中不存在面 {face} 的倒角端点")

    raw_corner_reversed = np.asarray(geometry["theoretical_corner_xz_mm"], dtype=float)
    raw_corner_vendor = _raw_reversed_point_to_vendor_local(
        raw_corner_reversed, camera_config, parameters
    )
    global_corner = global_origin + global_matrix @ raw_corner_vendor

    corrected_corner = np.asarray(metric_geometry["theoretical_corner_xz_mm"], dtype=float)
    corrected_endpoint = np.asarray(metric_geometry[endpoint_key], dtype=float)
    corrected_ray_reversed = corrected_endpoint - corrected_corner
    corrected_ray_vendor = np.array(
        [-corrected_ray_reversed[0], corrected_ray_reversed[1]], dtype=float
    )
    return global_corner + global_matrix @ corrected_ray_vendor


def _interpolate_points(
    samples: list[dict[str, Any]],
    target_y: np.ndarray,
    *,
    maximum_gap_mm: float,
) -> np.ndarray:
    """对一个相机端点序列按物理 Y 插值；过大缺口不跨越。"""

    result = np.full((target_y.size, 2), np.nan, dtype=float)
    if len(samples) < 2:
        return result
    ordered = sorted(samples, key=lambda item: float(item["physical_y_mm"]))
    y = np.asarray([item["physical_y_mm"] for item in ordered], dtype=float)
    points = np.asarray([item["global_xz_mm"] for item in ordered], dtype=float)
    for index, value in enumerate(target_y):
        right = int(np.searchsorted(y, value, side="left"))
        if right < y.size and abs(float(y[right] - value)) <= 1e-9:
            result[index] = points[right]
            continue
        left = right - 1
        if left < 0 or right >= y.size:
            continue
        gap = float(y[right] - y[left])
        if gap <= 0.0 or gap > maximum_gap_mm:
            continue
        fraction = float((value - y[left]) / gap)
        result[index] = points[left] * (1.0 - fraction) + points[right] * fraction
    return result


def _regional_means(records: list[dict[str, Any]], field: str) -> dict[str, float]:
    """保留头/中/尾三段均值，便于发现局部弯曲或坐标漂移。"""

    if not records:
        raise RuntimeError("没有可分段统计的对角线切片")
    groups = np.array_split(np.arange(len(records)), 3)
    labels = ("head", "middle", "tail")
    return {
        label: float(np.mean([float(records[int(index)][field]) for index in indices]))
        for label, indices in zip(labels, groups)
        if indices.size
    }


def measure_diagonals(
    input_path: Path,
    chamfer_calibration: Mapping[str, Any],
    global_coordinate_calibration: Mapping[str, Any],
    *,
    station_count: int | None = None,
) -> dict[str, Any]:
    """测量一个 HOBJ 的两条实体倒角端点对角线。"""

    input_path = Path(input_path).resolve()
    parameters = chamfer_calibration["algorithm_parameters"]
    runtime = global_coordinate_calibration["runtime_calibration"]
    model_rows = np.asarray(runtime["rows"], dtype=int)
    requested = int(station_count or model_rows.size)
    if requested < 10:
        raise ValueError("station_count 至少为 10")
    if requested == model_rows.size:
        model_indices = np.arange(model_rows.size, dtype=int)
    else:
        model_indices = np.unique(
            np.rint(np.linspace(0, model_rows.size - 1, requested)).astype(int)
        )
    rows = model_rows[model_indices]
    matrices = {
        camera: np.asarray(runtime["matrices"][camera], dtype=float)
        for camera in CAMERAS
    }
    origins = runtime["origins"]
    y_scale = float(runtime["y_scale_mm_per_row"])
    y_bias = {
        camera: float(runtime["camera_y_bias_mm_by_camera"][camera])
        for camera in CAMERAS
    }

    images, mappings = _open_hobj(input_path, chamfer_calibration)
    try:
        invalid = float(parameters.get("invalid_profile_threshold", -9999.0))
        bounds = {camera: _row_bounds(image, invalid) for camera, image in images.items()}
        margin = int(parameters.get("end_margin_rows", 250))
        endpoint_samples: dict[str, list[dict[str, Any]]] = {
            point: [] for point in ENDPOINTS
        }
        rejected: dict[str, list[dict[str, Any]]] = {point: [] for point in ENDPOINTS}

        for model_index, row in zip(model_indices, rows):
            for point_name, endpoint in ENDPOINTS.items():
                corner = endpoint["corner"]
                face = endpoint["face"]
                camera = endpoint["camera"]
                lower, upper = bounds[camera]
                if int(row) < lower + margin or int(row) > upper - margin:
                    rejected[point_name].append(
                        {"row": int(row), "reason": "超出该相机去端部余量后的有效范围"}
                    )
                    continue
                try:
                    x, z = _camera_profile(
                        images[camera],
                        int(row),
                        chamfer_calibration["hobj_channels"][camera],
                        parameters,
                    )
                    geometry = extract_corner_geometry(x, z, parameters)
                    metric_geometry = _metric_geometry(
                        geometry, chamfer_calibration["hobj_channels"][camera]
                    )
                    global_point = _global_endpoint(
                        geometry,
                        metric_geometry,
                        face=face,
                        corner_config=chamfer_calibration["corners"][corner],
                        camera_config=chamfer_calibration["hobj_channels"][camera],
                        parameters=parameters,
                        global_matrix=matrices[camera],
                        global_origin=np.asarray(origins[int(model_index)][camera], dtype=float),
                    )
                    endpoint_samples[point_name].append(
                        {
                            "row": int(row),
                            "physical_y_mm": float(row) * y_scale - y_bias[camera],
                            "global_xz_mm": global_point.tolist(),
                            "chamfer_fit_rms_mm": float(geometry["chamfer_line"].rms_mm),
                        }
                    )
                except Exception as exc:
                    rejected[point_name].append({"row": int(row), "reason": str(exc)})

        minimum_fraction = float(parameters.get("minimum_valid_station_fraction", 0.70))
        for point_name, samples in endpoint_samples.items():
            if len(samples) < math.ceil(rows.size * minimum_fraction):
                raise RuntimeError(
                    f"{point_name} 有效端点不足：{len(samples)}/{rows.size}，"
                    f"要求至少 {minimum_fraction:.0%}"
                )

        target_y = rows.astype(float) * y_scale
        common_start = max(samples[0]["physical_y_mm"] for samples in endpoint_samples.values())
        common_stop = min(samples[-1]["physical_y_mm"] for samples in endpoint_samples.values())
        target_y = target_y[(target_y >= common_start) & (target_y <= common_stop)]
        if target_y.size < 10:
            raise RuntimeError("固定 Y 同步后共同物理切片不足10片")
        nominal_step = float(np.median(np.diff(rows.astype(float)))) * y_scale
        maximum_gap = max(0.25, nominal_step * 4.0)
        aligned = {
            point_name: _interpolate_points(
                samples, target_y, maximum_gap_mm=maximum_gap
            )
            for point_name, samples in endpoint_samples.items()
        }

        slice_records: list[dict[str, Any]] = []
        diagonal_records: dict[str, list[dict[str, Any]]] = {
            diagonal: [] for diagonal in DIAGONALS
        }
        for index, physical_y in enumerate(target_y):
            record: dict[str, Any] = {
                "slice_index": int(index + 1),
                "common_physical_y_mm": float(physical_y),
            }
            for point_name, points in aligned.items():
                point = points[index]
                record[f"{point_name}_global_x_mm"] = (
                    float(point[0]) if np.all(np.isfinite(point)) else None
                )
                record[f"{point_name}_global_z_mm"] = (
                    float(point[1]) if np.all(np.isfinite(point)) else None
                )
            for diagonal, (first_name, second_name) in DIAGONALS.items():
                first = aligned[first_name][index]
                second = aligned[second_name][index]
                if np.all(np.isfinite(first)) and np.all(np.isfinite(second)):
                    distance = float(np.linalg.norm(first - second))
                    record[f"{diagonal}_mm"] = distance
                    diagonal_records[diagonal].append(
                        {
                            "slice_index": int(index + 1),
                            "common_physical_y_mm": float(physical_y),
                            f"{diagonal}_mm": distance,
                        }
                    )
                else:
                    record[f"{diagonal}_mm"] = None
            slice_records.append(record)
    finally:
        images.clear()
        mappings.clear()

    trim_fraction = float(parameters.get("aggregate_trim_fraction", 0.10))
    metrics: dict[str, Any] = {}
    values: dict[str, float] = {}
    for diagonal, records in diagonal_records.items():
        if len(records) < math.ceil(target_y.size * minimum_fraction):
            raise RuntimeError(
                f"{diagonal} 有效同步切片不足：{len(records)}/{target_y.size}"
            )
        field = f"{diagonal}_mm"
        statistics = _trimmed_statistics(
            [float(record[field]) for record in records], trim_fraction
        )
        statistics["regional_mean_mm"] = _regional_means(records, field)
        statistics["endpoint_names"] = list(DIAGONALS[diagonal])
        metrics[diagonal] = statistics
        values[field] = float(statistics["value"])

    return {
        "algorithm": {
            "name": ALGORITHM_NAME,
            "id": ALGORITHM_ID,
            "version": ALGORITHM_VERSION,
            "release_state": "first_runnable_candidate",
        },
        "input_path": str(input_path),
        "capture_id": input_path.stem,
        "definition": {
            "diagonal_1": "AB角点2(A侧端点) 到 CD角点6(C侧端点)",
            "diagonal_2": "AD角点1(A侧端点) 到 BC/CB角点5(C侧端点)",
        },
        "values": values,
        "metrics": metrics,
        "station_plan": {
            "requested_count": requested,
            "model_station_count": int(rows.size),
            "common_physical_station_count": int(target_y.size),
            "common_physical_y_range_mm": [float(target_y[0]), float(target_y[-1])],
            "maximum_interpolation_gap_mm": maximum_gap,
            "aggregate": f"{trim_fraction:.0%} trimmed mean",
        },
        "coordinate_chain": {
            "chamfer_endpoint_geometry": "v3 camera fingerprint metric correction",
            "global_xz": "long_rod_template_length camera matrices plus row-varying origins",
            "physical_y": "row * y_scale - fixed camera_y_bias",
            "y_scale_mm_per_row": y_scale,
            "camera_y_bias_mm_by_camera": y_bias,
            "length_bd_pair_k_used": False,
            "final_diagonal_offsets_used": False,
        },
        "endpoint_samples": endpoint_samples,
        "rejected_endpoint_samples": rejected,
        "slice_records": slice_records,
        "camera_row_bounds": {camera: list(bound) for camera, bound in bounds.items()},
        "orientation_inference_used": False,
        "orientation_mapping_applied": False,
        "final_value_compensation_used": False,
    }


def attach_model_traceability(
    result: dict[str, Any],
    *,
    chamfer_calibration_path: Path,
    global_coordinate_calibration_path: Path,
) -> dict[str, Any]:
    """在结果中记录两份低层标定文件的路径和哈希。"""

    result["calibration_files"] = {
        "chamfer_metric": {
            "path": str(Path(chamfer_calibration_path).resolve()),
            "sha256": _sha256(Path(chamfer_calibration_path)),
        },
        "global_coordinate": {
            "path": str(Path(global_coordinate_calibration_path).resolve()),
            "sha256": _sha256(Path(global_coordinate_calibration_path)),
        },
    }
    return result
