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

模型在棒身 5%～95% 每隔 5% 保存四台相机的局部 `(X,Z)` 漂移。每张采集以 5% 位置为锚点消除摆放平移。检测时四相机共同拟合一个幅度系数，禁止每台相机自由缩放。

训练结果：

```text
正常幅度：-0.051 ～ 0.020
异常幅度： 0.994 ～ 1.006
异常拟合 RMSE：约 0.0027 mm
异常四相机幅度极差：约 0.011
历史 CTT 11_42 独立验证：幅度 1.124，相关性 0.99984
```

## 3. 逐 HOBJ 分类

```text
normal
  不应用漂移修正，直接使用正常相机标定。

abnormal_corrected
  四相机方向、曲线形状、共同幅度和残差均命中模型；先修正局部坐标。

unknown_invalid
  处于正常/异常死区、形状不匹配、相机幅度不一致或残差超限；结果无效并要求重测。

not_applicable_orientation
  当前模型只标定正向；调头数据不应用该漂移模型。
```

当前保守阈值：正常幅度绝对值不超过 0.25 且相对正常模板 RMSE 不超过 0.10 mm；异常共同幅度为 0.65～1.35；相关性至少 0.98；拟合 RMSE 不超过 0.03 mm；四相机幅度极差不超过 0.15。

## 4. 数据追溯

每个切片 CSV 同时保留：

```text
drift_raw_A/B/C/D_mm、drift_raw_diag1/diag2...
A/B/C/D_mm、diag1/diag2...（漂移识别后的最终值）
drift_status、measurement_valid
drift_detected、drift_correction_applied
drift_model_version、drift_amplitude、drift_confidence
drift_fit_rmse_mm、drift_correlation
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
