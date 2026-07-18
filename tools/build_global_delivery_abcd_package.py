#!/usr/bin/env python3
"""Build the global Web package with delivered cross-section geometry metrics."""

from __future__ import annotations

import importlib.util
import hashlib
import json
import os
import shutil
import subprocess
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parent.parent
TOOLS = ROOT / "tools"
BUILD = ROOT / "build" / "global_delivery_geometry_package"
RELEASES = ROOT / "release"
# A future build must be a new artifact, never an overwrite of the validated
# 20260715/20260716 end-face delivery currently in release/.
PACKAGE_NAME = "SunshineSiRod_Global_DeliveryGeometry_20260715_EndfaceInteriorAngle_20260716_EdgeDiagonal_20260716"
PACKAGE_DIR = RELEASES / PACKAGE_NAME
ZIP_PATH = RELEASES / f"{PACKAGE_NAME}.zip"
BASE_PACKAGE_ZIP = RELEASES / "SunshineSiRod_Global_DeliveryGeometry_20260715_EndfaceInteriorAngle_20260716.zip"
BASE_PACKAGE_SHA256 = "0D815364FBAE9DBC43A224BF5BF301BA9436B1234D38D63823BF11A2B135A26B"
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
EDGE_DIAGONAL_MODULES = ("__init__.py", "core.py")

# These are source inputs only.  This script is intentionally not invoked as
# part of the merge; it merely describes the future package layout.
WORKSPACE_COLLEAGUE_ROOT = ROOT / "同事交付包代码"
DELIVERY_ROOT = (
    WORKSPACE_COLLEAGUE_ROOT
    / "方棒尺寸几何算法交接包_对角线弧长侧边夹角_20260715"
    / "方棒尺寸几何算法交接包_对角线弧长侧边夹角_20260715"
)
EDGE_DIAGONAL_ROOT = WORKSPACE_COLLEAGUE_ROOT / "方棒边长对角线工程算法交接包_20260716"


def run_pyinstaller(args: list[str]) -> None:
    subprocess.run(
        [sys.executable, "-m", "PyInstaller", "--noconfirm", "--clean", *args],
        check=True,
        cwd=ROOT,
    )


