#!/usr/bin/env python3
"""Build the locked, truth-free M0 Raw end-face trial package."""

from __future__ import annotations

import hashlib
import json
import os
import shutil
import subprocess
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parent.parent
TOOLS = ROOT / "tools"
BUILD = ROOT / "build" / "endface_m0_raw_trial"
RELEASE = ROOT / "release"
PACKAGE_NAME = "SunshineSiRod_Endface_M0_Raw_Trial"
PACKAGE_DIR = RELEASE / PACKAGE_NAME
ZIP_PATH = RELEASE / f"{PACKAGE_NAME}.zip"
CAMERA_MODEL = TOOLS / "calibration" / "models" / "camera_calibration_model_210_105.json"
BATCH_AUDIT_DIR = TOOLS / "results" / "endface_m0_raw_trial" / "batch_all_40"
BATCH_AUDIT_FILES = (
    "endface_raw_batch_summary.json",
    "endface_raw_batch_measurements.csv",
    "endface_raw_turnover_equivariance.csv",
)


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for block in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def run_pyinstaller(arguments: list[str]) -> None:
    subprocess.run(
        [sys.executable, "-m", "PyInstaller", "--noconfirm", "--clean", *arguments],
        check=True,
        cwd=ROOT,
    )


def validate_inputs() -> dict[str, object]:
    camera = json.loads(CAMERA_MODEL.read_text(encoding="utf-8"))
    if int(camera.get("version", 0)) < 7 or not str(camera.get("model", "")).startswith(
        "camera_oriented_transform"
    ):
        raise ValueError(f"Unsupported camera coordinate model: {CAMERA_MODEL}")
    audit_path = BATCH_AUDIT_DIR / "endface_raw_batch_summary.json"
    audit = json.loads(audit_path.read_text(encoding="utf-8"))
    if (
        audit.get("algorithm") != "M0_raw_truth_free_16_local_endface_angles"
        or int(audit.get("capture_count", 0)) != 40
        or audit.get("uses_professional_truth") is not False
        or audit.get("mechanical_drift_model_loaded") is not False
        or audit.get("endface_correction_model_loaded") is not False
        or audit.get("final_angle_correction_applied") is not False
    ):
        raise ValueError(f"M0 Raw 40-capture audit is incomplete or unsafe: {audit_path}")
    for name in BATCH_AUDIT_FILES:
        if not (BATCH_AUDIT_DIR / name).is_file():
            raise FileNotFoundError(BATCH_AUDIT_DIR / name)
    return audit


def write_launchers() -> None:
    start = (
        "@echo off\r\n"
        "chcp 65001 >nul\r\n"
        "setlocal\r\n"
        "cd /d \"%~dp0\"\r\n"
        "set SUNSHINE_ENDFACE_ONLY=1\r\n"
        "set SUNSHINE_ENDFACE_RAW_ONLY=1\r\n"
        "set SUNSHINE_DASHBOARD_PORT=8767\r\n"
        "taskkill /F /IM MeasurementDashboard.exe >nul 2>&1\r\n"
        "timeout /t 1 /nobreak >nul\r\n"
        "start \"Sunshine M0 Raw Dashboard\" /b \"%~dp0MeasurementDashboard\\MeasurementDashboard.exe\"\r\n"
        "timeout /t 2 /nobreak >nul\r\n"
        "start \"\" \"http://127.0.0.1:8767/?mode=m0-raw-trial\"\r\n"
    )
    stop = (
        "@echo off\r\n"
        "chcp 65001 >nul\r\n"
        "taskkill /F /IM MeasurementDashboard.exe >nul 2>&1\r\n"
        "echo M0 Raw Web 已停止。\r\n"
        "pause\r\n"
    )
    (PACKAGE_DIR / "启动端面M0_Raw试运行.bat").write_text(start, encoding="utf-8-sig")
    (PACKAGE_DIR / "停止端面M0_Raw试运行.bat").write_text(stop, encoding="utf-8-sig")


