from __future__ import annotations

import csv
import hashlib
import json
import math
import warnings
from collections import defaultdict
from dataclasses import dataclass
from pathlib import Path
from typing import Iterable, Mapping, Sequence

import numpy as np
import tifffile


ALGORITHM_VERSION = "2026-07-16-fixed-direction-v1"
X_SCALE_MM_PER_PX = 0.015
Y_SCALE_MM_PER_ROW = 0.05
INVALID_THRESHOLD = -9999.0
SLICE_END_MARGIN_ROWS = 250
CORNER_NEIGHBORHOOD_RADIUS_MM = 2.5
TRIM_PER_TAIL = 0.10
MIN_VALID_SLICES = 10

METRICS = ("A", "B", "C", "D", "D1", "D2")
CAMERA_ORDER = ("Top", "Right", "Left", "Down")
CAMERA_NUMBER = {"Left": 1, "Right": 2, "Top": 3, "Down": 4}
CAMERA_FILENAMES = {
    "Left": ("1-Left.tif", "1-Left.tiff"),
    "Right": ("2-Right.tif", "2-Right.tiff"),
    "Top": ("3-Top.tif", "3-Top.tiff"),
    "Down": ("4-Down.tif", "4-Down.tiff"),
}

CAMERA_GEOMETRY = {
    "Left": {
        "left_dir": np.array([0.0, -1.0]),
        "right_dir": np.array([-1.0, 0.0]),
    },
    "Right": {
        "left_dir": np.array([-1.0, 0.0]),
        "right_dir": np.array([0.0, 1.0]),
    },
    "Top": {
        "left_dir": np.array([0.0, 1.0]),
        "right_dir": np.array([1.0, 0.0]),
    },
    "Down": {
        "left_dir": np.array([1.0, 0.0]),
        "right_dir": np.array([0.0, -1.0]),
    },
}

CORNER_CAMERA = {"AB": "Right", "BC": "Left", "CD": "Down", "AD": "Top"}
EDGE_PAIRS = {
    "A": ("Top", "Right"),
    "B": ("Right", "Left"),
    "C": ("Left", "Down"),
    "D": ("Down", "Top"),
}
DIAGONALS = {"D1": ("AB", "CD"), "D2": ("AD", "BC")}
REVERSE_PHYSICAL_MAPPING = {
    "A": "A",
    "B": "D",
    "C": "C",
    "D": "B",
    "D1": "D2",
    "D2": "D1",
}


@dataclass
class CalibrationBundle:
    base_path: Path
    exchange_path: Path | None
    base_payload: dict
    exchange_payload: dict
    rows: np.ndarray
    windows: dict[str, dict[str, np.ndarray]]
    matrices: dict[str, np.ndarray]
    origins: list[dict[str, np.ndarray]]
    corrections_mm: dict[str, float]


def json_safe(value):
    if isinstance(value, dict):
        return {str(key): json_safe(item) for key, item in value.items()}
    if isinstance(value, (list, tuple)):
        return [json_safe(item) for item in value]
    if isinstance(value, np.ndarray):
        return value.tolist()
    if isinstance(value, (np.integer, int)):
        return int(value)
    if isinstance(value, (np.floating, float)):
        number = float(value)
        return number if math.isfinite(number) else None
    if isinstance(value, Path):
        return str(value)
    return value


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for block in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def write_csv(path: Path, rows: Iterable[Mapping]):
    rows = list(rows)
    path.parent.mkdir(parents=True, exist_ok=True)
    if not rows:
        path.write_text("", encoding="utf-8-sig")
        return
    headers: list[str] = []
    for row in rows:
        for key in row:
            if key not in headers:
                headers.append(str(key))
    with path.open("w", newline="", encoding="utf-8-sig") as handle:
        writer = csv.DictWriter(handle, fieldnames=headers)
        writer.writeheader()
        writer.writerows([{key: json_safe(row.get(key)) for key in headers} for row in rows])


