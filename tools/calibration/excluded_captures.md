# Excluded calibration captures

Calibration rule: each future model must use one physical standard bar only, with multiple valid captures of that same bar. A specification change requires a new single-bar calibration model; multiple model files may coexist, but each must identify one bar, one specification, and its measurement conditions. The captures below are historical diagnostics only and must never be combined with another bar's data for fitting.

## 2606005B22-CTT_3/11_42.hobj

- Status: excluded from calibration fitting
- Reason: capture-specific longitudinal four-camera stitching drift
- Evidence: from 15–25% to 70–80%, edge A error changed from about -0.054 mm to -0.973 mm, while edge C changed from about +0.030 mm to +1.057 mm; B/D remained comparatively stable.
- Comparison: repeat capture `2606005B22-CTT_3/11_46.hobj` did not show the same drift.
- Handling: preserve the raw HOBJ under `tools/calibration/excluded_hobj`; do not delete it or silently include it in future fitting.

## 2606006A61-DTB_3/turnover/20_22.hobj

- Status: excluded from calibration fitting
- Reason: capture-specific longitudinal A/C drift during turnover retest.
- Evidence: compared with the same physical bar's valid turnover repeat `20_25.hobj`, A decreased and C increased by about 1 mm toward the far end while B/D remained comparatively stable.
- Handling: preserve the raw HOBJ under `tools/calibration/excluded_hobj/2606006A61-DTB_3/turnover`; do not delete it or silently include it in future fitting.
