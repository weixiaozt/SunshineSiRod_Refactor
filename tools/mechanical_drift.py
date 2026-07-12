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


def centered_profile(profile: np.ndarray) -> np.ndarray:
    """Remove per-camera rigid placement offsets without trusting one station."""
    values = np.asarray(profile, dtype=float)
    if values.ndim != 3 or values.shape[0] != 4 or values.shape[2] != 2:
        raise ValueError("Drift profile must have shape 4 x stations x 2")
    return values - np.median(values, axis=1, keepdims=True)


def _shift_template(values: np.ndarray, fractions: np.ndarray, shift_fraction: float) -> np.ndarray:
    """Evaluate one reference curve after a small longitudinal alignment shift."""
    shifted = np.empty_like(values, dtype=float)
    targets = np.clip(fractions + float(shift_fraction), fractions[0], fractions[-1])
    for camera in range(values.shape[0]):
        for coordinate in range(values.shape[2]):
            shifted[camera, :, coordinate] = np.interp(
                targets, fractions, values[camera, :, coordinate]
            )
    return shifted


def _robust_amplitude(observed: np.ndarray, vector: np.ndarray) -> float:
    """Fit one shared drift amplitude with light Huber reweighting."""
    x = vector.reshape(-1)
    y = observed.reshape(-1)
    denominator = float(x @ x)
    if denominator <= 1e-12:
        raise ValueError("Mechanical drift template has no measurable displacement")
    amplitude = float(y @ x / denominator)
    for _ in range(6):
        residual = y - amplitude * x
        scale = float(np.median(np.abs(residual))) / 0.6745
        if scale <= 1e-9:
            break
        limit = 1.5 * scale
        weights = np.minimum(1.0, limit / np.maximum(np.abs(residual), 1e-12))
        weighted_denominator = float(np.sum(weights * x * x))
        if weighted_denominator <= 1e-12:
            break
        updated = float(np.sum(weights * x * y) / weighted_denominator)
        if abs(updated - amplitude) <= 1e-8:
            amplitude = updated
            break
        amplitude = updated
    return amplitude


def _fit_metrics(
    normal_template: np.ndarray,
    drift: np.ndarray,
    profile: np.ndarray,
    fractions: np.ndarray | None = None,
    phase_max_fraction: float = 0.0,
    phase_step_fraction: float = 0.005,
) -> dict[str, Any]:
    """Fit the known drift mode after robust placement and row alignment."""
    if fractions is None:
        fractions = np.linspace(0.0, 1.0, normal_template.shape[1])
    fractions = np.asarray(fractions, dtype=float)
    if phase_max_fraction > 0.0:
        phase_count = max(1, int(round(phase_max_fraction / max(phase_step_fraction, 1e-6))))
        phases = np.linspace(-phase_max_fraction, phase_max_fraction, phase_count * 2 + 1)
    else:
        phases = np.asarray([0.0])

    observed_profile = centered_profile(profile)
    best: tuple[float, float, np.ndarray, np.ndarray, np.ndarray] | None = None
    for phase in phases:
        shifted_normal = _shift_template(normal_template, fractions, float(phase))
        shifted_drift = _shift_template(drift, fractions, float(phase))
        deviation = observed_profile - centered_profile(shifted_normal)
        fitted_drift = centered_profile(shifted_drift)
        amplitude = _robust_amplitude(deviation, fitted_drift)
        fitted = amplitude * fitted_drift
        fit_rmse = float(np.sqrt(np.mean((deviation - fitted) ** 2)))
        if best is None or fit_rmse < best[0]:
            best = (fit_rmse, float(phase), deviation, fitted_drift, shifted_normal)

    assert best is not None
    fit_rmse, phase, deviation, fitted_drift, shifted_normal = best
    amplitude = _robust_amplitude(deviation, fitted_drift)
    fitted = amplitude * fitted_drift
    vector = fitted_drift.reshape(-1)
    observed = deviation.reshape(-1)
    fit_rmse = float(np.sqrt(np.mean((deviation - fitted) ** 2)))
    normal_rmse = float(np.sqrt(np.mean(deviation**2)))
    observed_norm = float(np.linalg.norm(observed))
    template_norm = float(np.linalg.norm(vector))
    correlation = float(observed @ vector / (observed_norm * template_norm)) if observed_norm > 1e-12 else 0.0
    per_camera: dict[str, float] = {}
    for index, obj in enumerate(OBJECTS):
        camera_template = fitted_drift[index].reshape(-1)
        camera_observed = deviation[index].reshape(-1)
        camera_denominator = float(camera_template @ camera_template)
        per_camera[str(obj)] = (
            float(camera_observed @ camera_template / camera_denominator)
            if camera_denominator > 1e-12
            else math.nan
        )
    finite_amplitudes = [value for value in per_camera.values() if math.isfinite(value)]
    spread = max(finite_amplitudes) - min(finite_amplitudes) if finite_amplitudes else math.inf
    shifted_drift = _shift_template(drift, fractions, phase)
    placement_offsets = np.median(
        np.asarray(profile, dtype=float) - shifted_normal - amplitude * shifted_drift,
        axis=1,
    )
    return {
        "amplitude": amplitude,
        "fit_rmse_mm": fit_rmse,
        "normal_rmse_mm": normal_rmse,
        "correlation": correlation,
        "per_camera_amplitude": per_camera,
        "camera_amplitude_spread": float(spread),
        "alignment_shift_fraction": float(phase),
        "placement_offsets_local_mm": {
            str(obj): {"x": float(placement_offsets[index, 0]), "z": float(placement_offsets[index, 1])}
            for index, obj in enumerate(OBJECTS)
        },
    }


