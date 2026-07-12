# Excluded calibration captures

Calibration rule: each future model must use one physical standard bar only, with multiple valid captures of that same bar. A specification change requires a new single-bar calibration model; multiple model files may coexist, but each must identify one bar, one specification, and its measurement conditions. The captures below are historical diagnostics only and must never be combined with another bar's data for fitting.

## 2606005B22-CTT_3/11_42.hobj

- Status: excluded from calibration fitting
- Reason: repeatable longitudinal four-camera mechanical drift.
- Evidence under the current unified coordinate mapping: from 15–25% to 70–80%, A/B/C/D changed by about `-0.773/+0.866/-0.832/+0.863 mm`; D1/D2 changed by about `+0.792/-1.493 mm`. The shape matches the CTB abnormal template with amplitude `1.124`, correlation `0.99984`, fit RMSE `0.0073 mm`, and four-camera amplitude spread `0.0135`.
- Comparison: repeat capture `2606005B22-CTT_3/11_46.hobj` did not show the same drift.
- Handling: preserve the raw HOBJ under `tools/calibration/excluded_hobj`; do not delete it or silently include it in future fitting.

## 2606006A61-DTB_3/turnover/20_22.hobj

- Status: excluded from calibration fitting
- Reason: repeatable longitudinal four-camera mechanical drift during a turnover retest.
- Evidence under the current unified coordinate mapping: from 15–25% to 70–80%, A/B/C/D changed by about `-0.714/+0.790/-0.761/+0.806 mm`; D1/D2 changed by about `+0.671/-1.349 mm`, nearly identical to the current CTB abnormal shape. The current drift model is normal-orientation only, so this turnover capture remains validation evidence and is not automatically corrected.
- Handling: preserve the raw HOBJ under `tools/calibration/excluded_hobj/2606006A61-DTB_3/turnover`; do not delete it or silently include it in future fitting.
