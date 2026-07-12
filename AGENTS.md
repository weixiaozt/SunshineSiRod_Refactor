# AGENTS.md

四相机机械漂移的模型分层、识别阈值、raw/corrected 字段和重建命令见：`tools\calibration\mechanical_drift_principles.md`。机械漂移模型必须独立于正常相机标定；禁止把正常/异常 HOBJ 混合平均。

本文件给后续接手本项目的编码代理/工程师使用，记录当前测量定义、标定逻辑、运行方式和注意事项。

详细的几何计算、字段定义与连续测量保存规则见：`tools\calibration\measurement_geometry_principles.md`。

## 双向标定（正向 / 调头）

当前默认模型只允许用于 `2606005B22-CTB_3` 单棒：

```text
tools\calibration\models\camera_calibration_model.json
tools\calibration\models\endface_calibration_model.json
```

两者均为 `single_bar_only`，仅由该棒正向3张与调头3张 HOBJ 加人工真值拟合。**单棒、多次拍摄、多图标定是铁律：不得混合多根棒的 HOBJ、人工真值或残差，不得跨棒平均、加权或追加拟合。规格一旦变化，必须新增该规格的单棒标定。** 新棒号、不同规格或不同测量条件必须独立采集、独立拟合和独立保存模型；不得将默认模型用于其他棒号或不同规格。

