# API接口协议

## 1. 基本信息

```text
Base URL: http://127.0.0.1:8780/
Content-Type: application/json; charset=utf-8
API schema: square_rod_measurement_api_v1
```

接口：

```text
GET  /api/v1/health
GET  /api/v1/ready
GET  /api/v1/version
POST /api/v1/measurements
GET  /api/v1/measurements/{job_id}
GET  /api/v1/measurements/{job_id}/audit
```

## 2. HOBJ请求

```json
{
  "request_id": "line1-20260724-000001",
  "capture_id": "capture-000001",
  "specimen": "105-11",
  "input": {
    "type": "hobj",
    "path": "D:\\Image1\\hobj_test\\normal_10_16.hobj"
  }
}
```

`request_id`必须在C#侧唯一。重复提交同一个`request_id`时，服务返回原任务，避免重复
计算。提交成功返回HTTP 202和`job_id`。

## 3. 四TIFF请求

```json
{
  "request_id": "line1-20260724-000002",
  "capture_id": "capture-000002",
  "input": {
    "type": "tiff_set",
    "cameras": {
      "Left": "D:\\Image1\\capture-000002\\1-Left.tif",
      "Right": "D:\\Image1\\capture-000002\\2-Right.tif",
      "Top": "D:\\Image1\\capture-000002\\3-Top.tif",
      "Down": "D:\\Image1\\capture-000002\\4-Down.tif"
    }
  }
}
```

相机映射只认JSON键，不根据文件名排序。TIFF现场格式尚待验收。

## 4. 任务与测量状态

任务技术状态：

```text
queued / starting / validating_input / measuring_geometry
measuring_endface / normalizing_result / completed / failed
```

工件测量状态：

```text
success         全部正式结果可用
partial_success 部分结果不可用，其余结果保留
failed          无法形成有效测量结果
```

`job_status=completed`可以同时对应`measurement_status=partial_success`。

## 5. 不可用值

不可用值固定使用：

```json
{
  "value": null,
  "status": "unavailable",
  "reason_code": "upstream_endpoint_unavailable",
  "reason": "依赖的倒角端点不可用：CD",
  "valid_station_count": 0,
  "required_station_count": 0
}
```

禁止把`null`替换成0、NaN、-9999或空字符串。

## 6. HTTP错误

```text
400 请求JSON或输入不合法
404 任务不存在
409 服务尚未就绪
429 当前一个任务运行、一个任务等待，队列已满
500 服务内部错误
```

429响应包含`retry_after_ms`。C#应提示排队或稍后重试，不得无限快速重发。
