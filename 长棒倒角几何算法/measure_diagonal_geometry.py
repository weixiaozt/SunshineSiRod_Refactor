#!/usr/bin/env python3
"""在窗口中测量并显示一个 HOBJ 的两条实体倒角端点对角线。"""

from __future__ import annotations

import argparse
import csv
import json
from pathlib import Path
import sys
from typing import Any


MODULE_ROOT = Path(__file__).resolve().parent
REPOSITORY_ROOT = MODULE_ROOT.parent
sys.path.insert(0, str(MODULE_ROOT / "src"))

from chamfer_geometry import load_calibration  # noqa: E402
from chamfer_geometry.diagonal import (  # noqa: E402
    attach_model_traceability,
    load_global_coordinate_calibration,
    measure_diagonals,
)


DEFAULT_CHAMFER_CALIBRATION = (
    MODULE_ROOT / "models" / "chamfer_camera_metric_geometry_210_105.json"
)
DEFAULT_GLOBAL_CALIBRATION = (
    REPOSITORY_ROOT
    / "ABCD_length6图完美模板标定法"
    / "models"
    / "long_rod_template_calibrated_length_210_105.json"
)


def _write_slice_csv(path: Path, result: dict[str, Any]) -> None:
    rows = result["slice_records"]
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("w", newline="", encoding="utf-8-sig") as handle:
        writer = csv.DictWriter(handle, fieldnames=list(rows[0]))
        writer.writeheader()
        writer.writerows(rows)


def _print_result(result: dict[str, Any]) -> None:
    print(f"路径: {result['input_path']}")
    plan = result["station_plan"]
    print(
        f"固定Y同步切片: {plan['common_physical_station_count']} 片，"
        f"Y={plan['common_physical_y_range_mm'][0]:.3f}.."
        f"{plan['common_physical_y_range_mm'][1]:.3f} mm，统计={plan['aggregate']}"
    )
    print()
    print("对角线  实体端点定义                         最终值(mm)  标准差(mm)  极差(mm)   有效片")
    print("------  -----------------------------------  ----------  ----------  ---------  ------")
    labels = {
        "diagonal_1": "角点2 AB-A  到  角点6 CD-C",
        "diagonal_2": "角点1 AD-A  到  角点5 BC-C",
    }
    for diagonal in ("diagonal_1", "diagonal_2"):
        item = result["metrics"][diagonal]
        print(
            f"{diagonal[-1]:<6}  {labels[diagonal]:<35}  "
            f"{item['value']:>10.6f}  {item['std']:>10.6f}  "
            f"{item['range']:>9.6f}  {item['valid_count']:>6}"
        )
        regional = item["regional_mean_mm"]
        print(
            "        头/中/尾均值: "
            f"{regional['head']:.6f} / {regional['middle']:.6f} / {regional['tail']:.6f} mm"
        )
    print()
    print("说明: 使用倒角几何v3实测端点 + 长棒模板长度模型的四相机全局坐标和固定Y同步。")
    print("说明: 未使用长度B/D的K值，未增加对角线终值偏移，未根据路径判断调头。")
    print("状态: 第一版可运行候选；独立对角线真值验证完成前，不宣称绝对精度正式放行。")


def main() -> int:
    parser = argparse.ArgumentParser(
        description="长棒实体倒角端点全局对角线算法：D1=角点2-6，D2=角点1-5"
    )
    parser.add_argument("input", type=Path, help="一个 .hobj 文件")
    parser.add_argument(
        "--chamfer-calibration",
        type=Path,
        default=DEFAULT_CHAMFER_CALIBRATION,
        help="倒角v3相机指纹公制度量 JSON",
    )
    parser.add_argument(
        "--global-calibration",
        type=Path,
        default=DEFAULT_GLOBAL_CALIBRATION,
        help="长棒模板长度全局坐标 JSON",
    )
    parser.add_argument("--stations", type=int, default=None, help="测试切片数，默认使用模型850片")
    parser.add_argument("--json", type=Path, default=None, help="可选：保存完整JSON")
    parser.add_argument("--slice-csv", type=Path, default=None, help="可选：保存逐切片CSV")
    args = parser.parse_args()

    chamfer_calibration = load_calibration(args.chamfer_calibration)
    global_calibration = load_global_coordinate_calibration(args.global_calibration)
    result = measure_diagonals(
        args.input,
        chamfer_calibration,
        global_calibration,
        station_count=args.stations,
    )
    attach_model_traceability(
        result,
        chamfer_calibration_path=args.chamfer_calibration,
        global_coordinate_calibration_path=args.global_calibration,
    )
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