def normalize_direction(value: str) -> str:
    text = str(value).strip().lower()
    if text in {"positive", "pos", "p", "正", "正向"}:
        return "positive"
    if text in {"reverse", "rev", "r", "反", "反向", "掉头"}:
        return "reverse"
    raise ValueError(f"无法识别扫描方向: {value!r}; 请使用 positive/正向 或 reverse/反向")


def load_base_runtime(base_path: Path) -> tuple[dict, np.ndarray, dict, dict, list]:
    payload = json.loads(base_path.read_text(encoding="utf-8"))
    runtime = payload["runtime_calibration"]
    rows = np.asarray(runtime["rows"], dtype=int)
    windows = {
        camera: {
            side: np.asarray(columns, dtype=int)
            for side, columns in sides.items()
        }
        for camera, sides in runtime["windows"].items()
    }
    matrices = {
        camera: np.asarray(matrix, dtype=float)
        for camera, matrix in runtime["matrices"].items()
    }
    origins = [
        {
            camera: np.asarray(origin, dtype=float)
            for camera, origin in row.items()
        }
        for row in runtime["origins"]
    ]
    if not (len(rows) == len(origins)):
        raise ValueError("基础标定 rows 与 origins 数量不一致")
    for camera in CAMERA_ORDER:
        if camera not in windows or camera not in matrices:
            raise ValueError(f"基础标定缺少相机 {camera}")
    return payload, rows, windows, matrices, origins


def load_calibration(calibration_dir: Path, apply_exchange: bool = True) -> CalibrationBundle:
    calibration_dir = Path(calibration_dir)
    base_path = calibration_dir / "current_calibration.json"
    exchange_path = calibration_dir / "global_exchange_calibration.json"
    base_payload, rows, windows, matrices, origins = load_base_runtime(base_path)
    if exchange_path.exists():
        exchange = json.loads(exchange_path.read_text(encoding="utf-8"))
    else:
        exchange = {"k_mm": 0.0, "corrections_mm": {metric: 0.0 for metric in METRICS}}
        exchange_path = None
    corrections = {
        metric: float(exchange.get("corrections_mm", {}).get(metric, 0.0))
        for metric in METRICS
    }
    if not apply_exchange:
        corrections = {metric: 0.0 for metric in METRICS}
    return CalibrationBundle(
        base_path=base_path,
        exchange_path=exchange_path,
        base_payload=base_payload,
        exchange_payload=exchange,
        rows=rows,
        windows=windows,
        matrices=matrices,
        origins=origins,
        corrections_mm=corrections,
    )


def find_camera_files(scan_dir: Path) -> dict[str, Path]:
    scan_dir = Path(scan_dir)
    if not scan_dir.is_dir():
        raise FileNotFoundError(f"扫描目录不存在: {scan_dir}")
    files: dict[str, Path] = {}
    for camera, names in CAMERA_FILENAMES.items():
        for name in names:
            candidate = scan_dir / name
            if candidate.exists():
                files[camera] = candidate
                break
    if len(files) < 4:
        for camera, number in CAMERA_NUMBER.items():
            if camera in files:
                continue
            matches = sorted(scan_dir.glob(f"*_{number}.tif")) + sorted(scan_dir.glob(f"*_{number}.tiff"))
            if matches:
                files[camera] = matches[0]
    missing = [camera for camera in CAMERA_ORDER if camera not in files]
    if missing:
        raise FileNotFoundError(
            f"扫描目录 {scan_dir} 缺少相机文件 {missing}; "
            "标准文件名为1-Left.tif、2-Right.tif、3-Top.tif、4-Down.tif"
        )
    return files


def section_profile(image, row: int, half_band: int = 2) -> np.ndarray:
    r0 = max(0, int(row) - half_band)
    r1 = min(image.shape[0], int(row) + half_band + 1)
    block = np.asarray(image[r0:r1], dtype=np.float64)
    block[(~np.isfinite(block)) | (block <= INVALID_THRESHOLD)] = np.nan
    with np.errstate(all="ignore"), warnings.catch_warnings():
        warnings.simplefilter("ignore", category=RuntimeWarning)
        return np.nanmedian(block, axis=0)


