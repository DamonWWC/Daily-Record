# 外部程序嵌入功能

## 概述

外部程序嵌入功能允许在WPF应用程序中嵌入并运行外部的EXE程序，实现无缝集成的用户体验。此功能特别适用于需要在主应用程序中集成三维程序、CAD软件或其他专业工具的场景。

## 功能特性

### 核心功能
- ✅ **程序嵌入**: 将外部EXE程序嵌入到指定的WPF控件区域
- ✅ **实时交互**: 支持鼠标点击、键盘输入等正常交互操作
- ✅ **窗口管理**: 自动调整窗口大小、移除边框和标题栏
- ✅ **进程监控**: 实时监控进程状态和运行时间
- ✅ **生命周期管理**: 优雅的进程启动和退出处理

### 技术特性
- 🔧 **Win32 API集成**: 使用SetParent、SetWindowLong等API实现窗口嵌入
- 🔧 **异步操作**: 异步启动和嵌入进程，避免UI阻塞
- 🔧 **异常处理**: 完善的错误处理和日志记录机制
- 🔧 **依赖注入**: 基于Microsoft.Extensions.DependencyInjection的服务架构
- 🔧 **MVVM模式**: 遵循MVVM设计模式，职责清晰

## 使用方法

### 基本使用

1. **启动应用程序**
   - 运行WPFDeveloper项目
   - 在导航栏中选择"外部程序嵌入"

2. **选择程序**
   - 点击"快选"按钮选择常用系统程序（如记事本、计算器等）
   - 或点击"浏览..."按钮选择任意EXE文件

3. **设置参数**（可选）
   - 在"启动参数"文本框中输入程序启动时需要的命令行参数

4. **启动程序**
   - 点击"启动程序"按钮
   - 程序将自动启动并嵌入到指定区域

5. **程序交互**
   - 在嵌入区域中可以正常使用鼠标和键盘与程序交互
   - 状态栏显示程序运行状态和运行时间

6. **停止程序**
   - 点击"停止程序"按钮优雅地关闭嵌入的程序

### 推荐测试程序

以下程序适合用于测试嵌入功能：

| 程序 | 路径 | 特点 |
|------|------|------|
| 记事本 | `notepad.exe` | 简单文本编辑，测试基本交互 |
| 计算器 | `calc.exe` | 按钮操作，测试鼠标点击 |
| 画图程序 | `mspaint.exe` | 图形编辑，测试复杂鼠标操作 |
| 写字板 | `write.exe` | 富文本编辑，测试键盘输入 |

## 架构设计

### 核心组件

#### 1. IExternalProcessHostService
```csharp
public interface IExternalProcessHostService
{
    Task<Process?> StartAndEmbedProcessAsync(string exePath, IntPtr hostHandle, string? arguments = null);
    bool ResizeEmbeddedProcess(Process process, int x, int y, int width, int height);
    bool StopEmbeddedProcess(Process process);
    // ... 其他方法和事件
}
```

#### 2. ExternalProcessHost 控件
- 提供完整的UI界面用于程序选择、启动和管理
- 支持依赖属性绑定和MVVM模式
- 集成状态监控和错误处理

#### 3. Win32Api 互操作
- 封装必要的Win32 API调用
- 提供窗口嵌入和管理功能
- 处理窗口样式修改

### 服务注册

在`ServiceCollectionExtensions.cs`中注册服务：

```csharp
services.AddSingleton<IExternalProcessHostService, ExternalProcessHostService>();
```

## 技术实现

### 窗口嵌入流程

1. **启动进程**: 使用`Process.Start()`启动外部程序
2. **等待窗口**: 轮询等待进程创建主窗口
3. **设置父窗口**: 使用`SetParent()`将外部程序窗口设为子窗口
4. **修改样式**: 移除标题栏、边框等装饰
5. **调整大小**: 根据宿主区域调整窗口大小和位置

### 关键API调用

```csharp
// 设置父窗口
SetParent(childWindowHandle, hostWindowHandle);

// 修改窗口样式
uint newStyle = currentStyle & ~(WS_CAPTION | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX);
newStyle |= WS_CHILD;
SetWindowLong(windowHandle, GWL_STYLE, newStyle);

// 调整窗口位置和大小
MoveWindow(windowHandle, x, y, width, height, true);
```

## 注意事项

### 兼容性
- ✅ 支持大部分标准Windows应用程序
- ⚠️ 某些程序可能有特殊的窗口管理机制，嵌入效果可能不理想
- ⚠️ UWP应用程序由于沙盒机制无法嵌入

### 权限要求
- 需要管理员权限来嵌入某些系统程序
- 部分程序可能需要特定的安全权限

### 性能考虑
- 嵌入的程序运行在独立的进程中，不会影响主应用程序性能
- 建议监控嵌入程序的资源使用情况

## 扩展功能

### 可能的扩展方向

1. **多程序支持**: 同时嵌入多个程序到不同区域
2. **程序通信**: 实现主程序与嵌入程序之间的数据交换
3. **配置管理**: 保存和恢复常用程序配置
4. **插件系统**: 为特定程序类型提供专门的嵌入策略

## 故障排除

### 常见问题

1. **程序启动失败**
   - 检查程序路径是否正确
   - 确认程序文件存在且可执行
   - 检查是否有足够的权限

2. **嵌入失败**
   - 某些程序可能不支持嵌入
   - 尝试以管理员身份运行主应用程序
   - 检查程序是否有特殊的窗口保护机制

3. **交互异常**
   - 确保嵌入区域获得了正确的焦点
   - 检查鼠标和键盘消息是否正确路由

### 日志记录

系统提供详细的日志记录，可以通过日志查看：
- 进程启动和退出事件
- 窗口嵌入操作结果
- 异常信息和错误详情

## 许可证

此功能作为WPFDeveloper项目的一部分，遵循项目的整体许可证条款。
