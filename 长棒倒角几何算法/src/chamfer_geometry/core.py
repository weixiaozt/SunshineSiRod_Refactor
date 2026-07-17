"""单条相机轮廓上的倒角几何计算。

本模块只做二维欧氏几何：拟合两条实体主面和一条倒角直线，然后由三条线的
交点计算理论尖角 P、两个倒角端点 T1/T2、弦长、双侧投影及主面内部夹角。
这里没有规格值补偿，也没有 45 度倒角假设。
"""

from __future__ import annotations

from dataclasses import dataclass
import math
from typing import Any, Mapping

import numpy as np


@dataclass(frozen=True)
class LineFit:
    """直线 z = slope * x + intercept 的鲁棒拟合结果。"""

    slope: float
    intercept: float
    rms_mm: float
    point_count: int


def robust_line_fit(
    x: np.ndarray,
    z: np.ndarray,
    *,
    min_points: int,
    iterations: int = 6,
    sigma_limit: float = 4.0,
    residual_floor_mm: float = 0.003,
) -> LineFit:
    """用 MAD 迭代剔除离群点后拟合直线。"""

    x = np.asarray(x, dtype=float)
    z = np.asarray(z, dtype=float)
    keep = np.isfinite(x) & np.isfinite(z)
    slope = intercept = float("nan")
    for _ in range(iterations):
        if int(np.sum(keep)) < int(min_points):
            raise RuntimeError("直线拟合的有效点不足")
        slope, intercept = np.polyfit(x[keep], z[keep], 1)
        residual = z - (slope * x + intercept)
        center = float(np.median(residual[keep]))
        mad = float(np.median(np.abs(residual[keep] - center)))
        sigma = max(1.4826 * mad, residual_floor_mm)
        next_keep = keep & (np.abs(residual - center) <= sigma_limit * sigma)
        if np.array_equal(next_keep, keep):
            break
        keep = next_keep
    if int(np.sum(keep)) < int(min_points):
        raise RuntimeError("离群点剔除后直线拟合点不足")
    residual = z[keep] - (slope * x[keep] + intercept)
    rms = math.sqrt(float(np.mean(residual**2)))
    return LineFit(float(slope), float(intercept), rms, int(np.sum(keep)))


def line_intersection(first: LineFit, second: LineFit) -> np.ndarray:
    """返回两条非平行直线的交点 [x, z]。"""

    denominator = first.slope - second.slope
    if abs(denominator) < 1e-9:
        raise RuntimeError("拟合直线近似平行，无法求交点")
    x = (second.intercept - first.intercept) / denominator
    return np.array([x, first.slope * x + first.intercept], dtype=float)


def point_line_distance(x: np.ndarray, z: np.ndarray, line: LineFit) -> np.ndarray:
    """计算轮廓点到拟合直线的垂直距离。"""

    return np.abs(z - (line.slope * x + line.intercept)) / math.sqrt(1.0 + line.slope**2)


def included_angle_deg(first_ray: np.ndarray, second_ray: np.ndarray) -> float:
    """返回两条有方向射线的 0..180 度夹角，不折叠成锐角。"""

    first_ray = np.asarray(first_ray, dtype=float)
    second_ray = np.asarray(second_ray, dtype=float)
    norm = float(np.linalg.norm(first_ray) * np.linalg.norm(second_ray))
    if norm <= 1e-12:
        raise RuntimeError("理论尖角到倒角端点的射线长度为零")
    cosine = float(np.dot(first_ray, second_ray) / norm)
    return math.degrees(math.acos(float(np.clip(cosine, -1.0, 1.0))))


def _component_near_apex(mask: np.ndarray, apex: int, max_apex_gap: int) -> np.ndarray:
    """取得自适应倒角掩码中包含或最靠近轮廓峰值的连续段。"""

    indices = np.flatnonzero(mask)
    if not indices.size:
        return np.empty(0, dtype=int)
    components = np.split(indices, np.flatnonzero(np.diff(indices) > 1) + 1)
    containing = [part for part in components if int(part[0]) <= apex <= int(part[-1])]
    if containing:
        return containing[0]
    nearest = min(components, key=lambda part: min(abs(int(part[0]) - apex), abs(int(part[-1]) - apex)))
    gap = min(abs(int(nearest[0]) - apex), abs(int(nearest[-1]) - apex))
    return nearest if gap <= max_apex_gap else np.empty(0, dtype=int)


