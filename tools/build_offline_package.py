#!/usr/bin/env python3
"""Build the Windows x64 offline distribution without bundling any HOBJ data."""

from __future__ import annotations

import os
import shutil
import subprocess
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parent.parent
TOOLS = ROOT / "tools"
BUILD = ROOT / "build" / "offline_package"
RELEASES = ROOT / "release"
PACKAGE_NAME = "SunshineSiRod"
PACKAGE_DIR = RELEASES / PACKAGE_NAME
ZIP_PATH = RELEASES / f"{PACKAGE_NAME}.zip"


def run_pyinstaller(args: list[str]) -> None:
    subprocess.run([sys.executable, "-m", "PyInstaller", "--noconfirm", "--clean", *args], check=True, cwd=ROOT)


def copy_runtime_resources(stage: Path) -> None:
    """Copy only the active models/truth; deliberately never copy calibration HOBJs."""
    calibration = stage / "calibration"
    models = calibration / "models"
    truth = calibration / "truth"
    models.mkdir(parents=True)
    truth.mkdir(parents=True)
    for name in (
        "camera_calibration_model_210_105.json",
        "endface_calibration_model_210_105.json",
        "mechanical_drift_model_210_105.json",
    ):
        shutil.copy2(TOOLS / "calibration" / "models" / name, models / name)
    shutil.copy2(TOOLS / "calibration" / "truth" / "210_105.csv", truth / "210_105.csv")


def write_launcher_and_readme() -> None:
    (PACKAGE_DIR / "hobj").mkdir(exist_ok=True)
    (PACKAGE_DIR / "results" / "measurements").mkdir(parents=True, exist_ok=True)
    (PACKAGE_DIR / "start_dashboard.bat").write_text(
        "@echo off\r\n"
        "set SUNSHINE_DASHBOARD_PORT=8766\r\n"
        "start \"Sunshine Si Rod Measurement\" /b \"%~dp0MeasurementDashboard\\MeasurementDashboard.exe\"\r\n"
        "timeout /t 2 /nobreak >nul\r\n"
        "start \"Sunshine Si Rod Measurement\" \"http://127.0.0.1:8766\"\r\n",
        encoding="utf-8",
    )
    (PACKAGE_DIR / "README_离线使用说明.txt").write_text(
        "Sunshine 硅棒测量离线包（210 × 104 规格）\r\n"
        "=====================================\r\n\r\n"
        "本包为 Windows x64 离线版：不需要联网，也不需要安装 Python。\r\n"
        "本包没有包含任何 HOBJ 采集文件。\r\n\r\n"
        "使用方法：\r\n"
        "1. 解压整个压缩包，勿只复制其中的 exe 或 _internal 文件夹。\r\n"
        "2. 双击 start_dashboard.bat。浏览器会打开 http://127.0.0.1:8766。\r\n"
        "3. 在网页设置中选择对方电脑上的 HOBJ 根目录；保存设置。\r\n"
        "4. 点击“开始持续测量”。再次点击可停止。已经成功测过的文件不会重复测量；\r\n"
        "   同路径文件更新后会再次测量。\r\n\r\n"
        "输出位置：results\\measurements。每个 HOBJ 保留一份切片 CSV；\r\n"
        "measurement_statistics.csv 只保存网页显示的汇总值。\r\n"
        "若该 CSV 被 Excel/WPS 占用，测量仍会保存到 measurement_statistics.sqlite；\r\n"
        "关闭表格后，下一次扫描会自动刷新 CSV。\r\n\r\n"
        "标定铁律：本包内置的 210_105 模型只适用于同一根标准棒、同一规格、同一采集条件。\r\n"
        "程序会逐个 HOBJ 自动识别正常/已知机械漂移；已知漂移先修正局部坐标，未知漂移要求重测。\r\n"
        "更换规格必须重新做“单棒多次拍多图”标定，并替换相应模型后再测量。\r\n",
        encoding="utf-8",
    )


def main() -> None:
    if shutil.which("pyinstaller") is None and not __import__("importlib.util").util.find_spec("PyInstaller"):
        raise SystemExit("PyInstaller is required. Install it in the build environment first.")
    shutil.rmtree(BUILD, ignore_errors=True)
    shutil.rmtree(PACKAGE_DIR, ignore_errors=True)
    ZIP_PATH.unlink(missing_ok=True)
    stage = BUILD / "resources"
    copy_runtime_resources(stage)
    dist = BUILD / "dist"
    work = BUILD / "work"
    spec = BUILD / "spec"
    data_sep = os.pathsep
    run_pyinstaller([
        "--onedir", "--name", "MeasureSquareRod", "--distpath", str(dist), "--workpath", str(work), "--specpath", str(spec),
        "--collect-all", "PIL", str(TOOLS / "measure_square_rod_edges.py"),
    ])
    run_pyinstaller([
        "--onedir", "--noconsole", "--name", "MeasurementDashboard", "--distpath", str(dist), "--workpath", str(work), "--specpath", str(spec),
        "--add-data", f"{TOOLS / 'dashboard'}{data_sep}dashboard",
        "--add-data", f"{stage / 'calibration'}{data_sep}calibration",
        str(TOOLS / "measurement_dashboard.py"),
    ])
    RELEASES.mkdir(exist_ok=True)
    shutil.move(str(dist / "MeasureSquareRod"), str(PACKAGE_DIR / "MeasureSquareRod"))
    shutil.move(str(dist / "MeasurementDashboard"), str(PACKAGE_DIR / "MeasurementDashboard"))
    write_launcher_and_readme()
    shutil.make_archive(str(ZIP_PATH.with_suffix("")), "zip", RELEASES, PACKAGE_NAME)
    print(f"Package directory: {PACKAGE_DIR}")
    print(f"Zip archive: {ZIP_PATH}")


if __name__ == "__main__":
    main()
