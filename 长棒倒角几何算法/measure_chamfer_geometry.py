#!/usr/bin/env python3
"""在窗口中测量并显示一个 HOBJ 的 AD/AB/CB/CD 倒角几何。"""

from __future__ import annotations

import argparse
import csv
import json
from pathlib import Path
import sys
from typing import Any


MODULE_ROOT = Path(__file__).resolve().parent
sys.path.insert(0, str(MODULE_ROOT / "src"))

from chamfer_geometry import load_calibration, measure_hobj  # noqa: E402


DEFAULT_CALIBRATION = MODULE_ROOT / "models" / "chamfer_camera_metric_geometry_210_105.json"


def _write_slice_csv(path: Path, result: dict[str, Any]) -> None:
    rows: list[dict[str, Any]] = []
    for corner, records in result["slice_records"].items():
        for record in records:
            flat = {
                "input_path": result["input_path"],
                "capture_id": result["capture_id"],
                "corner": corner,
                "row": record["row"],
                "camera": record["camera"],
                "chord_mm": record["chord_mm"],
                "corner_angle_deg": record["corner_angle_deg"],
                "chamfer_fit_rms_mm": record["chamfer_fit"]["fit_rms_mm"],
            }
            for key, value in record.items():
                if key.startswith("projection_") and key.endswith("_mm"):
                    flat[key] = value
            rows.append(flat)
    path.parent.mkdir(parents=True, exist_ok=True)
    headers: list[str] = []
    for row in rows:
        for key in row:
            if key not in headers:
                headers.append(key)
    with path.open("w", newline="", encoding="utf-8-sig") as handle:
        writer = csv.DictWriter(handle, fieldnames=headers)
        writer.writeheader()
        writer.writerows(rows)


def _print_result(result: dict[str, Any]) -> None:
    print(f"路径: {result['input_path']}")
    plan = result["station_plan"]
    print(
        f"切片: {plan['actual_count']} 片，行 {plan['first_row']}..{plan['last_row']}，"
        f"统计={plan['aggregate']}"
    )
    print(
        "相机底层二维公制度量: "
        + ("已应用" if result["camera_metric_transform_applied"] else "单位矩阵（未修正）")
    )
    print()
    print("角点  相机    倒角弦长(mm)    投影1(mm)        投影2(mm)        尖角夹角(°)  有效/拒绝")
    print("----  ------  --------------  ---------------  ---------------  -----------  ---------")
    for corner in ("AD", "AB", "CB", "CD"):
        item = result["corners"][corner]
        face1, face2 = item["output_face_order"]
        metrics = item["metrics"]
        print(
            f"{corner:<4}  {item['camera']:<6}  "
            f"{metrics['chord_mm']['value']:>14.6f}  "
            f"{face1}={metrics[f'projection_{face1}_mm']['value']:<12.6f}  "
            f"{face2}={metrics[f'projection_{face2}_mm']['value']:<12.6f}  "
            f"{metrics['corner_angle_deg']['value']:>11.6f}  "
            f"{item['valid_station_count']}/{item['rejected_station_count']}"
        )
    print()
    print("说明: 投影是理论尖角P到两个倒角端点的实际距离；未使用弦长/√2。")
    print("说明: 未根据文件名或父目录判断正反、调头或转向。")


def main() -> int:
    parser = argparse.ArgumentParser(
        description="长棒倒角几何·四相机指纹度量修正算法：测量AD/AB/CB/CD共16个值"
    )
    parser.add_argument("input", type=Path, help="一个 .hobj 文件")
    parser.add_argument("--calibration", type=Path, default=DEFAULT_CALIBRATION, help="相机公制标定 JSON")
    parser.add_argument("--stations", type=int, default=None, help="沿棒身均匀取样切片数，默认850")
    parser.add_argument("--json", type=Path, default=None, help="可选：保存完整结果和切片明细 JSON")
    parser.add_argument("--slice-csv", type=Path, default=None, help="可选：保存逐切片有效结果 CSV")
    args = parser.parse_args()

    calibration = load_calibration(args.calibration)
    result = measure_hobj(args.input, calibration, station_count=args.stations)
    _print_result(result)
    if args.json:
        args.json.parent.mkdir(parents=True, exist_ok=True)
        args.json.write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding="utf-8")
        print(f"JSON: {args.json.resolve()}")
    if args.slice_csv:
        _write_slice_csv(args.slice_csv, result)
        print(f"切片CSV: {args.slice_csv.resolve()}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