def row_bounds(image) -> tuple[int, int]:
    sampled = image[:, ::4]
    count = (np.isfinite(sampled) & (sampled >= -50.0) & (sampled <= 50.0)).sum(axis=1)
    rows = np.flatnonzero(count > 125)
    if not rows.size:
        raise RuntimeError("图像中没有检测到有效棒体行")
    return int(rows[0]), int(rows[-1])


def fixed_face_columns(image, row: int) -> tuple[np.ndarray, np.ndarray]:
    profile = section_profile(image, row)
    valid_columns = np.flatnonzero(np.isfinite(profile))
    z = profile[valid_columns]
    smooth = np.convolve(z, np.ones(21) / 21.0, mode="same")
    lo = int(z.size * 0.08)
    hi = int(z.size * 0.92)
    apex_index = lo + int(np.argmax(smooth[lo:hi]))
    margin = max(75, int(z.size * 0.055))
    left = valid_columns[int(z.size * 0.08):apex_index - margin]
    right = valid_columns[apex_index + margin:int(z.size * 0.92)]
    if left.size < 200 or right.size < 200:
        raise RuntimeError("无法定义两侧固定直线拟合窗口")
    return left, right


def band_profiles(image, rows: np.ndarray, columns: np.ndarray) -> np.ndarray:
    total = np.zeros((rows.size, columns.size), dtype=np.float64)
    count = np.zeros((rows.size, columns.size), dtype=np.int16)
    for delta in range(-2, 3):
        block = np.asarray(image[np.ix_(rows + delta, columns)], dtype=np.float64)
        valid = np.isfinite(block) & (block > INVALID_THRESHOLD)
        total += np.where(valid, block, 0.0)
        count += valid
    result = np.full_like(total, np.nan)
    np.divide(total, count, out=result, where=count > 0)
    return result


def vector_line_fit(x: np.ndarray, z: np.ndarray):
    x = np.asarray(x, dtype=np.float64)
    valid = np.isfinite(z)
    n = valid.sum(axis=1).astype(np.float64)
    xv = valid * x[None, :]
    zv = np.where(valid, z, 0.0)
    sx = xv.sum(axis=1)
    sy = zv.sum(axis=1)
    sxx = (xv * x[None, :]).sum(axis=1)
    sxy = (xv * zv).sum(axis=1)
    denominator = n * sxx - sx * sx
    slope = (n * sxy - sx * sy) / denominator
    intercept = (sy - slope * sx) / n
    return slope, intercept


def extract_corner_series(image, rows: np.ndarray, left_columns: np.ndarray, right_columns: np.ndarray) -> dict:
    left_z = band_profiles(image, rows, left_columns)
    right_z = band_profiles(image, rows, right_columns)
    left_x = left_columns.astype(float) * X_SCALE_MM_PER_PX
    right_x = right_columns.astype(float) * X_SCALE_MM_PER_PX
    ml, bl = vector_line_fit(left_x, left_z)
    mr, br = vector_line_fit(right_x, right_z)
    u = (br - bl) / (ml - mr)
    z = ml * u + bl
    return {
        "u_mm": u,
        "z_mm": z,
        "left_slope": ml,
        "right_slope": mr,
    }


def orthogonal_map(camera: str, baseline: Mapping[str, np.ndarray]) -> np.ndarray:
    ml = float(np.median(baseline["left_slope"]))
    mr = float(np.median(baseline["right_slope"]))
    left = np.array([-1.0, -ml])
    right = np.array([1.0, mr])
    left /= np.linalg.norm(left)
    right /= np.linalg.norm(right)
    local = np.column_stack([left, right])
    target = np.column_stack([
        CAMERA_GEOMETRY[camera]["left_dir"],
        CAMERA_GEOMETRY[camera]["right_dir"],
    ])
    u, _, vt = np.linalg.svd(target @ local.T)
    return u @ vt


def full_corner_profile_local(image, row: int) -> np.ndarray:
    profile = section_profile(image, int(row))
    valid = np.isfinite(profile) & (profile > INVALID_THRESHOLD) & (profile < 9999)
    columns = np.flatnonzero(valid).astype(float)
    z = profile[valid]
    if z.size < 1200:
        raise RuntimeError(f"第{row}行有效轮廓点少于1200")
    padded = np.pad(z, (2, 2), mode="edge")
    z_smooth = np.median(np.lib.stride_tricks.sliding_window_view(padded, 5), axis=1)
    return np.column_stack([columns * X_SCALE_MM_PER_PX, z_smooth])


