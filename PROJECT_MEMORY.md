# Project Memory

## 2026-07-15 M0 Raw现场试运行收口

- `tools/measure_square_rod_edges.py`新增纯图像质量门禁：端面Raw平面RMSE超过`0.50 mm`为拒测；同面双相机拼接变化`<=0.60 mm`通过、`0.60~0.80 mm`不确定、`>=0.80 mm`告警。所有状态都保留Raw审计字段，门禁不修改任何角度。
- `tools/endface_raw_batch_regression.py`按单根物理棒批量运行Web同一M0命令，固定`orientation=normal`，不加载漂移模型、端面模型或端面真值。40张图结果：专业棒19告警+`14_16`拒测；CTB六图全通过；待测A61与CTT各3通过+1告警。CTB长棒目录与待测CTB目录结果逐项一致，是重复副本。
- 调头自洽必须按物理棒分别计算，禁止把`pending_ctb/pending_a61/pending_ctt`混成一组。Raw 16角组中位调头RMS：专业棒`0.666883°`、CTB`0.567117°`、A61`0.572577°`、CTT`0.565745°`。
- `tools/measure_endface_raw.py`是试运行包专用入口，硬拒绝机械漂移模型、端面修正模型、端面真值、标定生成和`orientation!=normal`。`tools/build_endface_raw_trial_package.py`输出`release/SunshineSiRod_Endface_M0_Raw_Trial.zip`；Web通过环境变量锁定Raw，隐藏端面模型、漂移模型和真值设置。
- 包内EXE实测`20_14.hobj`为`pass`、`truth=false`、`correction=false`；ZIP资源检查无漂移模型、端面模型或truth目录；包内Web确认16个局部角、质量状态、Raw锁和零控制台错误。该包仅供现场Raw试运行，不代表绝对精度正式放行。

## 2026-07-15 纯HOBJ分层模型与M0决定

- 用户明确要求端面链路不再相信或加载机构/人工端面角。v15按M0/M1/M2分层：M0为Raw；M1只拟合一个图像可辨识非平面Y同步模态；M2含两个会与真实端面倾斜混淆的仿射模态，只允许诊断。
- 专业棒20图中M1固定盲测RMS `0.670085°→0.667200°`，5折只改善`0.44%`；固定应用到CTB六图且不重新拟合后，调头RMS `0.567151°→0.569005°`，恶化`0.33%`。M2交叉验证参数漂移`1.453618 mm`、输出重复范围`0.846462°`，禁止运行。因此当前自动选择M0，端面Web默认`raw_audit`。
- Web显示当前HOBJ的16个局部角、每端偏离90°最远的实际局部角和拼接/闭合/扭曲诊断；机构端面参考面板已移除。Raw模式不加载机械漂移模型、端面模型或机构/人工端面真值，且后端校验16角与代表角在数值容差内保持未修正。
- 当前拒绝证据：`tools/results/endface_v15_nested_image_audit/endface_calibration_model_210_105_v15_rejected.json`、`tools/results/ctb_external_m1_v15/endface_v15_nested_with_ctb_rejected.json`。

## 2026-07-14 禁止背答案铁律与端面链路重构

