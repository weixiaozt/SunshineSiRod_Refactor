# AGENTS.md

## 同事独立全局交付包最终状态（2026-07-16）

本节只约束“帮同事合并打包”的独立离线包，不属于 SunshineSiRod 原生测量链，
也不得把本节的临时报告公式移植到原生端面算法中。该包只复用本项目 Web 外壳；
尺寸和端面测量均使用同事交付包代码与标定。

### 唯一应使用的新包

```text
release\SunshineSiRod_Global_DeliveryGeometry_20260715_EndfaceInteriorAngle_20260716.zip
SHA256 = 0D815364FBAE9DBC43A224BF5BF301BA9436B1234D38D63823BF11A2B135A26B
```

母版 `release\SunshineSiRod_Global_DeliveryGeometry_20260715.zip` 只用于构建基线，
仍含旧端面角定义，不能作为本次新逻辑的现场运行包。不得直接运行或继续维护散落的
`release\SunshineSiRod_Global_DeliveryGeometry_20260715\` 旧解压目录；该目录曾残留旧的
`MeasureCoworkerEndface.exe`，会导致头部数值看似正常、尾部数值全部变成新结果的补角。

新 ZIP 必须完整解压到一个全新的空目录，禁止覆盖旧目录。启动前应关闭
`MeasurementDashboard.exe`、`MeasureCoworkerEndface.exe`、
`MeasureDeliveryGeometry.exe`，并确认 `127.0.0.1:8766` 未被旧服务占用。

### 算法边界

1. A/B/C/D、对角线、四角数据和棒长继续由母版中的
   `MeasureDeliveryGeometry\MeasureDeliveryGeometry.exe` 计算；该 EXE、尺寸标定、
   Web EXE 和 `start_dashboard.bat` 相对母版保持字节不变。
2. 端面只由 `MeasureCoworkerEndface\MeasureCoworkerEndface.exe` 计算。新包中的端面
   EXE SHA256 为
   `C7343F9557E1C5CAAD01BA092E27EE2A8CBF2761AB39A19FB0E1F978C38981C7`。
3. 端面 raw 定义为端面和对应侧面两个具有材料内部方向的实体半平面在棒材内部形成的
   量角器夹角，范围 `0°～180°`。可以小于或大于 90°；不使用法向量正负二义性
   选择答案，不取绝对值，不折叠到 90° 以下。
4. 头、尾端面的外部方向由当前 HOBJ 拟合出的头到尾材料轴确定。每张 HOBJ 都运行
   同一套几何逻辑；不得根据路径、父目录、文件名、棒号中的 `diaotou／调头／一反／反向`
   做头尾交换、B/D 交换或 `180°-raw`。同一 HOBJ 放入普通目录和上述标记目录的
   8个 raw 实测最大差值必须为 `0.0°`。
5. `head/tail` 只表示设备空间扫描起止端，不推断现实世界的物理 R/L。
6. 新端面定义以外的母版行为不变。不要调用 SunshineSiRod 原生 M0/M1/M2 端面算法、
   原生相机模型、机械漂移模型或历史端面模型。

### 截面、边、角点与四相机固定映射

以下映射来自同事尺寸几何交付代码和当前正式标定，是本独立包 Web、JSON 和 CSV 的
唯一解释。禁止凭相机英文名、TIFF 文件名前缀或画面左右自行重排。

交付模型的名义截面坐标为：`X` 从左向右，`Z` 从上向下；四个角点和四条边为：

```text
P3 / AD / (0,0) -------- A -------- P1 / AB / (W,0)
        |                                  |
        D                                  B
        |                                  |
