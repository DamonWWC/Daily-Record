using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Riley.Core.Http;
using System.Text.Json.Serialization;

namespace Riley.Core.Examples;

/// <summary>
/// 多BaseUrl HTTP客户端使用示例
/// </summary>
public static class MultiBaseUrlUsageExamples
{
    /// <summary>
    /// 基本多BaseUrl使用示例
    /// </summary>
    public static async Task BasicMultiBaseUrlExample()
    {
        Console.WriteLine("=== 基本多BaseUrl使用示例 ===");

        // 1. 配置服务
        var services = new ServiceCollection();
        //services.AddLogging(builder => builder.AddConsole());
        
        services.AddMultiBaseUrlHttpClientService(options =>
        {
            // 配置多个BaseUrl
            options.BaseUrls["jsonplaceholder"] = "https://jsonplaceholder.typicode.com";
            options.BaseUrls["httpbin"] = "https://httpbin.org";
            options.BaseUrls["github"] = "https://api.github.com";
            
            // 设置默认服务
            options.DefaultService = "jsonplaceholder";
            
            // 其他通用配置
            options.TimeoutSeconds = 30;
            options.MaxRetryCount = 3;
            options.DefaultHeaders.Add("Accept", "application/json");
        });

        var serviceProvider = services.BuildServiceProvider();
        var httpClient = serviceProvider.GetRequiredService<IMultiBaseUrlHttpClientService>();

        try
        {
            // 2. 使用不同的服务进行请求
            
            // 使用JSONPlaceholder API
            Console.WriteLine("\n--- 使用JSONPlaceholder API ---");
            var userResponse = await httpClient.GetAsync<User>("jsonplaceholder", "/users/1");
            if (userResponse.IsSuccess)
            {
                Console.WriteLine($"用户信息: {userResponse.Data?.Name} ({userResponse.Data?.Email})");
            }

            // 使用HttpBin API
            Console.WriteLine("\n--- 使用HttpBin API ---");
            var httpbinResponse = await httpClient.GetStringAsync("httpbin", "/json");
            if (httpbinResponse.IsSuccess)
            {
                Console.WriteLine($"HttpBin响应长度: {httpbinResponse.Data?.Length}");
            }

            // 使用GitHub API
            Console.WriteLine("\n--- 使用GitHub API ---");
            var githubHeaders = new Dictionary<string, string>
            {
                { "User-Agent", "Riley.Core.Example" }
            };
            var githubResponse = await httpClient.GetStringAsync("github", "/repos/microsoft/dotnet", githubHeaders);
            if (githubResponse.IsSuccess)
            {
                Console.WriteLine("GitHub API调用成功");
            }

            // 3. 使用默认服务（不指定服务名）
            Console.WriteLine("\n--- 使用默认服务 ---");
            var defaultResponse = await httpClient.GetAsync<User>("/users/2"); // 使用默认服务（jsonplaceholder）
            if (defaultResponse.IsSuccess)
            {
                Console.WriteLine($"默认服务用户: {defaultResponse.Data?.Name}");
            }

            // 4. 显示所有配置的服务
            Console.WriteLine("\n--- 已配置的服务 ---");
            foreach (var serviceName in httpClient.GetServiceNames())
            {
                var baseUrl = httpClient.GetBaseUrl(serviceName);
                Console.WriteLine($"服务: {serviceName} -> {baseUrl}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"示例执行出错: {ex.Message}");
        }
        finally
        {
            serviceProvider.Dispose();
        }
    }

    /// <summary>
    /// 命名客户端多BaseUrl示例
    /// </summary>
    public static async Task NamedClientMultiBaseUrlExample()
    {
        Console.WriteLine("\n=== 命名客户端多BaseUrl示例 ===");

        var services = new ServiceCollection();
        //services.AddLogging(builder => builder.AddConsole());

        // 配置不同的命名客户端，每个都有自己的多BaseUrl配置
        services.AddMultiBaseUrlHttpClientService("client1", options =>
        {
            options.BaseUrls["api1"] = "https://jsonplaceholder.typicode.com";
            options.BaseUrls["api2"] = "https://httpbin.org";
            options.DefaultService = "api1";
            options.TimeoutSeconds = 30;
            options.UserAgent = "Riley.Core.Client1/1.0";
        });

        services.AddMultiBaseUrlHttpClientService("client2", options =>
        {
            options.BaseUrls["github"] = "https://api.github.com";
            options.BaseUrls["gitlab"] = "https://gitlab.com/api/v4";
            options.DefaultService = "github";
            options.TimeoutSeconds = 60;
            options.UserAgent = "Riley.Core.Client2/1.0";
            options.DefaultHeaders.Add("Accept", "application/vnd.github.v3+json");
        });

        var serviceProvider = services.BuildServiceProvider();
        var clientFactory = serviceProvider.GetRequiredService<IMultiBaseUrlHttpClientServiceFactory>();

        try
        {
            // 使用不同的命名客户端
            var client1 = clientFactory.CreateClient("client1");
            var client2 = clientFactory.CreateClient("client2");

            Console.WriteLine("\n--- Client1 操作 ---");
            var client1Response = await client1.GetAsync<User>("api1", "/users/1");
            if (client1Response.IsSuccess)
            {
                Console.WriteLine($"Client1获取用户: {client1Response.Data?.Name}");
            }

            Console.WriteLine("\n--- Client2 操作 ---");
            var client2Response = await client2.GetStringAsync("github", "/repos/microsoft/dotnet");
            if (client2Response.IsSuccess)
            {
                Console.WriteLine("Client2 GitHub API调用成功");
            }

            // 显示每个客户端的服务配置
            Console.WriteLine("\n--- Client1 服务配置 ---");
            foreach (var service in client1.GetServiceNames())
            {
                Console.WriteLine($"{service}: {client1.GetBaseUrl(service)}");
            }

            Console.WriteLine("\n--- Client2 服务配置 ---");
            foreach (var service in client2.GetServiceNames())
            {
                Console.WriteLine($"{service}: {client2.GetBaseUrl(service)}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"命名客户端示例出错: {ex.Message}");
        }
        finally
        {
            serviceProvider.Dispose();
        }
    }

    /// <summary>
    /// 动态服务配置示例
    /// </summary>
    public static async Task DynamicServiceConfigurationExample()
    {
        Console.WriteLine("\n=== 动态服务配置示例 ===");

        var services = new ServiceCollection();
        //services.AddLogging(builder => builder.AddConsole());
        
        services.AddMultiBaseUrlHttpClientService(options =>
        {
            options.DefaultService = "primary";
            options.TimeoutSeconds = 30;
        });

        var serviceProvider = services.BuildServiceProvider();
        var httpClient = serviceProvider.GetRequiredService<IMultiBaseUrlHttpClientService>();

        try
        {
            // 模拟运行时动态配置服务
            var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<MultiBaseUrlOptions>>().Value;
            
            // 动态添加服务配置
            options.SetBaseUrl("primary", "https://jsonplaceholder.typicode.com");
            options.SetBaseUrl("backup", "https://httpbin.org");
            options.SetBaseUrl("test", "https://api.github.com");

            Console.WriteLine("动态配置的服务:");
            foreach (var service in options.GetServiceNames())
            {
                Console.WriteLine($"- {service}: {options.GetBaseUrl(service)}");
            }

            // 测试动态配置的服务
            Console.WriteLine("\n--- 测试动态配置的服务 ---");
            
            // 使用主服务
            var primaryResponse = await httpClient.GetAsync<User>("primary", "/users/1");
            if (primaryResponse.IsSuccess)
            {
                Console.WriteLine($"主服务响应: {primaryResponse.Data?.Name}");
            }

            // 使用备用服务
            var backupResponse = await httpClient.GetStringAsync("backup", "/json");
            if (backupResponse.IsSuccess)
            {
                Console.WriteLine("备用服务响应成功");
            }

            // 检查服务是否存在
            Console.WriteLine($"\n服务检查:");
            Console.WriteLine($"primary存在: {httpClient.HasService("primary")}");
            Console.WriteLine($"nonexistent存在: {httpClient.HasService("nonexistent")}");

            // 移除服务
            options.RemoveBaseUrl("test");
            Console.WriteLine($"\n移除test服务后:");
            Console.WriteLine($"test存在: {httpClient.HasService("test")}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"动态配置示例出错: {ex.Message}");
        }
        finally
        {
            serviceProvider.Dispose();
        }
    }

    /// <summary>
    /// 微服务架构示例
    /// </summary>
    public static async Task MicroserviceArchitectureExample()
    {
        Console.WriteLine("\n=== 微服务架构示例 ===");

        var services = new ServiceCollection();
        //services.AddLogging(builder => builder.AddConsole());
        
        // 配置微服务客户端
        services.AddMultiBaseUrlHttpClientService(options =>
        {
            // 配置各个微服务的BaseUrl
            options.BaseUrls["user-service"] = "https://jsonplaceholder.typicode.com";
            options.BaseUrls["post-service"] = "https://jsonplaceholder.typicode.com";
            options.BaseUrls["notification-service"] = "https://httpbin.org";
            options.BaseUrls["file-service"] = "https://httpbin.org";
            
            options.DefaultService = "user-service";
            options.TimeoutSeconds = 30;
            options.MaxRetryCount = 3;
            options.DefaultHeaders.Add("X-Client-Version", "1.0");
        });

        var serviceProvider = services.BuildServiceProvider();
        var httpClient = serviceProvider.GetRequiredService<IMultiBaseUrlHttpClientService>();

        try
        {
            Console.WriteLine("模拟微服务调用场景:");

            // 1. 用户服务 - 获取用户信息
            Console.WriteLine("\n--- 调用用户服务 ---");
            var userResponse = await httpClient.GetAsync<User>("user-service", "/users/1");
            if (userResponse.IsSuccess)
            {
                Console.WriteLine($"用户服务返回: {userResponse.Data?.Name}");
            }

            // 2. 文章服务 - 获取用户的文章
            Console.WriteLine("\n--- 调用文章服务 ---");
            var postsResponse = await httpClient.GetAsync<List<Post>>("post-service", "/posts?userId=1");
            if (postsResponse.IsSuccess && postsResponse.Data != null)
            {
                Console.WriteLine($"文章服务返回: {postsResponse.Data.Count} 篇文章");
                if (postsResponse.Data.Count > 0)
                {
                    Console.WriteLine($"第一篇文章: {postsResponse.Data[0].Title}");
                }
            }

            // 3. 通知服务 - 发送通知
            Console.WriteLine("\n--- 调用通知服务 ---");
            var notification = new
            {
                UserId = 1,
                Message = "您有新的消息",
                Type = "info"
            };
            var notificationResponse = await httpClient.PostAsync("notification-service", "/post", notification);
            if (notificationResponse.IsSuccess)
            {
                Console.WriteLine("通知发送成功");
            }

            // 4. 文件服务 - 获取服务状态
            Console.WriteLine("\n--- 调用文件服务 ---");
            var fileServiceResponse = await httpClient.GetStringAsync("file-service", "/status/200");
            if (fileServiceResponse.IsSuccess)
            {
                Console.WriteLine("文件服务状态正常");
            }

            // 5. 显示所有微服务配置
            Console.WriteLine("\n--- 微服务配置一览 ---");
            foreach (var service in httpClient.GetServiceNames())
            {
                var baseUrl = httpClient.GetBaseUrl(service);
                var status = "未知";
                
                try
                {
                    // 简单的健康检查
                    var healthResponse = await httpClient.GetStringAsync(service, "/");
                    status = healthResponse.IsSuccess ? "正常" : "异常";
                }
                catch
                {
                    status = "无法访问";
                }

                Console.WriteLine($"{service}: {baseUrl} [{status}]");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"微服务示例出错: {ex.Message}");
        }
        finally
        {
            serviceProvider.Dispose();
        }
    }

    /// <summary>
    /// 环境切换示例（开发、测试、生产）
    /// </summary>
    public static async Task EnvironmentSwitchingExample()
    {
        Console.WriteLine("\n=== 环境切换示例 ===");

        // 模拟不同环境的配置
        var environments = new Dictionary<string, Dictionary<string, string>>
        {
            ["development"] = new()
            {
                ["user-api"] = "https://dev-api.example.com",
                ["payment-api"] = "https://dev-payment.example.com"
            },
            ["testing"] = new()
            {
                ["user-api"] = "https://test-api.example.com", 
                ["payment-api"] = "https://test-payment.example.com"
            },
            ["production"] = new()
            {
                ["user-api"] = "https://jsonplaceholder.typicode.com", // 用真实API演示
                ["payment-api"] = "https://httpbin.org" // 用真实API演示
            }
        };

        foreach (var env in environments)
        {
            Console.WriteLine($"\n--- {env.Key.ToUpper()} 环境 ---");

            var services = new ServiceCollection();
            //services.AddLogging(builder => builder.AddConsole());
            
            services.AddMultiBaseUrlHttpClientService(options =>
            {
                // 根据环境配置不同的BaseUrl
                foreach (var service in env.Value)
                {
                    options.BaseUrls[service.Key] = service.Value;
                }
                
                options.DefaultService = "user-api";
                options.TimeoutSeconds = env.Key == "production" ? 60 : 30;
                options.MaxRetryCount = env.Key == "production" ? 5 : 2;
                options.DefaultHeaders.Add("X-Environment", env.Key);
            });

            using var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.GetRequiredService<IMultiBaseUrlHttpClientService>();

            try
            {
                // 测试用户API
                if (env.Key == "production") // 只在生产环境模拟中实际调用
                {
                    var userResponse = await httpClient.GetAsync<User>("user-api", "/users/1");
                    if (userResponse.IsSuccess)
                    {
                        Console.WriteLine($"用户API响应: {userResponse.Data?.Name}");
                    }
                    else
                    {
                        Console.WriteLine($"用户API调用失败: {userResponse.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"模拟调用用户API: {httpClient.GetBaseUrl("user-api")}");
                }

                // 显示环境配置
                Console.WriteLine("服务配置:");
                foreach (var service in httpClient.GetServiceNames())
                {
                    Console.WriteLine($"  {service}: {httpClient.GetBaseUrl(service)}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{env.Key} 环境测试出错: {ex.Message}");
            }
        }
    }
}

//// 复用之前的数据模型
//public class User
//{
//    [JsonPropertyName("id")]
//    public int Id { get; set; }

//    [JsonPropertyName("name")]
//    public string Name { get; set; } = string.Empty;

//    [JsonPropertyName("username")]
//    public string Username { get; set; } = string.Empty;

//    [JsonPropertyName("email")]
//    public string Email { get; set; } = string.Empty;
//}

//public class Post
//{
//    [JsonPropertyName("id")]
//    public int Id { get; set; }

//    [JsonPropertyName("userId")]
//    public int UserId { get; set; }

//    [JsonPropertyName("title")]
//    public string Title { get; set; } = string.Empty;

//    [JsonPropertyName("body")]
//    public string Body { get; set; } = string.Empty;
//}
