"""Mechanical four-camera longitudinal drift model helpers.

The camera calibration remains the normal baseline.  This module only models
the repeatable local-coordinate displacement that appears in unstable
mechanical captures, and classifies each HOBJ independently before correction.
"""

from __future__ import annotations

import math
from typing import Any

import numpy as np


OBJECTS = (1, 2, 3, 4)


def normalized_profile(profile: np.ndarray) -> np.ndarray:
    """Remove capture placement translation using the first sampled station."""
    values = np.asarray(profile, dtype=float)
    if values.ndim != 3 or values.shape[0] != 4 or values.shape[2] != 2:
        raise ValueError("Drift profile must have shape 4 x stations x 2")
    return values - values[:, :1, :]


def _fit_metrics(normal_template: np.ndarray, drift: np.ndarray, profile: np.ndarray) -> dict[str, Any]:
    deviation = normalized_profile(profile) - normal_template
    vector = drift.reshape(-1)
    observed = deviation.reshape(-1)
    denominator = float(vector @ vector)
    if denominator <= 1e-12:
        raise ValueError("Mechanical drift template has no measurable displacement")
    amplitude = float(observed @ vector / denominator)
    fitted = amplitude * drift
    fit_rmse = float(np.sqrt(np.mean((deviation - fitted) ** 2)))
    normal_rmse = float(np.sqrt(np.mean(deviation**2)))
    observed_norm = float(np.linalg.norm(observed))
    template_norm = float(np.linalg.norm(vector))
    correlation = float(observed @ vector / (observed_norm * template_norm)) if observed_norm > 1e-12 else 0.0
    per_camera: dict[str, float] = {}
    for index, obj in enumerate(OBJECTS):
        camera_template = drift[index].reshape(-1)
        camera_observed = deviation[index].reshape(-1)
        camera_denominator = float(camera_template @ camera_template)
        per_camera[str(obj)] = (
            float(camera_observed @ camera_template / camera_denominator)
            if camera_denominator > 1e-12
            else math.nan
        )
    finite_amplitudes = [value for value in per_camera.values() if math.isfinite(value)]
    spread = max(finite_amplitudes) - min(finite_amplitudes) if finite_amplitudes else math.inf
    return {
        "amplitude": amplitude,
        "fit_rmse_mm": fit_rmse,
        "normal_rmse_mm": normal_rmse,
        "correlation": correlation,
        "per_camera_amplitude": per_camera,
        "camera_amplitude_spread": float(spread),
    }


def build_model(
    normal_profiles: dict[str, np.ndarray],
    abnormal_profiles: dict[str, np.ndarray],
    fractions: list[float],
    *,
    bar_id: str,
    specification: str,
    orientation: str = "normal",
) -> dict[str, Any]:
    if len(normal_profiles) < 3:
        raise ValueError("Mechanical drift calibration requires at least three normal captures")
    if len(abnormal_profiles) < 2:
        raise ValueError("Mechanical drift calibration requires at least two abnormal captures")
    normal_stack = np.stack([normalized_profile(value) for value in normal_profiles.values()])
    abnormal_stack = np.stack([normalized_profile(value) for value in abnormal_profiles.values()])
    normal_template = np.median(normal_stack, axis=0)
    abnormal_template = np.median(abnormal_stack, axis=0)
    drift = abnormal_template - normal_template

    training: dict[str, dict[str, Any]] = {}
    for capture_id, profile in {**normal_profiles, **abnormal_profiles}.items():
        training[capture_id] = _fit_metrics(normal_template, drift, profile)

    normal_ids = set(normal_profiles)
    normal_amplitudes = [abs(training[name]["amplitude"]) for name in normal_profiles]
    normal_rmse = [training[name]["normal_rmse_mm"] for name in normal_profiles]
    abnormal_fit_rmse = [training[name]["fit_rmse_mm"] for name in abnormal_profiles]
    abnormal_correlations = [training[name]["correlation"] for name in abnormal_profiles]
    abnormal_spreads = [training[name]["camera_amplitude_spread"] for name in abnormal_profiles]
    thresholds = {
        # The measured groups are separated by roughly 20x in amplitude.  The
        # dead band (0.25..0.65) deliberately becomes unknown/retest.
        "normal_amplitude_abs_max": max(0.25, max(normal_amplitudes) + 0.10),
        "normal_rmse_max_mm": max(0.10, max(normal_rmse) * 4.0),
        "abnormal_amplitude_min": 0.65,
        "abnormal_amplitude_max": 1.35,
        "abnormal_correlation_min": min(0.98, min(abnormal_correlations) - 0.005),
        "abnormal_fit_rmse_max_mm": max(0.03, max(abnormal_fit_rmse) * 5.0),
        "camera_amplitude_spread_max": max(0.15, max(abnormal_spreads) * 5.0),
    }
    return {
        "version": 1,
        "model": "four_camera_longitudinal_local_drift",
        "bar_id": bar_id,
        "specification": specification,
        "orientation": orientation,
        "coordinate_stage": "local_xz_before_camera_calibration",
        "anchor_fraction": float(fractions[0]),
        "sample_fractions": [float(value) for value in fractions],
        "normal_capture_ids": list(normal_profiles),
        "abnormal_capture_ids": list(abnormal_profiles),
        "normal_capture_count": len(normal_profiles),
        "abnormal_capture_count": len(abnormal_profiles),
        "normal_reference_local_mm": {
            str(obj): {
                "x": normal_template[index, :, 0].tolist(),
                "z": normal_template[index, :, 1].tolist(),
            }
            for index, obj in enumerate(OBJECTS)
        },
        "drift_vectors_local_mm": {
            str(obj): {
                "x": drift[index, :, 0].tolist(),
                "z": drift[index, :, 1].tolist(),
            }
            for index, obj in enumerate(OBJECTS)
        },
        "thresholds": thresholds,
        "training_diagnostics": {
            capture_id: {**metrics, "class": "normal" if capture_id in normal_ids else "abnormal"}
            for capture_id, metrics in training.items()
        },
        "rules": {
            "normal": "No drift correction is applied.",
            "abnormal": "Subtract the jointly-scaled per-camera local drift before camera calibration.",
            "unknown": "Mark invalid and require reacquisition; never guess a correction.",
        },
    }


