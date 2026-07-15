# 测量几何原理与长期约定

本文记录方棒测量的几何定义、输出字段含义和已确认的工作经验。修改算法或解释结果时，以本文和 `AGENTS.md` 为准。

## 1. 标定铁律

1. 横截面尺寸标定如需人工尺寸，只允许使用一根明确棒号的标准棒、多次有效HOBJ和该棒尺寸真值；端面角度链路不得加载机构或人工端面角真值。
2. 不得混合棒号、人工真值或残差；CTB等第二根棒只能做固定参数外部验证，不得反向参与专业棒端面参数拟合。
3. 规格改变时，必须为新规格新增独立的单棒标定模型。
4. `tools\calibration\models\` 可保存多个模型文件；每个文件必须明确对应一个规格、一个物理标准棒和适用测量条件。
5. 端面v15固定分层：M0为Raw基线；M1只含一个图像可辨识非平面Y同步模态；M2仿射模态只诊断、永不运行。M1未同时通过固定盲测、交叉验证、相机状态和跨棒不重拟合验证时，Web必须使用M0。
6. M0 Raw质量门禁不得改数：端面边界平面RMSE超过`0.50 mm`为拒测；同面双相机拼接变化`<=0.60 mm`为通过、`0.60~0.80 mm`为不确定、`>=0.80 mm`为告警。所有状态均保留16角和两个代表角，且固定记录`uses_professional_truth=false`、`correction_applied=false`。

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

端面与每个侧面的夹角由带方向的平面法向量直接计算；不需要显式求两个平面的交线：

```text
θ_face = arccos(n_face · n_end)，范围0..180°
```

平面法向方案每个端面得到四个兼容角：

```text
head/tail_[A-D]_endface_angle_deg
```

旧版“机构真值减raw得到8个最终角度偏移”的方案已永久退役。下面公式只用于事故追溯，禁止在生产、标定或Web链路中实现：

```text
禁止：corrected_angle = raw_angle + per_channel_answer_offset
```

当前v13+只允许在三维点云拟合前应用可解释的相机扫描行同步参数；同一相机Y偏移必须同时作用于其纵向侧面点和端面边界点，再用共享生产线框几何计算16个局部夹角：

```text
side_point.cameraY += calibrated_camera_scan_row_offset
boundary_point.cameraY += calibrated_camera_scan_row_offset
corrected_side_planes = fit_four_side_planes(corrected_side_points)
corrected_end_edges = fit_four_equal_camera_weighted_edges(corrected_boundary_points)
corrected_ridges = fit_four_longitudinal_corner_ridges(corrected_side_points)
corrected_local_angles = project_edges_and_ridges_into_each_physical_face()
```

四相机零和Y偏移必须分解为一个图像共面性识别的非平面模态和两个机构标准棒约束的线性倾斜模态；两个端面、四个面和未调头/物理调头两个姿态共享同一组参数，禁止逐通道自由度。模型必须保留raw与物理修正值；运行时不得读取专业机构真值，不得用标准棒指纹选择方向或把结果拉回机构值。v13还必须保存标定时同面双相机拼接状态包络与机构棒长对Y行距的独立审计，并通过共享生产几何的16角固定盲测和配对交叉验证。当前HOBJ只有在四个面的拼接中位数、跨度和夹角均命中包络且机械漂移状态可解释时，才能应用Y同步参数。未命中时保留未修正几何并记录 `state_unmatched_unadjusted`，禁止强套。两种物理姿态设备参数不稳定、机械漂移与真实倾斜无法区分时必须告警或拒测。

### 5.1 四边线框端面算法（Web主显示）

为避免把不共面的四个角强行压成一张“魔毯平均平面”，生产测量同时直接重建端面的四条边：

1. 每条端面边由看见同一物理面的两台相机分别拟合，再按相机等权合成一条三维边线。
2. `P1/P2/P3/P4` 沿整根棒的轨迹分别拟合成四条纵向棱线。
3. 在对应侧面内，将一条端面边线与它两端的两条纵向棱线分别求夹角。
4. 一个端面得到8个局部角，头尾共16个；不再用一个端面平均平面替代这些局部差异。

```text
head/tail_A_left/right_endface_angle_deg
head/tail_B_top/bottom_endface_angle_deg
head/tail_C_top/bottom_endface_angle_deg
head/tail_D_left/right_endface_angle_deg
```

每个字段同时保留 `*_raw_angle_deg`。只有当前相机状态命中已放行物理模型时，产品字段才允许使用同一组低层Y同步参数重建；否则产品值保持未修正几何并明确告警，不能填入最终角度偏移。

每端最终代表值为8个局部角中**实际偏离90°最远的那个角度本身**，例如显示 `89.153°`，而不是显示误差 `0.847°`，也不是8角平均：

```text
head/tail_endface_representative_angle_deg
head/tail_endface_worst_local_channel
```

同时输出双相机边线拼接差、相邻边角点闭合差、8角RMS和线框对角扭曲量。这些字段用于识别相机状态、坏角和“魔毯”离散，不能反向作为补偿量。

当前HOBJ的 `head/tail` 定义为设备空间最小行/最大行，不是电机扫描起点/终点。真实棒体绕A/D轴端对端旋转后，固定设备有向角必须由点云自然满足：

```text
head ↔ tail
机构B ↔ C，A/D不变
θ_after = 180° - θ_before
```

不得根据文件夹名或最终角度在Web/CSV层硬交换。没有独立真实调头HOBJ等变验证时，模型只能是工程候选，生产运行和打包必须拒绝。

旧版Web和CSV仍保留端面与四个物理面夹角的算术平均兼容字段：

```text
head/tail_endface_verticality_deg
= mean(θ_A, θ_B, θ_C, θ_D)
```

该遗留字段名继续保留以兼容已有 CSV 读取逻辑，但数值不再计算 `abs(90-θ)`。例如四个夹角为 `89.90/90.02/89.95/90.01°`，保存值为 `89.97°`。新版Web主卡片使用上面的线框代表实测角，不再把该平均值当最终端面质量。

它不要与 `head/tail_endface_plane_verticality_deg` 混用；后者是端面拟合平面法向量相对棒轴的夹角。由于相对侧面夹角近似互补，四面算术平均天然接近90°，只能作为兼容显示字段，不能作为端面质量或标定准确度的主要证据。

## 6. 数据保存与连续测量

仪表盘启动后默认待机。用户点击“开始连续测量”后，递归扫描 HOBJ：

1. 每个未成功处理的稳定 HOBJ 运行一次。
2. 成功处理后，文件的路径、修改时间和大小写入 `tools\dashboard_continuous_seen.local.json`。
3. 未变更文件不重复运行；新增或更新文件在稳定后重新进入队列。
4. 再次点击同一按钮停止后续任务；当前已经启动的单张测量可完成。

保存规则：

```text
用户最终统计：<output_dir>\user_results\measurement_statistics.csv
用户补偿日志：<output_dir>\user_results\compensation_change_log.csv
研发切片明细：<output_dir>\developer_details\*_measure.csv
交付包原始结果：<output_dir>\developer_details\*_delivery_geometry.json
```

统计数据先写入 `developer_details\measurement_statistics.sqlite`，然后刷新
`user_results\measurement_statistics.csv` 快照。统计表每次测量追加一行，只保存页面可见的
最终汇总值、必要识别列和研发切片 CSV 路径。若 CSV 正被 WPS/Excel 锁定，SQLite 仍会记录
新数据；关闭 CSV 后连续监控会自动刷新快照。旧版直接放在结果根目录的文件会自动迁移到
上述两个目录，并同步修正统计库里的切片路径。

每次实际改变 A/B/C/D、D1/D2 或棒长补偿时，审计日志逐项记录修改时间、旧补偿、
新补偿、变化量，以及存在最近一次测量时的 raw 值、修改前显示值和修改后显示值。
耐文件锁的内部日志库保存为 `developer_details\compensation_change_log.sqlite`，用户查看的
CSV 快照保存为 `user_results\compensation_change_log.csv`。未发生数值变化时不写日志。

历史完整测量仪表盘仍保留用户明确输入的A/B/C/D、对角线和棒长显示补偿。所有模式都永久禁止加载、显示、保存或应用八个手工最终角度偏移；端面角只能来自当前HOBJ raw几何或已验证的低层物理设备参数重建。任何显示补偿都不得冒充标定模型或覆盖切片CSV。

## 10. 现场全局Web的交付几何覆盖（2026-07-15）

`SunshineSiRod_Global_DeliveryGeometry_20260715` 的产品汇总采用
`方棒尺寸几何算法交接包_对角线弧长侧边夹角_20260715`：

- A/B/C/D：头部前5个和尾部后5个有效标定截面分别平均，再取头尾平均。
- 对角线1/2：交付包在名义方向正负5度内搜索倒角邻域点云的平行卡尺最大支撑距离；头5、尾5分别平均后再取头尾平均。
- 弧长：交付包当前定义为倒角弦长，不是曲线积分长度。
- 投影1/2：交付包当前按倒角弦长的45度投影输出，两项通常相等。
- 四角主面夹角：交付包将同一侧面的两台相机点云共同拟合方向，再计算相邻侧面夹角。

Web角号固定为 `角1=AB/obj1/P1/右上`、`角2=CD/obj2/P2/左下`、
`角3=AD/obj3/P3/左上`、`角4=BC/obj4/P4/右下`。页面按真实方棒位置画出四张独立
角部示意图，并明确标注角号、相机号、点号和相邻面。兼容字段
`diag1_M1_M2_mm`、`diag2_M3_M4_mm` 和
`objN_chamfer_mm/projection_x_mm/projection_y_mm` 在该现场包中承载上述交付算法结果，
不再表示本文件前文的原生中点对角线和物理X/Y投影。全局运行时不再先运行本项目原生
几何算法，也不再输出 `native_*` 产品字段；A/B/C/D、对角线、弧长、投影、主面夹角和
棒长都只采用交付包结果。端面区域暂时留空，等待新的端面交付算法。