P2 / CD / (0,H) -------- C -------- P4 / BC / (W,H)
```

因此：

```text
A = 上边，P3(AD) 到 P1(AB)，约 210 mm
B = 右边，P1(AB) 到 P4(BC)，约 105 mm
C = 下边，P2(CD) 到 P4(BC)，约 210 mm
D = 左边，P3(AD) 到 P2(CD)，约 105 mm
```

Web 的“相机1～4/角1～4/obj1～4”是逻辑角点编号，不是 HOBJ 解包后 TIFF 文件名的
数字前缀。固定映射如下：

| Web逻辑编号 | Web角点 | 相邻面 | 画面位置 | 交付代码相机名 | HOBJ解包TIFF |
|---|---|---|---|---|---|
| 1 | P1 / AB | A、B | 右上 | `Right` | `2-Right.tif` |
| 2 | P2 / CD | C、D | 左下 | `Down` | `4-Down.tif` |
| 3 | P3 / AD | A、D | 左上 | `Top` | `3-Top.tif` |
| 4 | P4 / BC | B、C | 右下 | `Left` | `1-Left.tif` |

特别注意：`obj1/相机1` 实际来自 `Right/2-Right.tif`，绝不能映射成
`1-Left.tif`。这里的 `Top/Right/Down/Left` 是交付代码中的相机名称；Web 的
1～4是业务角点编号，两套编号必须通过上表转换。

每个实体侧面由相邻两台角点相机共同提供轮廓，代码映射固定为：

```text
A = Top/right   + Right/left
B = Right/right + Left/left
C = Left/right  + Down/left
D = Down/right  + Top/left
```

其中 `left/right` 是单台相机局部轮廓的左右分段，不是全局截面左右方向。端面
`head/tail_A/B/C/D_endface_angle_deg` 中的 A/B/C/D 也严格对应上述四个实体侧面。

### 四个角度、倒角与投影字段

Web 和统计 CSV 的四组 `objN_*` 固定映射为：

| Web字段前缀 | 物理角点 | 角度对应的两个主面 | 交付原始角字段 | 倒角原始字段 |
|---|---|---|---|---|
| `obj1_*` | AB / P1 / 右上 | A 与 B | `侧面垂直度1` | `弧长AB` |
| `obj2_*` | CD / P2 / 左下 | C 与 D | `侧面垂直度3` | `弧长CD` |
| `obj3_*` | AD / P3 / 左上 | A 与 D | `侧面垂直度4` | `弧长AD` |
| `obj4_*` | BC / P4 / 右下 | B 与 C | `侧面垂直度2` | `弧长BD`（历史名称，实际是BC） |

字段定义：

```text
objN_main_face_angle_deg = 对应角点两个相邻实体主面的夹角
objN_verticality_error_deg = 上述主面夹角的兼容别名，数值相同
objN_chamfer_mm = 对应倒角两条拟合交点之间的 chord 长度
objN_projection_x_mm = 当前交付的 projection_1 = chord / sqrt(2)
objN_projection_y_mm = 当前交付的 projection_2 = chord / sqrt(2)
```

页面所称“四个倒角角度”实际显示的是四个角点的**相邻主面夹角**，不是倒角斜面
自身相对某一主面的角度。当前所谓“弧长”也是倒角 chord 直线长度，不是曲线积分弧长；
`弧长BD` 是历史字段名，物理位置始终是 BC/P4/右下，禁止据名称把它映射到 B-D。

四个主面夹角和倒角 chord/投影按当前 HOBJ 在正式75个标定行中筛出的全部有效截面
平均；有效范围不足时可以少于75片，例如 `11_50` 实际使用74片。边长和对角线则分别
先取有效集合的头部5片、尾部5片平均，再将头尾两个结果平均为 Web 最终值：

```text
A_mm/B_mm/C_mm/D_mm = (对应头5片均值 + 对应尾5片均值) / 2
diag1_M1_M2_mm = (头diag1 + 尾diag1) / 2
diag2_M3_M4_mm = (头diag2 + 尾diag2) / 2
```

两条对角线不是简单角点中心距离，也不是本项目原生的倒角中点距离。本同事交付包使用
有限倒角区域点云的平行卡尺最大支撑距离，并在名义方向附近 `±5°` 搜索：

```text
diag1 = AB(P1/右上) 与 CD(P2/左下) 方向的最大支撑距离
diag2 = AD(P3/左上) 与 BC(P4/右下) 方向的最大支撑距离
```

棒长 `stick_length_mm` 使用四相机有效行范围长度的平均值；单相机长度为
`(有效结束行 - 有效起始行) × 0.05 mm`。

### 端面链使用的四相机轴向位置

扫描轴坐标固定为：

```text
Y_corrected = row × 0.05 mm - camera_y_bias
```

新 ZIP 当前端面标定的四相机 `camera_y_bias` 为：

```text
逻辑1 / Right / AB = +0.689062500000 mm
逻辑2 / Down  / CD = -1.441562500000 mm
逻辑3 / Top   / AD = +0.829062500000 mm
逻辑4 / Left  / BC = -0.076562500000 mm
```

这些偏移同时参与端面角点 Y 和纵向实体侧面拟合；不得只修端点、不修侧面，也不得因为
偏移正负或相机英文名重新解释 P1～P4、A～D。更换标定 JSON 后必须重新读取实际值，
禁止继续硬编码上述数值。

### Web 与统计 CSV 的最终输出口径

端面 EXE 向集成 Web 提供8个 raw 内部角；Web 和用户统计 CSV 仍按同事临时报告要求
只输出下式得到的 reported：

```text
reported = 90 + 0.5 × (raw - 90)
```

Web 与 `results\measurements\user_results\measurement_statistics.csv` 只显示/保存
`head/tail × A/B/C/D` 8个 reported，不增加 raw/reported 双套列，也不输出头尾四面平均值。
统计 CSV 固定为38列：前4列为 `measured_at、bar_id、capture_id、input_path`，其余为
Web 页面可见的全部测量值。

若 CSV 中只有尾部与最新程序窗口值对不上，先验证是否误启动旧解压目录。旧端面程序
产生的尾部 reported 恰好满足：

```text
old_tail_reported = 180 - new_tail_reported
```

这不是 ABCD 列顺序错乱，而是旧程序仍在用无限平面法向角，导致尾部整体取了补角。
2026-07-16 现场排障时，旧运行 EXE SHA256 为
`99EF7428CEBC45EA84B30DC645A7545E7013B019CA5021AE5DC591D8EE546709`；发现该哈希必须
立即停服并换用上面的新 ZIP，禁止在 CSV 或 Web 层硬交换字段来掩盖问题。

### 已验证基准

新程序直接测量并按临时报告公式换算后的结果如下（顺序均为 A/B/C/D）：

```text
11_46 raw head     = 89.976092, 89.863786, 90.024417, 90.135840
11_46 reported head= 89.988046, 89.931893, 90.012209, 90.067920
11_46 raw tail     = 89.955751, 90.197579, 90.043731, 89.802727
11_46 reported tail= 89.977875, 90.098790, 90.021865, 89.901364

