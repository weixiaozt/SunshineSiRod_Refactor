#!/usr/bin/env python3
"""Build the global Web package with delivered cross-section geometry metrics."""

from __future__ import annotations

import importlib.util
import os
import shutil
import subprocess
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parent.parent
TOOLS = ROOT / "tools"
BUILD = ROOT / "build" / "global_delivery_geometry_package"
RELEASES = ROOT / "release"
PACKAGE_NAME = "SunshineSiRod_Global_DeliveryGeometry_20260715"
PACKAGE_DIR = RELEASES / PACKAGE_NAME
ZIP_PATH = RELEASES / f"{PACKAGE_NAME}.zip"
DELIVERY_ROOT = Path(
    r"D:\Project\方棒尺寸几何算法交接包_对角线弧长侧边夹角_20260715\方棒尺寸几何算法交接包_对角线弧长侧边夹角_20260715"
)
DELIVERY_MODULES = (
    "analyze_absolute_slice_calibration.py",
    "analyze_caliper_contact_diagonals_ctb.py",
    "analyze_cmm_calibrated_sparse_slices.py",
    "analyze_joint_face_angles_ctb.py",
    "analyze_positive_repeatability.py",
    "measure_all_bars_ctb_slice_calibrated.py",
    "measure_ctb_full_metrics.py",
)


def run_pyinstaller(args: list[str]) -> None:
    subprocess.run(
        [sys.executable, "-m", "PyInstaller", "--noconfirm", "--clean", *args],
        check=True,
        cwd=ROOT,
    )


def prepare_stage() -> Path:
    stage = BUILD / "stage"
    delivery_source = stage / "delivery_src"
    delivery_calibration = stage / "calibration" / "delivery_geometry"
    delivery_source.mkdir(parents=True)
    delivery_calibration.mkdir(parents=True)

    for name in DELIVERY_MODULES:
        source = DELIVERY_ROOT / "src" / name
        if not source.is_file():
            raise SystemExit(f"Delivered algorithm module is missing: {source}")
        text = source.read_text(encoding="utf-8")
        text = text.replace("import matplotlib.pyplot as plt\n", "")
        (delivery_source / name).write_text(text, encoding="utf-8")

    delivery_model = DELIVERY_ROOT / "calibration" / "current_calibration.json"
    if not delivery_model.is_file():
        raise SystemExit(f"Delivered geometry calibration is missing: {delivery_model}")
    shutil.copy2(delivery_model, delivery_calibration / delivery_model.name)
    return stage


