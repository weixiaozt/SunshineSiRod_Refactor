"""HOBJ 四相机倒角测量、固定物理映射和多切片统计。"""

from __future__ import annotations

import json
import math
from pathlib import Path
from typing import Any, Mapping
import warnings

import numpy as np

from .core import LineFit, extract_corner_geometry, included_angle_deg


ALGORITHM_NAME = "长棒倒角几何·四相机指纹度量修正算法"
ALGORITHM_ID = "long_rod_chamfer_four_camera_fingerprint_metric_correction"
ALGORITHM_VERSION = 3
CALIBRATION_MODEL_ID = ALGORITHM_ID
REQUIRED_CORNERS = ("AD", "AB", "CB", "CD")
HOBJ_SHAPE = (20000, 3200)


def load_calibration(path: Path) -> dict[str, Any]:
    """读取低层相机公制标定；拒绝任何终值补偿和方向分类配置。"""

    path = Path(path)
    payload = json.loads(path.read_text(encoding="utf-8"))
    if payload.get("model") != CALIBRATION_MODEL_ID:
        raise ValueError(f"不是{ALGORITHM_NAME}的相机公制度量模型")
    forbidden_exact = {
        "final_value_offsets",
        "angle_offsets_deg",
        "orientation_detector",
        "orientation_models",
        "turnover_models",
        "runtime_orientation_detection",
    }

    def walk(value: Any, trail: str = "") -> None:
        if isinstance(value, Mapping):
            for key, child in value.items():
                lower = str(key).lower()
                if lower in forbidden_exact:
                    raise ValueError(f"标定中禁止出现终值补偿或方向分类字段：{trail}{key}")
                walk(child, f"{trail}{key}.")
        elif isinstance(value, list):
            for index, child in enumerate(value):
                walk(child, f"{trail}{index}.")

    walk(payload)
    cameras = payload.get("hobj_channels", {})
    corners = payload.get("corners", {})
    if set(corners) != set(REQUIRED_CORNERS):
        raise ValueError(f"角点映射必须完整包含 {REQUIRED_CORNERS}")
    if len(cameras) != 4:
        raise ValueError("HOBJ 必须配置四个相机通道")
    for camera, config in cameras.items():
        if float(config.get("local_x_scale_mm_per_pixel", 0.0)) <= 0.0:
            raise ValueError(f"{camera} 的横向公制比例无效")
        if float(config.get("profile_value_scale_mm_per_unit", 0.0)) <= 0.0:
            raise ValueError(f"{camera} 的高度公制比例无效")
        matrix = np.asarray(config.get("metric_transform_2x2", np.eye(2)), dtype=float)
        if matrix.shape != (2, 2) or not np.all(np.isfinite(matrix)):
            raise ValueError(f"{camera} 的 metric_transform_2x2 必须是有限2×2矩阵")
        singular_values = np.linalg.svd(matrix, compute_uv=False)
        if float(np.min(singular_values)) <= 0.25 or float(np.max(singular_values)) >= 2.0:
            raise ValueError(f"{camera} 的底层公制度量矩阵尺度异常")
        radial = float(config.get("metric_radial_distortion_per_mm2", 0.0))
        center = np.asarray(config.get("metric_distortion_center_xz_mm", [20.25, 0.0]), dtype=float)
        if not math.isfinite(radial) or abs(radial) > 5e-4:
            raise ValueError(f"{camera} 的径向公制畸变系数异常")
        if center.shape != (2,) or not np.all(np.isfinite(center)):
            raise ValueError(f"{camera} 的径向公制畸变中心必须是有限二维坐标")
        local_scale = float(config.get("metric_local_ray_scale_per_mm2", 0.0))
        local_reference = float(config.get("metric_local_ray_reference_mm2", 1.0))
        if not math.isfinite(local_scale) or abs(local_scale) > 0.5:
            raise ValueError(f"{camera} 的局部射线尺度非线性系数异常")
        if not math.isfinite(local_reference) or local_reference <= 0.0:
            raise ValueError(f"{camera} 的局部射线尺度参考必须为正数")
        local_bounds = np.asarray(
            config.get("metric_local_ray_delta_bounds_mm2", [-float("inf"), float("inf")]),
            dtype=float,
        )
        if local_bounds.shape != (2,) or local_bounds[0] >= local_bounds[1]:
            raise ValueError(f"{camera} 的局部射线标定域必须是递增的二维范围")
    for corner, config in corners.items():
        camera = config.get("camera")
        if camera not in cameras:
            raise ValueError(f"{corner} 引用了未知相机 {camera}")
        left_face = config.get("reversed_profile_left_face")
        right_face = config.get("reversed_profile_right_face")
        order = config.get("output_face_order")
        if set((left_face, right_face)) != set(order or []):
            raise ValueError(f"{corner} 的端点面号映射不自洽")
    return payload


