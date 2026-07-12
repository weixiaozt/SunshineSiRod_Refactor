# Project Memory

2026-07-12：新增独立的正向四相机机械漂移模型 `tools\calibration\models\mechanical_drift_model_210_105.json`。正式拟合仅使用同一根 CTB 标准棒正常3张与异常2张；每个 HOBJ 独立判定 normal / abnormal_corrected / unknown_invalid。详细规则见 `tools\calibration\mechanical_drift_principles.md`。Web 已开放棒长手工补偿。

详细测量几何、倒角/端面计算和连续测量保存规则：`tools\calibration\measurement_geometry_principles.md`。

## 标定铁律（长期有效）

- **只允许单棒、多次拍摄、多图标定。** 每份标定模型只能由一根已明确棒号的标准棒、多次有效拍摄和该棒人工真值拟合得到。
- 不得混合多根棒的 HOBJ、人工真值或补偿残差；不得以跨棒平均、等权、加权或其他方式生成模型。
- **规格一旦变化，必须新增该规格的单棒标定。** 新棒号、不同规格或不同测量条件必须独立采集、独立拟合、独立保存模型，不能向既有单棒模型追加数据。
- `tools\calibration\models\` 允许并存多个标定模型文件；每个文件必须清楚标识棒号、规格和适用测量条件，且只能对应一个物理标准棒。

## 2026-07-11 双向（调头）标定优化

### 默认模型切换（2026-07-12）

- 默认相机/端面模型已切换为 `2606005B22-CTB_3` 单棒模型，仅使用该棒正向3张、调头3张与该棒人工真值拟合。
- 两份默认 JSON 均含 `single_bar_metadata.scope = single_bar_only`；`captured_bar_ids` 仅允许 `2606005B22-CTB_3`。
- Web 默认路径不变，但只能用于这根棒及其相同测量条件。

- 调头的已确认映射：棒长位置 `15-25 <-> 70-80`、端面 `head <-> tail`、面号 `A -> A / B <-> D / C -> C`。
- 修复了调头目录识别：标记可能位于父目录；`<bar_id> diaotou_3` 归一为同一物理 `bar_id`，捕获 ID 保留 `turnover/` 以避免重名。
- 相机标定升级为 version 7，normal/turnover 分别保存方向专用变换；端面模型升级为 version 4，normal/turnover 分别保存8个角度偏移。
- 当前有效标定集：`2606005B22-CTB_3` 正向3张、调头3张；其他棒号的历史采集不得参与默认模型拟合。
- 双向复测：正向横截面 MAE 0.063402 mm、调头横截面 MAE 0.052855 mm；正向端面角 MAE 0.025014°、调头端面角 MAE 0.029772°。
- 倒角由拟合倒角线与两条主面求交得到；物理投影定义为 `X=|P-T2|`（沿 L2）、`Y=|P-T1|`（沿 L1）。横截面“垂直度”字段现输出 L1/L2 在 P 的实际夹角，不再减 90°。

本文件是项目当前状态的短版记录，方便下次继续工作时快速接上。

## 当前状态

- 仓库路径：`D:\ProjectCode\SunshineSiRod_Refactor`
- 主分支：`main`
- 远端：`git@github.com:weixiaozt/SunshineSiRod_Refactor.git`
- 核心脚本：`tools\measure_square_rod_edges.py`
- Web 仪表盘：`tools\measurement_dashboard.py`
- 本地 UI 配置：`tools\web_ui_config.local.json`，已按用户要求提交；换电脑后检查绝对路径。
- 生产图像根目录：`D:\Image_risen`，Web 会递归扫描所有子文件夹中的 HOBJ。
- Web 测量同时使用横截面模型和端面模型。
- Web 启动时不会自动测量；点击“开始连续测量”后才会依次处理所有未处理 HOBJ，再次点击停止后续扫描。已完成且未变更的文件不重复运行，新增或更新文件会在稳定后自动加入。每张图保留一份切片 CSV，所有统计结果统一追加至输出目录的 `measurement_statistics.csv`。

## 已确认的测量定义

```text
P3 ---- A ---- P1
|              |
D              B
|              |
P2 ---- C ---- P4
```

```text
A = distance(P3, P1)
B = distance(P1, P4)
C = distance(P2, P4)
D = distance(P3, P2)
```

对角线按倒角中点计算：

```text
diag1 = distance(M1, M2)
diag2 = distance(M3, M4)
```

不要强制 `A=C` 或 `B=D`，不要把结果拉回规格值。

## 标定数据

标定 HOBJ 目录：

```text
tools\calibration\hobj
```

默认模型的唯一标定棒：

```text
2606005B22-CTB_3 : 正向 3 张有效 HOBJ，调头 3 张有效 HOBJ
```

目录规则：

```text
tools\calibration\hobj\<bar_id>\<capture>.hobj
```

同一根标准棒的文件夹下可放多张 HOBJ，作为重复拍摄参与该棒的单棒多图拟合。其他棒号的历史数据只能留作排障或独立建模，不能与该棒混合拟合。

人工真值 CSV：

```text
tools\calibration\truth\manual_calibration_truth.csv
```

默认模型只使用 `2606005B22-CTB_3` 的人工真值及其正向、调头有效 HOBJ；真值 CSV 中其他棒号的历史记录不得参与拟合。

## 新端面标定逻辑

端面模型路径：

```text
tools\calibration\models\endface_calibration_model.json
```

逻辑：

1. 每张 HOBJ 测出 8 个端面夹角。
2. 对该同一根棒的多次有效拍摄分别测量，并在同一方向内求平均。
3. 用该棒对应人工真值均值减去 raw 均值，得到该方向的角度偏移。
4. 正向与调头分别保存；不执行跨棒平均或任何多棒加权。

当前模型为：

```text
version = 4
model = endface_face_angle_offset
scope = single_bar_only
```

## 常用验证

```powershell
python -m py_compile tools\measure_square_rod_edges.py tools\measurement_dashboard.py
python -m unittest tools.test_endface_perpendicularity -v
```

仅重建端面模型：

```powershell
python tools\measure_square_rod_edges.py --input "D:\ProjectCode\SunshineSiRod_Refactor\tools\calibration\hobj\2606005B22-CTB_3" --calibration "D:\ProjectCode\SunshineSiRod_Refactor\tools\calibration\models\camera_calibration_model.json" --calibration-truth-csv "D:\ProjectCode\SunshineSiRod_Refactor\tools\calibration\truth\manual_calibration_truth.csv" --save-endface-calibration "D:\ProjectCode\SunshineSiRod_Refactor\tools\calibration\models\endface_calibration_model.json" --output "D:\ProjectCode\SunshineSiRod_Refactor\tools\results\calibration\endface_sanity_ctb_3_single_bar.csv" --overwrite --step-mm 10
```

运行完整标定：

```powershell
python tools\measure_square_rod_edges.py --input "D:\ProjectCode\SunshineSiRod_Refactor\tools\calibration\hobj\2606005B22-CTB_3" --calibration-truth-csv "D:\ProjectCode\SunshineSiRod_Refactor\tools\calibration\truth\manual_calibration_truth.csv" --save-calibration "D:\ProjectCode\SunshineSiRod_Refactor\tools\calibration\models\camera_calibration_model.json" --save-endface-calibration "D:\ProjectCode\SunshineSiRod_Refactor\tools\calibration\models\endface_calibration_model.json" --output "D:\ProjectCode\SunshineSiRod_Refactor\tools\results\calibration\calibration_sanity_ctb_3_single_bar.csv" --overwrite --step-mm 10
```
