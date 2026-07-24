# Windows服务说明

本包使用WinSW 2.12.0 x64将算法控制台程序托管为Windows服务。WinSW采用MIT许可证，
许可证位于`third_party\WinSW_LICENSE.txt`。

## 服务分层恢复

- 算法Worker退出：HTTP宿主自动拉起新Worker；
- 算法任务超过硬超时：终止Worker并最多自动重试一次；
- HTTP宿主退出：Windows服务在5秒后重启；
- 电脑重启：服务自动延迟启动。

## 管理命令

管理员PowerShell：

```powershell
cd "D:\SquareRodAlgorithmService"
.\SquareRodServiceHost.exe install
.\SquareRodServiceHost.exe start
.\SquareRodServiceHost.exe status
.\SquareRodServiceHost.exe restart
.\SquareRodServiceHost.exe stop
.\SquareRodServiceHost.exe uninstall
```

也可直接使用随包BAT脚本。

## 权限

默认服务账户由Windows服务管理器决定。必须确保服务账户：

- 可读取程序目录和`D:\Image1`；
- 可读写`D:\SquareRodApiData`；
- 可监听本机8780端口。

若图片位于网络共享，不要使用交互用户的映射盘符；应改用UNC路径，并为服务账户授予
共享权限和NTFS权限。

## 日志

```text
D:\SquareRodApiData\logs\            算法服务日志
程序目录\service_logs\               WinSW stdout/stderr和宿主日志
Windows事件查看器                    服务安装和启动错误
```
