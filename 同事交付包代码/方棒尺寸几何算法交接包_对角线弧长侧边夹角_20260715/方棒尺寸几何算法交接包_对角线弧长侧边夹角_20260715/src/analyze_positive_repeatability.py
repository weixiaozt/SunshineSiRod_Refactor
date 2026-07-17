import csv
import json
import math
import warnings
from pathlib import Path

import matplotlib.pyplot as plt
import numpy as np
import tifffile


X_SCALE_MM_PER_PX = 0.015
Y_SCALE_MM_PER_ROW = 0.05
INVALID_THRESHOLD = -9999.0
SECTION_FRACTIONS = np.array([0.15, 0.30, 0.50, 0.70, 0.85])
TRACK_FRACTIONS = np.linspace(0.10, 0.90, 17)

# User-supplied calibrated section dimensions.
NOMINAL_WIDTH_MM = 210.05
NOMINAL_HEIGHT_MM = 105.05

CAMERAS = {
    "Left": {
        "glob": "1-Left.tif",
        "corner": np.array([NOMINAL_WIDTH_MM, NOMINAL_HEIGHT_MM]),
        "left_dir": np.array([0.0, -1.0]),
        "right_dir": np.array([-1.0, 0.0]),
    },
    "Right": {
        "glob": "2-Right.tif",
        "corner": np.array([NOMINAL_WIDTH_MM, 0.0]),
        "left_dir": np.array([-1.0, 0.0]),
        "right_dir": np.array([0.0, 1.0]),
    },
    "Top": {
        "glob": "3-Top.tif",
        "corner": np.array([0.0, 0.0]),
        "left_dir": np.array([0.0, 1.0]),
        "right_dir": np.array([1.0, 0.0]),
    },
    "Down": {
        "glob": "4-Down.tif",
        "corner": np.array([0.0, NOMINAL_HEIGHT_MM]),
        "left_dir": np.array([1.0, 0.0]),
        "right_dir": np.array([0.0, -1.0]),
    },
}
CAMERA_ORDER = ["Top", "Right", "Left", "Down"]


def py(value):
    if isinstance(value, dict):
        return {str(k): py(v) for k, v in value.items()}
    if isinstance(value, (list, tuple)):
        return [py(v) for v in value]
    if isinstance(value, np.ndarray):
        return value.tolist()
    if isinstance(value, (np.floating, float)):
        value = float(value)
        return value if math.isfinite(value) else None
    if isinstance(value, (np.integer, int)):
        return int(value)
    return value


def robust_line(x, y, min_points=80):
    x = np.asarray(x, dtype=float)
    y = np.asarray(y, dtype=float)
    keep = np.isfinite(x) & np.isfinite(y)
    for _ in range(6):
        if keep.sum() < min_points:
            raise RuntimeError("too few points for line fit")
        m, b = np.polyfit(x[keep], y[keep], 1)
        residual = y - (m * x + b)
        center = np.median(residual[keep])
        mad = np.median(np.abs(residual[keep] - center))
        sigma = max(1.4826 * mad, 0.001)
        new_keep = keep & (np.abs(residual - center) < 4.0 * sigma)
        if np.array_equal(new_keep, keep):
            break
        keep = new_keep
    rms = math.sqrt(float(np.mean((y[keep] - (m * x[keep] + b)) ** 2)))
    return float(m), float(b), rms, int(keep.sum())


def find_row_bounds(arr):
    sampled = arr[:, ::16]
    valid_ratio = np.mean(np.isfinite(sampled) & (sampled > INVALID_THRESHOLD), axis=1)
    rows = np.flatnonzero(valid_ratio > 0.05)
    if rows.size == 0:
        raise RuntimeError("no valid object rows")
    return int(rows[0]), int(rows[-1]), valid_ratio


def section_profile(arr, row, half_band=2):
    r0 = max(0, int(row) - half_band)
    r1 = min(arr.shape[0], int(row) + half_band + 1)
    block = np.asarray(arr[r0:r1], dtype=np.float64)
    block[(~np.isfinite(block)) | (block <= INVALID_THRESHOLD)] = np.nan
    with np.errstate(all="ignore"), warnings.catch_warnings():
        warnings.simplefilter("ignore", category=RuntimeWarning)
        return np.nanmedian(block, axis=0)


