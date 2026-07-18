# 方棒边长与固定方向对角线工程算法交接包

版本：`2026-07-16-fixed-direction-v1`

本包只包含以下内容：

- A、B、C、D四条边长；
- D1、D2两条固定方向卡尺对角线；
- CTB 13_08单次正扫的75切片基础标定；
- CTB三正三反六次扫描求得的唯一全局互换修正参数k；
- 正反扫描的物理对齐、重复性、掉头残差和互换显著度。

不包含弧长、弧长投影、角度、端面、垂直度、长度或其他物理量。

## 1. 直接测量新图像

安装环境：

```powershell
python -m pip install -r requirements.txt
```

单次扫描：

```powershell
python src/measure.py single `
  --scan-dir "D:\data\NEW-BAR\21_05" `
  --specimen "NEW-BAR" `
  --direction positive `
  --scan-id 21_05 `
  --output-dir "D:\results\NEW-BAR_21_05"
```

批量扫描：

```powershell
python src/measure.py batch `
  --manifest examples/input_manifest_template.csv `
  --output-dir "D:\results\batch_001"
```

每个扫描目录必须有四幅二维浮点轮廓TIFF：

- `1-Left.tif`
- `2-Right.tif`
- `3-Top.tif`
- `4-Down.tif`

也兼容以`_1.tif`至`_4.tif`结尾的旧命名。

## 2. 主要结果

- `slice_measurements.csv`：逐切片原始值、k修正后A/B/C/D/D1/D2；
- `scan_summary.csv`：每次扫描的整棒双侧10%截尾均值；
- `aligned_detail.csv`：正反扫描按同一物理量对齐后的明细、均值、极差和均值差；
- `specimen_summary.csv`：每根棒的BD差值、BD互换显著度、D1-D2差值、对角线互换显著度；
- `measurement_result.json`：程序接口用结构化结果；
- `run_audit.json`：算法版本、标定文件哈希、参数和图像来源。

## 3. 冻结标定

- 基础标定：`calibration/current_calibration.json`
- 全局互换修正：`calibration/global_exchange_calibration.json`
- 标定数据来源：`calibration/calibration_sources.csv`
- CTB六次标定扫描清单：`calibration/exchange_calibration_manifest.csv`

生产冻结值：

- `k = 0.013402766012028167 mm`
- B、D1每个切片统一加`-k/2 = -0.006701383006014083 mm`
- D、D2每个切片统一加`+k/2 = +0.006701383006014083 mm`
- A、C不修正。

## 4. 交接阅读顺序

1. `docs/01_算法规格.md`
2. `docs/02_标定与掉头修正.md`
3. `docs/03_输入输出接口.md`
4. `docs/04_对角线与人工卡尺.md`
5. `docs/05_验证与发布边界.md`
6. `docs/AI继续编程提示词.md`

## 5. 重要边界

CTB本身是基础标定和k标定的数据源，所以CTB结果不是独立验证。标定棒、104 NG-1、A61、B22-CTT均未进入本版标定，可作为外部验证数据。本包没有任何按棒子调参、逐棒补偿、对角线专用补偿或利用输出反推输入的策略。
