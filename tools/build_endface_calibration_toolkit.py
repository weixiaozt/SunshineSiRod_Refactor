#!/usr/bin/env python3
"""Build an offline field toolkit that can generate, but never use, a v12 model."""

from __future__ import annotations

import importlib.util
import os
import shutil
import subprocess
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parent.parent
TOOLS = ROOT / "tools"
BUILD = ROOT / "build" / "endface_calibration_toolkit"
RELEASES = ROOT / "release"
PACKAGE_NAME = "SunshineSiRod_Endface_CalibrationToolkit"
PACKAGE_DIR = RELEASES / PACKAGE_NAME
ZIP_PATH = RELEASES / f"{PACKAGE_NAME}.zip"


def run_pyinstaller(args: list[str]) -> None:
    subprocess.run(
        [sys.executable, "-m", "PyInstaller", "--noconfirm", "--clean", *args],
        check=True,
        cwd=ROOT,
    )


def calibration_batch() -> str:
    return (
        "@echo off\r\n"
        "chcp 65001 >nul\r\n"
        "setlocal EnableExtensions\r\n"
        "cd /d \"%~dp0\"\r\n"
        "set \"SPEC=210_105\"\r\n"
        "set \"SAMPLE=BP411B116321XTO\"\r\n"
        "set \"ROOT=%~dp0calibration_hobj\\%SPEC%\"\r\n"
        "set \"CAMERA=%~dp0calibration\\models\\camera_calibration_model_210_105.json\"\r\n"
        "set \"OUTPUT=%~dp0output\\endface_calibration_model_%SPEC%.json\"\r\n"
        "set \"DIAG=%~dp0output\\endface_calibration_model_%SPEC%_diagnostics.csv\"\r\n"
        "echo ============================================================\r\n"
        "echo  端面物理v13+标定器：只生成模型，不执行产品测量\r\n"
        "echo ============================================================\r\n"
        "echo 机构端点：R=物理头，L=物理尾；面号A上 B右 C左 D下。\r\n"
        "echo head_to_tail：标准棒未调头，固定设备行头对应物理R端，共10张。\r\n"
        "echo tail_to_head：同一根棒绕A/D轴物理调头180度，共10张；不是相机反扫。\r\n"
        "echo 四面同面双相机状态必须稳定并命中标定包络；不稳定会拒绝出模，禁止改低门限。\r\n"
        "echo 程序不生成最终8角补偿，不猜生产棒R/L，不会自动放行缺证据模型。\r\n"
        "echo.\r\n"
        "for %%D in (head_to_tail tail_to_head) do (\r\n"
        "  if not exist \"%ROOT%\\%%D\\*.hobj\" (echo ERROR: %%D 中没有HOBJ & pause & exit /b 2)\r\n"
        ")\r\n"
        "pause\r\n"
        "\"%~dp0EndfaceCalibrator\\EndfaceCalibrator.exe\" --input \"%ROOT%\" --camera-calibration \"%CAMERA%\" --output-model \"%OUTPUT%\" --diagnostics-csv \"%DIAG%\" --min-captures-per-direction 10 --expected-captures-per-direction 10 --holdout-fraction 0.20\r\n"
        "if errorlevel 1 (echo. & echo 标定被安全门拒绝；请查看屏幕原因和诊断CSV。 & pause & exit /b 3)\r\n"
        "echo.\r\n"
        "echo 模型生成成功：%OUTPUT%\r\n"
        "echo 诊断CSV：%DIAG%\r\n"
        "echo 只有模型JSON中 release_readiness.ready=true 才能进入正式离线包。\r\n"
        "pause\r\n"
    )


def camera_state_batch() -> str:
    return (
        "@echo off\r\n"
        "chcp 65001 >nul\r\n"
        "setlocal EnableExtensions\r\n"
        "cd /d \"%~dp0\"\r\n"
        "set \"ROOT=%~dp0calibration_hobj\\210_105\"\r\n"
        "set \"CAMERA=%~dp0calibration\\models\\camera_calibration_model_210_105.json\"\r\n"
        "set \"REPORT=%~dp0output\\camera_state_preflight.csv\"\r\n"
        "set \"FOUND=\"\r\n"
        "for /r \"%ROOT%\" %%F in (*.hobj) do set \"FOUND=1\"\r\n"
        "if not defined FOUND (echo ERROR: 四个采集目录中还没有HOBJ & pause & exit /b 2)\r\n"
        "echo 本检查只读取HOBJ图像和相机坐标模型，不读取机构真值、不生成补偿或端面模型。\r\n"
        "\"%~dp0EndfaceCalibrator\\EndfaceCalibrator.exe\" --input \"%ROOT%\" --camera-calibration \"%CAMERA%\" --camera-state-only --camera-state-report-csv \"%REPORT%\"\r\n"
        "set \"CODE=%ERRORLEVEL%\"\r\n"
        "echo.\r\n"
        "echo 预检CSV：%REPORT%\r\n"
        "if not \"%CODE%\"==\"0\" echo 机械/相机状态未通过；先处理结构并重新拍图，禁止降低门限。\r\n"
        "if \"%CODE%\"==\"0\" echo 当前目录内所有HOBJ均通过纯图像稳定性预检。\r\n"
        "pause\r\n"
        "exit /b %CODE%\r\n"
    )


