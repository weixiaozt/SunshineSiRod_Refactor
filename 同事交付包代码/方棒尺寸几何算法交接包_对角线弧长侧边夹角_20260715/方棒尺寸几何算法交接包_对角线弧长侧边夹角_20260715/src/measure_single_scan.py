import argparse
import csv
import json
import math
import os
from pathlib import Path

import numpy as np
import tifffile

import measure_all_bars_ctb_slice_calibrated as pipeline


PACKAGE_ROOT = Path(__file__).resolve().parents[1]
DEFAULT_CALIBRATION = PACKAGE_ROOT / "calibration" / "current_calibration.json"

HOBJ_OFFSETS = [120100, 256240185, 512360270, 768480355]
HOBJ_CHANNELS = ["Left", "Right", "Top", "Down"]
HOBJ_SHAPE = (20000, 3200)

NUMBER_TO_CAMERA = {
    "1": "Right",
    "2": "Down",
    "3": "Top",
    "4": "Left",
}
NUMBER_TO_FACES = {
    "1": ("A", "B"),
    "2": ("C", "D"),
    "3": ("A", "D"),
    "4": ("B", "C"),
}
END_INDEX = {"头": 0, "尾": 1}


def write_csv(path, rows, headers=None):
    rows = list(rows)
    if not rows:
        return
    if headers is None:
        headers = []
        for row in rows:
            for key in row:
                if key not in headers:
                    headers.append(key)
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("w", newline="", encoding="utf-8-sig") as handle:
        writer = csv.DictWriter(handle, fieldnames=headers)
        writer.writeheader()
        writer.writerows(rows)


def py(value):
    if isinstance(value, dict):
        return {str(k): py(v) for k, v in value.items()}
    if isinstance(value, list):
        return [py(v) for v in value]
    if isinstance(value, np.ndarray):
        return value.tolist()
    if isinstance(value, (np.floating, float)):
        value = float(value)
        return value if math.isfinite(value) else None
    if isinstance(value, (np.integer, int)):
        return int(value)
    return value


def load_runtime_calibration(path):
    payload = json.loads(Path(path).read_text(encoding="utf-8"))
    runtime = payload["runtime_calibration"]
    calibration = {
        "rows": np.asarray(runtime["rows"], dtype=int),
        "windows": {
            camera: {
                side: np.asarray(columns, dtype=int)
                for side, columns in sides.items()
            }
            for camera, sides in runtime["windows"].items()
        },
        "matrices": {
            camera: np.asarray(matrix, dtype=float)
            for camera, matrix in runtime["matrices"].items()
        },
        "origins": [
            {
                camera: np.asarray(origin, dtype=float)
                for camera, origin in origin_row.items()
            }
            for origin_row in runtime["origins"]
        ],
    }
    camera_y_bias = {
        camera: float(value)
        for camera, value in payload["camera_y_calibration"]["bias_total_mm_by_camera"].items()
    }
    return payload, calibration, camera_y_bias


def unpack_hobj(hobj_path, output_dir):
    output_dir.mkdir(parents=True, exist_ok=True)
    files = {}
    for index, (offset, channel) in enumerate(zip(HOBJ_OFFSETS, HOBJ_CHANNELS), start=1):
        target = output_dir / f"{index}-{channel}.tif"
        if not target.exists():
            partial = target.with_suffix(".partial.tif")
            partial.unlink(missing_ok=True)
            image = np.memmap(hobj_path, dtype=np.float32, mode="r", offset=offset, shape=HOBJ_SHAPE)
            tifffile.imwrite(
                partial,
                image,
                dtype=np.float32,
                compression=None,
                photometric="minisblack",
                rowsperstrip=64,
                metadata=None,
            )
            del image
            os.replace(partial, target)
        files[channel] = target
    return files


def first_existing(directory, patterns):
    for pattern in patterns:
        matches = sorted(directory.glob(pattern))
        if matches:
            return matches[0]
    return None


def discover_tiff_files(scan_path, output_dir):
    scan_path = Path(scan_path)
    if scan_path.is_file() and scan_path.suffix.lower() == ".hobj":
        return unpack_hobj(scan_path, output_dir / "_unpacked_hobj" / scan_path.stem)

    if not scan_path.is_dir():
        raise RuntimeError(f"input is not a folder or .hobj file: {scan_path}")

    hobj_files = sorted(scan_path.glob("*.hobj"))
    standard = {
        "Left": first_existing(scan_path, ["1-Left.tif", "1-Left.tiff", "*_1.tif", "*_1.tiff", "*Left*.tif", "*Left*.tiff"]),
        "Right": first_existing(scan_path, ["2-Right.tif", "2-Right.tiff", "*_2.tif", "*_2.tiff", "*Right*.tif", "*Right*.tiff"]),
        "Top": first_existing(scan_path, ["3-Top.tif", "3-Top.tiff", "*_3.tif", "*_3.tiff", "*Top*.tif", "*Top*.tiff"]),
        "Down": first_existing(scan_path, ["4-Down.tif", "4-Down.tiff", "*_4.tif", "*_4.tiff", "*Down*.tif", "*Down*.tiff"]),
    }
    if all(standard.values()):
        return standard

    if len(hobj_files) == 1:
        return unpack_hobj(hobj_files[0], output_dir / "_unpacked_hobj" / hobj_files[0].stem)

    missing = [camera for camera, path in standard.items() if path is None]
    raise RuntimeError(
        "cannot find four camera files. Missing: "
        + ", ".join(missing)
        + ". Expected 1-Left, 2-Right, 3-Top, 4-Down TIFFs or one .hobj file."
    )