`tools\calibration\models\` 允许并存多个标定模型文件。每个文件名和 `single_bar_metadata` 必须标识棒号、规格与适用测量条件；一个模型文件只能对应一根物理标准棒，不能作为多棒共用模型。

调头的物理映射已确认：

```text
棒长位置：15-25 <-> 70-80，45-55 不变
端面：head <-> tail
横截面面号：A -> A，B <-> D，C -> C
```

标定目录中带 `调头` 或 `diaotou` 的文件/父目录会自动识别为调头采集；其文件夹名中的调头标记会从物理 `bar_id` 中去除。因此下面两类目录归属于同一根棒：

```text
<bar_id>_3
<bar_id> diaotou_3
```

相机模型 `version = 7` 与端面模型 `version = 4` 均保存 `normal`、`turnover` 两套子模型。测量命令支持 `--orientation auto|normal|turnover`；`auto` 只根据路径中的调头标记识别，生产路径没有标记时默认 `normal`。

## 倒角投影定义

`objN_projection_x_mm` 和 `objN_projection_y_mm` 是以两条物理主面为坐标轴的倒角投影：物理 X 为 `|P-T2|`、沿 `L2`；物理 Y 为 `|P-T1|`、沿 `L1`。它们不再使用相机局部传感器 X/Z 坐标差。

倒角端点改为由“倒角拟合线”与两条主面拟合线求交。新增字段：

```text
objN_chamfer_face1_setback_mm
objN_chamfer_face2_setback_mm
```

它们是从理论尖角到两条倒角端点、沿各自主面的距离；对于对称45°倒角，这两个值应接近。

## 工作原则

1. 不要强制 `A=C` 或 `B=D`，也不要把结果强行拉回规格值。
2. 所有测量都要保留 `raw` 和 `corrected` 两套概念；原始 CSV 不得被补偿值覆盖。
3. 补偿必须来自标准棒人工真值，并且要能追溯到真值 CSV、标定模型和版本。
4. 不要用业务假设硬编码补偿值，不要用规格窗口过滤真实偏差。
5. `.hobj`、现场图像、本地 UI 配置、运行日志和生成 CSV 不提交 Git。
6. 修改算法后至少运行语法检查、端面单元测试，并用 `tools\calibration\hobj` 做一次 sanity check。

## 当前核心文件

```text
tools\measure_square_rod_edges.py
tools\test_endface_perpendicularity.py
tools\measurement_dashboard.py
tools\calibration\hobj\
tools\calibration\truth\manual_calibration_truth.csv
tools\calibration\models\camera_calibration_model.json
tools\calibration\models\endface_calibration_model.json
tools\web_ui_config.example.json
tools\start_measurement_dashboard.bat
```

## 横截面定义

物理点位关系：

```text
P3 ---- A ---- P1
|              |
D              B
|              |
P2 ---- C ---- P4
```

相机和角点映射：

```text
obj1 -> P1 右上
obj2 -> P2 左下
obj3 -> P3 左上
obj4 -> P4 右下
```

边长定义：

```text
A = distance(P3, P1)
B = distance(P1, P4)
C = distance(P2, P4)
D = distance(P3, P2)
```

对角线定义已经由用户确认，必须按倒角中点计算：

```text
diag1 = distance(M1, M2)
diag2 = distance(M3, M4)
```

这里的 `M1/M2/M3/M4` 是各倒角线段 `T1-T2` 的中点，不是理论尖角 `P1/P2/P3/P4`。

## 单角算法

每个相机每个 row 是一条 2D 高度轮廓。算法从轮廓中提取：

```text
L1/L2 : 倒角两侧主面
Lc    : 倒角面
P     : L1 与 L2 延长线交点
T1/T2 : Lc 与两侧主面的交点
M     : (T1 + T2) / 2
```

输出：

```text
主面夹角 = angle(L1, L2)
`objN_verticality_error_deg` 为兼容旧字段名，现输出该主面夹角本身，不再计算 `abs(90 - 角度)`。
倒角长度 = distance(T1, T2)
物理投影X = distance(P, T2)，沿 L2
物理投影Y = distance(P, T1)，沿 L1
```

如果没有找到可靠倒角段，对角线中点会退回对应理论角点，倒角相关字段为空。

## 标定 HOBJ 目录约定

当前标定 HOBJ 放在：

```text
tools\calibration\hobj
```

新的目录逻辑是：

```text
tools\calibration\hobj\<bar_id>\<capture>.hobj
```

同一个 `<bar_id>` 文件夹下的多张 `.hobj` 视为同一根标准棒的重复拍摄。默认模型可用的标定数据为：

```text
2606005B22-CTB_3 : 正向 3 张有效拍摄，调头 3 张有效拍摄
```

其他棒号的历史 HOBJ 只能保留作排障或将来的独立单棒模型，绝不能与上述数据混合标定。

## 人工真值 CSV

统一真值文件：

```text
tools\calibration\truth\manual_calibration_truth.csv
```

字段包括：

```text
record_type,bar_id,position_percent,end,face,position,A_mm,B_mm,C_mm,D_mm,angle_deg
```

`record_type=cross_section` 用于单根标准棒的横截面 A/B/C/D 标定。该棒至少需要 3 个位置，例如：

```text
15-25
45-55
70-80
```

每个位置必须填写 `A_mm/B_mm/C_mm/D_mm`。默认模型只读取 `2606005B22-CTB_3` 对应真值；CSV 中其他棒号记录不得参与同一次拟合。

`record_type=endface_angle` 用于端面角度补偿。该单根标准棒需要：

```text
2 个端面(head/tail) x 4 个面(A/B/C/D) x 3 个测点 = 24 条
```

默认模型使用 `2606005B22-CTB_3` 的端面角度真值；其他棒号真值只能用于各自独立的单棒模型。

## 标定模型

横截面相机模型：

```text
tools\calibration\models\camera_calibration_model.json
version = 7
model = camera_oriented_transform_with_corner_bias
```

该模型负责相机局部坐标到全局横截面坐标的方向、平移和角点残差修正。它不是业务规格补偿模型。

端面模型：

```text
tools\calibration\models\endface_calibration_model.json
version = 4
model = endface_face_angle_offset
```

端面模型逻辑：

1. 每张 HOBJ 先测出 `head/tail x A/B/C/D` 8 个 raw 夹角。
2. 只保留一个明确 `bar_id` 的多次有效拍摄；同一方向的重复拍摄先求平均。
3. 以该棒的 `人工真值均值 - raw 均值` 计算该方向的偏移。
4. 正向与调头分别拟合、分别保存；禁止多棒平均、跨棒加权或跨棒残差合并。

模型 JSON 会记录：

```text
single_bar_metadata.scope = single_bar_only
single_bar_metadata.physical_bar_id
orientation_models.normal
orientation_models.turnover
```

## 端面垂直度输出

端面结果有两类字段：

```text
*_endface_plane_verticality_deg
head/tail_[A-D]_endface_angle_deg
head/tail_endface_verticality_deg
```

其中 `head/tail_endface_verticality_deg` 是四个面与端面夹角相对 90 度的绝对误差平均值，更贴近用户用量规按四个面测端面的习惯。它与横截面角部的主面夹角字段是不同定义。

## 常用命令

语法和单元测试：

```powershell
python -m py_compile tools\measure_square_rod_edges.py tools\measurement_dashboard.py
python -m unittest tools.test_endface_perpendicularity -v
```

仅重建端面补偿模型：

```powershell
python tools\measure_square_rod_edges.py --input "D:\ProjectCode\SunshineSiRod_Refactor\tools\calibration\hobj\2606005B22-CTB_3" --calibration "D:\ProjectCode\SunshineSiRod_Refactor\tools\calibration\models\camera_calibration_model.json" --calibration-truth-csv "D:\ProjectCode\SunshineSiRod_Refactor\tools\calibration\truth\manual_calibration_truth.csv" --save-endface-calibration "D:\ProjectCode\SunshineSiRod_Refactor\tools\calibration\models\endface_calibration_model.json" --output "D:\ProjectCode\SunshineSiRod_Refactor\tools\results\calibration\endface_sanity_ctb_3_single_bar.csv" --overwrite --step-mm 10
```

同时重建横截面相机模型和端面模型：

```powershell
python tools\measure_square_rod_edges.py --input "D:\ProjectCode\SunshineSiRod_Refactor\tools\calibration\hobj\2606005B22-CTB_3" --calibration-truth-csv "D:\ProjectCode\SunshineSiRod_Refactor\tools\calibration\truth\manual_calibration_truth.csv" --save-calibration "D:\ProjectCode\SunshineSiRod_Refactor\tools\calibration\models\camera_calibration_model.json" --save-endface-calibration "D:\ProjectCode\SunshineSiRod_Refactor\tools\calibration\models\endface_calibration_model.json" --output "D:\ProjectCode\SunshineSiRod_Refactor\tools\results\calibration\calibration_sanity_ctb_3_single_bar.csv" --overwrite --step-mm 10
```

测量单个 HOBJ：

```powershell
python tools\measure_square_rod_edges.py --input "<input.hobj>" --calibration "D:\ProjectCode\SunshineSiRod_Refactor\tools\calibration\models\camera_calibration_model.json" --endface-calibration "D:\ProjectCode\SunshineSiRod_Refactor\tools\calibration\models\endface_calibration_model.json" --output "<output.csv>" --overwrite --step-mm 10
```

## Web 仪表盘

运行：

```powershell
tools\start_measurement_dashboard.bat
```

或：

```powershell
python tools\measurement_dashboard.py
```

仪表盘只监听：

```text
http://127.0.0.1:8765
```

生产 HOBJ 根目录为 `D:\Image_risen`，文件列表会递归扫描所有子文件夹。仪表盘启动时只读取配置和文件列表，不会自动测量；用户点击“开始连续测量”后，才会依次运行所有未处理的稳定 HOBJ。再次点击同一按钮会停止后续扫描；已成功测量且未变更的 HOBJ 不会重复运行，只有新增或更新后的文件才会再次测量。Web 测量必须同时加载横截面与端面模型；对应路径由 `calibration_path` 和 `endface_calibration_path` 配置。

每张图的切片明细保留为独立的 `*_measure.csv`。统计数据先写入输出目录的 `measurement_statistics.sqlite`，再刷新唯一的 `measurement_statistics.csv` 快照。统计表只保留页面可见的最终汇总值、必要识别列和切片 CSV 路径，不写入 raw/corrected/manual 补偿等内部明细。若 CSV 正被 WPS/Excel 锁定，测量和去重仍会成功，关闭文件后连续监控会自动刷新 CSV 快照。仪表盘的手工边长、对角线和端面角补偿只影响页面最终值及后续统计，不改写切片 CSV，也不改写标定模型。

`tools\web_ui_config.local.json` 含本机绝对路径，已按用户要求提交；换电脑后必须检查并修改。

## 当前限制

1. `2606005B22-CTT_3/11_42.hobj` 存在随 row 增大的 A/C 反向拼接漂移，已移至 `tools\calibration\excluded_hobj`；原因记录在 `tools\calibration\excluded_captures.md`。
2. 默认模型的当前有效拟合集仅为 `2606005B22-CTB_3`：正向3张、调头3张 HOBJ。新增拍摄只能追加到这一棒的对应方向，且必须使用该棒人工真值重新拟合。
3. 其他棒号的历史数据与复测统计不适用于默认模型，也不得作为跨棒精度或补偿依据。