def caliper_support(points_positive: np.ndarray, points_negative: np.ndarray, direction: np.ndarray) -> dict:
    direction = np.asarray(direction, dtype=float)
    norm = float(np.linalg.norm(direction))
    if norm == 0.0:
        raise RuntimeError("对角线名义方向为零向量")
    direction /= norm
    positive = points_positive[int(np.argmax(points_positive @ direction))]
    negative = points_negative[int(np.argmin(points_negative @ direction))]
    separation = float((positive - negative) @ direction)
    return {
        "caliper_mm": separation,
        "positive_contact": positive,
        "negative_contact": negative,
        "direction": direction,
    }


def trimmed_mean(values: Sequence[float], trim_per_tail: float = TRIM_PER_TAIL) -> dict:
    finite = sorted(float(value) for value in values if value is not None and math.isfinite(float(value)))
    if not finite:
        raise ValueError("截尾均值没有有效数据")
    trim_count = int(math.floor(len(finite) * trim_per_tail))
    kept = finite[trim_count:len(finite) - trim_count] if trim_count else finite
    if not kept:
        raise ValueError("截尾后没有保留数据")
    return {
        "mean": float(np.mean(kept)),
        "count": len(finite),
        "trim_each_tail": trim_count,
        "kept_count": len(kept),
    }


def quadrilateral_from_sides(sides: Mapping[str, float]) -> tuple[dict[str, np.ndarray], list[float], float]:
    """仅用A/B/C/D构造四边形；选择四角偏离90°总量最小的解。"""
    a, b, c, d = [float(sides[key]) for key in "ABCD"]
    lower = max(abs(a - b), abs(c - d)) + 1e-5
    upper = min(a + b, c + d) - 1e-5
    best = None
    for diagonal in np.linspace(lower, upper, 40001):
        x_left = (a * a + diagonal * diagonal - b * b) / (2.0 * a)
        z2 = diagonal * diagonal - x_left * x_left
        if z2 <= 0:
            continue
        left = np.array([x_left, math.sqrt(z2)])
        unit = left / diagonal
        along = (d * d - c * c + diagonal * diagonal) / (2.0 * diagonal)
        h2 = d * d - along * along
        if h2 <= 0:
            continue
        perpendicular = np.array([-unit[1], unit[0]])
        candidates = [
            along * unit + math.sqrt(h2) * perpendicular,
            along * unit - math.sqrt(h2) * perpendicular,
        ]
        down = min(candidates, key=lambda point: abs(point[0]) + abs(point[1] - d))
        points = [np.array([0.0, 0.0]), np.array([a, 0.0]), left, down]
        score = 0.0
        angles: list[float] = []
        for index in range(4):
            previous = points[(index - 1) % 4] - points[index]
            following = points[(index + 1) % 4] - points[index]
            cosine = float(np.dot(previous, following) / (np.linalg.norm(previous) * np.linalg.norm(following)))
            score += cosine * cosine
            angles.append(math.degrees(math.acos(np.clip(cosine, -1.0, 1.0))))
        if best is None or score < best[0]:
            best = (score, points, angles, float(diagonal))
    if best is None:
        raise RuntimeError("A/B/C/D不能构成凸四边形")
    _, points, angles, diagonal = best
    return {
        "Top": points[0],
        "Right": points[1],
        "Left": points[2],
        "Down": points[3],
    }, angles, diagonal


