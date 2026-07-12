# 测量几何原理与长期约定

本文记录方棒测量的几何定义、输出字段含义和已确认的工作经验。修改算法或解释结果时，以本文和 `AGENTS.md` 为准。

## 1. 标定铁律

1. 每份模型只允许使用一根明确棒号的标准棒、多次有效 HOBJ 和该棒人工真值。
2. 不得混合棒号、人工真值或残差；不得跨棒平均、加权或追加拟合。
3. 规格改变时，必须为新规格新增独立的单棒标定模型。
4. `tools\calibration\models\` 可保存多个模型文件；每个文件必须明确对应一个规格、一个物理标准棒和适用测量条件。

## 2. 坐标与横截面

棒长扫描方向为 `Y`。在一个固定的 `Y` 切片中，每台相机提供局部二维轮廓 `z(x)`：

```text
Z（高度）
^
|  轮廓 z(x)
|
+-----------------> X（相机横向）
```

横截面物理点和边定义：

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

不强制 `A=C` 或 `B=D`，不按规格窗口过滤真实偏差。

## 3. 单角、倒角与主面夹角

每个相机、每个 `Y` 切片，先平滑有效轮廓；在角两侧的稳定区域鲁棒拟合两条主面 `L1`、`L2`，并在过渡段鲁棒拟合倒角线 `Lc`：

```text
P  = L1 ∩ L2        理论尖角
T1 = L1 ∩ Lc        倒角与 L1 的交点
T2 = L2 ∩ Lc        倒角与 L2 的交点
M  = (T1 + T2) / 2  倒角中点
```

```text
                 T1 ●
                    |\\
                 L1 | \\ Lc（倒角）
                    |  \\
理论尖角 P       ●--+---● T2
                       L2
```

输出定义：

```text
主面夹角 θ = angle(L1, L2)
倒角长度   = distance(T1, T2)
```

`objN_verticality_error_deg` 是遗留字段名；当前它输出的是 `θ` 本身，即 L1/L2 在 P 的实际夹角，**不再计算** `abs(90 - θ)`。

## 4. 倒角的物理 X/Y 投影

倒角投影以理论尖角和两条主面为物理坐标基准，不使用相机局部传感器 X/Z 差：

```text
物理 X 投影 = |P - T2|，沿 L2
物理 Y 投影 = |P - T1|，沿 L1
```

对应 CSV 字段：

```text
objN_projection_x_mm = |P - T2| = objN_chamfer_face2_setback_mm
objN_projection_y_mm = |P - T1| = objN_chamfer_face1_setback_mm
```

当 `L1` 与 `L2` 的交角为 90° 时：

```text
倒角长度² = 物理 X 投影² + 物理 Y 投影²
```

但仅有主角为 90° 不代表两个投影相等；只有倒角相对两主面对称（典型为两个 45° 角）时，物理 X/Y 投影才相等。

## 5. 端面与四个侧面的垂直度

头端面和尾端面分别使用端部附近、四相机采集到的三维点云拟合空间平面：

```text
Y = ax * X + az * Z + c
n_end = normalize(-ax, 1, -az)
```

实际棒轴 `r` 由多条有效切片的横截面中心轨迹拟合，不假定它完全等于扫描 `Y` 轴。

每一个侧面由对应横截面边沿棒轴延伸形成空间平面。例如 A 面：

```text
e_A   = P1 - P3
n_A   = normalize(e_A × r)
```

四个侧面边为：

```text
A: P3 -> P1
B: P1 -> P4
C: P2 -> P4
D: P3 -> P2
```

端面与每个侧面的夹角由平面法向量直接计算；不需要显式求两个平面的交线：

```text
θ_face = arccos(|n_face · n_end|)
```

每个端面得到四个角：

```text
head/tail_[A-D]_endface_angle_deg
```

端面模型以该端、该面三次人工角度真值的均值减去 raw 均值，得到独立的角度偏移；最终角度为：

```text
corrected_angle = raw_angle + calibrated_offset
```

供量规习惯使用的端面垂直度为四面相对 90° 的平均绝对偏差：

```text
head/tail_endface_verticality_deg
= mean(|90 - θ_A|, |90 - θ_B|, |90 - θ_C|, |90 - θ_D|)
```

它不要与 `head/tail_endface_plane_verticality_deg` 混用；后者是端面拟合平面法向量相对棒轴的夹角。

## 6. 数据保存与连续测量

仪表盘启动后默认待机。用户点击“开始连续测量”后，递归扫描 HOBJ：

1. 每个未成功处理的稳定 HOBJ 运行一次。
2. 成功处理后，文件的路径、修改时间和大小写入 `tools\dashboard_continuous_seen.local.json`。
3. 未变更文件不重复运行；新增或更新文件在稳定后重新进入队列。
4. 再次点击同一按钮停止后续任务；当前已经启动的单张测量可完成。

保存规则：

```text
每张图切片数据：<output_dir>\*_measure.csv
统一统计数据：<output_dir>\measurement_statistics.csv
```

统计数据先写入 `measurement_statistics.sqlite`，然后刷新唯一的 `measurement_statistics.csv` 快照。统计表每次测量追加一行，只保存页面可见的最终汇总值、必要识别列和切片 CSV 路径；不保存 raw/corrected/manual 补偿等内部明细。若 CSV 正被 WPS/Excel 锁定，SQLite 仍会记录新数据，测量不会失败或重复；关闭 CSV 后连续监控会自动刷新快照。切片原始 CSV 不会被覆盖。

仪表盘人工补偿可填写 A/B/C/D、对角线 1/2 和八个端面角偏移。其计算均为 `页面最终值 = 当前页面值 + 手工偏移`，只影响页面显示和后续统计，不改写切片 CSV 或标定模型。