def _open_hobj(path: Path, calibration: Mapping[str, Any]) -> tuple[dict[str, np.memmap], list[np.memmap]]:
    path = Path(path)
    if not path.is_file() or path.suffix.lower() != ".hobj":
        raise ValueError(f"输入必须是 .hobj 文件：{path}")
    expected_minimum = max(
        int(config["byte_offset"]) + int(np.prod(HOBJ_SHAPE)) * np.dtype(np.float32).itemsize
        for config in calibration["hobj_channels"].values()
    )
    if path.stat().st_size < expected_minimum:
        raise ValueError(f"HOBJ 文件长度不足：{path.stat().st_size} < {expected_minimum}")
    images: dict[str, np.memmap] = {}
    mappings: list[np.memmap] = []
    for camera, config in calibration["hobj_channels"].items():
        image = np.memmap(
            path,
            dtype=np.float32,
            mode="r",
            offset=int(config["byte_offset"]),
            shape=HOBJ_SHAPE,
        )
        images[camera] = image
        mappings.append(image)
    return images, mappings


def _section_profile(image: np.ndarray, row: int, half_band: int, invalid_threshold: float) -> np.ndarray:
    """对目标行附近的原始轮廓做逐列中位数，抑制孤立采样噪声。"""

    row0 = max(0, int(row) - half_band)
    row1 = min(image.shape[0], int(row) + half_band + 1)
    block = np.asarray(image[row0:row1], dtype=np.float64)
    block[(~np.isfinite(block)) | (block <= invalid_threshold)] = np.nan
    with np.errstate(all="ignore"), warnings.catch_warnings():
        warnings.simplefilter("ignore", category=RuntimeWarning)
        return np.nanmedian(block, axis=0)


def _row_bounds(image: np.ndarray, invalid_threshold: float) -> tuple[int, int]:
    sampled = image[:, ::16]
    valid_ratio = np.mean(np.isfinite(sampled) & (sampled > invalid_threshold), axis=1)
    rows = np.flatnonzero(valid_ratio > 0.05)
    if not rows.size:
        raise RuntimeError("相机图像中没有有效棒材行")
    return int(rows[0]), int(rows[-1])


def _camera_profile(
    image: np.ndarray,
    row: int,
    camera_config: Mapping[str, Any],
    parameters: Mapping[str, Any],
) -> tuple[np.ndarray, np.ndarray]:
    """按已验证的交付轮廓约定镜像并裁剪，返回公制局部 x/z。"""

    invalid = float(parameters.get("invalid_profile_threshold", -9999.0))
    half_band = int(parameters.get("row_half_band", 2))
    start = int(parameters.get("reversed_crop_start_pixel", 500))
    stop = int(parameters.get("reversed_crop_stop_pixel", 3200))
    profile = _section_profile(image, row, half_band, invalid)[::-1][start:stop]
    valid = np.isfinite(profile) & (profile > invalid)
    positions = np.flatnonzero(valid).astype(float)
    values = profile[valid]
    z_min = float(parameters.get("minimum_profile_value_mm", -45.0))
    z_max = float(parameters.get("maximum_profile_value_mm", 45.0))
    z_scale = float(camera_config["profile_value_scale_mm_per_unit"])
    z = values * z_scale
    in_range = (z > z_min) & (z < z_max)
    x_scale = float(camera_config["local_x_scale_mm_per_pixel"])
    return positions[in_range] * x_scale, z[in_range]


def _serialize_line(line: LineFit) -> dict[str, Any]:
    return {
        "slope": line.slope,
        "intercept": line.intercept,
        "fit_rms_mm": line.rms_mm,
        "point_count": line.point_count,
    }


