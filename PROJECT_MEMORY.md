# Project Memory

本文档记录当前项目讨论到的关键事实、算法状态和后续约定，方便下次继续工作时快速接上上下文。

## 项目位置与仓库

- 本地路径：`D:\ProjectCode\SunshineSiRod_Refactor`
- GitHub：`git@github.com:weixiaozt/SunshineSiRod_Refactor.git`
- 当前主分支：`main`
- 首次推送提交：`5dd22c7 Initial project import`
- 大体积数据未进 Git：`.hobj`、`.tif/.tiff`、大 DLL、`bin/obj/packages` 等已通过 `.gitignore` 排除。

## 四相机物理位置

用户已确认四个相机/角点关系如下：

```text
3 ---- A ---- 1
|             |
D             B
|             |
2 ---- C ---- 4
```

物理含义：

- Object1 / Camera1：top，对应 P1，右上角
- Object2 / Camera2：down，对应 P2，左下角
- Object3 / Camera3：left，对应 P3，左上角
- Object4 / Camera4：right，对应 P4，右下角

边长定义：

```text
A = distance(P3, P1)
B = distance(P1, P4)
C = distance(P2, P4)
D = distance(P3, P2)
```

不允许强制：

```text
A = C
B = D
```

因为真实横截面可能不是标准矩形，也可能存在小的多边形差异。

## 用户确认的测量定义

### 倒角

每个角相机拟合三段：

- L1：一个主面
- L2：另一个主面
- Lc：倒角面

定义：

```text
P  = L1 与 L2 延长线交点，即理论尖角
T1 = L1 与 Lc 交点
T2 = L2 与 Lc 交点
M  = (T1 + T2) / 2，即倒角中点
```

输出：

```text
倒角弧长 = distance(T1, T2)
投影X = abs(T2.x - T1.x)
投影Z = abs(T2.z - T1.z)
夹角角度 = angle(L1, L2)，角点是 P，不是 M/T1/T2
垂直度误差 = abs(90 - angle(L1, L2))
```

### 对角线

用户已纠正：对角线不是理论尖角到理论尖角，而是倒角中点到倒角中点。

```text
对角线1 = distance(M1, M2)
对角线2 = distance(M3, M4)
```

其中 M1/M2/M3/M4 是四个倒角的中点。

## 当前 Python 工具

主脚本：

```text
tools\measure_square_rod_edges.py
```

标定文件：

```text
tools\camera_calibration_model.json
```

当前标定版本：

```text
version = 6
model = camera_oriented_transform_with_corner_bias
```

重要约定：

- 默认测量模式是 `free`
- 不再使用之前尝试过的 `soft` 差值限幅
- 不强制 A=C、B=D
- 边长由四个全局角点直接算

运行示例：

```powershell
python tools\measure_square_rod_edges.py --input "D:\ProjectCode\SunshineSiRod_Refactor\image\test_image\17_03.hobj" --calibration "D:\ProjectCode\SunshineSiRod_Refactor\tools\camera_calibration_model.json"
```

用标定目录重建标定：

```powershell
python tools\measure_square_rod_edges.py --input "D:\ProjectCode\SunshineSiRod_Refactor\image\demarcate" --save-calibration "D:\ProjectCode\SunshineSiRod_Refactor\tools\camera_calibration_model.json" --output "D:\ProjectCode\SunshineSiRod_Refactor\tools\demarcate_free_check.csv" --overwrite --step-mm 10
```

## 已用标定数据

目录：

```text
D:\ProjectCode\SunshineSiRod_Refactor\image\demarcate
```

文件：

```text
10_40.hobj
10_40调头.hobj
demarcate_data.png
```

人工测量值：

```text
头部20%：A=210.06, B=104.00, C=210.03, D=103.98
中部50%：A=210.07, B=103.99, C=210.03, D=103.98
尾部80%：A=210.06, B=104.01, C=210.03, D=103.99
```

`10_40调头.hobj` 需要按 B/D 互换处理。

## 当前三根测试数据结论

已跑三个 HOBJ：

```text
17_03.hobj
17_11.hobj
17_36.hobj
```

结果均值：

```text
17_03:
A=210.566640, B=103.949572, C=209.469904, D=106.345709
A-C=+1.096736, B-D=-2.396137

17_11:
A=209.758896, B=103.974525, C=210.291818, D=106.327326
A-C=-0.532922, B-D=-2.352801

17_36:
A=210.366424, B=103.951098, C=209.686277, D=106.346218
A-C=+0.680147, B-D=-2.395120
```

判断：

- B/D 差值非常稳定，约 `-2.35 ~ -2.40 mm`，很像系统性拼接/标定偏差。
- A/C 差值方向不稳定，不像系统性固定偏差，不能直接做固定 offset 补偿。
- A/C 应优先做角点稳定性诊断和鲁棒统计，不要用固定补偿硬修。

## 原 HALCON 方案补偿机制结论

原方案不是严格的真实尺寸补偿，而是模板差值法。

标定时：

```text
保存标准棒每个切片的 A1/B1/C1/D1 原始测量值
```

检测时：

```text
subA = 当前A1 - 标定A1
最终A = 规格A标准值 + subA
```

代码位置：

```text
Square_Sunshine\bin\x64\Release\Calibration.hdev
Square_Sunshine\bin\x64\Release\Square sticks_Measure.hdev
```

问题：

- 结果会被规格标准值牵引，不是纯真实测量。
- 超出标准值窗口的真实偏差可能被过滤掉。
- 按 row 模板补偿强依赖采集起点/棒长/速度一致。
- A/B/C/D 的过滤规则不一致。
- 补偿值物理意义不清晰，有的规格是几十，有的规格是 1400 多。
- 异常大量 `catch` 或 `continue`，容易掩盖失败。

结论：可以借鉴“标准棒补偿”的思想，但不能照搬原方案。

## 后续数据采集建议

用户准备继续找更多人工测量棒和图像。

推荐格式：

```text
棒号, 方向, hobj路径, 位置, A, B, C, D
001, 正常, xxx.hobj, 20%, 210.06, 104.00, 210.03, 103.98
001, 正常, xxx.hobj, 50%, 210.07, 103.99, 210.03, 103.98
001, 正常, xxx.hobj, 80%, 210.06, 104.01, 210.03, 103.99
001, 调头, xxx.hobj, 20%, ...
```

优先数据：

- B/D 接近相等的棒
- B/D 明显大于 104 的棒
- A/C 存在小差异的棒
- 同一根棒正常 + 调头都拍过

目标：

- 判断 B/D 是固定 offset，还是 scale + offset。
- 判断 A/C 是真实波动，还是某个相机角点不稳定。
- 建立 A/B/C/D 分边 raw/corrected 输出。