- 已确认旧端面v8的八通道最终角度偏移会使四面平均值在效果上锚定到专业机构均值；该方案不能作为任意棒测量准确度依据，必须退役。`raw四面平均≈90°` 主要来自相对面有向角两两互补的几何恒等关系，不代表端面真实垂直。
- 永久禁止对 `head/tail x A/B/C/D` 最终角度逐通道加精准补偿、裁剪到规格或用专业真值选择方向/结果。真值只能标定相机空间变换、扫描行同步、编码器比例和机械漂移等可解释设备参数；最终角度必须由当前HOBJ重新构造。
- 调头等变性必须来自图像几何。当前现场HOBJ轨迹审计证明有效文件由硬件保存成固定设备空间行序，因此 `head/tail` 表示HOBJ设备空间最小行/最大行，不是电机运动起点/终点；不得声称自动识别任意棒的物理R/L。旧v8标准棒几何分类器不得继续作为生产R/L判定依据。
- 四面直接夹角平均只能保留作兼容显示，不能作为质量指标或准确度证明。后续必须同时输出更有物理意义的端面法向相对棒轴倾角、倾斜分量或最大/RMS垂直度误差，并保留raw与设备参数修正后结果。
- 任何新模型必须通过标定留出、正反等变、非标定棒、合成已知倾斜、机械漂移隔离等验证；“重复性很好”或“接近机构答案”不等于测量准确。
- 现场视频已经明确证明：20图的 `head_to_tail` 是标准棒未调头姿态，`tail_to_head` 是同一根棒绕机构A/D轴180°物理调头后的姿态，并非相机正扫/反扫。HOBJ仍按固定设备行序保存不与物理调头矛盾；过去用“两组行号同序”推断棒子没动是错误结论，永久作废。第二组真值固定按 `head↔tail、B↔C、A/D不变、θ_after=180°-θ_before` 映射。行序QC改为每个物理姿态组内分别建立参考；`14_16.hobj`仍是相对调头组参考行序反向且端面平面残差超限的唯一图。
- v12补齐Y坐标一致性：四相机固定扫描行同步偏移不再只修端面边界点，而是同时作用于A/B/C/D纵向侧面点和端面点。公共Y步距`0.04891993841 mm/行`用机构棒长`466.7728 mm`和20图有效跨度反推为`0.04895619068 mm/行`，差`0.0741%`，通过0.5%只审计门；程序不据此偷偷改比例。整棒刚体倾斜同时旋转侧面和端面，二者仍为90°时属于物理正确；没有独立3D靶/编码器证据时禁止用机构八角过拟合高阶Y剪切。
- 无端面补偿重算3根现场非标定棒后，四面平均仍为 `90.0010°~90.0017°`，而端面平面倾斜为 `0.4956°~0.5485°`；这证明四面均值接近90°是几何恒等特性，不是算法准确或棒材相同。证据与CSV见 `tools/calibration/endface_no_answer_audit.md`。
- v9把3个相机Y自由度一起用点云共面性拟合，存在真实端面倾斜与相机同步不可辨识的问题，正反扫参数曾相差 `0.882824 mm`，已废弃。
- v10曾把四相机零和Y偏移分成 `1个非平面模态 + 2个线性倾斜模态`：非平面模态只用图像点到面残差，两个倾斜模态才用机构角度约束；没有逐端/逐面结果补偿。它的20图留出RMSE `约0.148°`、最大误差 `约0.291°`、正反扫最大参数差 `约0.134 mm`、逐图留一最大参数范围 `约0.019 mm`，但这些指标没有证明标定采集时的相机机械状态稳定，因此v10不再允许生产使用。
- v11新增纯图像 `calibration_state_reference`：用A/B/C/D各面相邻双相机拼接线的中位数、P05-P95跨度和夹角建立“只判断适用性、不产生补偿”的状态包络。现有20张专业标定HOBJ除`14_16`图像QC排除外，最大拼接变化均约`1.22～1.74 mm`，而现场普通棒约`0.34～0.42 mm`；程序无法排除相机/机械漂移或真实面非平面，已以退出码3拒绝生成v11模型，`valid=false`、`all_captures_stable=false`。拒绝审计位于 `tools/results/endface_v11_camera_state_audit_rejected.json` 与对应diagnostics CSV。
- v12运行时只有当前HOBJ的同面双相机状态命中标定包络、且独立机械漂移状态不是`unmatched_unadjusted`时，才允许把四相机Y同步同时应用到侧面与端面；否则输出 `state_unmatched_unadjusted`、`correction_applied=false`，继续保留raw测量并在Web告警。已知漂移若由独立模型修正，状态诊断使用修正后的局部几何，但诊断本身永远不反算补偿。
- 已知异常`10_08.hobj`的顺序回归通过：独立漂移模型先命中`abnormal_corrected`（幅值`0.996865`），同面状态再从原始约`1.276917 mm`降为修正后`0.439766 mm`并判为`relative_geometry_stable`；CSV明确记录`relative_camera_geometry_input_state=after_known_mechanical_drift_correction`且诊断自身`correction_applied=false`。
- 合成刚体调头测试只变换点云，不交换输出或读取真值，证明固定设备有向角下自然满足 `head↔tail、机构B↔C、A/D不变、θ_after=180°-θ_before`。当前20图已有现场视频作为真实调头外部证据，最后2+2张同时作为不参与拟合的调头盲测，不再要求另外四组采集目录。
- 现有 `release/SunshineSiRod_Endface*` 属于已退役v8产物，不得交付。
- 现场目录 `C:\Users\Administrator\Downloads\SunshineSiRod_Endface` 在2026-07-14审计时仍装着旧v8 `endface_face_angle_offset`：八通道偏移均值为head `+0.084655462°`、tail `-0.071998214°`，会把任意棒四面均值推向机构棒 `90.086250°/89.930417°`。用户观察到“换什么棒都与标定棒接近”由此得到直接解释；该目录不得继续作为有效测量软件。
- 20张专业HOBJ的120100字节头部只有拍摄时间字节19～23变化，没有扫描方向或物理R/L元数据。无外部方向证据时，程序只能保留设备最小行端/最大行端，不能可靠自动命名物理R/L。
- 旧CTB机械漂移V3不能跨棒泛化：专业标准棒19张有效图均为 `unmatched_unadjusted`（幅值约 `-2.07～-3.20`，相关约 `-0.996～-0.998`）。同面双相机拼接变化能把CTB正常的约 `0.27～0.42 mm` 与已知异常的约 `1.03～1.30 mm` 分开，但专业棒也约 `1.00～1.62 mm`，目前只能告警，不能据此补偿。通用修正需要同一专业棒正常/异常配对图或固定基准靶/编码器。
- 端面最终角度手工补偿已从完整Web和端面专用Web统一移除。旧配置键 `endface_angle_offsets_deg` 加载时丢弃、API提交时明确拒绝；边长、对角线和棒长显示补偿仍保留且不覆盖原始CSV。
- 同面双相机拼接诊断已接入测量、切片CSV和Web机械状态栏：最大鲁棒拼接变化≤`0.60 mm`为稳定、≥`0.80 mm`为相对运动/真实面非平面告警，中间为不确定；它永远记录 `correction_applied=false`，不改变坐标或测量结果。实图回归：CTB正常`0.411935 mm`，已知异常`1.276917 mm`，专业棒样本`1.570266 mm`。
- 旧release目录/ZIP均已改名为 `RETIRED_UNSAFE_*`。下载目录旧包因运行中不能改目录名，已把活动模型替换为 `valid=false` 退役哨兵并把启动BAT改成警告后退出；旧编译测量程序实测退出码1且不生成CSV。旧模型和BAT均以 `RETIRED_UNSAFE_V8_*.backup.*` 保留审计备份。
- 无网现场补采工具包构建逻辑已改为v12，只保留未调头`head_to_tail`10张与物理调头`tail_to_head`10张两个空目录；不再错误要求“相机正扫/反扫+额外调头前后”四组。现有ZIP若仍写四组说明则属于旧v11产物，重新构建前不得交付。
- v11工具包新增 `先检查相机机械状态.bat` / `--camera-state-only`：允许先用1～3张HOBJ做纯图像预检，不读取机构真值、不生成模型或补偿。实测普通棒`20_14.hobj`为`relative_geometry_stable`、最大`0.345425 mm`、退出码0；当前专业棒`13_20.hobj`为告警、最大`1.570266 mm`、退出码4。两份预检CSV均明确`correction_applied=false`、`uses_professional_truth=false`。
- v12按正确物理调头映射重算当前20图：留出RMSE`0.168165°`、最大误差`0.359142°`；两姿态映回同一物理通道最大差`0.238913°`；2+2真实调头盲测RMSE`0.196346°`、最大误差`0.247231°`、重复性极差`0.006177°`。重复性很好但系统等变误差超限，加上19张可用图机械状态门禁全部告警，模型以`valid=false`拒绝；审计文件为`tools/results/endface_v12_physical_turnover_y_audit_rejected.json`及对应CSV。