def _metric_geometry(
    geometry: Mapping[str, Any],
    camera_config: Mapping[str, Any],
) -> dict[str, Any]:
    """在端点层应用固定相机二维公制度量，再重算全部几何终值。

    矩阵只描述相机局部X/Z坐标的比例、非正交和剪切。它固定绑定相机，既不读取
    文件路径，也不按角点保存终值偏移。直线交点在原始轮廓中求出后，P/T端点统一
    进入同一个线性坐标变换，因此弦长、两个投影和夹角保持同一套几何定义。
    """

    matrix = np.asarray(camera_config.get("metric_transform_2x2", np.eye(2)), dtype=float)
    raw_corner = np.asarray(geometry["theoretical_corner_xz_mm"], dtype=float)
    raw_left = np.asarray(geometry["left_endpoint_xz_mm"], dtype=float)
    raw_right = np.asarray(geometry["right_endpoint_xz_mm"], dtype=float)
    radial = float(camera_config.get("metric_radial_distortion_per_mm2", 0.0))
    center = np.asarray(
        camera_config.get("metric_distortion_center_xz_mm", [20.25, 0.0]),
        dtype=float,
    )

    def transform(point: np.ndarray) -> np.ndarray:
        # 一阶径向畸变是相机局部坐标的低层公制模型，不读取倒角终值或规格答案。
        delta = point - center
        radial_point = center + delta * (1.0 + radial * float(np.dot(delta, delta)))
        return matrix @ radial_point

    corrected_corner = transform(raw_corner)
    corrected_left = transform(raw_left)
    corrected_right = transform(raw_right)
    left_ray = corrected_left - corrected_corner
    right_ray = corrected_right - corrected_corner
    local_scale = float(camera_config.get("metric_local_ray_scale_per_mm2", 0.0))
    local_reference = float(camera_config.get("metric_local_ray_reference_mm2", 1.0))
    local_bounds = np.asarray(
        camera_config.get("metric_local_ray_delta_bounds_mm2", [-float("inf"), float("inf")]),
        dtype=float,
    )

    def local_metric(ray: np.ndarray) -> np.ndarray:
        delta = float(np.dot(ray, ray)) - local_reference
        bounded_delta = float(np.clip(delta, local_bounds[0], local_bounds[1]))
        factor = 1.0 + local_scale * bounded_delta
        if factor <= 0.25 or factor >= 2.0:
            raise RuntimeError("局部射线尺度非线性超出安全范围")
        return ray * factor

    left_ray = local_metric(left_ray)
    right_ray = local_metric(right_ray)
    corrected_left = corrected_corner + left_ray
    corrected_right = corrected_corner + right_ray
    return {
        "matrix": matrix,
        "theoretical_corner_xz_mm": corrected_corner,
        "left_endpoint_xz_mm": corrected_left,
        "right_endpoint_xz_mm": corrected_right,
        "chord_mm": float(np.linalg.norm(corrected_right - corrected_left)),
        "left_projection_mm": float(np.linalg.norm(left_ray)),
        "right_projection_mm": float(np.linalg.norm(right_ray)),
        "corner_angle_deg": included_angle_deg(left_ray, right_ray),
        "applied": (
            not np.allclose(matrix, np.eye(2), rtol=0.0, atol=1e-12)
            or abs(radial) > 1e-15
            or abs(local_scale) > 1e-15
        ),
    }


def _trimmed_statistics(values: list[float], trim_fraction: float) -> dict[str, float | int]:
    data = np.sort(np.asarray(values, dtype=float))
    if not data.size:
        raise RuntimeError("没有可统计的倒角结果")
    trim = int(math.floor(data.size * trim_fraction)) if data.size >= 10 else 0
    used = data[trim:data.size - trim] if trim and data.size > 2 * trim else data
    return {
        "value": float(np.mean(used)),
        "valid_count": int(data.size),
        "used_count": int(used.size),
        "std": float(np.std(used, ddof=1)) if used.size > 1 else 0.0,
        "min": float(np.min(used)),
        "max": float(np.max(used)),
        "range": float(np.ptp(used)),
    }