def build_base_calibration_payload(
    scan_dir: Path,
    sides: Mapping[str, float],
    specimen: str,
    scan_id: str,
    slice_count: int = 75,
) -> dict:
    files = find_camera_files(scan_dir)
    images = {camera: tifffile.memmap(path) for camera, path in files.items()}
    bounds = {camera: row_bounds(image) for camera, image in images.items()}
    common_start = max(item[0] for item in bounds.values())
    common_end = min(item[1] for item in bounds.values())
    rows = np.rint(np.linspace(
        common_start + SLICE_END_MARGIN_ROWS,
        common_end - SLICE_END_MARGIN_ROWS,
        int(slice_count),
    )).astype(int)
    nominal, _angles, nominal_diagonal = quadrilateral_from_sides(sides)
    windows: dict[str, dict[str, np.ndarray]] = {}
    baseline: dict[str, dict] = {}
    matrices: dict[str, np.ndarray] = {}
    for camera in CAMERA_ORDER:
        left, right = fixed_face_columns(images[camera], int(rows[len(rows) // 2]))
        windows[camera] = {"left": left, "right": right}
        baseline[camera] = extract_corner_series(images[camera], rows, left, right)
        matrices[camera] = orthogonal_map(camera, baseline[camera])
    origins: list[dict[str, np.ndarray]] = []
    for index in range(len(rows)):
        row_origins = {}
        for camera in CAMERA_ORDER:
            local = np.array([baseline[camera]["u_mm"][index], baseline[camera]["z_mm"][index]])
            row_origins[camera] = nominal[camera] - matrices[camera] @ local
        origins.append(row_origins)
    return json_safe({
        "package_name": "square_bar_edge_diagonal_base_calibration",
        "status": "runtime calibration for A/B/C/D and fixed-direction D1/D2",
        "algorithm_version": ALGORITHM_VERSION,
        "scales": {
            "x_scale_mm_per_px": X_SCALE_MM_PER_PX,
            "y_scale_mm_per_row": Y_SCALE_MM_PER_ROW,
            "invalid_threshold": INVALID_THRESHOLD,
        },
        "abcd_length_calibration": {
            "source_scan_dir": str(Path(scan_dir).resolve()),
            "source_images": {camera: str(path.resolve()) for camera, path in files.items()},
            "used_fields_mm": {key: float(sides[key]) for key in "ABCD"},
            "calibration_specimen": specimen,
            "calibration_scan": scan_id,
            "slice_count": len(rows),
            "slice_end_margin_rows": SLICE_END_MARGIN_ROWS,
            "nominal_corner_positions_xz_mm": nominal,
        "nominal_diagonal_mm": nominal_diagonal,
        },
        "runtime_calibration": {
            "rows": rows,
            "windows": windows,
            "matrices": matrices,
            "origins": origins,
            "baseline_bounds_rows_by_camera": bounds,
        },
        "measurement_mappings": {
            "edge_pairs": EDGE_PAIRS,
            "corner_camera": CORNER_CAMERA,
            "diagonals": DIAGONALS,
        },
    })


def measure_scan(scan: Mapping[str, str], calibration: CalibrationBundle) -> tuple[list[dict], dict]:
    specimen = str(scan["specimen"])
    direction = normalize_direction(scan["direction"])
    scan_id = str(scan["scan"])
    scan_dir = Path(scan["scan_dir"])
    files = find_camera_files(scan_dir)
    images = {camera: tifffile.memmap(path) for camera, path in files.items()}
    for camera, image in images.items():
        if image.ndim != 2:
            raise ValueError(f"{camera}图像必须是二维数组，当前shape={image.shape}")
    bounds = {camera: row_bounds(image) for camera, image in images.items()}
    common_start = max(value[0] for value in bounds.values())
    common_end = min(value[1] for value in bounds.values())
    valid_indices = np.flatnonzero(
        (calibration.rows >= common_start + SLICE_END_MARGIN_ROWS)
        & (calibration.rows <= common_end - SLICE_END_MARGIN_ROWS)
    )
    if valid_indices.size < MIN_VALID_SLICES:
        raise RuntimeError(
            f"{specimen}/{scan_id}与标定绝对行重合的有效切片少于{MIN_VALID_SLICES}"
        )
    rows = calibration.rows[valid_indices]
    origins = [calibration.origins[int(index)] for index in valid_indices]
    sections = {
        camera: extract_corner_series(
            images[camera],
            rows,
            calibration.windows[camera]["left"],
            calibration.windows[camera]["right"],
        )
        for camera in CAMERA_ORDER
    }
    slice_rows: list[dict] = []
    for local_index, row in enumerate(rows):
        corners: dict[str, np.ndarray] = {}
        neighborhoods: dict[str, np.ndarray] = {}
        for corner, camera in CORNER_CAMERA.items():
            local_corner = np.array([
                sections[camera]["u_mm"][local_index],
                sections[camera]["z_mm"][local_index],
            ])
            corner_global = origins[local_index][camera] + calibration.matrices[camera] @ local_corner
            corners[corner] = corner_global
            local_profile = full_corner_profile_local(images[camera], int(row))
            global_profile = (
                calibration.matrices[camera] @ local_profile.T
            ).T + origins[local_index][camera]
            neighborhood = global_profile[
                np.linalg.norm(global_profile - corner_global, axis=1)
                <= CORNER_NEIGHBORHOOD_RADIUS_MM
            ]
            if neighborhood.shape[0] < 20:
                raise RuntimeError(
                    f"{specimen}/{scan_id} 第{int(row)}行 {corner}角部2.5mm邻域点少于20"
                )
            neighborhoods[corner] = neighborhood
        camera_corners = {camera: corners[corner] for corner, camera in CORNER_CAMERA.items()}
        raw = {
            edge: float(np.linalg.norm(camera_corners[camera1] - camera_corners[camera2]))
            for edge, (camera1, camera2) in EDGE_PAIRS.items()
        }
        for diagonal, (positive_corner, negative_corner) in DIAGONALS.items():
            nominal_direction = corners[positive_corner] - corners[negative_corner]
            result = caliper_support(
                neighborhoods[positive_corner],
                neighborhoods[negative_corner],
                nominal_direction,
            )
            raw[diagonal] = float(result["caliper_mm"])
        corrected = {
            metric: raw[metric] + calibration.corrections_mm[metric]
            for metric in METRICS
        }
        item = {
            "specimen": specimen,
            "direction": direction,
            "direction_zh": "正向" if direction == "positive" else "反向",
            "scan": scan_id,
            "slice_index": int(valid_indices[local_index]) + 1,
            "absolute_row": int(row),
            "position_mm": float((row - rows[0]) * Y_SCALE_MM_PER_ROW),
            **{f"raw_{metric}": raw[metric] for metric in METRICS},
            **corrected,
            "diagonal_angle_search_deg": 0.0,
            "source_dir": str(scan_dir.resolve()),
        }
        slice_rows.append(item)
    summary = {
        "specimen": specimen,
        "direction": direction,
        "direction_zh": "正向" if direction == "positive" else "反向",
        "scan": scan_id,
        "source_dir": str(scan_dir.resolve()),
        "valid_slice_count": len(slice_rows),
        "trim_each_tail": int(math.floor(len(slice_rows) * TRIM_PER_TAIL)),
    }
    for metric in METRICS:
        raw_stat = trimmed_mean([row[f"raw_{metric}"] for row in slice_rows])
        corrected_stat = trimmed_mean([row[metric] for row in slice_rows])
        summary[f"raw_{metric}"] = raw_stat["mean"]
        summary[metric] = corrected_stat["mean"]
        summary["kept_slice_count"] = corrected_stat["kept_count"]
    mapping = {metric: metric for metric in METRICS} if direction == "positive" else REVERSE_PHYSICAL_MAPPING
    for physical_metric, camera_metric in mapping.items():
        summary[f"aligned_{physical_metric}"] = summary[camera_metric]
    for camera in CAMERA_ORDER:
        summary[f"image_{camera}"] = str(files[camera].resolve())
    return slice_rows, summary


def _mean(rows: Sequence[Mapping], key: str) -> float:
    return float(np.mean([float(row[key]) for row in rows]))


def _range(rows: Sequence[Mapping], key: str) -> float:
    values = [float(row[key]) for row in rows]
    return float(max(values) - min(values)) if values else float("nan")


def _ratio(numerator: float, denominator: float):
    return None if denominator == 0.0 else float(numerator / denominator)


def build_specimen_reports(scan_rows: Sequence[Mapping]) -> tuple[list[dict], list[dict]]:
    by_specimen: dict[str, list[Mapping]] = defaultdict(list)
    for row in scan_rows:
        by_specimen[str(row["specimen"])].append(row)
    summary_rows: list[dict] = []
    detail_rows: list[dict] = []
    for specimen in sorted(by_specimen):
        positive = [row for row in by_specimen[specimen] if row["direction"] == "positive"]
        reverse = [row for row in by_specimen[specimen] if row["direction"] == "reverse"]
        for row in positive:
            detail_rows.append({
                "specimen": specimen,
                "type": "正",
                "scan": row["scan"],
                **{metric: row[f"aligned_{metric}"] for metric in METRICS},
            })
        pos_mean = {metric: _mean(positive, f"aligned_{metric}") for metric in METRICS} if positive else {}
        pos_range = {metric: _range(positive, f"aligned_{metric}") for metric in METRICS} if positive else {}
        if positive:
            detail_rows.append({"specimen": specimen, "type": "正平均", "scan": "", **pos_mean})
            detail_rows.append({"specimen": specimen, "type": "正极差", "scan": "", **pos_range})
        for row in reverse:
            detail_rows.append({
                "specimen": specimen,
                "type": "反",
                "scan": row["scan"],
                **{metric: row[f"aligned_{metric}"] for metric in METRICS},
            })
        rev_mean = {metric: _mean(reverse, f"aligned_{metric}") for metric in METRICS} if reverse else {}
        rev_range = {metric: _range(reverse, f"aligned_{metric}") for metric in METRICS} if reverse else {}
        if reverse:
            detail_rows.append({"specimen": specimen, "type": "反平均", "scan": "", **rev_mean})
            detail_rows.append({"specimen": specimen, "type": "反极差", "scan": "", **rev_range})
        if not (positive and reverse):
            summary_rows.append({
                "specimen": specimen,
                "positive_scan_count": len(positive),
                "reverse_scan_count": len(reverse),
                "status": "缺少正向或反向扫描，不能计算互换显著度",
            })
            continue
        diff = {metric: pos_mean[metric] - rev_mean[metric] for metric in METRICS}
        detail_rows.append({
            "specimen": specimen,
            "type": "正平均-反平均",
            "scan": "",
            **diff,
        })
        bd_difference = pos_mean["B"] - pos_mean["D"]
        bd_flip_residual = pos_mean["B"] - rev_mean["B"]
        diagonal_difference = pos_mean["D1"] - pos_mean["D2"]
        diagonal_flip_residual = pos_mean["D1"] - rev_mean["D1"]
        summary = {
            "specimen": specimen,
            "positive_scan_count": len(positive),
            "reverse_scan_count": len(reverse),
            **{f"positive_mean_{metric}": pos_mean[metric] for metric in METRICS},
            **{f"positive_range_{metric}": pos_range[metric] for metric in METRICS},
            **{f"reverse_mean_{metric}": rev_mean[metric] for metric in METRICS},
            **{f"reverse_range_{metric}": rev_range[metric] for metric in METRICS},
            **{f"positive_minus_reverse_{metric}": diff[metric] for metric in METRICS},
            "BD_difference_mm": bd_difference,
            "BD_flip_residual_mm": bd_flip_residual,
            "BD_exchange_significance": _ratio(bd_difference, bd_flip_residual),
            "D1_D2_difference_mm": diagonal_difference,
            "D1_flip_residual_mm": diagonal_flip_residual,
            "diagonal_exchange_significance": _ratio(diagonal_difference, diagonal_flip_residual),
        }
        summary_rows.append(summary)
        for row in detail_rows:
            if row.get("specimen") == specimen and row.get("type") == "正平均":
                row.update({
                    "BD_derived_value": bd_difference,
                    "BD_metric": "BD差值(mm)",
                    "diagonal_derived_value": diagonal_difference,
                    "diagonal_metric": "D1-D2差值(mm)",
                })
            if row.get("specimen") == specimen and row.get("type") == "正极差":
                row.update({
                    "BD_derived_value": summary["BD_exchange_significance"],
                    "BD_metric": "BD互换显著度",
                    "diagonal_derived_value": summary["diagonal_exchange_significance"],
                    "diagonal_metric": "对角线互换显著度",
                })
    return summary_rows, detail_rows


def load_manifest(manifest_path: Path) -> list[dict]:
    manifest_path = Path(manifest_path)
    with manifest_path.open("r", encoding="utf-8-sig", newline="") as handle:
        rows = list(csv.DictReader(handle))
    required = {"specimen", "direction", "scan", "scan_dir"}
    if not rows:
        raise ValueError("输入清单为空")
    if not required.issubset(rows[0]):
        raise ValueError(f"输入清单必须包含列: {sorted(required)}")
    output = []
    for row in rows:
        scan_dir = Path(row["scan_dir"])
        if not scan_dir.is_absolute():
            scan_dir = (manifest_path.parent / scan_dir).resolve()
        output.append({
            "specimen": row["specimen"],
            "direction": normalize_direction(row["direction"]),
            "scan": row["scan"],
            "scan_dir": str(scan_dir),
        })
    return output


def run_scans(scans: Sequence[Mapping[str, str]], calibration_dir: Path, output_dir: Path) -> dict:
    output_dir = Path(output_dir)
    output_dir.mkdir(parents=True, exist_ok=True)
    calibration = load_calibration(Path(calibration_dir), apply_exchange=True)
    all_slices: list[dict] = []
    summaries: list[dict] = []
    failures: list[dict] = []
    for index, scan in enumerate(scans, start=1):
        print(
            f"[{index}/{len(scans)}] {scan['specimen']} {normalize_direction(scan['direction'])} {scan['scan']}",
            flush=True,
        )
        try:
            slices, summary = measure_scan(scan, calibration)
            all_slices.extend(slices)
            summaries.append(summary)
        except Exception as exc:
            failures.append({**dict(scan), "error": repr(exc)})
    if failures:
        write_csv(output_dir / "failures.csv", failures)
        raise RuntimeError(f"{len(failures)}次扫描失败，详见 failures.csv")
    specimen_summary, aligned_detail = build_specimen_reports(summaries)
    write_csv(output_dir / "slice_measurements.csv", all_slices)
    write_csv(output_dir / "scan_summary.csv", summaries)
    write_csv(output_dir / "aligned_detail.csv", aligned_detail)
    write_csv(output_dir / "specimen_summary.csv", specimen_summary)
    result = {
        "algorithm_version": ALGORITHM_VERSION,
        "scan_count": len(summaries),
        "slice_count": len(all_slices),
        "specimen_summary": specimen_summary,
        "scan_summary": summaries,
    }
    (output_dir / "measurement_result.json").write_text(
        json.dumps(json_safe(result), ensure_ascii=False, indent=2),
        encoding="utf-8",
    )
    audit = {
        "algorithm_version": ALGORITHM_VERSION,
        "base_calibration_file": str(calibration.base_path.resolve()),
        "base_calibration_sha256": sha256(calibration.base_path),
        "exchange_calibration_file": str(calibration.exchange_path.resolve()) if calibration.exchange_path else None,
        "exchange_calibration_sha256": sha256(calibration.exchange_path) if calibration.exchange_path else None,
        "corrections_mm": calibration.corrections_mm,
        "fixed_parameters": {
            "x_scale_mm_per_px": X_SCALE_MM_PER_PX,
            "y_scale_mm_per_row": Y_SCALE_MM_PER_ROW,
            "slice_end_margin_rows": SLICE_END_MARGIN_ROWS,
            "corner_neighborhood_radius_mm": CORNER_NEIGHBORHOOD_RADIUS_MM,
            "diagonal_angle_search_deg": 0.0,
            "trim_per_tail": TRIM_PER_TAIL,
        },
        "input_scans": [dict(scan) for scan in scans],
        "output_scan_count": len(summaries),
        "output_slice_count": len(all_slices),
    }
    (output_dir / "run_audit.json").write_text(
        json.dumps(json_safe(audit), ensure_ascii=False, indent=2),
        encoding="utf-8",
    )
    return result