def fit_plane_tls(points):
    points = np.asarray(points, dtype=float)
    center = np.mean(points, axis=0)
    _, _, vt = np.linalg.svd(points - center, full_matrices=False)
    normal = vt[-1]
    normal = normal / np.linalg.norm(normal)
    d = -float(np.dot(normal, center))
    residual = points @ normal + d
    return {
        "normal": normal,
        "d": d,
        "center": center,
        "rms_mm": float(math.sqrt(np.mean(residual**2))),
        "max_abs_residual_mm": float(np.max(np.abs(residual))),
        "point_count": int(points.shape[0]),
    }


def orient_side_normals(planes):
    centers_xz = np.asarray([[plane["center"][0], plane["center"][2]] for plane in planes.values()])
    centroid_xz = np.mean(centers_xz, axis=0)
    for plane in planes.values():
        normal_xz = np.array([plane["normal"][0], plane["normal"][2]], dtype=float)
        face_vector = np.array([plane["center"][0], plane["center"][2]], dtype=float) - centroid_xz
        if np.dot(normal_xz, face_vector) < 0:
            plane["normal"] = -plane["normal"]
            plane["d"] = -plane["d"]


def side_planes_for_scan(scan_input, calibration, camera_bias):
    images = {camera: tifffile.memmap(path) for camera, path in scan_input.files.items()}
    bounds, common_start, common_end = pipeline.image_bounds(images)
    all_rows = calibration["rows"]
    valid_indices = np.flatnonzero(
        (all_rows >= common_start + pipeline.SLICE_END_MARGIN_ROWS)
        & (all_rows <= common_end - pipeline.SLICE_END_MARGIN_ROWS)
    )
    if valid_indices.size < 10:
        raise RuntimeError(f"{scan_input.specimen}/{scan_input.scan} has too few valid rows")
    rows = all_rows[valid_indices]
    origins = [calibration["origins"][int(index)] for index in valid_indices]
    face_points = pipeline.transform_face_points(
        images, rows, calibration["windows"], calibration["matrices"], origins
    )

    planes = {}
    for face, sources in pipeline.FACE_SOURCES.items():
        groups_3d = []
        for camera, side in sources:
            bias = float(camera_bias.get(camera, 0.0))
            for index, row in enumerate(rows):
                xz = face_points[camera][side][index]
                if xz.size == 0:
                    continue
                y = float(row) * pipeline.Y_SCALE_MM_PER_ROW - bias
                group = np.column_stack([xz[:, 0], np.full(xz.shape[0], y, dtype=float), xz[:, 1]])
                groups_3d.append(group)
        planes[face] = fit_plane_tls(np.vstack(groups_3d))
    orient_side_normals(planes)
    return planes, bounds


def corner_from_side_planes(planes, faces, y):
    rows = []
    rhs = []
    for face in faces:
        normal = planes[face]["normal"]
        rows.append([normal[0], normal[2]])
        rhs.append(-planes[face]["d"] - normal[1] * y)
    x, z = np.linalg.solve(np.asarray(rows, dtype=float), np.asarray(rhs, dtype=float))
    return np.array([float(x), float(y), float(z)], dtype=float)


def endpoint_points(planes, bounds, end, camera_bias):
    points = []
    for number in "1234":
        camera = NUMBER_TO_CAMERA[number]
        y = float(bounds[camera][END_INDEX[end]]) * pipeline.Y_SCALE_MM_PER_ROW - float(camera_bias.get(camera, 0.0))
        points.append(corner_from_side_planes(planes, NUMBER_TO_FACES[number], y))
    return np.asarray(points, dtype=float)


