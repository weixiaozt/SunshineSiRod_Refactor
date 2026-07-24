# Square Rod V7 HTTP API

本目录是 `release/Square Rod Measurement0723_V7` 的独立接口层。V7目录保持只读，
接口不修改、不覆盖、不向V7目录写入任何文件。

## 当前实现

- `127.0.0.1:8780` 本机HTTP接口；
- 四张未压缩、连续存储、单通道 `float32` TIFF直接使用 `tifffile.memmap`；
- 历史固定布局HOBJ继续使用NumPy内存映射；
- TIFF和HOBJ统一为 `Left/Right/Top/Down` 四相机数组；
- 复用V7的ABCD、倒角、闭合截面v2对角线、独立角度v3、棒长和端面模块；
- 一个运行任务加一个等待任务；
- SQLite持久化任务状态；
- 算法Worker常驻、宿主监控、崩溃/超时自动重启并最多重试一次；
- V7的 `success/partial_success/failed` 和逐项 `available/unavailable` 原样保留；
- 不使用HALCON，不写临时HOBJ，不从文件名或路径推断姿态。

## 开发启动

```powershell
cd D:\ProjectCode\SunshineSiRod_Refactor\interfaces\SquareRodHttpApi
python .\run_service.py --config .\api_config.local.json
```

启动阶段校验V7参考EXE、六个正式模型和端面标定SHA256。Worker完成导入、模型校验和
预热后，`GET /api/v1/ready`才返回`ready=true`。

运行数据写入本接口目录的`runtime`，不会写入V7目录。生产配置应将
`runtime_root`改为独立数据目录，例如`D:\SquareRodApiData`。

## 接口

```text
GET  /api/v1/health
GET  /api/v1/ready
GET  /api/v1/version
POST /api/v1/measurements
GET  /api/v1/measurements/{job_id}
GET  /api/v1/measurements/{job_id}/audit
```

### 四TIFF请求

```json
{
  "request_id": "20260724-000001",
  "capture_id": "capture-000001",
  "specimen": "105-11",
  "input": {
    "type": "tiff_set",
    "cameras": {
      "Left": "D:\\Capture\\capture-000001\\1-Left.tif",
      "Right": "D:\\Capture\\capture-000001\\2-Right.tif",
      "Top": "D:\\Capture\\capture-000001\\3-Top.tif",
      "Down": "D:\\Capture\\capture-000001\\4-Down.tif"
    }
  }
}
```

JSON字段决定相机映射，文件名不参与判断。第一版严格要求每张TIFF为
`20000×3200 float32`、不压缩且可直接内存映射。

### HOBJ请求

```json
{
  "request_id": "20260724-000002",
  "capture_id": "10_16",
  "input": {
    "type": "hobj",
    "path": "D:\\Capture\\10_16.hobj"
  }
}
```

### 部分成功

不可用项目使用标准JSON空值，不使用0、NaN、-9999或空字符串：

```json
{
  "value": null,
  "status": "unavailable",
  "reason_code": "upstream_endpoint_unavailable",
  "reason": "依赖的倒角端点不可用：AD",
  "valid_station_count": 0,
  "required_station_count": 595
}
```

任务技术状态和工件测量状态分开：

```text
job_status         = completed / failed
measurement_status = success / partial_success / failed
```

## 已完成的实测

同一张V7验收样本`105-11/105_3/10_16.hobj`：

- 冻结V7 EXE同机实测：约13.95秒；
- 常驻接口Worker暖缓存HOBJ：约9.90秒；
- 四张未压缩float32 TIFF直接读取：约14.05秒；
- TIFF输入映射：约0.006秒；
- TIFF与HOBJ的ABCD、对角线、四角、倒角、投影、棒长、端面及可用性字段完全一致。

首次冷文件缓存测试可能超过20秒，因此正式发布前仍需在目标生产机执行连续样本验收。
20秒是性能目标及慢任务告警线；60秒是Worker卡死保护线，两者用途不同。

现场异常样本`10_33.hobj`已验证为`partial_success`：A/B/C/D、棒长和四个独立主面角
仍正常返回；无法可靠计算的CD/CB倒角及依赖它们的对角线仅对应字段返回`null`和原因。

## 生产打包与C#自动启动

打包必须使用一个尚不存在的新目录，构建过程只读取V7并把必需运行文件复制到独立包：

```powershell
python .\build_service.py --build-root D:\Build\SquareRodApi_20260724
```

产物入口为：

```text
dist\SquareRodAlgorithmService\SquareRodAlgorithmService.exe
dist\SquareRodAlgorithmService\api_config.production.json
dist\SquareRodAlgorithmService\AlgorithmRuntime\V7\
```

`examples\SquareRodApiClient.cs`中的`EnsureServiceRunningAsync`会在C#启动时先探测
`/ready`，服务未运行时以隐藏进程拉起；随后等待Worker完成模型校验并就绪。运行中若
算法Worker崩溃或卡死，HTTP宿主仍然存活，会重启Worker并将当前任务最多自动重试一次。
若宿主本身退出，C#下一次健康检查可再次自动拉起，并向UI显示明确的启动失败或超时信息。

## 测试

```powershell
pytest -q .\tests
```

1 GiB真实HOBJ测试默认跳过：

```powershell
$env:SQUARE_ROD_RUN_REAL_HOBJ="1"
pytest -q .\tests\test_v7_equivalence.py
```