11_53 raw head     = 89.960249, 89.846779, 90.037941, 90.153420
11_53 reported head= 89.980125, 89.923390, 90.018970, 90.076710
11_53 raw tail     = 89.985237, 90.153206, 90.016572, 89.846543
11_53 reported tail= 89.992619, 90.076603, 90.008286, 89.923272
```

端到端验收曾确认：Web API 与 `measurement_statistics.csv` 的8个端面 reported 完全一致，
CSV为38列且不存在名称含 `raw` 或 `reported` 的额外列。测试产生的临时 CSV/JSON 必须
清理，不得放入最终 ZIP。

## 最高铁律：禁止背答案（2026-07-14）

**任何标定、测量和展示链路都不得把最终 A/B/C/D 端面夹角或头尾平均值用独立精准偏移拉向机构真值。** 禁止保存或应用 `head/tail x A/B/C/D` 八通道最终角度补偿，禁止裁剪到规格窗口，禁止用机构真值、文件夹名或标准棒几何指纹给生产HOBJ选择一套“更接近答案”的结果。

**2026-07-15用户进一步确认：端面算法不再加载或相信任何机构/人工端面角真值，即使声称只拟合低层参数也不允许用于当前端面生产候选。** 当前v15固定按M0/M1/M2分层：M0为Raw；M1只含一个从HOBJ端面点到面残差可独立识别的非平面相机Y同步模态，是唯一可能放行的修正；M2的两个仿射模态会与真实端面倾斜混淆，只允许诊断且运行时必须拒绝。现有20图中M1只改善约0.44%，固定应用到CTB且不重新拟合后反而恶化，因此当前Web默认且正式使用`raw_audit`/M0，不加载机械漂移模型、端面修正模型或端面真值。机构端面参考值不得在端面Web展示。证据见`tools/results/endface_v15_nested_image_audit`和`tools/results/ctb_external_m1_v15`。

**2026-07-15 M0 Raw现场试运行收口：** CSV/Web新增`endface_raw_quality_*`质量门禁；任一端面Raw边界平面RMSE超过`0.50 mm`为`rejected`，同面双相机P05-P95拼接变化`<=0.60 mm`为`pass`、`0.60~0.80 mm`为`uncertain`、`>=0.80 mm`为`warning`。告警和拒测都必须保留全部Raw审计值，不得借质量门禁裁剪角度。40张既有HOBJ按五个物理单棒组回归：专业棒19告警+已知磕碰`14_16`拒测，CTB六图全通过，待测A61/CTT各3通过+1告警；禁止把不同物理棒混在一起算调头自洽。试运行包由`tools/build_endface_raw_trial_package.py`构建为`release/SunshineSiRod_Endface_M0_Raw_Trial.zip`，专用入口硬拒绝漂移模型、端面模型、端面真值和标定生成参数，并锁定`orientation=normal`。该ZIP是Raw现场试运行包，不是绝对精度正式放行包，严禁用它绕过正式M1放行门。

机构真值只能用于拟合和验证有明确物理含义的设备参数，例如相机局部坐标到公共空间的变换、相机扫描行同步偏差、编码器比例和独立机械漂移模型。最终结果必须从当前HOBJ的四相机端面边界点云、四个纵向侧平面及这些设备参数重新计算；必须保留未标定raw结果和物理参数修正后的结果。无法把机械漂移与真实弯曲、锥度、扭转或端面倾斜可靠区分时，只能告警/拒测，不得为了出数强制修正。

物理调头后的端面数据必须由图像几何自然反向；不得靠方向分类器切换头尾补偿。当前现场HOBJ经图像轨迹审计确认按固定设备空间行序保存，因此 `head/tail` 固定表示HOBJ设备空间最小行/最大行，不是电机运动起点/终点，也不能声称自动识别任意棒的物理R/L。真实调头在固定设备有向角约定下必须自然满足 `head↔tail、机构B↔C、A/D不变、θ_after=180°-θ_before`。四个有向侧面夹角的算术平均因相对面法向近似相反而天然接近90°，只能作为显示字段，严禁作为端面质量或标定准确度的主要证据。

20张专业标定HOBJ的120100字节对象头已逐字节比较，除拍摄时间字节19～23外完全相同，文件内没有扫描方向、物理R/L或调头标志。没有外部标记、编码器方向或人工确认时，只能按设备最小行端/最大行端输出实测值；禁止从机构真值或标准棒细微形状反推物理端点身份。

**现场视频已在2026-07-14确认：当前20张标定图的两组不是相机正扫/反扫，而是同一根标准棒物理调头。** `head_to_tail`为未调头姿态，设备最小行端对应物理R；`tail_to_head`为同一根棒绕机构A/D轴180°调头后的姿态。HOBJ行号在两组中仍是固定设备坐标，不能用“行号同序”否定物理调头。第二组机构真值必须按 `head↔tail、B↔C、A/D不变、θ_after=180°-θ_before` 映射；此前“棒不动、相机往返”的判断永久作废，禁止恢复。

端面Y坐标必须与X/Z同属一套三维几何：公共Y步距来自设备行距并用机构棒长独立审计，四相机固定Y同步偏移必须同时作用于端面边界点和A/B/C/D纵向侧面点。只修端面、不修侧面会比较两个不同坐标系中的平面，禁止。整根棒的真实刚体倾斜同时旋转端面和侧面且不改变二者夹角，这是正确的不变量；不得把刚体姿态当垂直度误差。更高阶Y非正交/每列剪切没有独立三维靶或编码器证据时不可辨识，禁止用8个机构角度过拟合。

新算法至少必须通过：标定图留出盲测、正向/调头等变性、非标定棒验证、合成端面倾斜单调响应、机械漂移不覆盖真实形变、运行时不读取专业真值。仅证明重复性或“输出接近机构值”不算准确度验证。

四相机机械漂移的模型分层、识别阈值、raw/corrected 字段和重建命令见：`tools\calibration\mechanical_drift_principles.md`。机械漂移模型必须独立于正常相机标定；禁止把正常/异常 HOBJ 混合平均。V3 使用绝对设备扫描行参考曲线和整棒鲁棒摆放对齐，不得把不同长度/有效起点的棒各自拉伸到0%～100%；未命中已知漂移时使用 `unmatched_unadjusted`，继续保留未修正测量值并告警，禁止强制套用异常补偿。

V3的纵向参考曲线只对同一物理标准棒有效，跨棒审计已证明它会把棒材形状与设备漂移混淆；专业新标准棒20图全部无法命中旧CTB模型。跨棒通用修正必须有同一专业标准棒正常/异常机械状态配对图，或固定基准靶/编码器。相邻相机同面拼接诊断已接入CSV/Web，只允许告警且必须记录 `relative_camera_geometry_correction_applied=false`，不得直接反算补偿。

本文件给后续接手本项目的编码代理/工程师使用，记录当前测量定义、标定逻辑、运行方式和注意事项。

详细的几何计算、字段定义与连续测量保存规则见：`tools\calibration\measurement_geometry_principles.md`。

## 端面专用离线包（2026-07-13）

> **历史方案已作废：** 本节中关于 v8、8通道最终角度偏移、HOBJ方向分类器和
> “当前离线包已验证通过”的描述仅用于追溯事故，全部被文件开头的2026-07-14
> 最高铁律覆盖。`release/SunshineSiRod_Endface*` 旧产物不得交付或继续检测。
> 当前端面运行时只接受不含最终角度偏移/方向分类器的物理 v12+ 模型。v10曾将
> 四相机Y偏移分解为1个图像共面模态和2个机构真值约束的共享倾斜模态，但没有验证
> 当前相机机械状态是否仍与标定时一致，已被v11取代。v11新增纯图像同面双相机状态
> 门禁；v12在此基础上纠正物理调头真值映射，并把每相机Y同步同时用于侧面和端面。
> 现有20张专业棒图除图像QC排除的`14_16`外，最大拼接变化约1.22～1.74 mm，
> 全部无法证明机械状态稳定，因此标定已被拒绝，`valid=false`、
> `release_readiness.ready=false`，仍无可交付新包。
> 完整证据见 `tools/calibration/endface_no_answer_audit.md`。

`head_to_tail / tail_to_head` 在当前20图中分别表示标准棒未调头/物理调头姿态。两组
HOBJ都按固定设备空间行序保存不与该事实矛盾；行序审计只能在各自物理姿态组内建立
参考，不能跨组用同序轨迹推断棒子没动。`14_16.hobj` 因相对同组参考行序反向和端面
平面残差超限被图像QC排除。

端面专用包与下文历史完整测量程序分开。构建入口为 `tools\build_endface_offline_package.py`，现场标定入口为编译后的 `EndfaceCalibrator.exe`。该包只输出：

```text
head/tail_[A-D]_endface_angle_deg
head/tail_endface_verticality_deg
```

其中平均字段为四个直接夹角的算术平均，不减90°。Web 左侧显示实测10值，右侧显示模型内的专业机构标准10值；不得用右侧真值匹配待测棒方向。

当前没有已放行的端面离线包。旧v8包和模型已退役；构建脚本必须在PyInstaller之前检查模型，只允许 `version>=12`、`model=endface_camera_geometry_calibration`、`strategy=physical_decomposed_camera_scan_row_synchronization`、`valid=true`、`release_readiness.ready=true`、`calibration_state_reference.all_captures_stable=true`、`y_coordinate_correction.apply_to=all_longitudinal_side_points_and_end_boundary_points` 且不含 `angle_offsets_deg/orientation_models/orientation_detector` 的物理模型。任一条件不满足必须立即失败。

现场补采不依赖网络的独立工具包由 `tools/build_endface_calibration_toolkit.py` 构建，产物为 `release/SunshineSiRod_Endface_CalibrationToolkit.zip`。它只含编译后的 `EndfaceCalibrator.exe`、相机坐标模型、按规格机构真值、未调头/物理调头两组空采集目录和诊断输出目录；禁止包含 `MeasureSquareRod.exe`、`MeasurementDashboard.exe`、HOBJ或任何端面终值模型。标定器不得提供自动修改/激活Web配置的命令行旁路。该工具包生成的JSON仍必须通过 `release_readiness.ready=true`，否则只能保留作拒绝审计，不能进入正式测量包。

工具包必须同时提供 `先检查相机机械状态.bat`。该入口可对1张或若干HOBJ运行 `--camera-state-only`，只读取图像与相机坐标模型，CSV固定记录 `correction_applied=false`、`uses_professional_truth=false`，不读取机构真值、不拟合端面模型。实图基准：普通样本`20_14.hobj`最大拼接变化`0.345425 mm`、退出码0；当前专业标定样本`13_20.hobj`为`1.570266 mm`、退出码4。现场应先用1～3张试拍预检，稳定后再正式采20张。

现场标定按同一根标准棒 `head_to_tail` 未调头10张、`tail_to_head` 绕A/D轴物理调头10张组织，默认前8+8张拟合、后2+2张盲测与真实调头等变验证。机构报告端点固定为 **R=物理头、L=物理尾**；机构面号固定为 A=上、D=下、C=左、B=右。HOBJ必须在各自物理姿态组内通过设备空间行序审计。模型只允许拟合共享的低层相机/扫描物理参数，禁止最终8通道角度偏移和生产HOBJ方向分类器。

端面专用统计 CSV 固定为5个追溯字段加10个端面字段；每图切片 CSV 同样必须使用 `--endface-only`，不得暴露横截面、倒角、对角线、棒长或机械漂移结果。内部允许加载相机几何模型以识别四个侧面，但这不改变“产品只检测端面”的输出边界。

端面专用离线包固定监听 `127.0.0.1:8767`，不得与旧完整离线包的 `8766` 共用端口。页面标题必须显示 `End-face Perpendicularity Dashboard`，便于现场一眼识别是否启动了正确版本。

## 历史完整测量链路的双向标定（不适用于端面专用包）

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
2. 所有测量都要保留 `raw` 和 `corrected` 两套概念；原始 CSV 不得被补偿值覆盖。Web所有模式永久禁止8个端面最终角度手工补偿，旧配置键 `endface_angle_offsets_deg` 必须丢弃或拒绝；只允许保留边长、对角线和棒长的透明显示补偿。
3. 横截面等允许的补偿必须来自标准棒真值，并能追溯到真值 CSV、标定模型和版本；端面最终八角严禁补偿，机构端面真值只能拟合/验证低层物理设备参数。
4. 不要用业务假设硬编码补偿值，不要用规格窗口过滤真实偏差。
5. `.hobj`、现场图像、本地 UI 配置、运行日志和生成 CSV 不提交 Git。
6. 修改算法后至少运行语法检查、端面单元测试，并用 `tools\calibration\hobj` 做一次 sanity check。
7. 端面专业机构面号固定为 `A上、B右、C左、D下`，是Web和CSV唯一对外面号。内部历史软件坐标自动按 `A=A,B=B,C=D,D=C` 转换，禁止再让现场人员输入面号映射。
8. 端面专业机构物理模型v12+使用四个独立纵向侧平面与当前HOBJ端面点云计算 `0..180°` 有向夹角，并把同一四相机Y同步修正同时施加到纵向侧面点与端面边界点。不得恢复 `abs(normal dot normal)`，不得用共同棒轴强制相对面平行，不得添加最终角度偏移、方向分类器或Web/CSV硬交换。只有当前同面双相机图像状态命中标定状态包络时才允许应用扫描行同步；未命中必须输出 `state_unmatched_unadjusted`、保留raw并告警。无效、旧策略、错规格、物理参数不稳定、未通过真实物理调头HOBJ放行门或端面8角不完整必须拒绝；raw与物理修正结果及不确定度必须可追溯。

## 当前核心文件

```text
tools\measure_square_rod_edges.py
tools\test_endface_perpendicularity.py
tools\measurement_dashboard.py
tools\calibration\hobj\
tools\calibration\truth\manual_calibration_truth.csv
tools\calibration\models\camera_calibration_model.json
tools\calibration\models\endface_calibration_model_210_105.json  # 当前仍为退役v8，运行时会拒绝
tools\results\endface_v12_physical_turnover_y_audit_rejected.json  # 当前20图的拒绝审计产物
tools\endface_calibrator.py
tools\build_endface_offline_package.py
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