def build_model(
    normal_profiles: dict[str, np.ndarray],
    abnormal_profiles: dict[str, np.ndarray],
    fractions: list[float],
    *,
    bar_id: str,
    specification: str,
    orientation: str = "normal",
    reference_common_start_row: int | None = None,
    reference_common_end_row: int | None = None,
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
        training[capture_id] = _fit_metrics(normal_template, drift, profile, np.asarray(fractions, dtype=float))

    normal_ids = set(normal_profiles)
    normal_amplitudes = [abs(training[name]["amplitude"]) for name in normal_profiles]
    normal_rmse = [training[name]["normal_rmse_mm"] for name in normal_profiles]
    abnormal_fit_rmse = [training[name]["fit_rmse_mm"] for name in abnormal_profiles]
    abnormal_correlations = [training[name]["correlation"] for name in abnormal_profiles]
    abnormal_spreads = [training[name]["camera_amplitude_spread"] for name in abnormal_profiles]
    thresholds = {
        "normal_amplitude_abs_max": max(0.25, max(normal_amplitudes) + 0.10),
        "normal_rmse_max_mm": max(0.15, max(normal_rmse) * 6.0),
        "abnormal_amplitude_min": 0.30,
        "abnormal_amplitude_max": 2.00,
        "abnormal_correlation_min": max(0.90, min(0.95, min(abnormal_correlations) - 0.03)),
        "abnormal_fit_rmse_max_mm": max(0.08, max(abnormal_fit_rmse) * 3.0),
        "camera_amplitude_spread_max": max(0.30, max(abnormal_spreads) * 10.0),
        "alignment_phase_max_fraction": 0.0,
        "alignment_phase_step_fraction": 0.001,
    }
    return {
        "version": 3,
        "model": "four_camera_longitudinal_local_drift",
        "bar_id": bar_id,
        "specification": specification,
        "orientation": orientation,
        "coordinate_stage": "local_xz_before_camera_calibration",
        "longitudinal_coordinate": "absolute_scan_row",
        "reference_common_start_row": reference_common_start_row,
        "reference_common_end_row": reference_common_end_row,
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
            "unmatched": "Keep the uncorrected measurement available and raise a warning; never guess a correction.",
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


def _resample_model_curve(values: np.ndarray, source: np.ndarray, target: np.ndarray) -> np.ndarray:
    sampled = np.empty((values.shape[0], len(target), values.shape[2]), dtype=float)
    for camera in range(values.shape[0]):
        for coordinate in range(values.shape[2]):
            sampled[camera, :, coordinate] = np.interp(target, source, values[camera, :, coordinate])
    return sampled


def classify(
    model: dict[str, Any],
    profile: np.ndarray,
    sample_fractions: list[float] | np.ndarray | None = None,
) -> dict[str, Any]:
    fractions, normal, drift = arrays_from_model(model)
    if sample_fractions is not None:
        selected = np.asarray(sample_fractions, dtype=float)
        if len(selected) != np.asarray(profile).shape[1]:
            raise ValueError("Drift sample fractions must match the sampled profile stations")
        normal = _resample_model_curve(normal, fractions, selected)
        drift = _resample_model_curve(drift, fractions, selected)
        fractions = selected
    thresholds = model["thresholds"]
    metrics = _fit_metrics(
        normal,
        drift,
        profile,
        fractions,
        float(thresholds.get("alignment_phase_max_fraction", 0.003)),
        float(thresholds.get("alignment_phase_step_fraction", 0.001)),
    )
    amplitude = metrics["amplitude"]
    if (
        abs(amplitude) <= float(thresholds["normal_amplitude_abs_max"])
        and metrics["normal_rmse_mm"] <= float(thresholds["normal_rmse_max_mm"])
    ):
        status = "normal"
        reason = "known_drift_component_below_normal_limit"
        confidence = min(
            max(0.0, 1.0 - abs(amplitude) / float(thresholds["normal_amplitude_abs_max"])),
            max(0.0, 1.0 - min(1.0, metrics["normal_rmse_mm"] / float(thresholds["normal_rmse_max_mm"]))),
        )
    elif (
        float(thresholds["abnormal_amplitude_min"]) <= amplitude <= float(thresholds["abnormal_amplitude_max"])
        and metrics["correlation"] >= float(thresholds["abnormal_correlation_min"])
        and metrics["fit_rmse_mm"] <= float(thresholds["abnormal_fit_rmse_max_mm"])
        and metrics["camera_amplitude_spread"] <= float(thresholds["camera_amplitude_spread_max"])
    ):
        status = "abnormal_corrected"
        reason = "known_four_camera_drift_mode_matched"
        confidence = min(
            1.0,
            max(0.0, (metrics["correlation"] - float(thresholds["abnormal_correlation_min"])) / (1.0 - float(thresholds["abnormal_correlation_min"]))),
            max(0.0, 1.0 - metrics["fit_rmse_mm"] / float(thresholds["abnormal_fit_rmse_max_mm"])),
            max(0.0, 1.0 - metrics["camera_amplitude_spread"] / float(thresholds["camera_amplitude_spread_max"])),
        )
    else:
        status = "unmatched_unadjusted"
        failures: list[str] = []
        if amplitude < float(thresholds["abnormal_amplitude_min"]):
            failures.append("drift_amplitude_below_correction_limit")
        elif amplitude > float(thresholds["abnormal_amplitude_max"]):
            failures.append("drift_amplitude_above_calibrated_limit")
        if metrics["correlation"] < float(thresholds["abnormal_correlation_min"]):
            failures.append("drift_shape_correlation_too_low")
        if metrics["fit_rmse_mm"] > float(thresholds["abnormal_fit_rmse_max_mm"]):
            failures.append("drift_fit_residual_too_high")
        if metrics["camera_amplitude_spread"] > float(thresholds["camera_amplitude_spread_max"]):
            failures.append("four_camera_amplitude_inconsistent")
        if metrics["normal_rmse_mm"] > float(thresholds["normal_rmse_max_mm"]):
            failures.append("normal_reference_residual_too_high")
        reason = ";".join(failures) or "known_drift_mode_not_confirmed"
        confidence = 0.0
    return {
        "status": status,
        "detected": status == "abnormal_corrected",
        "correction_applied": status == "abnormal_corrected",
        "valid": True,
        "warning": status == "unmatched_unadjusted",
        "reason": reason,
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
