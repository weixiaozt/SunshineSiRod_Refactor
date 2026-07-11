# AGENTS.md

本文件供后续接手本项目的编码代理/工程师使用，记录当前测量定义、运行方式、校准边界和已知风险。最后根据提交 `6a80920` 更新。

## 工作原则

1. 不要强制 `A=C` 或 `B=D`，更不能用“矩形化”结果替代真实测量。
2. 不要把结果拉回规格值，也不要用规格窗口过滤真实偏差。
3. 所有测量与补偿必须保留 `raw` 和 `corrected` 两套结果；原始 CSV 不得被补偿覆盖。
4. 补偿必须来自同一标准棒的人工真值，且应能追溯到真值文件、标定模型和版本；不能凭业务假设填写固定偏置。
5. `.hobj`、图像、运行库、仪表盘本地配置和生成的 CSV 不提交 Git；当前 `.gitignore` 已覆盖它们。
6. 改动算法后，至少运行端面单元测试，并用 `tools\calibration\hobj` 中的标定棒做 sanity check。

## 当前核心文件

```text
tools\measure_square_rod_edges.py                  # 命令行测量、截面标定、端面测量/标定
tools\calibration\hobj\                          # 三根联合标定 HOBJ
tools\calibration\truth\manual_calibration_truth.csv
tools\calibration\models\camera_calibration_model.json
tools\results\measurements\                       # 测量结果 CSV
tools\test_endface_perpendicularity.py
tools\measurement_dashboard.py      # 仅本机监听的 Web 仪表盘服务
tools\dashboard\                   # 仪表盘前端
tools\web_ui_config.example.json    # 仪表盘配置模板
tools\start_measurement_dashboard.bat
```

## 输入、输出与坐标

测量脚本可输入：

- 一个 HALCON `.hobj`（四个 `3200 x 20000` 高度图）；
- 四个 TIFF（按 `obj1 obj2 obj3 obj4` 顺序）；
- 包含多个 `.hobj` 的标定目录，用于重建截面标定。

默认尺寸比例为 `X=0.015 mm/pixel`、`Y=0.04891993841 mm/row`。HOBJ 使用固定的四对象偏移读取；如采集格式不同，可通过 `--hobj-offsets` 覆盖。

一个输入产生一个 CSV，包含：

- `slice`：每一个有效横截面；
- `mean`、`min`、`max`、`range`、`std`：仅对有效 slice 的数值字段统计；
- `endface_fit`：头/尾端面平面拟合的 raw、校正后（如有）和人工真值（如有）记录。

仪表盘另写 `*_dashboard_summary.csv`，其中明确分为 `raw_mean`、`corrected_mean` 和所用的手工偏置；它不会改写原测量 CSV。

## 截面算法与定义

### 单相机角点

每个相机的每个 row 是一条 2D 高度轮廓。算法使用绝对传感器列坐标，平滑后对倒角两侧的主面做 MAD 鲁棒直线拟合，得到：

```text
L1/L2 : 两主面
P     : L1、L2 延长线的交点（理论尖角）
T1/T2 : 检出的倒角段两端
M     : (T1 + T2) / 2
```

输出单角主面夹角、`abs(90 - angle)` 垂直度误差、倒角弧长 `|T1-T2|` 及其 X/Z 投影。未找到倒角段时，计算对角线会退回该角理论点 `P`，CSV 中倒角字段为空；分析时需注意这一降级行为。

### 四相机拼接与边长

