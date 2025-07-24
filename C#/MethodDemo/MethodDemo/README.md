# Flyleaf Video Player - Optimized Version

## 项目概述 (Project Overview)

Flyleaf是一个基于FlyleafLib的视频播放器项目，通过动态加载FlyleafLib1.dll来创建LiveControl视频播放控件。本项目已经过全面优化，提供了更好的性能、错误处理和用户体验。

Flyleaf is a video player project based on FlyleafLib that dynamically loads FlyleafLib1.dll to create LiveControl video playback controls. This project has been comprehensively optimized to provide better performance, error handling, and user experience.

## 主要优化 (Key Optimizations)

### 1. 动态加载优化 (Dynamic Loading Optimization)
- **单例模式**: 使用`DynamicControlLoader`单例类管理DLL加载，避免重复加载
- **缓存机制**: 缓存已加载的Assembly和Type，提高性能
- **错误处理**: 完善的错误处理和日志记录机制

### 2. 架构改进 (Architecture Improvements)
- **服务分离**: 将动态加载逻辑分离到独立的服务类
- **配置管理**: 添加配置文件支持，便于自定义设置
- **资源管理**: 实现IDisposable接口，确保资源正确释放

### 3. 用户界面优化 (UI Optimization)
- **现代化界面**: 重新设计的用户界面，支持现代化样式
- **功能增强**: 添加截图、全屏、URL加载等功能
- **状态显示**: 实时显示播放状态和操作反馈

### 4. 错误处理和日志 (Error Handling & Logging)
- **完善的异常处理**: 替换空的catch块，添加详细的错误信息
- **日志系统**: 集成日志记录，便于调试和问题排查
- **用户友好的错误提示**: 提供清晰的错误消息给用户

## 项目结构 (Project Structure)

```
Flyleaf/
├── Configuration/
│   └── VideoPlayerConfig.cs          # 配置管理类
├── Services/
│   └── DynamicControlLoader.cs       # 动态控件加载器
├── FlyleafVideoNewControl.xaml       # 视频控件XAML
├── FlyleafVideoNewControl.xaml.cs    # 视频控件逻辑
├── MainWindow.xaml                   # 主窗口XAML
├── MainWindow.xaml.cs                # 主窗口逻辑
├── App.xaml                          # 应用程序XAML
├── App.xaml.cs                       # 应用程序逻辑
├── appsettings.json                  # 配置文件
└── Flyleaf.csproj                    # 项目文件

FlyleafLib/
├── Controls/
│   └── LiveControl.cs                # 核心视频控件
├── MediaPlayer/                      # 媒体播放器核心
├── Engine/                           # 播放引擎
└── FlyleafLib.csproj                 # 库项目文件
```

## 主要功能 (Key Features)

### 1. 视频播放 (Video Playback)
- 支持多种视频格式和协议 (RTMP, HTTP, 本地文件)
- 自动重试机制
- 播放状态监控

### 2. 用户界面 (User Interface)
- 播放/停止控制
- URL输入和加载
- 全屏模式切换
- 实时状态显示

### 3. 截图功能 (Screenshot Feature)
- 一键截图当前视频帧
- 自动生成文件名
- 可配置保存目录

### 4. 配置管理 (Configuration Management)
- JSON配置文件支持
- 运行时配置修改
- 默认设置和自定义设置

## 使用方法 (Usage)

### 1. 基本使用 (Basic Usage)

```csharp
// 创建视频控件
var videoControl = new FlyleafVideoNewControl();

// 设置播放状态回调
videoControl.PlayAction = (isPlaying) => {
    Console.WriteLine($"播放状态: {(isPlaying ? "播放中" : "已停止")}");
};

// 开始播放
await videoControl.StartPlayAsync("rtmp://your-stream-url");

// 停止播放
videoControl.StopPlay();

// 截图
videoControl.TakeSnapshot("screenshot.png");
```

### 2. 配置使用 (Configuration Usage)

```csharp
// 加载配置
var config = VideoPlayerConfig.LoadFromFile("appsettings.json");

// 使用配置
videoControl.DelayPlayTimes = config.PlaybackDelayMs;

// 保存配置
config.SaveToFile("appsettings.json");
```

