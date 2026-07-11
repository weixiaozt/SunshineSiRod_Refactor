# Project Memory

本文件是项目当前状态的短版记录，方便下次继续工作时快速接上。

## 当前状态

- 仓库路径：`D:\ProjectCode\SunshineSiRod_Refactor`
- 主分支：`main`
- 远端：`git@github.com:weixiaozt/SunshineSiRod_Refactor.git`
- 核心脚本：`tools\measure_square_rod_edges.py`
- Web 仪表盘：`tools\measurement_dashboard.py`
- 本地 UI 配置：`tools\web_ui_config.local.json`，不提交。

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

当前三根棒：

```text
2606005B22-CTB_3 : 3 张 hobj
2606005B22-CTT_3 : 2 张 hobj
2606006A61-DTB_3 : 2 张 hobj
```

目录规则：

```text
tools\calibration\hobj\<bar_id>\<capture>.hobj
```

同一文件夹下多张 HOBJ 是同一根棒的重复拍摄。代码会把 `CTT _3` 这种下划线前多空格的文件夹名规范化为 `CTT_3`。

人工真值 CSV：

```text
tools\calibration\truth\manual_calibration_truth.csv
```

三根棒的端面角度数据已完整：每根 `head/tail x A/B/C/D x 3`，共 24 条。`cross_section` 的 9 行 A/B/C/D 目前仍为空，填完前不能重建横截面相机模型。

## 新端面标定逻辑

端面模型路径：

```text
tools\calibration\models\endface_calibration_model.json
```

逻辑：

1. 每张 HOBJ 测出 8 个端面夹角。
2. 同一根棒的重复拍摄先平均。
3. 每根棒求 `人工真值 - raw`。
4. 多根棒等权平均，避免重复次数多的棒子权重过大。

当前模型为：

```text
version = 3
model = endface_face_angle_offset
bar_weighting = equal_per_bar_then_equal_per_repeat
```

## 常用验证

```powershell
python -m py_compile tools\measure_square_rod_edges.py tools\measurement_dashboard.py
python -m unittest tools.test_endface_perpendicularity -v
```

仅重建端面模型：

```powershell
python tools\measure_square_rod_edges.py --input "D:\ProjectCode\SunshineSiRod_Refactor\tools\calibration\hobj" --calibration "D:\ProjectCode\SunshineSiRod_Refactor\tools\calibration\models\camera_calibration_model.json" --calibration-truth-csv "D:\ProjectCode\SunshineSiRod_Refactor\tools\calibration\truth\manual_calibration_truth.csv" --save-endface-calibration "D:\ProjectCode\SunshineSiRod_Refactor\tools\calibration\models\endface_calibration_model.json" --output "D:\ProjectCode\SunshineSiRod_Refactor\tools\results\calibration\endface_sanity_all_bars.csv" --overwrite --step-mm 10
```

等 A/B/C/D 填完后，运行完整标定：

```powershell
python tools\measure_square_rod_edges.py --input "D:\ProjectCode\SunshineSiRod_Refactor\tools\calibration\hobj" --calibration-truth-csv "D:\ProjectCode\SunshineSiRod_Refactor\tools\calibration\truth\manual_calibration_truth.csv" --save-calibration "D:\ProjectCode\SunshineSiRod_Refactor\tools\calibration\models\camera_calibration_model.json" --save-endface-calibration "D:\ProjectCode\SunshineSiRod_Refactor\tools\calibration\models\endface_calibration_model.json" --output "D:\ProjectCode\SunshineSiRod_Refactor\tools\results\calibration\calibration_sanity_all_bars.csv" --overwrite --step-mm 10
```