def fit_section(arr, row):
    profile = section_profile(arr, row)
    ok = np.isfinite(profile)
    xs_px = np.flatnonzero(ok)
    z = profile[ok]
    if xs_px.size < 1200:
        raise RuntimeError(f"too few valid profile points at row {row}")

    smooth_width = 21
    smooth = np.convolve(z, np.ones(smooth_width) / smooth_width, mode="same")
    lo = int(z.size * 0.08)
    hi = int(z.size * 0.92)
    apex_i = lo + int(np.argmax(smooth[lo:hi]))
    margin = max(75, int(z.size * 0.055))
    left_a = int(z.size * 0.08)
    left_b = apex_i - margin
    right_a = apex_i + margin
    right_b = int(z.size * 0.92)
    if left_b - left_a < 100 or right_b - right_a < 100:
        raise RuntimeError(f"invalid apex position at row {row}")

    x_mm = xs_px.astype(float) * X_SCALE_MM_PER_PX
    ml, bl, rms_l, nl = robust_line(x_mm[left_a:left_b], z[left_a:left_b])
    mr, br, rms_r, nr = robust_line(x_mm[right_a:right_b], z[right_a:right_b])
    if abs(ml - mr) < 0.1:
        raise RuntimeError(f"near-parallel face fits at row {row}")

    u = (br - bl) / (ml - mr)
    depth = ml * u + bl
    left_angle = math.degrees(math.atan(ml))
    right_angle = math.degrees(math.atan(mr))
    return {
        "row": int(row),
        "u_mm": float(u),
        "z_mm": float(depth),
        "left_slope": ml,
        "right_slope": mr,
        "left_angle_deg": left_angle,
        "right_angle_deg": right_angle,
        "bisector_roll_deg": (left_angle + right_angle) / 2.0,
        "included_angle_deg": left_angle - right_angle,
        "left_fit_rms_um": rms_l * 1000.0,
        "right_fit_rms_um": rms_r * 1000.0,
        "left_fit_count": nl,
        "right_fit_count": nr,
        "valid_col0": int(xs_px[0]),
        "valid_col1": int(xs_px[-1]),
    }


def robust_axis_fit(items, ref_span_rows):
    y = np.array([item["fraction"] * ref_span_rows * Y_SCALE_MM_PER_ROW for item in items])
    u = np.array([item["u_mm"] for item in items])
    z = np.array([item["z_mm"] for item in items])
    mu, bu, rms_u, _ = robust_line(y, u, min_points=8)
    mz, bz, rms_z, _ = robust_line(y, z, min_points=8)
    return {
        "du_dy": mu,
        "dz_dy": mz,
        "u_axis_tilt_deg": math.degrees(math.atan(mu)),
        "z_axis_tilt_deg": math.degrees(math.atan(mz)),
        "u_axis_fit_rms_um": rms_u * 1000.0,
        "z_axis_fit_rms_um": rms_z * 1000.0,
        "u_at_head_mm": bu,
        "z_at_head_mm": bz,
    }


def local_to_global_matrix(camera, baseline_sections):
    ml = float(np.median([x["left_slope"] for x in baseline_sections]))
    mr = float(np.median([x["right_slope"] for x in baseline_sections]))
    left_vec = np.array([-1.0, -ml])
    right_vec = np.array([1.0, mr])
    left_vec /= np.linalg.norm(left_vec)
    right_vec /= np.linalg.norm(right_vec)
    local_basis = np.column_stack([left_vec, right_vec])
    spec = CAMERAS[camera]
    global_basis = np.column_stack([spec["left_dir"], spec["right_dir"]])
    # Preserve the calibrated metric scale. The unrestricted orthogonal map
    # may have det=-1 because some height-map axes use mirrored conventions.
    u, _, vt = np.linalg.svd(global_basis @ local_basis.T)
    return u @ vt


def rigid_fit_2d(p, q):
    p = np.asarray(p, dtype=float)
    q = np.asarray(q, dtype=float)
    pc = p.mean(axis=0)
    qc = q.mean(axis=0)
    h = (p - pc).T @ (q - qc)
    u, _, vt = np.linalg.svd(h)
    r = vt.T @ u.T
    if np.linalg.det(r) < 0:
        vt[-1] *= -1
        r = vt.T @ u.T
    t = qc - r @ pc
    predicted = (r @ p.T).T + t
    residual = q - predicted
    angle = math.degrees(math.atan2(r[1, 0], r[0, 0]))
    return r, t, angle, residual