历史端面模型（已退役，仅用于事故追溯）：

```text
tools\calibration\models\endface_calibration_model_210_105.json
version = 8
model = endface_face_angle_offset
```

旧逻辑为每个最终端面角保存固定偏移并用方向分类器切换结果。该方法会把不同棒子的结果拉向标准棒，已经违反“禁止背答案”铁律，运行时和打包器必须拒绝。

允许的新端面模型必须满足：

```text
version >= 12
model = endface_camera_geometry_calibration
strategy = physical_decomposed_camera_scan_row_synchronization
runtime_orientation_detection = none
release_readiness.ready = true
calibration_state_reference.all_captures_stable = true
禁止 angle_offsets_deg / orientation_models / orientation_detector
```

它只能保存四相机扫描行同步等低层物理参数，最终八角必须对每张当前 HOBJ 重新拟合。现场视频已经为当前两组提供真实物理调头证据；v12重算仍因同面双相机拼接变化超过稳定门限、留出最大误差及调头等变误差超限而拒绝出模。

## 端面垂直度输出

端面结果有两类字段：

```text
*_endface_plane_verticality_deg
head/tail_[A-D]_endface_angle_deg
head/tail_endface_verticality_deg
```

其中 `head/tail_endface_verticality_deg` 是遗留兼容字段名，当前数值定义为端面与 A/B/C/D 四个物理面夹角的算术平均值：`mean(θA,θB,θC,θD)`，不再计算 `mean(abs(90-θ))`。该平均因相对面近似互补而天然接近90°，只能显示，不能证明准确度；端面专用 Web、切片 CSV 和统计 CSV 必须使用同一定义且不得叠加手工八角补偿。

