#!/usr/bin/env python3
"""Build an isolated latest-Web/first-algorithm preview package."""

from __future__ import annotations

import importlib.util
import json
import os
import shutil
import subprocess
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parent.parent
TOOLS = ROOT / "tools"
BUILD = ROOT / "build" / "v1_latest_web_preview"
RELEASES = ROOT / "release"
PACKAGE_NAME = "PREVIEW_UNSAFE_V1_ALGORITHM_LATEST_WEB"
PACKAGE_DIR = RELEASES / PACKAGE_NAME
ZIP_PATH = RELEASES / f"{PACKAGE_NAME}.zip"
FIRST_COMMIT = "5dd22c7bc2d0cab21ff1f56455f119eaeb1c6fb0"


def git_file(path: str) -> bytes:
    return subprocess.check_output(
        ["git", "show", f"{FIRST_COMMIT}:{path}"],
        cwd=ROOT,
    )


def run_pyinstaller(arguments: list[str]) -> None:
    subprocess.run(
        [sys.executable, "-m", "PyInstaller", "--noconfirm", "--clean", *arguments],
        check=True,
        cwd=ROOT,
    )


def prepare_stage() -> Path:
    stage = BUILD / "stage"
    algorithm_dir = stage / "v1_algorithm"
    dashboard_dir = stage / "dashboard"
    model_dir = stage / "calibration" / "models"
    algorithm_dir.mkdir(parents=True)
    dashboard_dir.mkdir(parents=True)
    model_dir.mkdir(parents=True)

    (algorithm_dir / "measure_square_rod_edges_v1.py").write_bytes(
        git_file("tools/measure_square_rod_edges.py")
    )
    (model_dir / "camera_calibration_model_210_105.json").write_bytes(
        git_file("tools/camera_calibration_model.json")
    )
    unavailable = {
        "version": 0,
        "model": "not_available_in_first_algorithm_preview",
        "valid": False,
        "preview_only": True,
        "reason": "The first algorithm version did not contain this algorithm.",
    }
    for name in (
        "endface_calibration_model_210_105.json",
        "mechanical_drift_model_210_105.json",
    ):
        (model_dir / name).write_text(
            json.dumps(unavailable, ensure_ascii=False, indent=2),
            encoding="utf-8",
        )

    for source in (TOOLS / "dashboard").iterdir():
        if source.is_file():
            shutil.copy2(source, dashboard_dir / source.name)
    adapt_preview_web(dashboard_dir)
    return stage


def adapt_preview_web(dashboard_dir: Path) -> None:
    app_path = dashboard_dir / "app.js"
    app = app_path.read_text(encoding="utf-8")
    app = app.replace(
        "text.en.perpendicularity = 'Main-face angle';",
        "text.en.perpendicularity = 'V1 verticality error |90°−angle|';",
    ).replace(
        "text.zh.perpendicularity = '主面夹角';",
        "text.zh.perpendicularity = '首版垂直度误差 |90°−夹角|';",
    ).replace(
        "format(summary[`obj${item.obj}_projection_y_mm`])",
        "format(summary[`obj${item.obj}_projection_y_mm`] ?? summary[`obj${item.obj}_projection_z_mm`])",
    )
    app_path.write_text(app, encoding="utf-8")

    index_path = dashboard_dir / "index.html"
    index = index_path.read_text(encoding="utf-8")
    index = index.replace(
        "<body>",
        "<body><div class=\"v1-preview-banner\">仅供界面预览：最新 Web + 第一版横截面算法；无端面算法、无机械漂移算法，禁止用于正式检测。</div>",
        1,
    )
    index_path.write_text(index, encoding="utf-8")

    style_path = dashboard_dir / "styles.css"
    style = style_path.read_text(encoding="utf-8")
    style += "\n.v1-preview-banner{background:#9b3a1d;color:#fff;text-align:center;padding:7px 12px;font-weight:800;letter-spacing:.2px}\n"
    style_path.write_text(style, encoding="utf-8")


def write_package_files() -> None:
    (PACKAGE_DIR / "hobj").mkdir(parents=True)
    (PACKAGE_DIR / "results" / "measurements").mkdir(parents=True)
    (PACKAGE_DIR / "start_dashboard.bat").write_text(
        "@echo off\r\n"
        "set SUNSHINE_DASHBOARD_PORT=8768\r\n"
        "start \"V1 Algorithm Preview\" /b \"%~dp0MeasurementDashboard\\MeasurementDashboard.exe\"\r\n"
        "timeout /t 2 /nobreak >nul\r\n"
        "start \"V1 Algorithm Preview\" \"http://127.0.0.1:8768\"\r\n",
        encoding="utf-8",
    )
    (PACKAGE_DIR / "README_仅供预览.txt").write_text(
        "最新 Web 界面 + 第一版算法预览包\r\n"
        "================================\r\n\r\n"
        "本包仅用于查看界面和回顾第一版横截面算法，禁止用于正式检测或出具结果。\r\n\r\n"
        f"第一版来源提交：{FIRST_COMMIT}\r\n"
        "界面来源：打包时 tools\\dashboard 当前工作区最新版。\r\n\r\n"
        "第一版已有：A/B/C/D、倒角中点对角线、四角角度、倒角长度、X/Z投影、棒长。\r\n"
        "第一版没有：端面八角、端面垂直度、机械漂移识别与修正、v12物理Y同步。\r\n"
        "因此最新界面中的端面和漂移区域会保持无数据，程序不会补造这些结果。\r\n\r\n"
        "双击 start_dashboard.bat，浏览器访问 http://127.0.0.1:8768。\r\n"
        "压缩包不包含任何 HOBJ、现场图像、运行日志或测量 CSV。\r\n",
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

    run_pyinstaller([
        "--onedir",
        "--name", "MeasureSquareRod",
        "--distpath", str(dist),
        "--workpath", str(work),
        "--specpath", str(spec),
        "--collect-all", "PIL",
        "--hidden-import", "numpy",
        "--add-data", f"{stage / 'v1_algorithm'}{data_separator}v1_algorithm",
        str(TOOLS / "v1_preview_measure_runner.py"),
    ])
    run_pyinstaller([
        "--onedir",
        "--noconsole",
        "--name", "MeasurementDashboard",
        "--distpath", str(dist),
        "--workpath", str(work),
        "--specpath", str(spec),
        "--add-data", f"{stage / 'dashboard'}{data_separator}dashboard",
        str(TOOLS / "measurement_dashboard.py"),
    ])

    RELEASES.mkdir(exist_ok=True)
    shutil.move(str(dist / "MeasureSquareRod"), str(PACKAGE_DIR / "MeasureSquareRod"))
    shutil.move(str(dist / "MeasurementDashboard"), str(PACKAGE_DIR / "MeasurementDashboard"))
    shutil.copytree(stage / "calibration", PACKAGE_DIR / "calibration")
    write_package_files()
    shutil.make_archive(str(ZIP_PATH.with_suffix("")), "zip", RELEASES, PACKAGE_NAME)
    print(f"Package directory: {PACKAGE_DIR}")
    print(f"Zip archive: {ZIP_PATH}")


if __name__ == "__main__":
    main()
