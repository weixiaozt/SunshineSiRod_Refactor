#!/usr/bin/env python3
"""Build the fully offline end-face-only Windows package."""

from __future__ import annotations

import os
import json
import math
import shutil
import subprocess
import sys
from pathlib import Path

try:
    from .endface_wireframe_geometry import has_valid_wireframe_release_evidence
except ImportError:  # Direct script / PyInstaller entry point.
    from endface_wireframe_geometry import has_valid_wireframe_release_evidence


ROOT = Path(__file__).resolve().parent.parent
TOOLS = ROOT / "tools"
BUILD = ROOT / "build" / "endface_offline_package"
RELEASES = ROOT / "release"
PACKAGE_NAME = "SunshineSiRod_Endface"
PACKAGE_DIR = RELEASES / PACKAGE_NAME
ZIP_PATH = RELEASES / f"{PACKAGE_NAME}.zip"


def run_pyinstaller(args: list[str]) -> None:
    subprocess.run([sys.executable, "-m", "PyInstaller", "--noconfirm", "--clean", *args], check=True, cwd=ROOT)


def validated_endface_model_source() -> Path:
    endface_source = TOOLS / "calibration" / "models" / "endface_calibration_model_210_105.json"
    endface_data = json.loads(endface_source.read_text(encoding="utf-8"))
    prohibited = {"angle_offsets_deg", "orientation_models", "orientation_detector"}
    if (
        int(endface_data.get("version", 0)) < 15
        or endface_data.get("valid") is not True
        or endface_data.get("model") != "endface_camera_geometry_calibration"
        or endface_data.get("strategy")
        != "physical_decomposed_camera_scan_row_synchronization"
        or endface_data.get("runtime_orientation_detection") != "none"
        or endface_data.get("self_consistency_selected_model_level") != "M1"
        or endface_data.get("runtime_selected_model_level") != "M1"
        or endface_data.get("runtime_correction_applied") is not True
        or endface_data.get("runtime_endpoint_label_contract")
        != {
            "head": "hobj_device_row_min",
            "tail": "hobj_device_row_max",
            "physical_R_L_inferred": False,
            "reason": "HOBJ contains no physical endpoint identity metadata",
        }
        or endface_data.get("report_to_software_face_map")
        != {"A": "A", "B": "B", "C": "D", "D": "C"}
        or not isinstance(endface_data.get("y_coordinate_correction"), dict)
        or endface_data["y_coordinate_correction"].get("method")
        != "per_camera_scan_row_offset"
        or endface_data["y_coordinate_correction"].get("apply_to")
        != "all_longitudinal_side_points_and_end_boundary_points"
        or not isinstance(endface_data.get("release_readiness"), dict)
        or endface_data["release_readiness"].get("ready") is not True
        or prohibited.intersection(endface_data)
    ):
        raise ValueError(
            f"A valid no-answer image-only physical v15+ M1 end-face model with complete Y correction and a camera-state gate is required before packaging: {endface_source}"
        )
    if not has_valid_wireframe_release_evidence(endface_data):
        raise ValueError(
            f"A passing image-only shared-geometry 16-angle v15 M1 validation is required before packaging: {endface_source}"
        )
    offsets = endface_data.get("camera_y_offsets_mm")
    if not isinstance(offsets, dict):
        raise ValueError(f"Physical end-face model has no camera Y offsets: {endface_source}")
    try:
        values = [float(offsets[str(obj)]) for obj in (1, 2, 3, 4)]
    except (KeyError, TypeError, ValueError) as exc:
        raise ValueError(f"Physical end-face model has incomplete camera Y offsets: {endface_source}") from exc
    if (
        any(not math.isfinite(value) or abs(value) > 1.5 for value in values)
        or abs(sum(values)) > 1e-6
    ):
        raise ValueError(f"Physical end-face model has invalid camera Y offsets: {endface_source}")
    state_reference = endface_data.get("calibration_state_reference")
    if not (
        isinstance(state_reference, dict)
        and state_reference.get("method")
        == "same_physical_face_dual_camera_line_separation_no_truth"
        and state_reference.get("fit_target")
        == "image_geometry_applicability_only_no_correction"
        and state_reference.get("all_captures_stable") is True
        and state_reference.get("complete") is True
        and isinstance(state_reference.get("faces"), dict)
    ):
        raise ValueError(
            f"Physical end-face model has no stable image-only camera-state reference: {endface_source}"
        )
    for face in "ABCD":
        envelope = state_reference["faces"].get(face)
        if not isinstance(envelope, dict) or envelope.get("valid") is not True:
            raise ValueError(
                f"Physical end-face model has an incomplete {face}-face state envelope: {endface_source}"
            )
        try:
            limits = [
                float(envelope[name])
                for name in (
                    "seam_median_min_mm",
                    "seam_median_max_mm",
                    "seam_span_max_mm",
                    "pair_angle_p90_max_deg",
                )
            ]
        except (KeyError, TypeError, ValueError) as exc:
            raise ValueError(
                f"Physical end-face model has an invalid {face}-face state envelope: {endface_source}"
            ) from exc
        if any(not math.isfinite(value) for value in limits):
            raise ValueError(
                f"Physical end-face model has a non-finite {face}-face state envelope: {endface_source}"
            )
    basis = endface_data.get("physical_mode_basis")
    fit_details = endface_data.get("fit_details")
    if not isinstance(basis, dict) or not isinstance(fit_details, dict):
        raise ValueError(f"Physical end-face model has no decomposed-mode provenance: {endface_source}")
    try:
        modes = [
            [float(value) for value in basis[name]]
            for name in ("affine_x_mode", "affine_z_mode", "nonplanar_mode")
        ]
    except (KeyError, TypeError, ValueError) as exc:
        raise ValueError(f"Physical end-face model has an invalid mode basis: {endface_source}") from exc
    if any(
        len(mode) != 4
        or any(not math.isfinite(value) for value in mode)
        or abs(sum(mode)) > 1e-6
        for mode in modes
    ):
        raise ValueError(f"Physical end-face model has an invalid mode basis: {endface_source}")
    selected_fit = fit_details.get("selected")
    if (
        not isinstance(selected_fit, dict)
        or selected_fit.get("method")
        != "single_nonplanar_camera_scan_row_synchronization"
        or selected_fit.get("model_level") != "M1"
        or selected_fit.get("affine_modes", {}).get("enabled") is not False
    ):
        raise ValueError(f"Physical end-face model has invalid fit provenance: {endface_source}")
    return endface_source


