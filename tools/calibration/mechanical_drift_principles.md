# 四相机机械漂移识别与修正规则

## 1. 模型分层

正常相机标定与机械漂移模型必须独立保存：

```text
camera_calibration_model_210_105.json      正常相机局部坐标 -> 全局横截面
mechanical_drift_model_210_105.json        已知机械异常的局部 X/Z 漂移曲线
endface_calibration_model_210_105.json     端面角度标定
```

禁止把正常和异常 HOBJ 混合平均生成相机标定。漂移修正顺序固定为：

```text
原始局部轮廓
  -> 每个 HOBJ 独立识别漂移模式
  -> 命中异常时减去四相机局部漂移
  -> 正常相机标定
  -> A/B/C/D、对角线、端面计算
```

## 2. 当前 210_105 漂移模型

正式拟合数据只来自同一根标准棒 `2606005B22-CTB_3` 的正向采集：

```text
正常：13_08.hobj、13_12.hobj、13_15.hobj
异常：10_08.hobj、11_59.hobj
```

CTT `11_42.hobj` 和 DTB turnover `20_22.hobj` 只用于独立验证，不参与拟合。

模型在标准扫描行 `1351..18256` 的 5%～95% 每隔 5% 保存四台相机局部 `(X,Z)` 正常位置参考曲线和异常漂移曲线。V3 借鉴原 HALCON 逐绝对行位置模板：机械漂移固定在设备扫描行程坐标上，禁止把不同长度或不同有效起点的棒各自拉伸成 0%～100%。短棒只使用与标定行程实际重叠的参考站点，少于 5 个站点时不猜测修正。

检测不依赖单个锚点，而是用重叠区中位数分别消除每台相机固定摆放平移，再由四相机共同拟合一个鲁棒幅度系数；禁止每台相机自由缩放。逐切片和端面修正也必须按绝对扫描行映射到同一漂移曲线。

训练结果：

```text
正常幅度：-0.051 ～ 0.020
异常幅度： 0.994 ～ 1.006
异常拟合 RMSE：约 0.023 mm
异常四相机幅度极差：小于 0.004
历史 CTT 11_42 和 DTB 20_22 仍作为独立模式验证
```

现场短棒 `2606006A61-FTT_unknown_3/14_46.hobj` 的有效行只有 `8522..18254`。按各自棒长百分比会把漂移幅度错误估成约 `1.24`；V3 按绝对行重叠区 `45%..95%` 得到幅度约 `2.62`、相关性约 `0.998`、四相机幅度极差约 `0.020`。该幅度尚无同棒正常图或人工真值验证，因此只作诊断，不进入训练，也不扩大强制修正范围。

## 3. 逐 HOBJ 分类

```text
normal
  不应用漂移修正，直接使用正常相机标定。

abnormal_corrected
  四相机方向、曲线形状、共同幅度和残差均命中模型；先修正局部坐标。

unmatched_unadjusted
  未可靠命中已知异常模式；禁止猜测补偿，但继续输出未修正测量值并给出明确告警原因。

not_applicable_orientation
  当前模型只标定正向；调头数据不应用该漂移模型。
```

V3 不再设置 `0.25～0.65` 的无效死区。正常要求幅度绝对值不超过 0.25，并且整棒相对正常参考曲线的 RMSE 不超过 0.15 mm；即使已知漂移投影接近零，明显偏离正常参考的全新模式也必须告警。已知异常模式从幅度 0.30 起连续修正，当前由同棒正常/异常数据验证的幅度上限为 2.00；相关性至少 0.95，拟合 RMSE 不超过 0.08 mm，四相机幅度极差不超过 0.30。超出幅度范围的现场样本必须先取得同棒正常HOBJ或人工真值，禁止只为了出数而放宽。

`unmatched_unadjusted` 只是“继续出数”的安全回退，不等于确认机械正常。页面和 CSV 必须保留告警，禁止将其静默改名为正常，也禁止对它应用异常修正。

## 4. 数据追溯

每个切片 CSV 同时保留：

```text
drift_raw_A/B/C/D_mm、drift_raw_diag1/diag2...
A/B/C/D_mm、diag1/diag2...（漂移识别后的最终值）
drift_status、measurement_valid
drift_detected、drift_correction_applied
drift_model_version、drift_amplitude、drift_confidence
drift_fit_rmse_mm、drift_correlation
drift_sample_station_count、drift_overlap_start_fraction、drift_overlap_end_fraction
drift_alignment_shift_fraction、drift_warning、drift_reason
obj1..obj4_drift_x_mm / drift_z_mm / drift_amplitude
```

端面也保留 `drift_raw_*` 输入结果；命中异常后，端面边界点先在局部坐标层应用同一漂移曲线，再进入端面拟合和端面标定。

## 5. Web 与手工补偿

Web 连续测量对每个 HOBJ 独立分类，允许正常、异常、正常交替出现，无需人工切换模式。页面显示漂移状态、共同幅度、置信度、RMSE、模型版本及四台相机平均漂移。

手工补偿支持 A/B/C/D、D1/D2、棒长及八个端面角。手工补偿只影响页面最终值和后续统计，不覆盖切片 CSV、机械漂移模型或相机标定模型。

## 6. 重建命令

```powershell
python tools\build_mechanical_drift_model.py `
  --normal-dir "tools\calibration\hobj\2606005B22-CTB_3" `
  --abnormal-dir "tools\calibration\hobj\2606005B22-CTB yichang_3" `
  --output "tools\calibration\models\mechanical_drift_model_210_105.json" `
  --bar-id "2606005B22-CTB_3" `
  --specification "210_105" `
  --orientation normal
```