def sha256_file(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for block in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest().upper()


def prepare_stage(validated_base: Path) -> Path:
    stage = BUILD / "stage"
    delivery_source = stage / "delivery_src"
    delivery_calibration = stage / "calibration" / "delivery_geometry"
    edge_diagonal_source = stage / "edge_diagonal_src" / "square_bar"
    edge_diagonal_calibration = stage / "calibration" / "edge_diagonal"
    delivery_source.mkdir(parents=True)
    delivery_calibration.mkdir(parents=True)
    edge_diagonal_source.mkdir(parents=True)
    edge_diagonal_calibration.mkdir(parents=True)
    dashboard_source = validated_base / "MeasurementDashboard" / "_internal" / "dashboard"
    dashboard_stage = stage / "dashboard"
    if not dashboard_source.is_dir():
        raise SystemExit(f"Validated dashboard assets are missing: {dashboard_source}")
    shutil.copytree(dashboard_source, dashboard_stage)
    dashboard_app = dashboard_stage / "app.js"
    dashboard_text = dashboard_app.read_text(encoding="utf-8")
    old_poll = "setInterval(loadLatestResult, 3000);"
    if old_poll not in dashboard_text:
        raise SystemExit("Validated dashboard does not contain the expected 3-second result poll")
    dashboard_app.write_text(
        dashboard_text.replace(old_poll, "setInterval(loadLatestResult, 1000);"),
        encoding="utf-8",
    )

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

    for name in EDGE_DIAGONAL_MODULES:
        source = EDGE_DIAGONAL_ROOT / "src" / "square_bar" / name
        if not source.is_file():
            raise SystemExit(f"Delivered 20260716 edge/diagonal module is missing: {source}")
        shutil.copy2(source, edge_diagonal_source / name)
    for name in ("current_calibration.json", "global_exchange_calibration.json"):
        source = EDGE_DIAGONAL_ROOT / "calibration" / name
        if not source.is_file():
            raise SystemExit(f"Delivered 20260716 edge/diagonal calibration is missing: {source}")
        shutil.copy2(source, edge_diagonal_calibration / name)
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


def write_package_metadata() -> None:
    manifest = {
        "package": PACKAGE_NAME,
        "integration_scope": "20260716_edges_diagonals_with_existing_auxiliary_geometry_and_endface",
        "edge_diagonal_component": {
            "runtime": "MeasureDeliveryGeometry/MeasureDeliveryGeometry.exe",
            "calibration": "calibration/edge_diagonal/current_calibration.json",
            "exchange_calibration": "calibration/edge_diagonal/global_exchange_calibration.json",
            "source": "方棒边长对角线工程算法交接包_20260716",
            "final_fields": ["A_mm", "B_mm", "C_mm", "D_mm", "diag1_M1_M2_mm", "diag2_M3_M4_mm"],
            "aggregation": "whole_bar_10_percent_two_sided_trimmed_mean",
        },
        "unchanged_auxiliary_geometry": [
            "corner_main_face_angles",
            "chamfer_chords",
            "chamfer_projections",
            "stick_length",
        ],
        "endface_component": {
            "runtime": "MeasureCoworkerEndface/MeasureCoworkerEndface.exe",
            "calibration": "coworker_endface_delivery/calibration/current_calibration.json",
            "raw_angle_definition": "material_interior_half_plane_dihedral_0_to_180_deg",
            "turnover_inference": "none",
            "final_formula": "reported_deg = 90 + 0.5 * (raw_deg - 90)",
            "final_angle_count": 8,
        },
        "statistics": {
            "metadata_fields": ["measured_at", "bar_id", "capture_id", "input_path"],
            "column_count": 37,
        },
        "timing": {
            "continuous_scan_seconds": 1,
            "stable_file_seconds": 2,
            "web_result_poll_seconds": 1,
            "target_end_to_end_seconds": "about 20",
        },
        "web_component": "MeasurementDashboard",
        "sunshine_native_measurement_algorithm_included": False,
    }
    (PACKAGE_DIR / "package_manifest.json").write_text(
        json.dumps(manifest, ensure_ascii=False, indent=2) + "\n",
        encoding="utf-8",
    )
    readme = """同事算法独立全局测量现场包（边长/对角线 2026-07-16 版）
========================================================

1. 必须完整解压到一个全新的空目录，建议使用较短路径，例如 D:\\SiRod。
2. 双击 start_dashboard.bat，浏览器地址为 http://127.0.0.1:8766。
3. 最终 A/B/C/D 和两条对角线只使用“方棒边长对角线工程算法交接包_20260716”：整棒有效截面排序后，两端各裁剪10%，对剩余截面取均值，并应用交付标定。
4. 四角主面夹角、倒角 chord、双投影和棒长仍使用原 20260715 尺寸几何交付算法，未替换。
5. 端面仍由 MeasureCoworkerEndface 计算，算法、EXE 和标定不变；每张 HOBJ 均按同一套材料内部半平面几何计算，不根据目录名或文件名交换头尾/面号。
6. Web 和统计 CSV 中的8个端面值仍按 reported = 90 + 0.5 × (raw - 90) 输出。
7. 连续目录检查间隔为1秒，文件稳定等待为2秒，Web结果轮询为1秒；目标约20秒显示新结果，实际时间取决于电脑和HOBJ大小。
8. 用户统计文件为 results\\measurements\\user_results\\measurement_statistics.csv，固定37列：4个追溯字段、25个旧表字段名的尺寸字段、8个当前端面字段。
9. 研发明细和同事程序原始JSON保存在 results\\measurements\\developer_details。
10. 本包不调用 SunshineSiRod 原生尺寸、端面、M0/M1/M2或机械漂移算法。

注意：更换规格、相机布局、标定或扫描范围后必须重新验证，禁止直接套用当前标定。
"""
    (PACKAGE_DIR / "README_现场部署说明.txt").write_text(readme, encoding="utf-8")


def main() -> None:
    if shutil.which("pyinstaller") is None and importlib.util.find_spec("PyInstaller") is None:
        raise SystemExit("PyInstaller is required in the build environment")
    shutil.rmtree(BUILD, ignore_errors=True)
    shutil.rmtree(PACKAGE_DIR, ignore_errors=True)
    ZIP_PATH.unlink(missing_ok=True)
    if not BASE_PACKAGE_ZIP.is_file():
        raise SystemExit(f"Validated base package is missing: {BASE_PACKAGE_ZIP}")
    actual_base_sha256 = sha256_file(BASE_PACKAGE_ZIP)
    if actual_base_sha256 != BASE_PACKAGE_SHA256:
        raise SystemExit(
            "Validated base package SHA256 mismatch: "
            f"expected {BASE_PACKAGE_SHA256}, got {actual_base_sha256}"
        )
    unpack_root = BUILD / "validated_base"
    shutil.unpack_archive(str(BASE_PACKAGE_ZIP), str(unpack_root), "zip")
    base_candidates = [
        path for path in unpack_root.iterdir()
        if path.is_dir() and (path / "MeasurementDashboard").is_dir()
    ]
    if len(base_candidates) != 1:
        raise SystemExit("Validated base package must contain exactly one package root")
    validated_base = base_candidates[0]
    stage = prepare_stage(validated_base)
    dist = BUILD / "dist"
    work = BUILD / "work"
    spec = BUILD / "spec"
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
            "--paths",
            str(stage / "edge_diagonal_src"),
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
            f"{stage / 'dashboard'}{os.pathsep}dashboard",
            str(TOOLS / "measurement_dashboard.py"),
        ]
    )
    RELEASES.mkdir(exist_ok=True)
    shutil.copytree(validated_base, PACKAGE_DIR)
    shutil.rmtree(PACKAGE_DIR / "MeasureDeliveryGeometry")
    shutil.move(str(dist / "MeasureDeliveryGeometry"), str(PACKAGE_DIR / "MeasureDeliveryGeometry"))
    shutil.rmtree(PACKAGE_DIR / "MeasurementDashboard")
    shutil.move(str(dist / "MeasurementDashboard"), str(PACKAGE_DIR / "MeasurementDashboard"))
    shutil.copytree(stage / "calibration", PACKAGE_DIR / "calibration", dirs_exist_ok=True)
    dashboard_config = PACKAGE_DIR / "MeasurementDashboard" / "web_ui_config.local.json"
    dashboard_config.write_text(
        json.dumps(
            {
                "continuous_scan_seconds": 1,
                "stable_file_seconds": 2,
            },
            ensure_ascii=False,
            indent=2,
        ),
        encoding="utf-8",
    )
    write_package_metadata()
    shutil.make_archive(str(ZIP_PATH.with_suffix("")), "zip", RELEASES, PACKAGE_NAME)
    print(f"Package directory: {PACKAGE_DIR}")
    print(f"Zip archive: {ZIP_PATH}")


if __name__ == "__main__":
    main()