def end_plane_angles(planes, bounds, end, camera_bias):
    points = endpoint_points(planes, bounds, end, camera_bias)
    plane = fit_plane_tls(points)
    if plane["normal"][1] < 0:
        plane["normal"] = -plane["normal"]
        plane["d"] = -plane["d"]
    out = {
        f"{end}fit_rms_mm": plane["rms_mm"],
        f"{end}fit_max_abs_residual_mm": plane["max_abs_residual_mm"],
    }
    for face in "ABCD":
        cos_angle = float(
            np.dot(plane["normal"], planes[face]["normal"])
            / (np.linalg.norm(plane["normal"]) * np.linalg.norm(planes[face]["normal"]))
        )
        angle = math.degrees(math.acos(np.clip(cos_angle, -1.0, 1.0)))
        out[f"{end}{face.lower()}"] = angle
        out[f"{end}{face.lower()}_偏离90_deg"] = angle - 90.0
    return out


def calibrated_endface_from_offsets(bounds, metrics, camera_bias):
    offsets = pipeline.endpoint_offset_metrics(bounds, camera_bias)
    out = {}
    for key, value in offsets.items():
        out[f"{key}_当前Y标定"] = value
    for end_cn, prefix, edge_prefix in (("头", "起点偏移量", "头-边长"), ("尾", "结束点偏移量", "尾-边长")):
        for edge, (camera1, camera2) in pipeline.EDGE_PAIRS.items():
            numbers = [number for number, (_, camera) in pipeline.OFFSET_POINTS.items() if camera in (camera1, camera2)]
            if len(numbers) != 2:
                continue
            dy = abs(offsets[f"{prefix}{numbers[0]}"] - offsets[f"{prefix}{numbers[1]}"])
            side = float(metrics[f"{edge_prefix}{edge}"])
            out[f"{end_cn}-端面垂直度-当前Y标定-{edge}"] = 90.0 - math.degrees(math.atan2(dy, side))
    return out


def main():
    parser = argparse.ArgumentParser(description="Measure one square-bar scan folder with the packaged current calibration.")
    parser.add_argument("input", help="Folder containing four TIFFs, or one .hobj file")
    parser.add_argument("--output", "-o", default=None, help="Output folder. Default: <input>/measurement_output")
    parser.add_argument("--specimen", default=None, help="Specimen name written to output")
    parser.add_argument("--scan", default=None, help="Scan name written to output")
    parser.add_argument("--calibration", default=str(DEFAULT_CALIBRATION), help="Calibration JSON path")
    args = parser.parse_args()

    input_path = Path(args.input).resolve()
    output_dir = Path(args.output).resolve() if args.output else (
        input_path.parent / f"{input_path.stem}_measurement_output" if input_path.is_file() else input_path / "measurement_output"
    )
    output_dir.mkdir(parents=True, exist_ok=True)

    calibration_payload, calibration, camera_bias = load_runtime_calibration(args.calibration)
    files = discover_tiff_files(input_path, output_dir)
    default_name = input_path.stem if input_path.is_file() else input_path.name
    specimen = args.specimen or (input_path.parent.name if input_path.is_file() else input_path.name)
    scan = args.scan or default_name
    scan_input = pipeline.ScanInput(specimen, scan, files)

    result = pipeline.measure_scan(scan_input, calibration)
    images = {camera: tifffile.memmap(path) for camera, path in files.items()}
    bounds, _, _ = pipeline.image_bounds(images)
    current_y_metrics = calibrated_endface_from_offsets(bounds, result["metrics"], camera_bias)

    planes, plane_bounds = side_planes_for_scan(scan_input, calibration, camera_bias)
    angle_row = {"被测棒": specimen, "扫描": scan}
    for end in ("头", "尾"):
        angle_row.update(end_plane_angles(planes, plane_bounds, end, camera_bias))
    for face in "ABCD":
        angle_row[f"{face.lower()}面fit_rms_mm"] = planes[face]["rms_mm"]
        angle_row[f"{face.lower()}面点数"] = planes[face]["point_count"]

    scan_metrics = {
        "被测棒": specimen,
        "扫描": scan,
        **result["metrics"],
        **current_y_metrics,
    }
    write_csv(output_dir / "scan_metrics.csv", [scan_metrics])
    write_csv(output_dir / "slice_measurements.csv", result["slice_records"])
    write_csv(output_dir / "measured_side_end_plane_angles.csv", [angle_row])

    summary = {
        "input": str(input_path),
        "files": {camera: str(path) for camera, path in files.items()},
        "calibration": {
            "path": str(Path(args.calibration).resolve()),
            "abcd_used_fields_mm": calibration_payload["abcd_length_calibration"]["used_fields_mm"],
            "camera_y_bias_total_mm_by_camera": camera_bias,
        },
        "scan_metrics": scan_metrics,
        "measured_side_end_plane_angles": angle_row,
        "diagnostics": result["diagnostics"],
    }
    (output_dir / "measurement_result.json").write_text(
        json.dumps(py(summary), ensure_ascii=False, indent=2),
        encoding="utf-8",
    )
    print(output_dir)


if __name__ == "__main__":
    main()