### 3. 动态加载器使用 (Dynamic Loader Usage)

```csharp
// 获取动态加载器实例
var loader = DynamicControlLoader.Instance;

// 初始化
if (loader.Initialize())
{
    // 创建控件
    var control = loader.CreateLiveControl();
    
    // 设置URL
    loader.SetCameraUrl(control, "your-video-url");
    
    // 截图
    loader.TakeSnapshot(control, "snapshot.png");
}
```

## 配置选项 (Configuration Options)

### appsettings.json 配置说明

```json
{
  "DefaultVideoUrls": [                    // 默认视频URL列表
    "rtmp://example.com/stream",
    "local-video.mp4"
  ],
  "PlaybackDelayMs": 200,                  // 播放延迟(毫秒)
  "EnableAutoRetry": true,                 // 启用自动重试
  "MaxRetryAttempts": 3,                   // 最大重试次数
  "RetryDelayMs": 1000,                    // 重试延迟(毫秒)
  "EnableDebugLogging": true,              // 启用调试日志
  "SnapshotDirectory": "Snapshots",        // 截图保存目录
  "SnapshotFileNameFormat": "snapshot_{0:yyyyMMdd_HHmmss}.png", // 截图文件名格式
  "FlyleafLibPath": "FlyleafLib1.dll",     // FlyleafLib DLL路径
  "Window": {                              // 窗口设置
    "DefaultWidth": 1000,                  // 默认宽度
    "DefaultHeight": 600,                  // 默认高度
    "MinWidth": 600,                       // 最小宽度
    "MinHeight": 400,                      // 最小高度
    "RememberWindowState": true,           // 记住窗口状态
    "StartFullscreen": false               // 启动时全屏
  }
}
```

## 性能优化建议 (Performance Optimization Tips)

### 1. 内存管理 (Memory Management)
- 及时调用`Dispose()`方法释放资源
- 避免创建多个视频控件实例
- 监控内存使用情况

### 2. 网络优化 (Network Optimization)
- 使用合适的缓冲区大小
- 配置网络超时设置
- 实现断线重连机制

### 3. UI响应性 (UI Responsiveness)
- 使用异步方法进行耗时操作
- 在UI线程上更新界面元素
- 避免阻塞主线程

## 故障排除 (Troubleshooting)

### 常见问题 (Common Issues)

1. **FlyleafLib1.dll未找到**
   - 确保DLL文件在应用程序目录中
   - 检查配置文件中的路径设置

2. **视频播放失败**
   - 检查视频URL是否有效
   - 确认网络连接正常
   - 查看日志文件获取详细错误信息

3. **截图功能不工作**
   - 确保有视频正在播放
   - 检查截图目录的写入权限
   - 验证文件名格式是否正确

### 调试建议 (Debugging Tips)

1. 启用调试日志记录
2. 使用Visual Studio调试器
3. 检查Windows事件日志
4. 监控系统资源使用情况

## 依赖项 (Dependencies)

- .NET Framework 4.7.2+
- FlyleafLib (基于FFmpeg/DirectX)
- System.Text.Json
- Microsoft.Extensions.Logging

## 许可证 (License)

本项目遵循FlyleafLib的LGPL-3.0许可证。

## 贡献 (Contributing)

欢迎提交问题报告和功能请求。在提交代码之前，请确保：

1. 代码符合项目的编码规范
2. 添加适当的单元测试
3. 更新相关文档
4. 测试所有功能正常工作

## 更新日志 (Changelog)

### v2.0.0 (优化版本)
- ✅ 重构动态加载机制，使用单例模式
- ✅ 添加配置文件支持
- ✅ 改进用户界面设计
- ✅ 完善错误处理和日志记录
- ✅ 添加截图和全屏功能
- ✅ 实现资源管理和内存优化
- ✅ 提供完整的API文档

### v1.0.0 (原始版本)
- 基本视频播放功能
- 简单的用户界面
- 基础的动态加载机制