def arrays_from_model(model: dict[str, Any]) -> tuple[np.ndarray, np.ndarray, np.ndarray]:
    fractions = np.asarray(model["sample_fractions"], dtype=float)
    normal = np.asarray(
        [
            np.column_stack(
                [model["normal_reference_local_mm"][str(obj)]["x"], model["normal_reference_local_mm"][str(obj)]["z"]]
            )
            for obj in OBJECTS
        ],
        dtype=float,
    )
    drift = np.asarray(
        [
            np.column_stack(
                [model["drift_vectors_local_mm"][str(obj)]["x"], model["drift_vectors_local_mm"][str(obj)]["z"]]
            )
            for obj in OBJECTS
        ],
        dtype=float,
    )
    return fractions, normal, drift


def classify(model: dict[str, Any], profile: np.ndarray) -> dict[str, Any]:
    _, normal, drift = arrays_from_model(model)
    metrics = _fit_metrics(normal, drift, profile)
    thresholds = model["thresholds"]
    amplitude = metrics["amplitude"]
    if (
        abs(amplitude) <= float(thresholds["normal_amplitude_abs_max"])
        and metrics["normal_rmse_mm"] <= float(thresholds["normal_rmse_max_mm"])
        and metrics["camera_amplitude_spread"] <= float(thresholds["camera_amplitude_spread_max"])
    ):
        status = "normal"
        confidence = min(
            max(0.0, 1.0 - abs(amplitude) / float(thresholds["normal_amplitude_abs_max"])),
            max(0.0, 1.0 - metrics["normal_rmse_mm"] / float(thresholds["normal_rmse_max_mm"])),
        )
    elif (
        float(thresholds["abnormal_amplitude_min"]) <= amplitude <= float(thresholds["abnormal_amplitude_max"])
        and metrics["correlation"] >= float(thresholds["abnormal_correlation_min"])
        and metrics["fit_rmse_mm"] <= float(thresholds["abnormal_fit_rmse_max_mm"])
        and metrics["camera_amplitude_spread"] <= float(thresholds["camera_amplitude_spread_max"])
    ):
        status = "abnormal_corrected"
        confidence = min(
            1.0,
            max(0.0, (metrics["correlation"] - float(thresholds["abnormal_correlation_min"])) / (1.0 - float(thresholds["abnormal_correlation_min"]))),
            max(0.0, 1.0 - metrics["fit_rmse_mm"] / float(thresholds["abnormal_fit_rmse_max_mm"])),
            max(0.0, 1.0 - metrics["camera_amplitude_spread"] / float(thresholds["camera_amplitude_spread_max"])),
        )
    else:
        status = "unknown_invalid"
        confidence = 0.0
    return {
        "status": status,
        "detected": status == "abnormal_corrected",
        "correction_applied": status == "abnormal_corrected",
        "valid": status != "unknown_invalid",
        "confidence": float(confidence),
        "model_version": int(model.get("version", 0)),
        **metrics,
    }


def local_shift(model: dict[str, Any], obj: int, fraction: float, amplitude: float) -> tuple[float, float]:
    fractions, _, drift = arrays_from_model(model)
    # Cross-section calibration samples cover 5%..95%.  The physical anomaly
    # begins near zero at the head; use zero before the first station and a
    # linear continuation of the final measured segment to the tail.
    t = float(np.clip(fraction, 0.0, 1.0))
    index = OBJECTS.index(obj)
    x_values = drift[index, :, 0]
    z_values = drift[index, :, 1]
    if t < fractions[0]:
        scale = t / fractions[0] if fractions[0] > 0.0 else 0.0
        x = float(x_values[0] * scale)
        z = float(z_values[0] * scale)
    elif t > fractions[-1] and len(fractions) >= 2:
        ratio = (t - fractions[-1]) / (fractions[-1] - fractions[-2])
        x = float(x_values[-1] + ratio * (x_values[-1] - x_values[-2]))
        z = float(z_values[-1] + ratio * (z_values[-1] - z_values[-2]))
    else:
        x = float(np.interp(t, fractions, x_values))
        z = float(np.interp(t, fractions, z_values))
    return amplitude * x, amplitude * z