def write_package_files() -> None:
    (PACKAGE_DIR / "hobj").mkdir(exist_ok=True)
    (PACKAGE_DIR / "results" / "measurements" / "user_results").mkdir(parents=True, exist_ok=True)
    (PACKAGE_DIR / "results" / "measurements" / "developer_details").mkdir(parents=True, exist_ok=True)
    (PACKAGE_DIR / "start_dashboard.bat").write_text(
        "@echo off\r\n"
        "set SUNSHINE_DASHBOARD_PORT=8766\r\n"
        "start \"Sunshine Si Rod Global Measurement\" /b \"%~dp0MeasurementDashboard\\MeasurementDashboard.exe\"\r\n"
        "timeout /t 2 /nobreak >nul\r\n"
        "start \"Sunshine Si Rod Global Measurement\" \"http://127.0.0.1:8766\"\r\n",
        encoding="utf-8",
    )
    (PACKAGE_DIR / "README_现场部署说明.txt").write_text(
        "方棒全局测量现场包（2026-07-15）\r\n"
        "================================\r\n\r\n"
        "1. 完整解压后双击 start_dashboard.bat。\r\n"
        "2. 浏览器地址为 http://127.0.0.1:8766。\r\n"
        "3. A/B/C/D 使用“方棒尺寸几何算法交接包_对角线弧长侧边夹角_20260715”的75截面算法。\r\n"
        "   Web最终四边值 = 头部前5个有效截面均值与尾部后5个有效截面均值的平均。\r\n"
        "4. 对角线使用交付包平行卡尺最大支撑距离，最终值为头5与尾5结果的平均。\r\n"
        "5. 弧长、双投影、四角主面夹角使用交付包全75截面平均算法。\r\n"
        "6. 棒长同样使用交付包算法；全局运行时不启动SunshineSiRod原生几何算法。\r\n"
        "7. 端面垂直度及头尾夹角暂时留空，等待新的端面算法交付包。\r\n"
        "   Web不加载端面模型，也不显示机构或人工标定真值。\r\n"
        "8. 右侧提供 A/B/C/D、两条对角线和棒长共7项显示补偿。\r\n"
        "   每次修改都会记录旧补偿、新补偿、Raw值及修改前后显示值。\r\n"
        "   原始值保留在结果中；端面最终角度补偿永久禁用。\r\n"
        "9. 倒角区域按真实横截面方向显示角1～角4，并标明对应相机1～相机4。\r\n\r\n"
        "输入目录默认为 hobj，输出目录默认为 results\\measurements。\r\n"
        "用户文件：results\\measurements\\user_results\\measurement_statistics.csv。\r\n"
        "补偿日志：results\\measurements\\user_results\\compensation_change_log.csv。\r\n"
        "研发明细：results\\measurements\\developer_details\\*_measure.csv。\r\n"
        "交付包原始JSON、SQLite内部库也保存在 developer_details。\r\n\r\n"
        "注意：交付包75截面标定固定使用CTB 13_08和当前210/105规格条件。\r\n"
        "更换规格、相机布局或扫描行范围后必须重新验证，禁止直接套用。\r\n",
        encoding="utf-8",
    )


def main() -> None:
    if shutil.which("pyinstaller") is None and importlib.util.find_spec("PyInstaller") is None:
        raise SystemExit("PyInstaller is required in the build environment")
    shutil.rmtree(BUILD, ignore_errors=True)
    shutil.rmtree(PACKAGE_DIR, ignore_errors=True)
    ZIP_PATH.unlink(missing_ok=True)
    stage = prepare_stage()
    dist = BUILD / "dist"
    work = BUILD / "work"
    spec = BUILD / "spec"
    data_separator = os.pathsep

    run_pyinstaller(
        [
            "--onedir",
            "--name",
            "MeasureDeliveryGeometry",
            "--distpath",
            str(dist),
            "--workpath",
            str(work),
            "--specpath",
            str(spec),
            "--paths",
            str(stage / "delivery_src"),
            "--collect-all",
            "tifffile",
            str(TOOLS / "delivery_abcd_measure.py"),
        ]
    )
    run_pyinstaller(
        [
            "--onedir",
            "--noconsole",
            "--name",
            "MeasurementDashboard",
            "--distpath",
            str(dist),
            "--workpath",
            str(work),
            "--specpath",
            str(spec),
            "--add-data",
            f"{TOOLS / 'dashboard'}{data_separator}dashboard",
            str(TOOLS / "measurement_dashboard.py"),
        ]
    )

    RELEASES.mkdir(exist_ok=True)
    PACKAGE_DIR.mkdir()
    shutil.move(str(dist / "MeasureDeliveryGeometry"), str(PACKAGE_DIR / "MeasureDeliveryGeometry"))
    shutil.move(str(dist / "MeasurementDashboard"), str(PACKAGE_DIR / "MeasurementDashboard"))
    shutil.copytree(stage / "calibration", PACKAGE_DIR / "calibration")
    write_package_files()
    shutil.make_archive(str(ZIP_PATH.with_suffix("")), "zip", RELEASES, PACKAGE_NAME)
    print(f"Package directory: {PACKAGE_DIR}")
    print(f"Zip archive: {ZIP_PATH}")


if __name__ == "__main__":
    main()