def describe(values):
    a = np.asarray(values, dtype=float)
    a = a[np.isfinite(a)]
    if not a.size:
        return {"count": 0}
    return {
        "count": int(a.size),
        "mean": float(np.mean(a)),
        "std": float(np.std(a, ddof=1)) if a.size > 1 else 0.0,
        "min": float(np.min(a)),
        "max": float(np.max(a)),
        "range": float(np.ptp(a)),
        "rms": float(math.sqrt(np.mean(a**2))),
        "p95_abs": float(np.percentile(np.abs(a), 95)),
    }


def discover_paths(root):
    manifests = list(root.rglob("_positive_unpacked/manifest.json"))
    if len(manifests) != 1:
        raise RuntimeError(f"expected one unpacked manifest, found {len(manifests)}")
    unpacked = manifests[0].parent
    return unpacked, unpacked.parent / "_positive_analysis"


def write_csv(path, rows):
    rows = list(rows)
    if not rows:
        return
    keys = []
    for row in rows:
        for key in row:
            if key not in keys:
                keys.append(key)
    with path.open("w", newline="", encoding="utf-8-sig") as f:
        writer = csv.DictWriter(f, fieldnames=keys)
        writer.writeheader()
        writer.writerows(rows)


def plot_results(out_dir, scan_names, pose_rows, camera_summary, section_rows, relative_rows, bar_length_mm):
    x = np.arange(len(scan_names))
    fig, axes = plt.subplots(2, 2, figsize=(13, 8), constrained_layout=True)
    tx = np.array([r["relative_x_mm"] for r in pose_rows]) * 1000
    tz = np.array([r["relative_z_mm"] for r in pose_rows]) * 1000
    rot = np.array([r["relative_roll_deg"] for r in pose_rows])
    resid = np.array([r["camera_relative_rms_mm"] for r in pose_rows]) * 1000
    axes[0, 0].plot(x, tx, "o-", label="X")
    axes[0, 0].plot(x, tz, "s-", label="Z")
    axes[0, 0].axhline(0, color="0.6", lw=0.8)
    axes[0, 0].set_ylabel("Relative shift (um)")
    axes[0, 0].legend()
    axes[0, 1].plot(x, rot, "o-", color="tab:green")
    axes[0, 1].axhline(0, color="0.6", lw=0.8)
    axes[0, 1].set_ylabel("Relative section roll (deg)")
    axes[1, 0].plot(x, resid, "o-", color="tab:red")
    axes[1, 0].set_ylabel("Inter-camera residual RMS (um)")
    axes[1, 1].plot(x, [r["relative_y_mm"] * 1000 for r in pose_rows], "o-")
    axes[1, 1].set_ylabel("Head-row relative Y (um)")
    for ax in axes.flat:
        ax.set_xticks(x, scan_names, rotation=45, ha="right")
        ax.grid(True, alpha=0.25)
    fig.suptitle("Positive-scan relative pose and four-camera consistency")
    fig.savefig(out_dir / "repeatability_overview.png", dpi=180)
    plt.close(fig)

    fig, axes = plt.subplots(2, 2, figsize=(13, 8), constrained_layout=True)
    for ax, camera in zip(axes.flat, CAMERA_ORDER):
        rows = [r for r in section_rows if r["camera"] == camera and abs(r["fraction"] - 0.5) < 1e-9]
        rows.sort(key=lambda r: scan_names.index(r["scan"]))
        du = (np.array([r["u_mm"] for r in rows]) - rows[0]["u_mm"]) * 1000
        dz = (np.array([r["z_mm"] for r in rows]) - rows[0]["z_mm"]) * 1000
        ax.plot(x, du, "o-", label="local u")
        ax.plot(x, dz, "s-", label="local z")
        ax.set_title(camera)
        ax.set_ylabel("Center-section change (um)")
        ax.set_xticks(x, scan_names, rotation=45, ha="right")
        ax.grid(True, alpha=0.25)
        ax.legend()
    fig.suptitle("Same physical center section in each camera coordinate frame")
    fig.savefig(out_dir / "same_section_camera_changes.png", dpi=180)
    plt.close(fig)

    fig, ax = plt.subplots(figsize=(11, 6), constrained_layout=True)
    p = np.array([CAMERAS[c]["corner"] for c in CAMERA_ORDER])
    closed = np.vstack([p, p[0]])
    ax.plot(closed[:, 0], closed[:, 1], color="0.35", lw=2)
    scale = 80.0
    colors = plt.cm.tab10(np.linspace(0, 1, len(scan_names)))
    for scan, color in zip(scan_names, colors):
        rr = [r for r in relative_rows if r["scan"] == scan and abs(r["fraction"] - 0.5) < 1e-9]
        rr.sort(key=lambda r: CAMERA_ORDER.index(r["camera"]))
        q = p + scale * np.array([[r["global_dx_mm"], r["global_dz_mm"]] for r in rr])
        ax.scatter(q[:, 0], q[:, 1], s=18, color=color, label=scan)
    for camera, corner in zip(CAMERA_ORDER, p):
        ax.text(corner[0], corner[1], f"  {camera}", va="bottom")
    ax.set_aspect("equal")
    ax.invert_yaxis()
    ax.set_xlabel("Bar X (mm)")
    ax.set_ylabel("Bar Z (mm)")
    ax.set_title("Center-section corner changes, displacement magnified 80x")
    ax.legend(ncol=2, fontsize=8)
    ax.grid(True, alpha=0.2)
    fig.savefig(out_dir / "cross_section_overlay_magnified.png", dpi=180)
    plt.close(fig)

    fig, ax = plt.subplots(figsize=(11, 6), constrained_layout=True)
    cams = CAMERA_ORDER
    rel_rms = [camera_summary[c]["rigid_fit_residual_norm_mm"]["rms"] * 1000 for c in cams]
    rel_p95 = [camera_summary[c]["rigid_fit_residual_norm_mm"]["p95_abs"] * 1000 for c in cams]
    xx = np.arange(len(cams))
    ax.bar(xx - 0.18, rel_rms, width=0.36, label="RMS")
    ax.bar(xx + 0.18, rel_p95, width=0.36, label="95th percentile")
    ax.set_xticks(xx, cams)
    ax.set_ylabel("Residual after common rigid pose removal (um)")
    ax.set_title("Relative return consistency of the four camera channels")
    ax.grid(True, axis="y", alpha=0.25)
    ax.legend()
    fig.savefig(out_dir / "inter_camera_relative_repeatability.png", dpi=180)
    plt.close(fig)

    fig, axes = plt.subplots(1, 2, figsize=(13, 5), constrained_layout=True)
    axes[0].plot(x, [r["bar_axis_x_tilt_deg"] for r in pose_rows], "o-", label="X/Y")
    axes[0].plot(x, [r["bar_axis_z_tilt_deg"] for r in pose_rows], "s-", label="Z/Y")
    axes[0].set_ylabel("Bar axis in reconstructed frame (deg)")
    axes[0].legend()
    axes[1].plot(x, [r["axis_intercamera_residual_mdeg"] for r in pose_rows], "o-", color="tab:red")
    axes[1].set_ylabel("Inter-camera axis disagreement RMS (mdeg)")
    for ax in axes:
        ax.set_xticks(x, scan_names, rotation=45, ha="right")
        ax.grid(True, alpha=0.25)
    fig.suptitle("Longitudinal bar-axis reconstruction from 17 sections")
    fig.savefig(out_dir / "longitudinal_axis_repeatability.png", dpi=180)
    plt.close(fig)

    fig, ax = plt.subplots(figsize=(11, 6), constrained_layout=True)
    corners = np.array([CAMERAS[c]["corner"] for c in CAMERA_ORDER])
    closed = np.vstack([corners, corners[0]])
    ax.plot(closed[:, 0], closed[:, 1], color="0.25", lw=2.2, label="A=210.05, B=105.05 mm")
    for camera in CAMERA_ORDER:
        summary = camera_summary[camera]
        origin = np.asarray(summary["measurement_origin_global_xz_mm"], dtype=float)
        corner = np.asarray(summary["corner_global_xz_mm"], dtype=float)
        matrix = np.asarray(summary["local_to_global_xz_matrix"], dtype=float)
        ax.plot([origin[0], corner[0]], [origin[1], corner[1]], "--", color="0.55", lw=1.2)
        ax.scatter(origin[0], origin[1], marker="s", s=55)
        ax.scatter(corner[0], corner[1], marker="o", s=28, color="0.25")
        ax.quiver(origin[0], origin[1], *(matrix[:, 0] * 8.0), angles="xy", scale_units="xy", scale=1, width=0.004)
        ax.quiver(origin[0], origin[1], *(matrix[:, 1] * 8.0), angles="xy", scale_units="xy", scale=1, width=0.004)
        distance = summary["corner_distance_from_measurement_origin_mm"]
        angle = summary["profile_plane_to_bar_axis_deg"]["mean"]
        text_offsets = {"Top": (6, 8), "Right": (6, -50), "Left": (6, 8), "Down": (6, 8)}
        ax.annotate(
            f"{camera}\norigin=({origin[0]:.2f}, {origin[1]:.2f}) mm\ncorner distance={distance:.2f} mm\nplane/axis={angle:.4f} deg",
            origin,
            xytext=text_offsets[camera],
            textcoords="offset points",
            fontsize=8,
        )
    ax.set_aspect("equal")
    ax.invert_yaxis()
    ax.set_xlabel("Bar X (mm)")
    ax.set_ylabel("Bar Z (mm)")
    ax.set_title("A/B absolute section and calibrated camera-coordinate origins")
    ax.grid(True, alpha=0.2)
    ax.legend(loc="center")
    fig.savefig(out_dir / "absolute_cross_section_camera_geometry.png", dpi=200)
    plt.close(fig)

    fig = plt.figure(figsize=(12, 8), constrained_layout=True)
    ax = fig.add_subplot(111, projection="3d")
    a = NOMINAL_WIDTH_MM
    b = NOMINAL_HEIGHT_MM
    length = bar_length_mm
    vertices = np.array([
        [0, 0, 0], [a, 0, 0], [a, 0, b], [0, 0, b],
        [0, length, 0], [a, length, 0], [a, length, b], [0, length, b],
    ], dtype=float)
    edges = [
        (0, 1), (1, 2), (2, 3), (3, 0), (4, 5), (5, 6), (6, 7), (7, 4),
        (0, 4), (1, 5), (2, 6), (3, 7),
    ]
    for i, j in edges:
        ax.plot(*zip(vertices[i], vertices[j]), color="0.3", lw=1.5)
    face_defs = [
        [vertices[i] for i in (0, 1, 5, 4)],
        [vertices[i] for i in (3, 2, 6, 7)],
        [vertices[i] for i in (0, 3, 7, 4)],
        [vertices[i] for i in (1, 2, 6, 5)],
    ]
    from mpl_toolkits.mplot3d.art3d import Poly3DCollection
    faces = Poly3DCollection(face_defs, alpha=0.10, facecolors=["tab:blue", "tab:green", "tab:orange", "tab:red"])
    faces.set_edgecolor("none")
    ax.add_collection3d(faces)
    for camera in CAMERA_ORDER:
        summary = camera_summary[camera]
        origin_xz = np.asarray(summary["measurement_origin_global_xz_mm"], dtype=float)
        corner_xz = np.asarray(summary["corner_global_xz_mm"], dtype=float)
        origin = np.array([origin_xz[0], length * 0.5, origin_xz[1]])
        corner = np.array([corner_xz[0], length * 0.5, corner_xz[1]])
        ax.scatter(*origin, marker="s", s=38)
        ax.plot(*zip(origin, corner), "--", color="0.5", lw=1.0)
        ax.text(*(origin + np.array([2.0, 0.0, 2.0])), camera, fontsize=8)
    ax.set_xlabel("X / A (mm)")
    ax.set_ylabel("Y / motion (mm)")
    ax.set_zlabel("Z / B (mm)")
    ax.set_title(f"Reconstructed calibrated bar: A={a:.2f}, B={b:.2f}, scanned length~{length:.2f} mm")
    ax.set_box_aspect((a, length, b))
    ax.view_init(elev=22, azim=-55)
    fig.savefig(out_dir / "absolute_3d_bar_reconstruction.png", dpi=200)
    plt.close(fig)