def copy_resources() -> None:
    models = PACKAGE_DIR / "calibration" / "models"
    models.mkdir(parents=True, exist_ok=True)
    # Camera geometry is required to express the four local camera profiles in
    # one coordinate system.  Only the no-answer physical v15 M1 model may be
    # packaged; a rejected candidate or legacy final-angle model fails closed.
    shutil.copy2(TOOLS / "calibration" / "models" / "camera_calibration_model_210_105.json", models)
    drift_source = TOOLS / "calibration" / "models" / "mechanical_drift_model_210_105.json"
    drift_data = json.loads(drift_source.read_text(encoding="utf-8"))
    if drift_data.get("model") != "four_camera_longitudinal_local_drift":
        raise ValueError(f"A valid independent mechanical drift model is required: {drift_source}")
    shutil.copy2(drift_source, models)
    endface_source = validated_endface_model_source()
    shutil.copy2(endface_source, models)
    visualization = TOOLS / "calibration" / "truth" / "三坐标报告4.23_数据可视化.md"
    if visualization.is_file():
        shutil.copy2(visualization, truth / visualization.name)


def calibration_batch() -> str:
    return (
        "@echo off\r\n"
        "chcp 65001 >nul\r\n"
        "setlocal EnableExtensions\r\n"
        "cd /d \"%~dp0\"\r\n"
        "echo ================================================\r\n"
        "echo  现场生成端面物理标定模型（完全离线）\r\n"
        "echo ================================================\r\n"
        "set \"SPEC=210_105\"\r\n"
        "set \"SAMPLE=BP411B116321XTO\"\r\n"
        "set \"HEAD_LABEL=R\"\r\n"
        "set \"HOBJ_ROOT=%~dp0calibration_hobj\\%SPEC%\"\r\n"
        "set \"TRUTH=%~dp0calibration\\truth\\%SPEC%_institution_report.csv\"\r\n"
        "set \"CAMERA=%~dp0calibration\\models\\camera_calibration_model_210_105.json\"\r\n"
        "set \"OUTPUT=%~dp0calibration\\models\\endface_calibration_model_%SPEC%.json\"\r\n"
        "set \"DIAG=%~dp0calibration\\models\\endface_calibration_model_%SPEC%_diagnostics.csv\"\r\n"
        "echo.\r\n"
        "echo 标定棒固定：R=物理头，L=物理尾；机构面号 A上 B右 C左 D下。\r\n"
        "echo head_to_tail：标准棒未调头，设备行头对应物理R端，共10张。\r\n"
        "echo tail_to_head：同一根棒绕A/D轴物理调头180度，共10张；不是相机反扫。\r\n"
        "echo HOBJ始终按固定设备空间行序审计；第二组按真实调头规律映射机构真值。\r\n"
        "echo 模型只允许拟合低层相机扫描行同步，不生成8个最终角度偏移，不猜生产棒R/L。\r\n"
        "echo.\r\n"
        "if not exist \"%HOBJ_ROOT%\\head_to_tail\\*.hobj\" (echo ERROR: head_to_tail 中没有HOBJ & pause & exit /b 2)\r\n"
        "if not exist \"%HOBJ_ROOT%\\tail_to_head\\*.hobj\" (echo ERROR: tail_to_head 中没有HOBJ & pause & exit /b 2)\r\n"
        "pause\r\n"
        "\"%~dp0EndfaceCalibrator\\EndfaceCalibrator.exe\" --input \"%HOBJ_ROOT%\" --truth-csv \"%TRUTH%\" --camera-calibration \"%CAMERA%\" --output-model \"%OUTPUT%\" --diagnostics-csv \"%DIAG%\" --sample-id \"%SAMPLE%\" --head-label \"%HEAD_LABEL%\" --min-captures-per-direction 10 --expected-captures-per-direction 10 --holdout-fraction 0.20\r\n"
        "if errorlevel 1 (echo. & echo 模型生成失败；旧模型不会被保留，请查看拒绝原因和诊断CSV。 & pause & exit /b 3)\r\n"
        "echo.\r\n"
        "echo 物理模型生成成功：%OUTPUT%\r\n"
        "echo 诊断数据：%DIAG%\r\n"
        "pause\r\n"
    )


