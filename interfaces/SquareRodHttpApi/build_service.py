#!/usr/bin/env python3
"""Build an independent SquareRod HTTP API production directory."""

from __future__ import annotations

import argparse
import json
import os
import shutil
import subprocess
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
API_ROOT = Path(__file__).resolve().parent
V7_ROOT = ROOT / "release" / "Square Rod Measurement0723_V7"
END_FACE_SOURCE = (
    ROOT
    / "release"
    / "Square Rod Measurement0720_V4"
    / "coworker_endface_delivery"
    / "src"
)


def locate_vendor_source() -> Path:
    candidates = [
        path.parent
        for path in (ROOT / "同事交付包代码").rglob(
            "measure_all_bars_ctb_slice_calibrated.py"
        )
        if "对角线弧长侧边夹角" in str(path)
    ]
    if len(candidates) != 1:
        raise RuntimeError(
            f"Expected exactly one vendor geometry source, found {len(candidates)}"
        )
    return candidates[0]


def copy_v7_runtime(target: Path) -> None:
    """Copy an immutable V7 runtime snapshot without writing to the V7 source."""

    required = (
        "MeasureUnified",
        "calibration",
        "coworker_endface_delivery",
        "package_manifest.json",
    )
    for relative in required:
        source = V7_ROOT / relative
        destination = target / relative
        if source.is_dir():
            shutil.copytree(source, destination)
        elif source.is_file():
            destination.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(source, destination)
        else:
            raise FileNotFoundError(f"Required V7 runtime component missing: {source}")


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "--build-root",
        required=True,
        help="A new empty directory used for PyInstaller output",
    )
    args = parser.parse_args()
    build_root = Path(args.build_root).resolve()
    if build_root.exists():
        raise FileExistsError(f"Build root must be new: {build_root}")
    build_root.mkdir(parents=True)

    vendor_source = locate_vendor_source()
    command = [
        sys.executable,
        "-m",
        "PyInstaller",
        "--noconfirm",
        "--clean",
        "--onedir",
        "--name",
        "SquareRodAlgorithmService",
        "--distpath",
        str(build_root / "dist"),
        "--workpath",
        str(build_root / "work"),
        "--specpath",
        str(build_root / "spec"),
        "--paths",
        str(API_ROOT),
        "--paths",
        str(ROOT / "tools"),
        "--paths",
        str(ROOT / "ABCD_length6图完美模板标定法" / "src"),
        "--paths",
        str(ROOT / "长棒倒角几何算法" / "src"),
        "--paths",
        str(ROOT / "长棒棒长计算算法" / "src"),
        "--paths",
        str(ROOT / "四角独立角度算法" / "src"),
        "--paths",
        str(END_FACE_SOURCE),
        "--paths",
        str(vendor_source),
        "--add-data",
        f"{vendor_source}{os.pathsep}vendor_delivery/src",
        "--hidden-import",
        "rebuilt_delivery_geometry_measure",
        "--hidden-import",
        "unified_square_rod_measure",
        "--hidden-import",
        "measure_single_scan",
        "--exclude-module",
        "matplotlib",
        "--exclude-module",
        "PIL",
        "--collect-all",
        "tifffile",
        str(API_ROOT / "run_service.py"),
    ]
    subprocess.run(command, cwd=ROOT, check=True)

    bundle = build_root / "dist" / "SquareRodAlgorithmService"
    executable = bundle / "SquareRodAlgorithmService.exe"
    if not executable.is_file():
        raise RuntimeError(f"PyInstaller did not create {executable}")

    copy_v7_runtime(bundle / "AlgorithmRuntime" / "V7")
    config_payload = json.loads(
        (API_ROOT / "api_config.production.example.json").read_text(
            encoding="utf-8"
        )
    )
    (bundle / "api_config.production.json").write_text(
        json.dumps(config_payload, ensure_ascii=False, indent=2) + "\n",
        encoding="utf-8",
    )
    shutil.copy2(API_ROOT / "README.md", bundle / "README.md")
    print(bundle)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
