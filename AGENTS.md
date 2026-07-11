# AGENTS.md

本文件给后续接手本项目的编码代理/工程师使用，记录当前测量定义、标定逻辑、运行方式和注意事项。

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
角度 = angle(L1, L2)
角垂直度误差 = abs(90 - 角度)
倒角长度 = distance(T1, T2)
投影X = abs(T2.x - T1.x)
投影Z = abs(T2.z - T1.z)
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

同一个 `<bar_id>` 文件夹下的多张 `.hobj` 视为同一根标准棒的重复拍摄。当前数据为：

```text
2606005B22-CTB_3 : 3 张
2606005B22-CTT_3 : 2 张
2606006A61-DTB_3 : 2 张
```

程序会规范化棒号中下划线前后的多余空格，例如 `2606005B22-CTT _3` 会匹配 CSV 中的 `2606005B22-CTT_3`。

## 人工真值 CSV

统一真值文件：

```text
tools\calibration\truth\manual_calibration_truth.csv
```

字段包括：

```text
record_type,bar_id,position_percent,end,face,position,A_mm,B_mm,C_mm,D_mm,angle_deg
```

`record_type=cross_section` 用于横截面 A/B/C/D 标定。每根棒至少需要 3 个位置，例如：

```text
15-25
45-55
70-80
```

每个位置必须填写 `A_mm/B_mm/C_mm/D_mm`。截至本次更新，CSV 中三根棒的 `endface_angle` 数据完整，但 `cross_section` 的 A/B/C/D 仍为空，因此不能重建 `camera_calibration_model.json`。

`record_type=endface_angle` 用于端面角度补偿。每根棒需要：

```text
2 个端面(head/tail) x 4 个面(A/B/C/D) x 3 个测点 = 24 条
```

当前三根棒的端面角度真值均已齐全。

## 标定模型

横截面相机模型：

```text
tools\calibration\models\camera_calibration_model.json
version = 6
model = camera_oriented_transform_with_corner_bias
```

该模型负责相机局部坐标到全局横截面坐标的方向、平移和角点残差修正。它不是业务规格补偿模型。

端面模型：

```text
tools\calibration\models\endface_calibration_model.json
version = 3
model = endface_face_angle_offset
```

端面模型逻辑：

1. 每张 HOBJ 先测出 `head/tail x A/B/C/D` 8 个 raw 夹角。
2. 同一根棒的重复拍摄先求平均。
3. 每根棒计算 `人工真值均值 - raw 均值`。
4. 多根棒之间等权平均，避免 3 次重复拍摄的棒子压过 2 次重复拍摄的棒子。

模型 JSON 会记录：

```text
captured_bar_ids
unused_truth_bar_ids
per_bar.capture_count
per_bar.mean_raw_angles_deg
per_bar.manual_truth_angles_deg
per_bar.angle_offsets_deg
```

## 端面垂直度输出

端面结果有两类字段：

```text
*_endface_plane_verticality_deg
head/tail_[A-D]_endface_angle_deg
head/tail_endface_verticality_deg
```

其中 `head/tail_endface_verticality_deg` 是四个面与端面夹角相对 90 度的绝对误差平均值，更贴近用户用量规按四个面测端面的习惯。

## 常用命令

语法和单元测试：

```powershell
python -m py_compile tools\measure_square_rod_edges.py tools\measurement_dashboard.py
python -m unittest tools.test_endface_perpendicularity -v
```

仅重建端面补偿模型，适用于 cross_section A/B/C/D 尚未填完的情况：

```powershell
python tools\measure_square_rod_edges.py --input "D:\ProjectCode\SunshineSiRod_Refactor\tools\calibration\hobj" --calibration "D:\ProjectCode\SunshineSiRod_Refactor\tools\calibration\models\camera_calibration_model.json" --calibration-truth-csv "D:\ProjectCode\SunshineSiRod_Refactor\tools\calibration\truth\manual_calibration_truth.csv" --save-endface-calibration "D:\ProjectCode\SunshineSiRod_Refactor\tools\calibration\models\endface_calibration_model.json" --output "D:\ProjectCode\SunshineSiRod_Refactor\tools\results\calibration\endface_sanity_all_bars.csv" --overwrite --step-mm 10
```

cross_section A/B/C/D 填完后，重建横截面相机模型和端面模型：

```powershell
python tools\measure_square_rod_edges.py --input "D:\ProjectCode\SunshineSiRod_Refactor\tools\calibration\hobj" --calibration-truth-csv "D:\ProjectCode\SunshineSiRod_Refactor\tools\calibration\truth\manual_calibration_truth.csv" --save-calibration "D:\ProjectCode\SunshineSiRod_Refactor\tools\calibration\models\camera_calibration_model.json" --save-endface-calibration "D:\ProjectCode\SunshineSiRod_Refactor\tools\calibration\models\endface_calibration_model.json" --output "D:\ProjectCode\SunshineSiRod_Refactor\tools\results\calibration\calibration_sanity_all_bars.csv" --overwrite --step-mm 10
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

`tools\web_ui_config.local.json` 是本地配置，不应提交。仪表盘的手工补偿只影响页面显示和 dashboard summary，不改写 raw CSV，也不改写标定模型。

## 当前限制

1. 当前三根棒的端面角度数据完整，端面补偿模型已可生成。
2. 当前 `manual_calibration_truth.csv` 的 `cross_section` A/B/C/D 为空，横截面相机模型暂时不能用这三根棒重建。
3. 填完 9 行 cross_section 的 A/B/C/D 后，需要运行完整标定命令并再次提交更新后的 `camera_calibration_model.json`。
