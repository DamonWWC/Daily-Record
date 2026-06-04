# Riley.Core HTTP客户端

一个基于.NET 8的强类型HTTP客户端库，提供简单易用的API来进行HTTP请求。

## 特性

- 🚀 **简单易用**: 提供直观的API接口
- 🔄 **自动重试**: 支持可配置的重试机制
- 📝 **强类型**: 支持泛型响应类型和自动JSON序列化/反序列化
- 🔧 **高度可配置**: 支持超时、请求头、SSL设置等
- 📊 **日志集成**: 集成Microsoft.Extensions.Logging
- 🏭 **依赖注入**: 原生支持DI容器
- 📁 **文件操作**: 支持文件上传和下载
- 🏷️ **多客户端**: 支持命名客户端配置

## 快速开始

### 1. 安装依赖

确保你的项目引用了以下NuGet包：

```xml
<PackageReference Include="Microsoft.Extensions.Http" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Options" Version="8.0.2" />
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.1" />
<PackageReference Include="System.Text.Json" Version="8.0.4" />
```

### 2. 配置服务

```csharp
using Microsoft.Extensions.DependencyInjection;
using Riley.Core.Http;

var services = new ServiceCollection();

// 添加日志
services.AddLogging();

// 添加HTTP客户端服务
services.AddHttpClientService(options =>
{
    options.BaseUrl = "https://api.example.com";
    options.TimeoutSeconds = 30;
    options.MaxRetryCount = 3;
    options.DefaultHeaders.Add("Accept", "application/json");
});

var serviceProvider = services.BuildServiceProvider();
```

### 3. 使用HTTP客户端

```csharp
var httpClient = serviceProvider.GetRequiredService<IHttpClientService>();

// GET请求
var response = await httpClient.GetAsync<User>("/users/1");
if (response.IsSuccess)
{
    Console.WriteLine($"用户: {response.Data.Name}");
}

// POST请求
var newUser = new User { Name = "张三", Email = "zhangsan@example.com" };
var createResponse = await httpClient.PostAsync<User>("/users", newUser);

// PUT请求
var updateResponse = await httpClient.PutAsync<User>("/users/1", updatedUser);

// DELETE请求
var deleteResponse = await httpClient.DeleteAsync("/users/1");
```

## API 参考

### 核心接口

#### IHttpClientService

主要的HTTP客户端接口，提供以下方法：

- `GetAsync<T>()` - GET请求
- `PostAsync<T>()` - POST请求
- `PutAsync<T>()` - PUT请求
- `DeleteAsync<T>()` - DELETE请求
- `PatchAsync<T>()` - PATCH请求
- `UploadFileAsync<T>()` - 文件上传
- `DownloadFileAsync()` - 文件下载

### 配置选项

#### HttpClientOptions

```csharp
public class HttpClientOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
    public Dictionary<string, string> DefaultHeaders { get; set; } = new();
    public bool IgnoreSslErrors { get; set; } = false;
    public int MaxRetryCount { get; set; } = 3;
    public int RetryDelayMilliseconds { get; set; } = 1000;
    public string UserAgent { get; set; } = "Riley.Core.HttpClient/1.0";
}
```

### 响应模型

#### ApiResponse<T>

```csharp
public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public Exception? Exception { get; set; }
    public Dictionary<string, IEnumerable<string>>? Headers { get; set; }
}
```

## 使用示例

### 单BaseUrl基本用法

```csharp
// 配置服务
var services = new ServiceCollection();
services.AddLogging();
services.AddHttpClientService(options =>
{
    options.BaseUrl = "https://jsonplaceholder.typicode.com";
    options.TimeoutSeconds = 30;
});

var serviceProvider = services.BuildServiceProvider();
var httpClient = serviceProvider.GetRequiredService<IHttpClientService>();

// GET请求
var userResponse = await httpClient.GetAsync<User>("/users/1");
if (userResponse.IsSuccess)
{
    Console.WriteLine($"用户: {userResponse.Data.Name}");
}
```

### 多客户端配置

```csharp
// 配置多个命名客户端
services.AddHttpClientService("api1", options =>
{
    options.BaseUrl = "https://api1.example.com";
});

services.AddHttpClientService("api2", options =>
{
    options.BaseUrl = "https://api2.example.com";
});

// 使用工厂创建客户端
var factory = serviceProvider.GetRequiredService<IHttpClientServiceFactory>();
var client1 = factory.CreateClient("api1");
var client2 = factory.CreateClient("api2");
```

### 文件上传

```csharp
var uploadResponse = await httpClient.UploadFileAsync<UploadResult>(
    "/upload",
    "path/to/file.txt",
    "file",
    new Dictionary<string, string> { { "description", "测试文件" } }
);
```

### 文件下载

```csharp
var downloadResponse = await httpClient.DownloadFileAsync(
    "/files/document.pdf",
    "local/path/document.pdf"
);
```

### 自定义请求头

```csharp
var headers = new Dictionary<string, string>
{
    { "Authorization", "Bearer your-token" },
    { "X-Custom-Header", "custom-value" }
};

var response = await httpClient.GetAsync<Data>("/protected-resource", headers);
```

### 错误处理

