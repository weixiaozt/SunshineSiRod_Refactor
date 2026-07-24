# C#接入说明

## 1. 交付边界

C#负责：

- UI和用户操作；
- 四相机采图；
- 确保文件写入完成并关闭句柄；
- 提交测量请求；
- 轮询任务状态；
- 显示数值、不可用原因和服务异常。

算法服务负责：

- HOBJ/TIFF读取；
- V7正式算法计算；
- 任务排队、超时和Worker恢复；
- JSON结果和完整审计文件。

## 2. 推荐流程

1. C#启动时轮询`GET /api/v1/ready`；
2. `ready=true`后开放“检测”按钮；
3. 每次采集使用唯一`capture_id`和独立目录；
4. 四张文件全部写完并关闭后提交；
5. 保存HTTP 202返回的`job_id`；
6. 每250～500毫秒查询一次任务；
7. 到达`completed`或`failed`后停止轮询；
8. 按每个结果项的`status/value/reason`显示；
9. 保存`request_id`、`capture_id`、`job_id`用于追溯。

## 3. UI建议

```text
ready=false                  算法服务初始化中/异常
queued                       排队中
measuring_geometry           尺寸几何计算中
measuring_endface            端面计算中
success                      测量完成
partial_success              部分项目不可用
failed                       测量失败
CALCULATION_SLOW             计算超过20秒
QUEUE_FULL                   队列已满
```

部分成功必须显示已获得的正常数值，不能因为一个字段为`null`清空整页。

## 4. 示例代码

`examples\SquareRodApiClient.cs`提供：

- `/ready`轮询；
- HOBJ/四TIFF请求结构；
- 异步提交；
- 任务轮询；
- 429队列满处理；
- 超时和取消。

生产环境注册Windows服务后，只使用就绪检查和HTTP调用，不调用示例中的独立进程
启动逻辑。
