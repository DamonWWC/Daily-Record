# SourceGenerator 自动依赖注入使用指南

## 概述

本项目实现了一个 SourceGenerator，可以自动扫描标记了 `[ServiceRegistration]` 特性的类，并生成相应的依赖注入注册代码。

## 功能特性

- 🚀 **自动发现服务**：扫描所有标记了 `[ServiceRegistration]` 特性的类
- 🔧 **生命周期支持**：支持 Singleton、Scoped、Transient 三种服务生命周期
- 🎯 **接口绑定**：支持将实现类绑定到指定接口
- 📦 **零运行时开销**：编译时生成代码，运行时无反射
- 🔄 **增量生成**：只在相关代码变更时重新生成

## 使用方法

### 1. 标记服务类

在需要注册到 DI 容器的类上添加 `[ServiceRegistration]` 特性：

```csharp
using WPFDeveloper.Attributes;

// 注册为自身，使用默认生命周期（Transient）
[ServiceRegistration]
public class MyService
{
    // 服务实现
}

// 注册到指定接口，指定生命周期
[ServiceRegistration(typeof(IMyService), ServiceLifetime.Singleton)]
public class MyService : IMyService
{
    // 服务实现
}

// 仅指定生命周期
[ServiceRegistration(ServiceLifetime.Scoped)]
public class AnotherService
{
    // 服务实现
}
```

### 2. 调用生成的注册方法

在应用程序启动时调用生成的扩展方法：

```csharp
// 在 App.xaml.cs 或服务配置方法中
services.AddGeneratedServices();
```

### 3. 查看生成的服务信息（可选）

```csharp
// 获取所有自动注册的服务信息
var serviceInfos = ServiceCollectionExtensions.GetGeneratedServiceInfos();
foreach (var info in serviceInfos)
{
    Console.WriteLine($"Service: {info.ServiceType.Name} -> {info.ImplementationType.Name} ({info.Lifetime})");
}
```

## 支持的特性参数

### ServiceRegistrationAttribute

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| ServiceType | Type | null | 服务接口类型，如果不指定则使用类本身 |
| Lifetime | ServiceLifetime | Transient | 服务生命周期 |
| ReplaceExisting | bool | false | 是否替换已存在的注册（暂未实现） |

### ServiceLifetime 枚举

| 值 | 说明 |
|----|------|
| Transient | 瞬态：每次请求都创建新实例 |
| Scoped | 作用域：在同一作用域内复用同一实例 |
| Singleton | 单例：全局唯一实例 |

## 项目中的示例

### 服务类示例

```csharp
// NavigationService.cs
[ServiceRegistration(typeof(INavigationRegistry), ServiceLifetime.Singleton)]
public class NavigationRegistry : INavigationRegistry
{
    // 实现代码...
}

// ExternalProcessHostService.cs
[ServiceRegistration(typeof(IExternalProcessHostService), ServiceLifetime.Singleton)]
public class ExternalProcessHostService : IExternalProcessHostService, IDisposable
{
    // 实现代码...
}
```

### ViewModel 示例

```csharp
// MainWindowViewModel.cs
[ServiceRegistration(ServiceLifetime.Transient)]
public class MainWindowViewModel : ObservableObject
{
    // ViewModel 实现...
}
```

### 视图示例

```csharp
// MainWindow.xaml.cs
[ServiceRegistration(ServiceLifetime.Transient)]
public partial class MainWindow : Window
{
    // 窗口实现...
}

// AboutView.xaml.cs
[View("about", DisplayName = "关于", Group = "General", Order = 1)]
[ServiceRegistration(ServiceLifetime.Transient)]
public partial class AboutView : UserControl
{
    // 视图实现...
}
```

## 生成的代码示例

SourceGenerator 会自动生成类似以下的扩展方法：

```csharp
// ServiceRegistrationExtensions.g.cs (自动生成)
public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddGeneratedServices(this IServiceCollection services)
    {
        // Singleton 服务
        services.AddSingleton<INavigationRegistry, NavigationRegistry>();
        services.AddSingleton<IExternalProcessHostService, ExternalProcessHostService>();

        // Transient 服务
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<MainWindow>();
        services.AddTransient<AboutView>();

        return services;
    }

    public static IReadOnlyList<GeneratedServiceInfo> GetGeneratedServiceInfos()
    {
        return new[]
        {
            new GeneratedServiceInfo(
                serviceType: typeof(INavigationRegistry),
                implementationType: typeof(NavigationRegistry),
                lifetime: ServiceLifetime.Singleton),
            // 更多服务信息...
        };
    }
}
```

## 注意事项

1. **命名空间冲突**：注意区分 `WPFDeveloper.Attributes.ServiceLifetime` 和 `Microsoft.Extensions.DependencyInjection.ServiceLifetime`
2. **partial 类**：如果手动创建了 `ServiceCollectionExtensions` 类，需要添加 `partial` 修饰符
3. **构建顺序**：确保 SourceGenerator 项目先于主项目构建
4. **增量编译**：修改特性标记后需要重新构建才能生效

## 架构优势

- **编译时安全**：所有依赖在编译时验证，避免运行时错误
- **性能优化**：无反射开销，启动速度更快
- **代码可见**：生成的注册代码清晰可见，便于调试
- **维护简便**：只需添加特性，无需手动维护注册代码
- **类型安全**：充分利用 C# 类型系统，减少配置错误

这个 SourceGenerator 极大地简化了依赖注入的配置工作，让开发者能够专注于业务逻辑而不是基础设施代码。