def write_launchers_and_readme() -> None:
    spec = "210_105"
    (PACKAGE_DIR / "calibration_hobj" / spec / "head_to_tail").mkdir(parents=True, exist_ok=True)
    (PACKAGE_DIR / "calibration_hobj" / spec / "tail_to_head").mkdir(parents=True, exist_ok=True)
    (PACKAGE_DIR / "hobj").mkdir(exist_ok=True)
    (PACKAGE_DIR / "results" / "measurements").mkdir(parents=True, exist_ok=True)
    (PACKAGE_DIR / "start_endface_dashboard.bat").write_text(
        "@echo off\r\n"
        "set SUNSHINE_ENDFACE_ONLY=1\r\n"
        "set SUNSHINE_DASHBOARD_PORT=8767\r\n"
        "taskkill /F /IM MeasurementDashboard.exe >nul 2>&1\r\n"
        "timeout /t 1 /nobreak >nul\r\n"
        "start \"Sunshine Endface Measurement\" /b \"%~dp0MeasurementDashboard\\MeasurementDashboard.exe\"\r\n"
        "timeout /t 2 /nobreak >nul\r\n"
        "start \"Sunshine Endface Measurement\" \"http://127.0.0.1:8767/?mode=endface-physical-v13\"\r\n",
        encoding="utf-8",
    )
    (PACKAGE_DIR / "现场生成端面物理模型.bat").write_text(
        calibration_batch(),
        encoding="utf-8-sig",
    )
    readme = """端面垂直度检测离线包
======================

重要：本包只允许通过共享16角盲测和交叉验证的物理 v13+ 模型。任何含8个最终角度偏移、方向分类器、答案匹配或
规格拉回的旧模型都会被程序拒绝。标定失败时不保留上一次活动模型。

一、现场标定
1. 只使用同一根专业标准棒，R为物理头、L为物理尾。
2. 标准棒未调头时连续拍10张，设备行头对应物理R端，放入
   calibration_hobj\\210_105\\head_to_tail。
3. 同一根棒绕A/D轴180°物理调头后再拍10张，放入
   calibration_hobj\\210_105\\tail_to_head；这不是相机反向扫描。
5. 双击“现场生成端面物理模型.bat”。前8+8张拟合，后2+2张留出验证。
6. HOBJ会按图像轨迹检查固定设备空间行序；不会因为文件夹名交换结果或真值。
7. 机构真值映射固定为头尾交换、B/C交换、A/D不变，有向角满足
   theta_after=180°-theta_before；不得再解释为相机往返扫描。

二、生产检测
1. 双击 start_endface_dashboard.bat，固定打开 http://127.0.0.1:8767。
2. 页面标题必须是 End-face Perpendicularity Dashboard。
3. 程序只在人工点击开始后连续处理新HOBJ。
4. head/tail固定表示HOBJ设备空间最小行/最大行，不是电机运动起点/终点；
   程序不会用标准棒答案猜物理R/L。
5. 逐面角度来自当前HOBJ端面边界点云、四个纵向侧平面和物理设备参数的重建。
6. 四面直接夹角平均天然接近90度，只是兼容显示值；不得单独拿它判断端面质量。
7. 切片CSV保留raw与物理修正审计字段；统计CSV仍为5个追溯字段加10个产品字段。

三、禁止事项
- 禁止使用旧v8端面包或旧 angle_offsets_deg 模型。
- 禁止把标定棒机构真值用于生产方向判断。
- 禁止为了出数而放宽机械稳定性、留出误差或完整性检查。
- 无法区分机械漂移和真实棒材形变时，必须告警或拒测。
"""
    (PACKAGE_DIR / "现场使用说明.txt").write_text(
        readme.replace("\n", "\r\n"),
        encoding="utf-8-sig",
    )


