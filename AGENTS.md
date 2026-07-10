# AGENTS.md

本文件给后续接手本项目的编码代理/工程师使用，记录工作方式、算法理解、经验教训和下一步计划。

## 工作原则

1. 不要强制 `A=C` 或 `B=D`。
2. 不要把结果强行拉回规格值。
3. 所有测量都要保留 `raw` 和 `corrected` 两套概念。
4. 如果补偿，必须来自标准棒人工真值，不要用业务假设修结果。
5. `.hobj` 和大型运行库不要提交 Git；它们已被 `.gitignore` 排除。
6. 修改算法后，至少用 `demarcate` 标定图和 `test_image` 下的测试 HOBJ 跑一次 sanity check。

## 当前核心脚本

```text
tools\measure_square_rod_edges.py
tools\camera_calibration_model.json
```

脚本支持：

- 输入单个 `.hobj`
- 输入四个 TIFF
- 从标定目录重建标定
- 一个输入文件输出一个 CSV

注意：

- 当前默认是自由四点测量，不限幅、不强制矩形。
- 之前尝试过 `soft` 模式，但用户指出不符合真实测量原则，已经移除。

## 算法说明

### 1. 单相机轮廓处理

每个相机每个 row 是一条 2D 高度轮廓。算法需要从轮廓中提取：

```text
L1: 主面1
L2: 主面2
Lc: 倒角面
P : L1 与 L2 延长线交点
T1: L1 与 Lc 交点
T2: L2 与 Lc 交点
M : (T1 + T2) / 2
```

输出：

```text
角度 = angle(L1, L2)
垂直度误差 = abs(90 - 角度)
倒角长度 = distance(T1, T2)
投影X = abs(T2.x - T1.x)
投影Z = abs(T2.z - T1.z)
```

### 2. 四相机全局拼接

每个相机的局部坐标需要映射到统一全局横截面坐标：

```text
[x_camera, z_camera] -> [X_global, Z_global]
```

当前标定模型是以标准棒人工 A/B/C/D 建立的相机到全局坐标变换，并有 per-corner bias。这个模型能完成基本毫米换算，但 B/D 方向还有明显系统误差。

### 3. 四边长

点位关系：

```text
P3 ---- A ---- P1
|              |
D              B
|              |
P2 ---- C ---- P4
```

边长：

```text
A = distance(P3, P1)
B = distance(P1, P4)
C = distance(P2, P4)
D = distance(P3, P2)
```

### 4. 对角线

用户已确认对角线按倒角中点算：

```text
diag1 = distance(M1, M2)
diag2 = distance(M3, M4)
```

不要改回理论尖角 P 到 P。

### 5. 棒长

棒长基于四个相机共同有效 row 区间：

```text
start = max(cam_start)
end = min(cam_end)
stick_length = (end - start) * y_scale
```

## 已发现的问题

### B/D 系统性偏差

三根测试棒均显示：

```text
B-D ≈ -2.35 ~ -2.40 mm
```

这说明当前拼接/标定很可能让 B 偏小、D 偏大。后续应通过多根标准棒拟合 B/D 的系统补偿，不能强制 B=D。

可选模型：

```text
B_corrected = B_raw * scale_B + offset_B
D_corrected = D_raw * scale_D + offset_D
```

如果数据证明仅固定偏差足够，可先用 offset；若不同尺寸有比例误差，再用 scale + offset。

### A/C 非系统性波动

三根测试棒：

```text
17_03: A-C = +1.096736
17_11: A-C = -0.532922
17_36: A-C = +0.680147
```

方向不稳定，不能做固定补偿。A/C 后续重点应是：

- 角点稳定性诊断
- row 方向鲁棒统计
- 检查 P1/P2/P3/P4 哪个点在跳

### 原 HALCON 补偿机制问题

原方案是：

```text
final = spec_standard + (current_raw - calibration_template_raw)
```

它会被规格标准值牵引，且会用标准值窗口过滤真实偏差。该思路只适合同规格相对修正，不适合多规格真实测量。

## 推荐后续工作

### 1. 标准棒数据格式

让用户提供 CSV 或表格：

```text
bar_id, orientation, hobj_path, position_percent, A, B, C, D
001, normal, xxx.hobj, 20, ...
001, normal, xxx.hobj, 50, ...
001, normal, xxx.hobj, 80, ...
001, flipped, xxx.hobj, 20, ...
```

至少：

- 2-3 根不同尺寸棒
- 每根 20%/50%/80%
- 最好每根正常和调头各拍一次

### 2. 新标定模型

实现独立的 calibration builder：

```text
raw_measurements + manual_truth -> edge correction model
```

输出：

```text
A_raw, B_raw, C_raw, D_raw
A_corrected, B_corrected, C_corrected, D_corrected
correction_model_version
```

先做分边 `scale + offset`，后续若 row 方向有规律，再做平滑 row offset。

### 3. A/C 诊断

给 CSV 增加：

```text
A_minus_C
B_minus_D
P1/P2/P3/P4 point jitter
per-corner fit quality
per-edge std/range
```

目标是区分：

- 真实 A/C 差异
- 某个相机角点识别不稳
- row 方向局部异常

### 4. 鲁棒统计

最终结果不要只用全 row 普通平均。建议输出：

```text
head_20_region
mid_50_region
tail_80_region
trimmed_mean
median
std
range
```

对异常值使用中位数/MAD 或截尾均值，而不是直接删掉超规格点。

### 5. 可视化

保留并更新全局可视化图：

```text
tools\global_all_parameters_visual_final.png
```

如果算法定义变化，必须同步更新图，避免再次出现对角线、角度、倒角定义误解。

## 常用命令

重建标定：

```powershell
python tools\measure_square_rod_edges.py --input "D:\ProjectCode\SunshineSiRod_Refactor\image\demarcate" --save-calibration "D:\ProjectCode\SunshineSiRod_Refactor\tools\camera_calibration_model.json" --output "D:\ProjectCode\SunshineSiRod_Refactor\tools\demarcate_free_check.csv" --overwrite --step-mm 10
```

测量单个 HOBJ：

```powershell
python tools\measure_square_rod_edges.py --input "D:\ProjectCode\SunshineSiRod_Refactor\image\test_image\17_03.hobj" --calibration "D:\ProjectCode\SunshineSiRod_Refactor\tools\camera_calibration_model.json" --output "D:\ProjectCode\SunshineSiRod_Refactor\tools\17_03_free_v6_measure.csv" --overwrite --step-mm 10
```

语法检查：

```powershell
python -m py_compile tools\measure_square_rod_edges.py
```

查看测试均值：

```powershell
Import-Csv "tools\17_03_free_v6_measure.csv" | Where-Object {$_.record -eq 'mean'}
```