物理映射为 `obj1 -> P1`（上）、`obj3 -> P3`（左）、`obj2 -> P2`（下）、`obj4 -> P4`（右）。各相机先用由主面方向求得的正交矩阵及平移映射到全局 `X/Z`，随后由四条全局侧线的交点重建四个角点，并应用标定模型中的 `corner_biases`。

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
diag1 = distance(M1, M2)
diag2 = distance(M3, M4)
```

对角线一律按倒角中点 `M1/M2/M3/M4` 定义，不能改回理论尖角 `P`。棒长取四相机共同有效 row 区间：`(end - start) * y_scale`。

默认 `--geometry-mode free` 是真实的自由四角测量。`--geometry-mode rectangular` 会把对边融合成矩形（因此强制 `A=C`、`B=D`），只可用于诊断相机对齐残差，不能作为业务最终值或校准目标。

## 截面标定

当前模型版本为 `6`，模型名 `camera_oriented_transform_with_corner_bias`。它以标准棒在 20%/50%/80% 三个位置的人工 A/B/C/D 真值建立各相机的方向/平移和每个角点残差偏置；标定目录中的文件名含“调头”、`diaotou` 或兼容的乱码标记时，重建过程会交换 B/D 真值方向。

模型解决的是相机到全局截面的换算，不能被误认为已完成各边的通用误差补偿。需以多根不同尺寸标准棒（最好包含正常与调头、20/50/80%）复核 A/B/C/D 的 raw 误差后，才可新增可追溯的分边 `scale + offset` 模型。

历史测试显示 B/D 有约 `-2.35 ~ -2.40 mm` 的差异，A/C 差异方向不稳定。这是旧数据中的待验证诊断结论，不是允许硬编码的补偿值；算法或标定更新后必须重新测量确认。

`tools\calibration\truth\manual_calibration_truth.csv` 将截面和端面真值合并在一起。每根用于联合标定的标准棒都要有自己的 `bar_id`（等于 HOBJ 文件名，不含 `.hobj`）和至少三条 `record_type=cross_section` 记录。`position_percent` 可填写人工测量位置范围，例如 `15-25`、`45-55`、`70-80`；程序会在对应 row 区间抽取多个轮廓，选取中位附近的代表性角点参与标定。不要为了凑格式改写为固定 20/50/80；填写对应的 `A_mm/B_mm/C_mm/D_mm`。可按相同格式继续增加标准棒和位置点。用 `--calibration-truth-csv` 读取时，程序按 `bar_id` 将每根 HOBJ 匹配到自己的真值，并让所有匹配棒共同参与截面标定；端面标定只读取有 `end/face/angle_deg` 的行。

## 端面垂直度与端面校准

脚本会从每个相机头/尾有效高度边界的连续材料点提取点云，在全局 `X/Y/Z` 中对 `Y = slope_x * X + slope_z * Z + intercept` 做 MAD 鲁棒平面拟合（至少 12 个点），并以拟合的实际棒轴而不是扫描器 Y 轴计算。

端面结果有两种互补定义：

- `*_endface_plane_verticality_deg`：端面平面法向与棒轴的夹角；
- `head/tail_[A-D]_endface_angle_deg`：四个实际侧面与端面的夹角；`head/tail_endface_verticality_deg` 是四个夹角相对 90° 的绝对误差均值。

端面人工真值 CSV 支持两种格式，均需正确匹配 `bar_id`：

1. 直接夹角：每个 `head/tail × A/B/C/D` 恰好三条，字段可用 `bar_id,end,face,position,angle_deg`；保存标定后生成 `endface_face_angle_offset`（人工三次均值减 raw 视觉夹角）。
2. 有符号位移：每个端面每面三条，共 24 条，字段可用 `bar_id,end,face,position,deviation_mm`，可选 `x_mm,z_mm`；没有坐标时按面上的位置推算。保存标定后生成 `endface_plane_slope_offset`。

两类模型都保留 raw 字段。载入模型使用 `--endface-calibration`；以真值新建模型使用 `--endface-truth-csv` 或统一文件 `--calibration-truth-csv` 加 `--save-endface-calibration`。不要将仪表盘的手动端面偏置误作正式端面标定模型。

## 本地 Web 仪表盘

运行 `tools\start_measurement_dashboard.bat` 或 `python tools\measurement_dashboard.py`，服务仅监听 `http://127.0.0.1:8765`。首次使用可复制 `tools\web_ui_config.example.json` 为 `tools\web_ui_config.local.json` 后填写数据根目录、结果目录、脚本、截面标定和可选人工真值 CSV 路径。

仪表盘：

- 读取测量 CSV 的 `mean` 行展示，不在浏览器重新计算；
- 可执行选中的 `.hobj` 或四 TIFF 目录测量，单次子进程超时为 15 分钟；
- 可显示 raw / corrected。其手工 A/B/C/D 与八个端面夹角偏置只影响显示和 dashboard summary，绝不改写 raw CSV 或标定 JSON；填写前必须有人工真值依据；
- 可选自动检测：首次启动只记录现有 HOBJ，不回补历史文件；之后每 10 秒扫描一次，仅处理新增/更新且稳定至少 30 秒的 HOBJ。状态记录与本地配置均被 Git 忽略。

## 验证与常用命令

语法与端面回归测试：

```powershell
python -m py_compile tools\measure_square_rod_edges.py tools\measurement_dashboard.py
python -m unittest tools.test_endface_perpendicularity
```

从 `demarcate` 目录中的标准棒重建截面标定（会输出首个标定文件的 sanity CSV）：

```powershell
python tools\measure_square_rod_edges.py --input "D:\ProjectCode\SunshineSiRod_Refactor\tools\calibration\hobj" --calibration-truth-csv "D:\ProjectCode\SunshineSiRod_Refactor\tools\calibration\truth\manual_calibration_truth.csv" --save-calibration "D:\ProjectCode\SunshineSiRod_Refactor\tools\calibration\models\camera_calibration_model.json" --save-endface-calibration "D:\ProjectCode\SunshineSiRod_Refactor\tools\calibration\models\endface_calibration_model.json" --output "D:\ProjectCode\SunshineSiRod_Refactor\tools\results\calibration\calibration_sanity.csv" --overwrite --step-mm 10
```

以既有截面标定测量单个 HOBJ：

```powershell
python tools\measure_square_rod_edges.py --input "D:\ProjectCode\SunshineSiRod_Refactor\tools\calibration\hobj\17_03.hobj" --calibration "D:\ProjectCode\SunshineSiRod_Refactor\tools\calibration\models\camera_calibration_model.json" --output "D:\ProjectCode\SunshineSiRod_Refactor\tools\results\measurements\17_03_measure.csv" --overwrite --step-mm 10
```

查看统计行：

```powershell
Import-Csv "tools\results\measurements\17_03_measure.csv" | Where-Object {$_.record -in 'mean','min','max','range','std'}
```

对端面算法、CSV 字段或前端展示做变更时，必须同步检查 `tools\global_all_parameters_visual_final.png` 和仪表盘中的中英文字段说明，避免边长、对角线、倒角或端面定义再次漂移。
