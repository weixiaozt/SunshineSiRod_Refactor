from __future__ import annotations

from dataclasses import dataclass
from pathlib import Path
from typing import Any, Mapping

import numpy as np
import tifffile


CAMERAS = ("Left", "Right", "Top", "Down")
HOBJ_OFFSETS = (120100, 256240185, 512360270, 768480355)
HOBJ_SHAPE = (20000, 3200)
HOBJ_BYTES_PER_IMAGE = HOBJ_SHAPE[0] * HOBJ_SHAPE[1] * np.dtype(np.float32).itemsize
HOBJ_MINIMUM_SIZE = HOBJ_OFFSETS[-1] + HOBJ_BYTES_PER_IMAGE


class InputValidationError(ValueError):
    def __init__(self, code: str, message: str) -> None:
        super().__init__(message)
        self.code = code


@dataclass
class OpenedMeasurementInput:
    images: dict[str, Any]
    mappings: list[Any]
    trace_path: Path
    provenance: dict[str, Any]

    def close(self) -> None:
        seen: set[int] = set()
        for image in list(self.images.values()) + list(self.mappings):
            identifier = id(image)
            if identifier in seen:
                continue
            seen.add(identifier)
            mapping = getattr(image, "_mmap", None)
            if mapping is not None:
                try:
                    mapping.close()
                except (BufferError, OSError):
                    pass
        self.images.clear()
        self.mappings.clear()


def _validate_float32_image(
    camera: str,
    image: Any,
    expected_shape: tuple[int, int],
) -> None:
    if tuple(image.shape) != expected_shape:
        raise InputValidationError(
            "INVALID_TIFF_SHAPE",
            f"{camera} TIFF shape must be {expected_shape}, got {tuple(image.shape)}",
        )
    dtype = np.dtype(image.dtype)
    if dtype.kind != "f" or dtype.itemsize != 4:
        raise InputValidationError(
            "INVALID_TIFF_DTYPE",
            f"{camera} TIFF dtype must be float32, got {dtype}",
        )


def open_tiff_set(
    camera_paths: Mapping[str, Any],
    *,
    capture_id: str,
    expected_shape: tuple[int, int],
) -> OpenedMeasurementInput:
    if set(camera_paths) != set(CAMERAS):
        missing = sorted(set(CAMERAS) - set(camera_paths))
        extra = sorted(set(camera_paths) - set(CAMERAS))
        raise InputValidationError(
            "INVALID_TIFF_SET",
            f"TIFF camera mapping must contain exactly {CAMERAS}; "
            f"missing={missing}, extra={extra}",
        )

    images: dict[str, Any] = {}
    mappings: list[Any] = []
    resolved_paths: dict[str, str] = {}
    try:
        for camera in CAMERAS:
            path = Path(str(camera_paths[camera])).expanduser().resolve()
            if not path.is_file() or path.suffix.lower() not in {".tif", ".tiff"}:
                raise InputValidationError(
                    "INPUT_NOT_FOUND", f"{camera} TIFF does not exist: {path}"
                )
            try:
                image = tifffile.memmap(path, mode="r")
            except Exception as exc:
                raise InputValidationError(
                    "TIFF_NOT_MEMMAPPABLE",
                    f"{camera} TIFF must be an uncompressed contiguous float32 image: {path}",
                ) from exc
            _validate_float32_image(camera, image, expected_shape)
            images[camera] = image
            mappings.append(image)
            resolved_paths[camera] = str(path)
    except Exception:
        OpenedMeasurementInput(images, mappings, Path("."), {}).close()
        raise

    parents = {str(Path(path).parent) for path in resolved_paths.values()}
    trace_parent = Path(next(iter(parents))) if len(parents) == 1 else Path.cwd()
    trace_path = (trace_parent / f"{capture_id}.tiffset").resolve()
    return OpenedMeasurementInput(
        images=images,
        mappings=mappings,
        trace_path=trace_path,
        provenance={
            "type": "tiff_set",
            "camera_mapping_explicit": True,
            "orientation_inferred_from_filename": False,
            "shape": list(expected_shape),
            "dtype": "float32",
            "files": resolved_paths,
            "memory_mapped": True,
            "intermediate_hobj_written": False,
            "halcon_runtime_used": False,
        },
    )

def open_hobj(path_value: Any, *, expected_shape: tuple[int, int]) -> OpenedMeasurementInput:
    if expected_shape != HOBJ_SHAPE:
        raise InputValidationError(
            "UNSUPPORTED_HOBJ_FORMAT",
            f"Legacy HOBJ compatibility requires shape {HOBJ_SHAPE}",
        )
    path = Path(str(path_value)).expanduser().resolve()
    if not path.is_file() or path.suffix.lower() != ".hobj":
        raise InputValidationError("INPUT_NOT_FOUND", f"HOBJ does not exist: {path}")
    if path.stat().st_size < HOBJ_MINIMUM_SIZE:
        raise InputValidationError(
            "INVALID_HOBJ",
            f"HOBJ is incomplete: {path} ({path.stat().st_size} bytes)",
        )

    images: dict[str, Any] = {}
    mappings: list[Any] = []
    try:
        for camera, offset in zip(CAMERAS, HOBJ_OFFSETS):
            image = np.memmap(
                path,
                dtype=np.float32,
                mode="r",
                offset=offset,
                shape=HOBJ_SHAPE,
            )
            images[camera] = image
            mappings.append(image)
    except Exception:
        OpenedMeasurementInput(images, mappings, path, {}).close()
        raise
    return OpenedMeasurementInput(
        images=images,
        mappings=mappings,
        trace_path=path,
        provenance={
            "type": "hobj",
            "path": str(path),
            "reader": "fixed_offset_numpy_memmap",
            "shape": list(HOBJ_SHAPE),
            "dtype": "float32",
            "orientation_inferred_from_filename": False,
            "memory_mapped": True,
            "halcon_runtime_used": False,
        },
    )


def open_measurement_input(
    input_payload: Mapping[str, Any],
    *,
    capture_id: str,
    expected_shape: tuple[int, int],
) -> OpenedMeasurementInput:
    input_type = str(input_payload.get("type", "")).strip().lower()
    if input_type == "tiff_set":
        cameras = input_payload.get("cameras")
        if not isinstance(cameras, Mapping):
            raise InputValidationError(
                "INVALID_TIFF_SET", "input.cameras must be an object"
            )
        return open_tiff_set(
            cameras, capture_id=capture_id, expected_shape=expected_shape
        )
    if input_type == "hobj":
        return open_hobj(input_payload.get("path"), expected_shape=expected_shape)
    raise InputValidationError(
        "UNSUPPORTED_INPUT_TYPE", "input.type must be 'tiff_set' or 'hobj'"
    )
