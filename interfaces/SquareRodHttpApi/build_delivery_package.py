#!/usr/bin/env python3
"""Build the HOBJ-validated C# integration preview delivery ZIP."""

from __future__ import annotations

import argparse
import hashlib
import json
import shutil
import subprocess
import sys
import urllib.request
import zipfile
from datetime import date
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
API_ROOT = Path(__file__).resolve().parent
DEFAULT_NAME = "SquareRodAlgorithmHttpApi_HOBJ_Preview_20260724"
WINSW_ROOT = API_ROOT / "vendor" / "winsw" / "v2.12.0"
WINSW_EXE_URL = (
    "https://github.com/winsw/winsw/releases/download/v2.12.0/WinSW-x64.exe"
)
WINSW_LICENSE_URL = (
    "https://raw.githubusercontent.com/winsw/winsw/v2.12.0/LICENSE.txt"
)
WINSW_EXPECTED_SHA256 = (
    "05B82D46AD331CC16BDC00DE5C6332C1EF818DF8CEEFCD49C726553209B3A0DA"
)


def sha256(path: Path, chunk_size: int = 8 * 1024 * 1024) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        while chunk := handle.read(chunk_size):
            digest.update(chunk)
    return digest.hexdigest().upper()


def write_hash_list(package_dir: Path) -> None:
    lines = []
    for path in sorted(
        (item for item in package_dir.rglob("*") if item.is_file()),
        key=lambda item: item.relative_to(package_dir).as_posix().lower(),
    ):
        if path.name == "SHA256SUMS.txt":
            continue
        relative = path.relative_to(package_dir).as_posix()
        lines.append(f"{sha256(path)}  {relative}")
    (package_dir / "SHA256SUMS.txt").write_text(
        "\n".join(lines) + "\n", encoding="utf-8"
    )


def obtain_winsw(build_root: Path) -> tuple[Path, Path]:
    cached_exe = WINSW_ROOT / "WinSW-x64.exe"
    cached_license = WINSW_ROOT / "LICENSE.txt"
    download_root = build_root / "downloaded_dependencies" / "winsw-2.12.0"
    download_root.mkdir(parents=True, exist_ok=True)

    if cached_exe.is_file():
        winsw_exe = cached_exe
    else:
        winsw_exe = download_root / "WinSW-x64.exe"
        urllib.request.urlretrieve(WINSW_EXE_URL, winsw_exe)

    if cached_license.is_file():
        winsw_license = cached_license
    else:
        winsw_license = download_root / "LICENSE.txt"
        urllib.request.urlretrieve(WINSW_LICENSE_URL, winsw_license)

    actual_hash = sha256(winsw_exe)
    if actual_hash != WINSW_EXPECTED_SHA256:
        raise ValueError(
            "WinSW SHA256 mismatch: "
            f"expected {WINSW_EXPECTED_SHA256}, got {actual_hash}"
        )
    return winsw_exe, winsw_license


def build_zip(package_dir: Path, zip_path: Path) -> None:
    with zipfile.ZipFile(
        zip_path, "w", compression=zipfile.ZIP_DEFLATED, compresslevel=6
    ) as archive:
        for path in sorted(package_dir.rglob("*")):
            if path.is_file():
                archive.write(
                    path,
                    (Path(package_dir.name) / path.relative_to(package_dir)).as_posix(),
                )


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--build-root", required=True)
    parser.add_argument(
        "--release-root",
        default=str(ROOT / "release"),
    )
    parser.add_argument("--package-name", default=DEFAULT_NAME)
    args = parser.parse_args()

    build_root = Path(args.build_root).resolve()
    release_root = Path(args.release_root).resolve()
    package_dir = release_root / args.package_name
    zip_path = release_root / f"{args.package_name}.zip"
    zip_hash_path = release_root / f"{args.package_name}.zip.sha256"

    for target in (build_root, package_dir, zip_path, zip_hash_path):
        if target.exists():
            raise FileExistsError(f"Output target must not exist: {target}")
    build_root.mkdir(parents=True)
    release_root.mkdir(parents=True, exist_ok=True)

    winsw_exe, winsw_license = obtain_winsw(build_root)
    actual_winsw_hash = sha256(winsw_exe)

    service_build_root = build_root / "service_build"
    subprocess.run(
        [
            sys.executable,
            str(API_ROOT / "build_service.py"),
            "--build-root",
            str(service_build_root),
        ],
        cwd=ROOT,
        check=True,
    )
    built_service = (
        service_build_root / "dist" / "SquareRodAlgorithmService"
    )
    shutil.copytree(built_service, package_dir)

    shutil.copytree(API_ROOT / "docs", package_dir / "docs")
    shutil.copytree(API_ROOT / "examples", package_dir / "examples")
    shutil.copy2(API_ROOT / "README.md", package_dir / "README_开发说明.md")
    shutil.copy2(
        API_ROOT / "docs" / "README_部署说明.md",
        package_dir / "README_请先阅读.md",
    )
    shutil.copy2(
        winsw_exe,
        package_dir / "SquareRodServiceHost.exe",
    )
    third_party = package_dir / "third_party"
    third_party.mkdir()
    shutil.copy2(winsw_license, third_party / "WinSW_LICENSE.txt")
    for source in (API_ROOT / "service").iterdir():
        if source.is_file():
            shutil.copy2(source, package_dir / source.name)

    manifest = {
        "package": args.package_name,
        "built_on": date.today().isoformat(),
        "release_status": "csharp_integration_preview",
        "hobj_validated": True,
        "tiff_interface_implemented": True,
        "tiff_production_validated": False,
        "python_installation_required": False,
        "halcon_runtime_used": False,
        "api_schema": "square_rod_measurement_api_v1",
        "api_version": "0.1.0",
        "algorithm_baseline": "Square Rod Measurement0723_V7",
        "algorithm_baseline_version": "2026-07-23-v7",
        "windows_service_wrapper": {
            "name": "WinSW",
            "version": "2.12.0",
            "sha256": actual_winsw_hash,
            "license": "MIT",
        },
        "important": [
            "Copy and deploy the complete directory; do not copy only the EXE.",
            "TIFF production validation is pending.",
            "Do not overwrite a running old deployment directory.",
        ],
    }
    (package_dir / "PACKAGE_MANIFEST.json").write_text(
        json.dumps(manifest, ensure_ascii=False, indent=2) + "\n",
        encoding="utf-8",
    )
    write_hash_list(package_dir)
    build_zip(package_dir, zip_path)
    zip_hash_path.write_text(
        f"{sha256(zip_path)}  {zip_path.name}\n", encoding="utf-8"
    )

    print(package_dir)
    print(zip_path)
    print(zip_hash_path)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