> 下方2026-07-13的v8方案为历史记录，涉及最终8角补偿、方向分类器、已验证离线包的结论全部作废，不代表当前可用状态。

## 2026-07-13 专业机构真值端面专用离线版

- 新增 `tools\endface_calibrator.py`：现场读取同一根专业标定棒的正向10张与物理调头10张 HOBJ，以及按规格分开的机构报告 CSV，生成一个统一的8通道端面角度补偿模型。
- 统一模型只允许一套物理R/L `angle_offsets_deg`，不生成 `normal/turnover` 两套答案。机构报告端点固定为 **R=头、L=尾**；面号固定为 A上、B右、C左、D下，是端面专版唯一对外标准。软件内部自动使用 `A=A,B=B,C=D,D=C`，Web和CSV始终按机构ABCD显示。反向标定图先交换 `head/tail` 到物理R/L坐标再参与同一模型拟合；A/B/C/D不因扫描方向改变。运行时由HOBJ几何分类器自动识别是否反向，再将物理补偿映射回当前图像位置。分类器不读取机构角度真值，诊断CSV不新增人工动作字段。
- 当前端面专业机构离线包只开放 `210_105`，规格和样品号写死，不让现场选择 `210_210`。
- 端面专版模型当前为 `version=8`，策略为 `single_physical_endpoint_offset_with_hobj_orientation_classifier`。A/B/C/D四个纵向侧平面分别由各自两条角点轨迹沿棒身拟合，端面夹角使用带方向的 `0..180°` 平面法向夹角。方向分类器使用“调头相机模型下的截面规格RMSE、正常坐标下两端Z斜率均值、两端Z斜率差”三个HOBJ特征；不得读取右侧机构真值。生成模型前执行逐图留一异常检查、分类留一验证和剩余有效图全部2+2盲测组合穷举。现场20图排除 `14_16.hobj` 后，留出RMSE为 `0.026512°`、最大误差 `0.071121°`，1620/1620组合通过，分类训练及留一准确率均为100%。另外用 `D:\Image_risen` 的3张正常和3张物理调头图作非拟合验证，6/6方向识别正确。
- 端面专版运行时必须拒绝 `valid!=true`、旧策略、规格不符或8通道不完整的模型；任一端面角缺失时整张HOBJ失败且不得标记为已处理。现场标定失败时删除旧输出模型，避免误用上次JSON。
- 默认以前8+8张拟合、后2+2张盲测，并检查正向/调头残差极差、盲测 RMSE 和最大误差；两方向无法由同一模型解释时拒绝生成模型。
- 新增端面专用离线构建脚本 `tools\build_endface_offline_package.py`，编译 Measure、Web Dashboard、EndfaceCalibrator 三个程序，不需要现场安装 Python 或联网，不携带 HOBJ。当前包预装由20张专业标准棒图生成并验证通过的v8模型，同时保留现场同规格重新标定入口；严禁回退或夹带旧人工端面模型。
- 端面专用 Web 左侧只显示当前 HOBJ 的头/尾四面夹角和两个四面平均夹角，右侧显示模型中保存的专业机构标准值。标准值固定按物理头/尾展示，不能用来猜测随机测试棒的朝向。
- 端面专用 `measurement_statistics.csv` 只保存5个追溯字段和10个端面结果字段；每张 HOBJ 的切片 CSV 也只保存端面相关数据，不输出边长、对角线、倒角、棒长或机械漂移字段。
- `head/tail_endface_verticality_deg` 仍是四个直接夹角的算术平均值，不是相对90°的误差。
- 端面专用 Web 固定使用独立端口 `8767`，避免旧完整离线包占用 `8766` 时误打开旧页面；端面版浏览器标题为 `End-face Perpendicularity Dashboard`。

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
