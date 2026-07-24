# TIFF输入规范（待现场验收）

当前接口实现支持四张独立TIFF直接内存映射，不依赖HALCON，也不生成临时HOBJ。

当前代码要求：

```text
相机数量：4
相机键：Left / Right / Top / Down
图像尺寸：20000 × 3200
通道数：单通道
像素类型：float32
压缩：无压缩
存储：连续、可由tifffile.memmap映射
```

固定业务映射：

```text
Left  -> 逻辑角4 / BC
Right -> 逻辑角1 / AB
Top   -> 逻辑角3 / AD
Down  -> 逻辑角2 / CD
```

建议目录：

```text
D:\Image1\capture-000001\
  1-Left.tif
  2-Right.tif
  3-Top.tif
  4-Down.tif
```

C#必须等四张文件全部写入完成并关闭文件句柄后再提交请求。每次采集使用独立目录，
不能覆盖算法仍在读取的上一组图片。

本预交付包尚未使用现场相机TIFF验收。拿到文件后必须检查实际dtype、shape、压缩、
页数、字节序和四相机映射；如现场为uint16或压缩TIFF，应修改并重新验证输入适配层，
不得直接声明生产放行。