def main() -> None:
    # Fail before a costly PyInstaller build if the only available model is a
    # rejected candidate or a legacy answer-offset model.
    validated_endface_model_source()
    if shutil.which("pyinstaller") is None and not __import__("importlib.util").util.find_spec("PyInstaller"):
        raise SystemExit("PyInstaller is required in the build environment")
    shutil.rmtree(BUILD, ignore_errors=True)
    shutil.rmtree(PACKAGE_DIR, ignore_errors=True)
    ZIP_PATH.unlink(missing_ok=True)
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
        "--add-data", f"{TOOLS / 'dashboard'}{data_sep}dashboard", str(TOOLS / "measurement_dashboard.py"),
    ])
    run_pyinstaller([
        "--onedir", "--name", "EndfaceCalibrator", "--distpath", str(dist), "--workpath", str(work), "--specpath", str(spec),
        "--collect-all", "PIL", str(TOOLS / "endface_calibrator.py"),
    ])
    RELEASES.mkdir(exist_ok=True)
    PACKAGE_DIR.mkdir(parents=True)
    for name in ("MeasureSquareRod", "MeasurementDashboard", "EndfaceCalibrator"):
        shutil.move(str(dist / name), str(PACKAGE_DIR / name))
    copy_resources()
    write_launchers_and_readme()
    shutil.make_archive(str(ZIP_PATH.with_suffix("")), "zip", RELEASES, PACKAGE_NAME)
    print(f"Package directory: {PACKAGE_DIR}")
    print(f"Zip archive: {ZIP_PATH}")


if __name__ == "__main__":
    main()
