# 实时视频流截图功能 (LiveStreamFrameCapture)

## 概述

`LiveStreamFrameCapture` 是专门为实时视频流截图而设计的新类，提供了灵活的帧截取功能。该类不修改项目中现有的任何实现，是一个全新的独立功能模块。

## 主要特性

- **实时流监听**: 持续监听视频流并处理帧数据
- **多种截图触发方式**: 支持按时间、按帧数、立即截图等多种方式
- **任务队列管理**: 支持同时安排多个截图任务
- **持续截图**: 支持按间隔持续截图功能
- **流状态监控**: 实时获取流的状态信息（帧率、帧数、运行时间等）
- **灵活的输出配置**: 支持自定义输出尺寸和格式

## 功能详解

### 1. 基本使用流程

```csharp
// 创建实例
using var liveCapture = new LiveStreamFrameCapture("rtsp://your-stream-url");

// 启动流监听
bool started = await liveCapture.StartAsync(timeoutSeconds: 10);
if (started)
{
    // 执行截图操作...
    
    // 停止监听
    liveCapture.Stop();
}
```

### 2. 截图功能

#### 2.1 立即截图
```csharp
// 立即截取当前帧
bool success = await liveCapture.CaptureNowAsync("output/current_frame.jpg", width: 640, height: 480);
```

#### 2.2 延时截图
```csharp
// 在流开始播放5秒后截图
string taskId = liveCapture.CaptureAfterSeconds(5.0, "output/after_5s.jpg");
```

#### 2.3 指定帧截图
```csharp
// 在第100帧时截图
string taskId = liveCapture.CaptureAfterFrames(100, "output/frame_100.jpg");
```

#### 2.4 持续截图
```csharp
// 每2秒截图一次，最多10张
var cts = new CancellationTokenSource();
await liveCapture.StartContinuousCaptureAsync(
    intervalSeconds: 2.0,
    outputFolder: "output/continuous",
    maxFrames: 10,
    cancellationToken: cts.Token
);
```

### 3. 流信息监控

```csharp
var info = liveCapture.GetStreamInfo();
Console.WriteLine($"当前帧数: {info.CurrentFrame}");
Console.WriteLine($"帧率: {info.Fps} fps");
Console.WriteLine($"运行时长: {info.ElapsedSeconds} 秒");
Console.WriteLine($"是否运行: {info.IsRunning}");
```

## 使用示例

### 运行演示程序

1. 编译并运行程序
2. 选择选项 `4` (Use Live Stream Frame Capture)
3. 输入视频流URL或本地视频文件路径
4. 根据菜单提示进行各种截图操作

### 批量任务示例

选择选项 `5` (Live Stream Batch Capture Example) 可以看到如何安排多个截图任务的演示。

## 支持的视频源

- **RTSP流**: `rtsp://example.com/stream`
- **HTTP流**: `http://example.com/stream.m3u8`
- **本地视频文件**: `video.mp4`, `video.avi` 等
- **网络摄像头**: 设备索引如 `0`, `1` 等

## 输出格式

支持常见的图片格式：
- **JPEG**: `.jpg`, `.jpeg` (质量级别95)
- **PNG**: `.png` (压缩级别9)
- **其他格式**: 根据文件扩展名自动识别

## 技术实现

- **基于OpenCV**: 使用OpenCvSharp库进行视频处理
- **多线程设计**: 独立的捕获线程处理帧数据，主线程处理用户交互
- **任务队列**: 使用线程安全的队列管理截图任务
- **异步支持**: 所有IO操作均支持异步执行

## 使用场景

- **安防监控**: 定时截取监控画面
- **直播录制**: 按时间间隔保存直播画面
- **视频分析**: 提取特定时刻的视频帧进行分析
- **自动化测试**: 在特定时间点验证视频内容

## 注意事项

1. **资源管理**: 使用完毕后请调用 `Dispose()` 或使用 `using` 语句
2. **网络稳定性**: 对于网络流，建议设置合适的超时时间
3. **性能考虑**: 持续截图时注意磁盘空间和系统性能
4. **格式支持**: 确保OpenCV支持目标视频格式

## 错误处理

类中包含了完善的错误处理机制：
- 连接超时检测
- 帧读取失败处理
- 文件写入异常处理
- 线程安全的状态管理

如果遇到问题，请检查控制台输出的错误信息。