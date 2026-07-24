# 方棒测量算法HTTP服务部署说明

## 1. 用途

本服务在同一台Windows电脑上为C#客户端提供方棒测量能力。C#负责UI、相机采图和
结果展示；算法服务负责读取HOBJ或四张TIFF、执行冻结V7算法并返回JSON。

服务只监听`127.0.0.1:8780`，不对局域网开放。生产电脑不需要安装Python，也不得只
复制单个EXE，必须完整复制整个目录。

运行环境为64位Windows 10/11；Windows服务宿主需要.NET Framework 4.6.1或更高版本。
C#客户端使用.NET Framework 4.7.2时已经满足该要求。

## 2. 推荐安装目录

将完整目录复制到：

```text
D:\SquareRodAlgorithmService\
```

运行数据默认写入：

```text
D:\SquareRodApiData\
```

相机数据约定保存到：

```text
D:\Image1\
```

## 3. 安装Windows服务

以管理员身份运行：

```text
install_service.bat
```

然后运行：

```text
service_status.bat
```

正常返回应包含：

```json
{
  "ready": true,
  "state": "ready",
  "baseline_verified": true
}
```

Windows服务名称为`SquareRodAlgorithmService`，启动类型为自动（延迟启动）。

## 4. 停止和卸载

重启服务：

```text
restart_service.bat
```

卸载服务：

```text
uninstall_service.bat
```

卸载服务不会删除`D:\SquareRodApiData`中的历史任务和审计结果。

## 5. C#启动逻辑

C#启动后调用`GET /api/v1/ready`。只有`ready=true`时才允许提交测量。注册成Windows
服务后，C#不得再次直接启动`SquareRodAlgorithmService.exe`，否则会造成8780端口冲突。

## 6. 升级

1. 停止Windows服务；
2. 备份旧程序目录和生产配置；
3. 将新版本解压到新的空目录；
4. 校验ZIP和目录内`SHA256SUMS.txt`；
5. 卸载旧服务；
6. 在新目录安装服务；
7. 检查`/ready`并使用基准HOBJ验收。

禁止把新包覆盖到正在运行的旧目录。
