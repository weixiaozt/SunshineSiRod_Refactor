# 端面物理标定说明（v15，禁止背答案）

## 1. 最重要的结论

旧v8把 `head/tail × A/B/C/D` 最终角度分别加固定偏移，结果会向标准棒靠拢，已经永久退役。v13使用机构端面角约束两个共享仿射参数，数值自洽较好，但这不是独立于机构答案的准确度证明，也已退出生产候选。

v15完全不读取机构或人工端面角，按M0/M1/M2分层审核。现有数据中M1在专业棒上只改善`0.44%`，固定应用到CTB后调头RMS反而恶化`0.33%`；M2参数跨折漂移`1.453618 mm`，禁止运行。因此当前Web正式使用M0 Raw：不加载机械漂移模型或端面修正模型，保留16角、每端最差实际局部角和几何诊断。

本文后面关于v13机构真值约束的章节仅保留为历史事故追溯，不是当前实施方法。

## 2. 坐标和字段

机构面号是唯一对外标准：

```text
A=上，B=右，C=左，D=下
R=标定棒物理头，L=标定棒物理尾
```

软件内部历史面号为 `A上、B右、C下、D左`，程序固定转换：

```text
机构A=软件A，机构B=软件B，机构C=软件D，机构D=软件C
```

现场HOBJ已经由设备保存成固定空间行序。产品字段：

```text
head = HOBJ设备空间最小行一端
tail = HOBJ设备空间最大行一端
```

它们不是电机运动起点/终点，也不自动代表任意待测棒的物理R/L。程序严禁用机构答案猜R/L。

## 3. 20张标定图的真实含义（现场视频已确认）

```text
head_to_tail：同一专业标准棒未调头，设备最小行端对应物理R，10张
tail_to_head：同一根棒绕机构A/D轴180°物理调头，10张
两组不是相机正扫/反扫；HOBJ行号始终是固定设备坐标
```

过去因为两组HOBJ都是固定行序，错误地判断成“棒不动、相机往返”。固定行序只说明文件坐标，不能证明工件没转。行序审计现在分别在两个物理姿态组内建立参考；`14_16.hobj` 是相对调头组参考反序且端面平面残差超限的图，仍仅凭图像证据排除。

调头组的机构真值固定映射为：`head↔tail、机构B↔C、A/D不变、θ_after=180°-θ_before`。这只用于已知标准棒标定与盲测，不代表生产程序能猜任意棒的物理R/L。

## 4. v13怎样标定物理参数

四个相机Y偏移先分成三个可辨识模态：

```text
1个非平面同步模态：让四相机端面点出现“不共面”
2个线性倾斜模态：在横截面X/Z方向改变重建端面斜率
```

识别规则严格分层：

1. 非平面模态只用HOBJ点到端面平面的残差拟合，不读取机构角度。
2. 两个线性倾斜模态会和棒子真实端面倾斜混淆，因此只允许用机构标准棒八角真值约束这两个共享设备参数。
3. 三个参数同时作用于头、尾、A/B/C/D和两个物理姿态组；不存在逐端、逐面补偿。
4. 四相机偏移满足零和基准：`δ1+δ2+δ3+δ4=0`。
5. 四个物理面各自用相邻两相机主面线建立纯图像状态包络；它只允许/禁止应用参数，不产生补偿。
6. 每相机Y偏移必须同时加到该相机贡献的纵向侧面点和端面边界点；只修端面属于坐标系不一致。
7. 公共Y行距用机构棒长反推值做0.5%只审计门，不自动改比例；没有独立3D靶或编码器证据时不拟合高阶Y剪切。
8. 标定器和生产测量共用 `endface_wireframe_geometry.measure_wireframe_angles`，避免“标定算一套、Web算另一套”。
9. 固定2+2盲测和5折配对交叉验证都必须检查16角调头等变性；验证阶段不读机构角度真值，不施加最终角补偿。

禁止模型字段：

```text
angle_offsets_deg
orientation_models
orientation_detector
```

## 5. 当前v13拒绝审计结果

20张专业标准棒HOBJ中，固定2+2盲测和5折配对交叉验证都直接运行生产版16角线框重建；`14_16.hobj`由行序及端面几何QC排除。验证阶段不读机构角度真值，结果为：

```text
16角固定2+2盲测RMSE             0.223209°（通过0.30°门）
16角固定2+2盲测最大误差         0.379123°（通过0.60°门）
16角5折交叉验证raw RMSE          0.665504°
16角5折交叉验证修正后逐对RMSE      0.221808°（改善66.85%）
16角5折交叉验证逐对最大误差        0.488848°（通过0.60°门）
16角5折交叉验证全留出重复性极差    0.186820°（通过0.20°门）
5折中最大相机Y参数波动             0.008780 mm（通过0.15 mm门）
```

这证明“四边线框+16角”方案有明显技术可行性，但不等于模型可放行。19张可用图的最大同面拼接变化约为 `1.22～1.74 mm`，全部超过`0.80 mm`告警门限；程序无法判断这是相机架移动还是标准棒真实面非平面，所以v13仍为 `valid=false` 且拒绝出模。

## 6. 为什么现在仍不能发布离线包

当前20张已经有现场视频作为真实物理调头证据，不再缺调头动作证明；但机械状态、留出最大误差和调头等变误差都没有通过。固定设备有向角约定下，真实刚体调头必须由图像自然满足：

```text
head ↔ tail
机构B ↔ C，A/D不变
θ_after = 180° - θ_before
```

不通过该HOBJ验证时，模型只保留拒绝审计；测量端、Web和打包器都会拒绝生产使用。

## 7. 四面平均为什么仍接近90°

近似相反侧面满足：

```text
θA+θD≈180°，θB+θC≈180°
四面算术平均≈90°
```

所以平均值只能兼容显示，不能判断质量或证明准确。必须看八个逐面角、端面平面倾角、raw/corrected、机械漂移状态和标定不确定度。

## 8. 机械漂移

机械漂移模型独立于端面物理标定。运行顺序为：

```text
当前HOBJ漂移识别
→ 命中已知异常时修正相机局部X/Z轨迹
→ 先检查v13同面双相机状态包络
→ 命中才应用相机Y同步参数重建端面；未命中保留raw并告警
→ 计算八角
```

正常图不修正；未命中已知模式时保留未修正值并告警；禁止强套异常曲线。切片CSV保留raw、漂移状态、漂移修正、v13适用性状态及是否实际应用物理校准。

## 9. 命令

生成v13物理模型（两组必须是未调头/物理调头）：

```powershell
python tools\endface_calibrator.py --input "<20图目录>" --truth-csv "tools\calibration\truth\210_105_institution_report.csv" --camera-calibration "tools\calibration\models\camera_calibration_model_210_105.json" --output-model "<工程候选.json>" --sample-id "BP411B116321XTO" --head-label R --expected-captures-per-direction 10
```

自测：

```powershell
python -m py_compile tools\measure_square_rod_edges.py tools\endface_calibrator.py tools\measurement_dashboard.py tools\build_endface_offline_package.py
python -m unittest discover -s tools -p "test_*.py" -v
```

生产模型必须同时满足：

```text
version >= 13
model = endface_camera_geometry_calibration
strategy = physical_decomposed_camera_scan_row_synchronization
valid = true
release_readiness.ready = true
wireframe_validation.passed = true
wireframe_validation.fixed_blind_holdout.passed = true
wireframe_validation.paired_cross_validation.passed = true
runtime_orientation_detection = none
不含最终角度偏移或方向分类器
```