def extract_corner_geometry(
    x_mm: np.ndarray,
    z_mm: np.ndarray,
    parameters: Mapping[str, Any],
) -> dict[str, Any]:
    """从一条完整角部轮廓计算 P、两个端点和四个最终几何量。

    输入轮廓必须按相机局部 x 单调递增。算法先在峰值两侧的宽窗口内拟合两条
    主面，再用“同时离开两条主面”的点组成自适应倒角段。这个判据只寻找几何
    分段边界，不把结果拉向任何标称弦长或角度。
    """

    x = np.asarray(x_mm, dtype=float)
    z = np.asarray(z_mm, dtype=float)
    finite = np.isfinite(x) & np.isfinite(z)
    x, z = x[finite], z[finite]
    minimum_profile_points = int(parameters.get("minimum_profile_points", 1200))
    if x.size < minimum_profile_points:
        raise RuntimeError(f"完整轮廓点不足：{x.size} < {minimum_profile_points}")
    if np.any(np.diff(x) < 0.0):
        raise RuntimeError("轮廓 x 坐标必须单调递增")

    smooth_points = int(parameters.get("apex_smoothing_points", 21))
    if smooth_points < 3 or smooth_points % 2 == 0:
        raise ValueError("apex_smoothing_points 必须是大于等于3的奇数")
    smooth = np.convolve(z, np.ones(smooth_points) / smooth_points, mode="same")
    search_outer = float(parameters.get("apex_search_outer_fraction", 0.03))
    search_lo = max(1, int(x.size * search_outer))
    search_hi = min(x.size - 1, int(x.size * (1.0 - search_outer)))
    if search_hi <= search_lo:
        raise RuntimeError("轮廓峰值搜索范围无效")
    apex = search_lo + int(np.argmax(smooth[search_lo:search_hi]))

    outer = float(parameters.get("main_face_outer_fraction", 0.08))
    inner = float(parameters.get("main_face_inner_fraction", 0.08))
    minimum_face_points = int(parameters.get("minimum_main_face_points", 80))
    left_start = int(x.size * outer)
    left_stop = max(left_start + minimum_face_points, int(apex - x.size * inner))
    right_start = min(x.size - minimum_face_points, int(apex + x.size * inner))
    right_stop = int(x.size * (1.0 - outer))
    if left_stop > apex or right_start < apex or left_stop > right_start:
        raise RuntimeError("峰值两侧没有足够的主面拟合区")

    left_face = robust_line_fit(
        x[left_start:left_stop],
        z[left_start:left_stop],
        min_points=minimum_face_points,
    )
    right_face = robust_line_fit(
        x[right_start:right_stop],
        z[right_start:right_stop],
        min_points=minimum_face_points,
    )
    left_distance = point_line_distance(x, z, left_face)
    right_distance = point_line_distance(x, z, right_face)

    base_distance = float(parameters.get("chamfer_shoulder_distance_mm", 0.02))
    rms_multiplier = float(parameters.get("chamfer_shoulder_face_rms_multiplier", 4.0))
    start_threshold = max(base_distance, rms_multiplier * max(left_face.rms_mm, right_face.rms_mm))
    minimum_chamfer_points = int(parameters.get("minimum_chamfer_points", 15))
    maximum_chamfer_points = int(parameters.get("maximum_chamfer_points", 180))
    max_apex_gap = int(parameters.get("maximum_apex_gap_points", 12))

    # 若较严格的距离阈值使倒角段过短，逐级放宽，但永远不使用标称弦长定位端点。
    thresholds = []
    for candidate in (start_threshold, base_distance, 0.015, 0.010, 0.006):
        candidate = float(candidate)
        if candidate > 0.0 and all(abs(candidate - old) > 1e-12 for old in thresholds):
            thresholds.append(candidate)
    chamfer_indices = np.empty(0, dtype=int)
    used_threshold = float("nan")
    for threshold in thresholds:
        candidate = _component_near_apex(
            (left_distance > threshold) & (right_distance > threshold),
            apex,
            max_apex_gap,
        )
        if minimum_chamfer_points <= candidate.size <= maximum_chamfer_points:
            chamfer_indices = candidate
            used_threshold = threshold
            break
    if not chamfer_indices.size:
        raise RuntimeError("未找到可靠的自适应倒角轮廓段")

    chamfer_line = robust_line_fit(
        x[chamfer_indices],
        z[chamfer_indices],
        min_points=max(10, minimum_chamfer_points // 2),
        residual_floor_mm=0.001,
    )
    left_endpoint = line_intersection(left_face, chamfer_line)
    right_endpoint = line_intersection(chamfer_line, right_face)
    theoretical_corner = line_intersection(left_face, right_face)
    if left_endpoint[0] >= right_endpoint[0]:
        raise RuntimeError("倒角端点顺序异常")

    left_ray = left_endpoint - theoretical_corner
    right_ray = right_endpoint - theoretical_corner
    chord_mm = float(np.linalg.norm(right_endpoint - left_endpoint))
    left_projection_mm = float(np.linalg.norm(left_ray))
    right_projection_mm = float(np.linalg.norm(right_ray))
    corner_angle_deg = included_angle_deg(left_ray, right_ray)
    values = (chord_mm, left_projection_mm, right_projection_mm, corner_angle_deg)
    if not all(math.isfinite(value) for value in values):
        raise RuntimeError("倒角几何结果含非有限值")
    if min(chord_mm, left_projection_mm, right_projection_mm) <= 0.0:
        raise RuntimeError("倒角几何长度必须为正数")

    return {
        "chord_mm": chord_mm,
        "left_projection_mm": left_projection_mm,
        "right_projection_mm": right_projection_mm,
        "corner_angle_deg": corner_angle_deg,
        "theoretical_corner_xz_mm": theoretical_corner,
        "left_endpoint_xz_mm": left_endpoint,
        "right_endpoint_xz_mm": right_endpoint,
        "left_face": left_face,
        "right_face": right_face,
        "chamfer_line": chamfer_line,
        "apex_index": int(apex),
        "chamfer_first_index": int(chamfer_indices[0]),
        "chamfer_last_index": int(chamfer_indices[-1]),
        "chamfer_detection_distance_mm": float(used_threshold),
    }