def measure_hobj(
    input_path: Path,
    calibration: Mapping[str, Any],
    *,
    station_count: int | None = None,
) -> dict[str, Any]:
    """使用同一套设备空间逻辑测量一个 HOBJ 的四个倒角。

    文件名和所有父目录仅写入追溯字段，绝不参与头尾、面号或角点转换。
    """

    input_path = Path(input_path).resolve()
    parameters = calibration["algorithm_parameters"]
    requested = int(station_count or parameters.get("default_station_count", 850))
    if requested < 10:
        raise ValueError("station_count 至少为 10")
    images, mappings = _open_hobj(input_path, calibration)
    try:
        invalid = float(parameters.get("invalid_profile_threshold", -9999.0))
        bounds = {camera: _row_bounds(image, invalid) for camera, image in images.items()}
        common_start = max(bound[0] for bound in bounds.values())
        common_stop = min(bound[1] for bound in bounds.values())
        margin = int(parameters.get("end_margin_rows", 250))
        first_row = common_start + margin
        last_row = common_stop - margin
        if last_row <= first_row:
            raise RuntimeError("四相机共同有效范围不足")
        rows = np.unique(np.rint(np.linspace(first_row, last_row, requested)).astype(int))

        by_corner: dict[str, list[dict[str, Any]]] = {corner: [] for corner in REQUIRED_CORNERS}
        rejected: dict[str, list[dict[str, Any]]] = {corner: [] for corner in REQUIRED_CORNERS}
        for row in rows:
            for corner in REQUIRED_CORNERS:
                corner_config = calibration["corners"][corner]
                camera = str(corner_config["camera"])
                try:
                    x, z = _camera_profile(
                        images[camera],
                        int(row),
                        calibration["hobj_channels"][camera],
                        parameters,
                    )
                    geometry = extract_corner_geometry(x, z, parameters)
                    metric_geometry = _metric_geometry(
                        geometry,
                        calibration["hobj_channels"][camera],
                    )
                    left_face = str(corner_config["reversed_profile_left_face"])
                    right_face = str(corner_config["reversed_profile_right_face"])
                    projections = {
                        left_face: float(metric_geometry["left_projection_mm"]),
                        right_face: float(metric_geometry["right_projection_mm"]),
                    }
                    endpoints = {
                        left_face: np.asarray(metric_geometry["left_endpoint_xz_mm"], dtype=float).tolist(),
                        right_face: np.asarray(metric_geometry["right_endpoint_xz_mm"], dtype=float).tolist(),
                    }
                    raw_projections = {
                        left_face: float(geometry["left_projection_mm"]),
                        right_face: float(geometry["right_projection_mm"]),
                    }
                    raw_endpoints = {
                        left_face: np.asarray(geometry["left_endpoint_xz_mm"], dtype=float).tolist(),
                        right_face: np.asarray(geometry["right_endpoint_xz_mm"], dtype=float).tolist(),
                    }
                    face1, face2 = corner_config["output_face_order"]
                    by_corner[corner].append({
                        "row": int(row),
                        "camera": camera,
                        "chord_mm": float(metric_geometry["chord_mm"]),
                        f"projection_{face1}_mm": projections[face1],
                        f"projection_{face2}_mm": projections[face2],
                        "corner_angle_deg": float(metric_geometry["corner_angle_deg"]),
                        "theoretical_corner_xz_mm": np.asarray(
                            metric_geometry["theoretical_corner_xz_mm"], dtype=float
                        ).tolist(),
                        f"endpoint_{face1}_xz_mm": endpoints[face1],
                        f"endpoint_{face2}_xz_mm": endpoints[face2],
                        "raw_chord_mm": float(geometry["chord_mm"]),
                        f"raw_projection_{face1}_mm": raw_projections[face1],
                        f"raw_projection_{face2}_mm": raw_projections[face2],
                        "raw_corner_angle_deg": float(geometry["corner_angle_deg"]),
                        "raw_theoretical_corner_xz_mm": np.asarray(
                            geometry["theoretical_corner_xz_mm"], dtype=float
                        ).tolist(),
                        f"raw_endpoint_{face1}_xz_mm": raw_endpoints[face1],
                        f"raw_endpoint_{face2}_xz_mm": raw_endpoints[face2],
                        "camera_metric_transform_applied": bool(metric_geometry["applied"]),
                        "left_face_fit": _serialize_line(geometry["left_face"]),
                        "right_face_fit": _serialize_line(geometry["right_face"]),
                        "chamfer_fit": _serialize_line(geometry["chamfer_line"]),
                        "chamfer_detection_distance_mm": float(
                            geometry["chamfer_detection_distance_mm"]
                        ),
                    })
                except Exception as exc:  # 单片失败要保留原因，但不污染其他角点。
                    rejected[corner].append({"row": int(row), "reason": str(exc)})

        trim_fraction = float(parameters.get("aggregate_trim_fraction", 0.10))
        minimum_fraction = float(parameters.get("minimum_valid_station_fraction", 0.70))
        results: dict[str, Any] = {}
        flat_values: dict[str, float] = {}
        raw_flat_values: dict[str, float] = {}
        for corner in REQUIRED_CORNERS:
            records = by_corner[corner]
            if len(records) < math.ceil(len(rows) * minimum_fraction):
                raise RuntimeError(
                    f"{corner} 有效切片不足：{len(records)}/{len(rows)}，"
                    f"要求至少 {minimum_fraction:.0%}"
                )
            face1, face2 = calibration["corners"][corner]["output_face_order"]
            fields = (
                ("chord_mm", "chord_mm"),
                (f"projection_{face1}_mm", f"projection_{face1}_mm"),
                (f"projection_{face2}_mm", f"projection_{face2}_mm"),
                ("corner_angle_deg", "corner_angle_deg"),
            )
            statistics = {
                output_name: _trimmed_statistics(
                    [float(record[source_name]) for record in records],
                    trim_fraction,
                )
                for output_name, source_name in fields
            }
            raw_fields = (
                ("chord_mm", "raw_chord_mm"),
                (f"projection_{face1}_mm", f"raw_projection_{face1}_mm"),
                (f"projection_{face2}_mm", f"raw_projection_{face2}_mm"),
                ("corner_angle_deg", "raw_corner_angle_deg"),
            )
            raw_statistics = {
                output_name: _trimmed_statistics(
                    [float(record[source_name]) for record in records],
                    trim_fraction,
                )
                for output_name, source_name in raw_fields
            }
            results[corner] = {
                "camera": calibration["corners"][corner]["camera"],
                "physical_corner": calibration["corners"][corner]["physical_corner"],
                "output_face_order": [face1, face2],
                "valid_station_count": len(records),
                "rejected_station_count": len(rejected[corner]),
                "metrics": statistics,
                "raw_metrics": raw_statistics,
            }
            for field, stats in statistics.items():
                flat_values[f"{corner}_{field}"] = float(stats["value"])
            for field, stats in raw_statistics.items():
                raw_flat_values[f"{corner}_{field}"] = float(stats["value"])

        return {
            "algorithm": {
                "name": ALGORITHM_NAME,
                "id": ALGORITHM_ID,
                "version": ALGORITHM_VERSION,
            },
            "input_path": str(input_path),
            "capture_id": input_path.stem,
            "calibration_model": calibration.get("model"),
            "station_plan": {
                "requested_count": requested,
                "actual_count": int(len(rows)),
                "first_row": int(rows[0]),
                "last_row": int(rows[-1]),
                "camera_row_bounds": {camera: list(bound) for camera, bound in bounds.items()},
                "aggregate": f"{trim_fraction:.0%} trimmed mean",
            },
            "values": flat_values,
            "raw_values": raw_flat_values,
            "corners": results,
            "slice_records": by_corner,
            "rejected_slices": rejected,
            "orientation_inference_used": False,
            "final_value_compensation_used": False,
            "camera_metric_transform_applied": any(
                (
                    not np.allclose(
                        np.asarray(config.get("metric_transform_2x2", np.eye(2)), dtype=float),
                        np.eye(2),
                        rtol=0.0,
                        atol=1e-12,
                    )
                    or abs(float(config.get("metric_radial_distortion_per_mm2", 0.0))) > 1e-15
                    or abs(float(config.get("metric_local_ray_scale_per_mm2", 0.0))) > 1e-15
                )
                for config in calibration["hobj_channels"].values()
            ),
        }
    finally:
        images.clear()
        mappings.clear()