```csharp
var response = await httpClient.GetAsync<User>("/users/999");
if (!response.IsSuccess)
{
    Console.WriteLine($"请求失败: {response.StatusCode}");
    Console.WriteLine($"错误信息: {response.Message}");
    
    if (response.Exception != null)
    {
        Console.WriteLine($"异常详情: {response.Exception.Message}");
    }
}
```

## 高级功能

### SSL证书忽略

```csharp
services.AddHttpClientService(options =>
{
    options.IgnoreSslErrors = true; // 仅用于开发环境
});
```

### 重试配置

```csharp
services.AddHttpClientService(options =>
{
    options.MaxRetryCount = 5;
    options.RetryDelayMilliseconds = 2000;
});
```

## 多BaseUrl支持

当你需要在一个应用中调用多个不同的API服务时，可以使用多BaseUrl功能。

### 多BaseUrl配置

```csharp
services.AddMultiBaseUrlHttpClientService(options =>
{
    // 配置多个服务的BaseUrl
    options.BaseUrls["user-service"] = "https://api.users.com";
    options.BaseUrls["order-service"] = "https://api.orders.com";
    options.BaseUrls["payment-service"] = "https://api.payments.com";
    
    // 设置默认服务
    options.DefaultService = "user-service";
    
    // 其他配置
    options.TimeoutSeconds = 30;
    options.MaxRetryCount = 3;
});
```

### 多BaseUrl使用

```csharp
var httpClient = serviceProvider.GetRequiredService<IMultiBaseUrlHttpClientService>();

// 指定服务名称进行请求
var userResponse = await httpClient.GetAsync<User>("user-service", "/users/1");
var orderResponse = await httpClient.GetAsync<Order>("order-service", "/orders/123");
var paymentResponse = await httpClient.PostAsync("payment-service", "/payments", paymentData);

// 使用默认服务（不指定服务名）
var defaultResponse = await httpClient.GetAsync<User>("/users/2");
```

### 多BaseUrl命名客户端

```csharp
// 配置不同的命名客户端
services.AddMultiBaseUrlHttpClientService("client1", options =>
{
    options.BaseUrls["api1"] = "https://api1.example.com";
    options.BaseUrls["api2"] = "https://api2.example.com";
});

services.AddMultiBaseUrlHttpClientService("client2", options =>
{
    options.BaseUrls["github"] = "https://api.github.com";
    options.BaseUrls["gitlab"] = "https://gitlab.com/api/v4";
});

// 使用工厂创建不同的客户端
var factory = serviceProvider.GetRequiredService<IMultiBaseUrlHttpClientServiceFactory>();
var client1 = factory.CreateClient("client1");
var client2 = factory.CreateClient("client2");

await client1.GetAsync<Data>("api1", "/data");
await client2.GetAsync<Repo>("github", "/repos/microsoft/dotnet");
```

### 动态服务管理

```csharp
var httpClient = serviceProvider.GetRequiredService<IMultiBaseUrlHttpClientService>();

// 检查服务是否存在
if (httpClient.HasService("user-service"))
{
    var response = await httpClient.GetAsync<User>("user-service", "/users/1");
}

// 获取所有服务名称
foreach (var serviceName in httpClient.GetServiceNames())
{
    var baseUrl = httpClient.GetBaseUrl(serviceName);
    Console.WriteLine($"{serviceName}: {baseUrl}");
}
```

### 微服务架构示例

```csharp
services.AddMultiBaseUrlHttpClientService(options =>
{
    // 配置各个微服务
    options.BaseUrls["user-service"] = "https://users.microservice.com";
    options.BaseUrls["product-service"] = "https://products.microservice.com";
    options.BaseUrls["order-service"] = "https://orders.microservice.com";
    options.BaseUrls["notification-service"] = "https://notifications.microservice.com";
    
    options.DefaultService = "user-service";
    options.TimeoutSeconds = 30;
    options.DefaultHeaders.Add("X-Service-Version", "v1");
});

// 业务流程中调用不同的微服务
var user = await httpClient.GetAsync<User>("user-service", "/users/1");
var products = await httpClient.GetAsync<Product[]>("product-service", "/products");
var order = await httpClient.PostAsync<Order>("order-service", "/orders", orderData);
await httpClient.PostAsync("notification-service", "/notifications", notification);
```

### 环境配置示例

```csharp
// 根据环境配置不同的BaseUrl
var environment = Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "development";

services.AddMultiBaseUrlHttpClientService(options =>
{
    switch (environment.ToLower())
    {
        case "development":
            options.BaseUrls["api"] = "https://dev-api.example.com";
            options.BaseUrls["auth"] = "https://dev-auth.example.com";
            break;
        case "testing":
            options.BaseUrls["api"] = "https://test-api.example.com";
            options.BaseUrls["auth"] = "https://test-auth.example.com";
            break;
        case "production":
            options.BaseUrls["api"] = "https://api.example.com";
            options.BaseUrls["auth"] = "https://auth.example.com";
            break;
    }
    
    options.DefaultService = "api";
    options.DefaultHeaders.Add("X-Environment", environment);
});
```

### 自定义JSON序列化

HTTP客户端使用`System.Text.Json`进行序列化，默认配置：
- 使用camelCase命名策略
- 大小写不敏感
- 紧凑格式输出

## 完整示例

查看`Examples/HttpClientUsageExamples.cs`和`Examples/Program.cs`文件获取完整的使用示例。

## 许可证

MIT License