def write_readme(audit: dict[str, object]) -> None:
    datasets = audit.get("datasets", {})
    readme = f"""端面垂直度 M0 Raw 现场试运行包
================================

一、这是什么
- 本包固定使用 M0 Raw：从当前HOBJ重新计算每端8个局部角，共16个角。
- 每端代表角是偏离90度最大的那个真实局部角，不是四面平均值。
- 本包不加载机械漂移模型、端面修正模型、机构端面真值或人工端面真值。
- 本包是现场试运行版，不是已证明绝对精度的正式放行版。

二、怎么使用
1. 把待测HOBJ放入 hobj 文件夹，也可以在Web设置中选择其他目录。
2. 双击“启动端面M0_Raw试运行.bat”。
3. 浏览器标题必须为 End-face Perpendicularity Dashboard。
4. 页面模式必须显示 RAW audit: unadjusted / Raw审计：无修正。
5. 选择HOBJ并测量；结果保存在 results\\measurements。

三、质量状态
- pass：端面平面残差不超过0.50 mm，且同面双相机拼接变化不超过0.60 mm。
- uncertain：拼接变化大于0.60 mm但小于0.80 mm；保留全部Raw值。
- warning：拼接变化达到0.80 mm；无法区分机械变化与真实面非平面，保留全部Raw值并告警。
- rejected：任一端面平面RMSE超过0.50 mm或端面拟合无效；Raw值仅用于审计，不应作为正式结论。

四、40张既有HOBJ回归
- 数据组数量：{audit.get('dataset_count')}
- HOBJ总数：{audit.get('capture_count')}
- 专业210x105组：19张告警、1张已知磕碰异常拒测。
- CTB长棒6张：全部通过；与待测目录中的CTB副本结果一致。
- 待测A61与CTT：各3张通过、1张告警。
- 详细结果见 audit 文件夹。

五、固定约束
- head/tail只表示HOBJ设备空间最小行端/最大行端，不推断物理R/L。
- 所有测量固定orientation=normal；调头必须由图像几何自然体现。
- ZIP内只有相机到公共空间的坐标模型，没有机械漂移模型和端面修正模型。
"""
    (PACKAGE_DIR / "现场使用说明.txt").write_text(readme, encoding="utf-8-sig")


def build_package() -> None:
    audit = validate_inputs()
    shutil.rmtree(BUILD, ignore_errors=True)
    shutil.rmtree(PACKAGE_DIR, ignore_errors=True)
    ZIP_PATH.unlink(missing_ok=True)
    dist = BUILD / "dist"
    work = BUILD / "work"
    spec = BUILD / "spec"
    data_separator = os.pathsep
    run_pyinstaller(
        [
            "--onedir",
            "--name",
            "MeasureSquareRod",
            "--distpath",
            str(dist),
            "--workpath",
            str(work),
            "--specpath",
            str(spec),
            "--collect-all",
            "PIL",
            str(TOOLS / "measure_endface_raw.py"),
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
    RELEASE.mkdir(exist_ok=True)
    PACKAGE_DIR.mkdir(parents=True)
    shutil.move(str(dist / "MeasureSquareRod"), str(PACKAGE_DIR / "MeasureSquareRod"))
    shutil.move(
        str(dist / "MeasurementDashboard"),
        str(PACKAGE_DIR / "MeasurementDashboard"),
    )
    models = PACKAGE_DIR / "calibration" / "models"
    models.mkdir(parents=True)
    shutil.copy2(CAMERA_MODEL, models / CAMERA_MODEL.name)
    audit_dir = PACKAGE_DIR / "audit"
    audit_dir.mkdir()
    for name in BATCH_AUDIT_FILES:
        shutil.copy2(BATCH_AUDIT_DIR / name, audit_dir / name)
    (PACKAGE_DIR / "hobj").mkdir()
    (PACKAGE_DIR / "results" / "measurements").mkdir(parents=True)
    write_launchers()
    write_readme(audit)
    manifest = {
        "package": PACKAGE_NAME,
        "mode": "M0_raw_audit_locked",
        "trial_only": True,
        "uses_professional_endface_truth": False,
        "mechanical_drift_model_included": False,
        "endface_correction_model_included": False,
        "final_angle_correction_applied": False,
        "camera_coordinate_model": {
            "path": f"calibration/models/{CAMERA_MODEL.name}",
            "sha256": sha256(CAMERA_MODEL),
        },
        "regression_capture_count": audit.get("capture_count"),
        "quality_limits_mm": {
            "raw_endface_plane_rmse_reject": 0.50,
            "same_face_seam_stable": 0.60,
            "same_face_seam_warning": 0.80,
        },
    }
    (PACKAGE_DIR / "package_manifest.json").write_text(
        json.dumps(manifest, ensure_ascii=False, indent=2),
        encoding="utf-8",
    )
    model_names = [path.name for path in models.iterdir() if path.is_file()]
    if model_names != [CAMERA_MODEL.name]:
        raise RuntimeError(f"Unexpected packaged model files: {model_names}")
    shutil.make_archive(str(ZIP_PATH.with_suffix("")), "zip", RELEASE, PACKAGE_NAME)
    print(f"Package directory: {PACKAGE_DIR}")
    print(f"Zip archive: {ZIP_PATH}")


if __name__ == "__main__":
    build_package()
