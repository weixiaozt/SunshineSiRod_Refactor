from __future__ import annotations

import hashlib
import json
from pathlib import Path
from typing import Any

from .config import ServiceConfig


def file_sha256(path: Path, chunk_size: int = 8 * 1024 * 1024) -> str:
    digest = hashlib.sha256()
    with Path(path).open("rb") as handle:
        while chunk := handle.read(chunk_size):
            digest.update(chunk)
    return digest.hexdigest().upper()


def verify_baseline(config: ServiceConfig) -> dict[str, Any]:
    """Verify the immutable V7 executable, formal models, and runtime manifests."""

    manifest_path = config.package_manifest_path
    if not manifest_path.is_file():
        raise FileNotFoundError(f"V7 package manifest is missing: {manifest_path}")
    manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
    if manifest.get("version") != "2026-07-23-v7":
        raise ValueError("The configured baseline is not Square Rod Measurement V7")
    if manifest.get("release_ready") is not True:
        raise ValueError("The configured V7 baseline is not release-ready")

    checks: list[dict[str, Any]] = []

    def check(path: Path, expected: str, component: str) -> None:
        if not path.is_file():
            raise FileNotFoundError(f"Baseline component is missing: {path}")
        actual = file_sha256(path)
        expected_upper = str(expected).upper()
        checks.append(
            {
                "component": component,
                "path": str(path),
                "expected_sha256": expected_upper,
                "actual_sha256": actual,
                "match": actual == expected_upper,
            }
        )
        if actual != expected_upper:
            raise ValueError(
                f"Baseline SHA256 mismatch for {component}: "
                f"expected {expected_upper}, got {actual}"
            )

    executable_path = config.baseline_root / "MeasureUnified" / "MeasureUnified.exe"
    check(
        executable_path,
        manifest["components"]["MeasureUnified.exe_sha256"],
        "MeasureUnified.exe",
    )
    model_root = config.baseline_root / "calibration" / "delivery_geometry"
    for filename, expected in manifest["formal_models"].items():
        check(model_root / filename, expected, filename)
    check(
        config.endface_calibration_path,
        manifest["components"]["coworker_endface_calibration_sha256"],
        "coworker_endface_calibration",
    )

    geometry_manifest = json.loads(
        config.geometry_manifest_path.read_text(encoding="utf-8")
    )
    if (
        geometry_manifest.get("model") != "rebuilt_long_rod_geometry_module_set"
        or int(geometry_manifest.get("version", 0)) != 3
        or geometry_manifest.get("valid") is not True
        or geometry_manifest.get("release_ready") is not True
    ):
        raise ValueError("The V7 geometry manifest is not valid release-ready version 3")
    angle = geometry_manifest.get("models", {}).get("main_face_angle", {})
    if (
        angle.get("path")
        != "long_rod_four_independent_spatial_camera_fingerprint_v3.json"
    ):
        raise ValueError("The V7 geometry manifest does not select formal angle V3")

    if config.require_workspace_sources:
        required_source_paths = (
            config.project_root / "tools" / "unified_square_rod_measure.py",
            config.project_root / "tools" / "rebuilt_delivery_geometry_measure.py",
            config.project_root
            / "四角独立角度算法"
            / "src"
            / "independent_corner_angles"
            / "measurement.py",
        )
        missing_sources = [
            str(path) for path in required_source_paths if not path.is_file()
        ]
        if missing_sources:
            raise FileNotFoundError(
                "V7 source entrypoints required by the interface worker are missing: "
                + ", ".join(missing_sources)
            )

    return {
        "verified": True,
        "package": manifest["package"],
        "version": manifest["version"],
        "release_ready": True,
        "checks": checks,
        "geometry_manifest": str(config.geometry_manifest_path),
        "endface_calibration": str(config.endface_calibration_path),
    }