## 常用命令

语法和单元测试：

```powershell
python -m py_compile tools\measure_square_rod_edges.py tools\endface_calibrator.py tools\measurement_dashboard.py tools\build_endface_offline_package.py
python -m unittest discover -s tools -p "test_*.py" -v
```

生成端面物理模型（仅在全部物理稳定性和盲测门限通过时才会写出有效模型）：

```powershell
python tools\endface_calibrator.py --input "<包含未调头head_to_tail和物理调头tail_to_head各10图的目录>" --truth-csv "tools\calibration\truth\210_105_institution_report.csv" --camera-calibration "tools\calibration\models\camera_calibration_model_210_105.json" --output-model "tools\calibration\models\endface_calibration_model_210_105.json" --diagnostics-csv "tools\results\endface_physical_diagnostics.csv" --sample-id "<标准棒编号>" --head-label R --expected-captures-per-direction 10
```

`measure_square_rod_edges.py --save-endface-calibration` 是旧最终角度补偿入口，现已硬性报错，禁止恢复。

测量单个 HOBJ：

```powershell
python tools\measure_square_rod_edges.py --input "<input.hobj>" --calibration "tools\calibration\models\camera_calibration_model_210_105.json" --endface-calibration "<release_readiness.ready=true的v12+物理模型.json>" --drift-calibration "tools\calibration\models\mechanical_drift_model_210_105.json" --output "<output.csv>" --overwrite --step-mm 10 --endface-only
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

每张图的切片明细保留为独立的 `*_measure.csv`。统计数据先写入输出目录的 `measurement_statistics.sqlite`，再刷新唯一的 `measurement_statistics.csv` 快照。统计表只保留页面可见的最终汇总值、必要识别列和切片 CSV 路径，不写入 raw/corrected/manual 补偿等内部明细。若 CSV 正被 WPS/Excel 锁定，测量和去重仍会成功，关闭文件后连续监控会自动刷新 CSV 快照。历史完整仪表盘的手工边长、对角线、棒长补偿是透明用户输入；所有模式均不得显示、保存或应用八个端面最终角手工补偿。

`tools\web_ui_config.local.json` 含本机绝对路径，已按用户要求提交；换电脑后必须检查并修改。

## 当前限制

1. `2606005B22-CTT_3/11_42.hobj` 存在随 row 增大的 A/C 反向拼接漂移，已移至 `tools\calibration\excluded_hobj`；原因记录在 `tools\calibration\excluded_captures.md`。
2. 默认模型的当前有效拟合集仅为 `2606005B22-CTB_3`：正向3张、调头3张 HOBJ。新增拍摄只能追加到这一棒的对应方向，且必须使用该棒人工真值重新拟合。
3. 其他棒号的历史数据与复测统计不适用于默认模型，也不得作为跨棒精度或补偿依据。