def main():
    root = Path.cwd()
    unpacked, out_dir = discover_paths(root)
    out_dir.mkdir(exist_ok=True)
    scan_dirs = sorted([p for p in unpacked.iterdir() if p.is_dir()])
    scan_names = [p.name for p in scan_dirs]
    if len(scan_names) != 10:
        raise RuntimeError(f"expected 10 positive scans, found {len(scan_names)}")

    bounds = {}
    for scan_dir in scan_dirs:
        bounds[scan_dir.name] = {}
        for camera, spec in CAMERAS.items():
            arr = tifffile.memmap(scan_dir / spec["glob"])
            r0, r1, _ = find_row_bounds(arr)
            bounds[scan_dir.name][camera] = {"row0": r0, "row1": r1, "span_rows": r1 - r0}

    reference = scan_names[0]
    reference_spans = {c: bounds[reference][c]["span_rows"] for c in CAMERAS}
    bar_length_mm = float(np.median(list(reference_spans.values())) * Y_SCALE_MM_PER_ROW)
    section_measurements = []
    camera_scan = {}

    for scan_dir in scan_dirs:
        scan = scan_dir.name
        camera_scan[scan] = {}
        for camera, spec in CAMERAS.items():
            arr = tifffile.memmap(scan_dir / spec["glob"])
            row0 = bounds[scan][camera]["row0"]
            span = reference_spans[camera]
            highlighted = []
            for fraction in SECTION_FRACTIONS:
                item = fit_section(arr, row0 + round(float(fraction) * span))
                item["scan"] = scan
                item["camera"] = camera
                item["fraction"] = float(fraction)
                item["distance_from_head_mm"] = float(fraction * span * Y_SCALE_MM_PER_ROW)
                highlighted.append(item)
                section_measurements.append(item.copy())

            track = []
            for fraction in TRACK_FRACTIONS:
                item = fit_section(arr, row0 + round(float(fraction) * span))
                item["fraction"] = float(fraction)
                track.append(item)
            axis = robust_axis_fit(track, span)
            center = min(highlighted, key=lambda x: abs(x["fraction"] - 0.5))
            camera_scan[scan][camera] = {
                "bounds": bounds[scan][camera],
                "sections": highlighted,
                "axis": axis,
                "center": center,
            }

    matrices = {
        camera: local_to_global_matrix(
            camera,
            [section for scan in scan_names for section in camera_scan[scan][camera]["sections"]],
        )
        for camera in CAMERAS
    }

    relative_rows = []
    pose_sections = []
    nominal = np.array([CAMERAS[c]["corner"] for c in CAMERA_ORDER])
    for scan in scan_names:
        for fraction in SECTION_FRACTIONS:
            global_delta = []
            for camera in CAMERA_ORDER:
                current = next(x for x in camera_scan[scan][camera]["sections"] if x["fraction"] == fraction)
                base = next(x for x in camera_scan[reference][camera]["sections"] if x["fraction"] == fraction)
                local_delta = np.array([current["u_mm"] - base["u_mm"], current["z_mm"] - base["z_mm"]])
                delta = matrices[camera] @ local_delta
                global_delta.append(delta)
            global_delta = np.asarray(global_delta)
            _, translation, angle, residual = rigid_fit_2d(nominal, nominal + global_delta)
            pose_sections.append({
                "scan": scan,
                "fraction": float(fraction),
                "relative_x_mm": float(translation[0]),
                "relative_z_mm": float(translation[1]),
                "relative_roll_deg": angle,
                "camera_relative_rms_mm": float(math.sqrt(np.mean(np.sum(residual**2, axis=1)))),
            })
            for i, camera in enumerate(CAMERA_ORDER):
                relative_rows.append({
                    "scan": scan,
                    "camera": camera,
                    "fraction": float(fraction),
                    "global_dx_mm": float(global_delta[i, 0]),
                    "global_dz_mm": float(global_delta[i, 1]),
                    "rigid_residual_x_mm": float(residual[i, 0]),
                    "rigid_residual_z_mm": float(residual[i, 1]),
                    "rigid_residual_norm_mm": float(np.linalg.norm(residual[i])),
                })

    axis_rows = []
    for scan in scan_names:
        mapped = []
        for camera in CAMERA_ORDER:
            axis = camera_scan[scan][camera]["axis"]
            vector = matrices[camera] @ np.array([axis["du_dy"], axis["dz_dy"]])
            mapped.append(vector)
        mapped = np.asarray(mapped)
        common = np.median(mapped, axis=0)
        residual = mapped - common
        axis_rows.append({
            "scan": scan,
            "bar_axis_dx_dy": float(common[0]),
            "bar_axis_dz_dy": float(common[1]),
            "bar_axis_x_tilt_deg": math.degrees(math.atan(common[0])),
            "bar_axis_z_tilt_deg": math.degrees(math.atan(common[1])),
            "axis_intercamera_residual_mdeg": float(
                math.degrees(math.sqrt(np.mean(np.sum(residual**2, axis=1)))) * 1000.0
            ),
        })
    axis_reference = axis_rows[0]

    pose_rows = []
    for scan in scan_names:
        rows = [r for r in pose_sections if r["scan"] == scan]
        dy = [
            (bounds[scan][c]["row0"] - bounds[reference][c]["row0"]) * Y_SCALE_MM_PER_ROW
            for c in CAMERAS
        ]
        axis = next(r for r in axis_rows if r["scan"] == scan)
        pose_rows.append({
            "scan": scan,
            "relative_x_mm": float(np.median([r["relative_x_mm"] for r in rows])),
            "relative_y_mm": float(np.median(dy)),
            "relative_z_mm": float(np.median([r["relative_z_mm"] for r in rows])),
            "relative_roll_deg": float(np.median([r["relative_roll_deg"] for r in rows])),
            "camera_relative_rms_mm": float(math.sqrt(np.mean([r["camera_relative_rms_mm"] ** 2 for r in rows]))),
            "head_y_camera_spread_mm": float(np.ptp(dy)),
            "bar_axis_x_tilt_deg": axis["bar_axis_x_tilt_deg"],
            "bar_axis_z_tilt_deg": axis["bar_axis_z_tilt_deg"],
            "relative_axis_x_tilt_deg": axis["bar_axis_x_tilt_deg"] - axis_reference["bar_axis_x_tilt_deg"],
            "relative_axis_z_tilt_deg": axis["bar_axis_z_tilt_deg"] - axis_reference["bar_axis_z_tilt_deg"],
            "axis_intercamera_residual_mdeg": axis["axis_intercamera_residual_mdeg"],
        })

    camera_summary = {}
    for camera in CAMERA_ORDER:
        center_rows = [camera_scan[s][camera]["center"] for s in scan_names]
        axis_rows = [camera_scan[s][camera]["axis"] for s in scan_names]
        mean_u = float(np.mean([r["u_mm"] for r in center_rows]))
        mean_z = float(np.mean([r["z_mm"] for r in center_rows]))
        measurement_origin = CAMERAS[camera]["corner"] - matrices[camera] @ np.array([mean_u, mean_z])
        plane_angles = [
            90.0 - math.degrees(math.atan(math.hypot(r["du_dy"], r["dz_dy"])))
            for r in axis_rows
        ]
        residual_norms = [
            r["rigid_residual_norm_mm"]
            for r in relative_rows
            if r["camera"] == camera and r["scan"] != reference
        ]
        head_changes = [
            (bounds[s][camera]["row0"] - bounds[reference][camera]["row0"]) * Y_SCALE_MM_PER_ROW
            for s in scan_names
        ]
        camera_summary[camera] = {
            "center_section_u_mm": describe([r["u_mm"] for r in center_rows]),
            "center_section_z_mm": describe([r["z_mm"] for r in center_rows]),
            "bisector_roll_deg": describe([r["bisector_roll_deg"] for r in center_rows]),
            "included_angle_deg": describe([r["included_angle_deg"] for r in center_rows]),
            "u_axis_tilt_deg": describe([r["u_axis_tilt_deg"] for r in axis_rows]),
            "z_axis_tilt_deg": describe([r["z_axis_tilt_deg"] for r in axis_rows]),
            "profile_plane_to_bar_axis_deg": describe(plane_angles),
            "profile_plane_perpendicularity_error_deg": describe([90.0 - v for v in plane_angles]),
            "head_y_change_mm": describe(head_changes),
            "rigid_fit_residual_norm_mm": describe(residual_norms),
            "face_fit_rms_um": describe([
                value
                for r in center_rows
                for value in (r["left_fit_rms_um"], r["right_fit_rms_um"])
            ]),
            "local_to_global_xz_matrix": matrices[camera],
            "measurement_origin_global_xz_mm": measurement_origin,
            "corner_global_xz_mm": CAMERAS[camera]["corner"],
            "corner_distance_from_measurement_origin_mm": float(math.hypot(mean_u, mean_z)),
        }

    pose_summary = {
        "relative_x_mm": describe([r["relative_x_mm"] for r in pose_rows]),
        "relative_y_mm": describe([r["relative_y_mm"] for r in pose_rows]),
        "relative_z_mm": describe([r["relative_z_mm"] for r in pose_rows]),
        "relative_roll_deg": describe([r["relative_roll_deg"] for r in pose_rows]),
        "camera_relative_rms_mm": describe([r["camera_relative_rms_mm"] for r in pose_rows[1:]]),
        "head_y_camera_spread_mm": describe([r["head_y_camera_spread_mm"] for r in pose_rows[1:]]),
        "bar_axis_x_tilt_deg": describe([r["bar_axis_x_tilt_deg"] for r in pose_rows]),
        "bar_axis_z_tilt_deg": describe([r["bar_axis_z_tilt_deg"] for r in pose_rows]),
        "relative_axis_x_tilt_deg": describe([r["relative_axis_x_tilt_deg"] for r in pose_rows]),
        "relative_axis_z_tilt_deg": describe([r["relative_axis_z_tilt_deg"] for r in pose_rows]),
        "axis_intercamera_residual_mdeg": describe([r["axis_intercamera_residual_mdeg"] for r in pose_rows]),
    }

    results = {
        "method": {
            "x_scale_mm_per_px": X_SCALE_MM_PER_PX,
            "y_scale_mm_per_row": Y_SCALE_MM_PER_ROW,
            "invalid_threshold": INVALID_THRESHOLD,
            "reference_scan": reference,
            "section_fractions_from_each_camera_head": SECTION_FRACTIONS,
            "user_supplied_edge_A_width_mm": NOMINAL_WIDTH_MM,
            "user_supplied_edge_B_height_mm": NOMINAL_HEIGHT_MM,
            "reconstructed_scanned_length_mm": bar_length_mm,
            "important_limit": (
                "All scan-to-scan values are relative camera/bar pose. A common rigid camera-stage return error "
                "is mathematically indistinguishable from the opposite bar reclamping motion. Image coordinates "
                "are treated as calibrated absolute millimetres; reported camera positions refer to each "
                "height map's calibrated measurement-coordinate origin."
            ),
        },
        "bounds": bounds,
        "pose_by_scan": pose_rows,
        "pose_summary": pose_summary,
        "camera_summary": camera_summary,
        "section_pose": pose_sections,
        "axis_by_scan": axis_rows,
        "relative_camera_rows": relative_rows,
    }
    (out_dir / "positive_repeatability_results.json").write_text(
        json.dumps(py(results), indent=2, ensure_ascii=False), encoding="utf-8"
    )
    write_csv(out_dir / "scan_pose_summary.csv", pose_rows)
    write_csv(out_dir / "section_measurements.csv", section_measurements)
    write_csv(out_dir / "relative_camera_residuals.csv", relative_rows)

    camera_csv = []
    for camera, summary in camera_summary.items():
        camera_csv.append({
            "camera": camera,
            "center_u_mean_mm": summary["center_section_u_mm"]["mean"],
            "center_u_std_mm": summary["center_section_u_mm"]["std"],
            "center_u_range_mm": summary["center_section_u_mm"]["range"],
            "center_z_mean_mm": summary["center_section_z_mm"]["mean"],
            "center_z_std_mm": summary["center_section_z_mm"]["std"],
            "center_z_range_mm": summary["center_section_z_mm"]["range"],
            "bisector_roll_mean_deg": summary["bisector_roll_deg"]["mean"],
            "bisector_roll_std_deg": summary["bisector_roll_deg"]["std"],
            "included_angle_mean_deg": summary["included_angle_deg"]["mean"],
            "u_axis_tilt_mean_deg": summary["u_axis_tilt_deg"]["mean"],
            "u_axis_tilt_std_deg": summary["u_axis_tilt_deg"]["std"],
            "z_axis_tilt_mean_deg": summary["z_axis_tilt_deg"]["mean"],
            "z_axis_tilt_std_deg": summary["z_axis_tilt_deg"]["std"],
            "profile_plane_to_bar_axis_mean_deg": summary["profile_plane_to_bar_axis_deg"]["mean"],
            "profile_plane_to_bar_axis_std_deg": summary["profile_plane_to_bar_axis_deg"]["std"],
            "profile_plane_perpendicularity_error_deg": summary["profile_plane_perpendicularity_error_deg"]["mean"],
            "head_y_range_mm": summary["head_y_change_mm"]["range"],
            "relative_residual_rms_mm": summary["rigid_fit_residual_norm_mm"]["rms"],
            "relative_residual_p95_mm": summary["rigid_fit_residual_norm_mm"]["p95_abs"],
            "face_fit_rms_mean_um": summary["face_fit_rms_um"]["mean"],
            "corner_distance_from_measurement_origin_mm": summary["corner_distance_from_measurement_origin_mm"],
            "measurement_origin_global_x_mm": summary["measurement_origin_global_xz_mm"][0],
            "measurement_origin_global_z_mm": summary["measurement_origin_global_xz_mm"][1],
        })
    write_csv(out_dir / "camera_repeatability_summary.csv", camera_csv)
    plot_results(out_dir, scan_names, pose_rows, camera_summary, section_measurements, relative_rows, bar_length_mm)

    print(json.dumps(py({"output": str(out_dir), "pose_summary": pose_summary, "camera_summary": camera_csv}), indent=2))


if __name__ == "__main__":
    main()