def write_resources() -> None:
    models = PACKAGE_DIR / "calibration" / "models"
    truth = PACKAGE_DIR / "calibration" / "truth"
    models.mkdir(parents=True, exist_ok=True)
    truth.mkdir(parents=True, exist_ok=True)
    shutil.copy2(
        TOOLS / "calibration" / "models" / "camera_calibration_model_210_105.json",
        models,
    )
    shutil.copy2(
        TOOLS / "calibration" / "truth" / "210_105_institution_report.csv",
        truth,
    )
    visualization = TOOLS / "calibration" / "truth" / "三坐标报告4.23_数据可视化.md"
    if visualization.is_file():
        shutil.copy2(visualization, truth)
    root = PACKAGE_DIR / "calibration_hobj" / "210_105"
    for name in ("head_to_tail", "tail_to_head"):
        (root / name).mkdir(parents=True, exist_ok=True)
        (root / name / "请把HOBJ放在这里.txt").write_text(
            "本目录只接受同一根专业标准棒的HOBJ；不要混入其他棒。\r\n",
            encoding="utf-8-sig",
        )
    (PACKAGE_DIR / "output").mkdir(parents=True, exist_ok=True)
    (PACKAGE_DIR / "生成端面物理模型.bat").write_text(
        calibration_batch(), encoding="utf-8-sig"
    )
    (PACKAGE_DIR / "先检查相机机械状态.bat").write_text(
        camera_state_batch(), encoding="utf-8-sig"
    )
    readme = """端面物理标定采集工具包（不是测量软件）
========================================

本工具包只负责在无网现场生成带共享16角验证的物理v13+候选模型和诊断CSV，不包含产品测量程序，
也不包含任何旧v8最终角度补偿模型。

必须使用同一根专业标准棒：
0. 每次先放入1～3张试拍HOBJ并双击“先检查相机机械状态.bat”；全部稳定后再正式采集。
1. 标准棒未调头时连续拍10次，固定设备行头对应物理R端，放入 head_to_tail。
2. 同一根棒绕A/D轴180°物理调头后再连续拍10次，放入 tail_to_head。
   两组不是相机正扫/反扫；HOBJ行号始终是固定设备空间行序。
3. 双击“生成端面物理模型.bat”。前8+8张拟合，后2+2张只做盲测和真实调头等变验证。

标定器会先检查四个物理面的同面双相机拼接状态。任一可用图不能证明相机状态稳定，
都会生成rejected审计并拒绝出模；不要为了通过而降低门限或手改JSON。现有旧20张图已
在该门禁下被拒绝，需要机械状态稳定后重新采集。

机构定义固定：R=物理头、L=物理尾；A上、B右、C左、D下。
调头的物理规律固定为：设备head/tail交换、机构B/C交换、A/D不变，
有向角满足 theta_after = 180° - theta_before。程序不会用文件夹名交换结果，
不会用机构真值猜生产棒方向，也不会生成8个最终角度补偿。

只有 output 中模型的 release_readiness.ready=true 才可拷回项目并构建正式测量包；
否则必须保留诊断CSV并重新采集，禁止手工改JSON放行。
"""
    (PACKAGE_DIR / "使用说明.txt").write_text(
        readme.replace("\n", "\r\n"), encoding="utf-8-sig"
    )


def main() -> None:
    if shutil.which("pyinstaller") is None and importlib.util.find_spec("PyInstaller") is None:
        raise SystemExit("PyInstaller is required in the build environment")
    shutil.rmtree(BUILD, ignore_errors=True)
    shutil.rmtree(PACKAGE_DIR, ignore_errors=True)
    ZIP_PATH.unlink(missing_ok=True)
    dist = BUILD / "dist"
    work = BUILD / "work"
    spec = BUILD / "spec"
    run_pyinstaller(
        [
            "--onedir",
            "--name",
            "EndfaceCalibrator",
            "--distpath",
            str(dist),
            "--workpath",
            str(work),
            "--specpath",
            str(spec),
            "--collect-all",
            "PIL",
            str(TOOLS / "endface_calibrator.py"),
        ]
    )
    RELEASES.mkdir(exist_ok=True)
    PACKAGE_DIR.mkdir(parents=True)
    shutil.move(str(dist / "EndfaceCalibrator"), str(PACKAGE_DIR / "EndfaceCalibrator"))
    write_resources()
    shutil.make_archive(str(ZIP_PATH.with_suffix("")), "zip", RELEASES, PACKAGE_NAME)
    print(f"Calibration toolkit: {PACKAGE_DIR}")
    print(f"Zip archive: {ZIP_PATH}")


if __name__ == "__main__":
    main()
