using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Riley.Core.Http;
using System.Text.Json.Serialization;

namespace Riley.Core.Examples;

/// <summary>
/// HTTP客户端使用示例
/// </summary>
public static class HttpClientUsageExamples
{
    /// <summary>
    /// 基本使用示例
    /// </summary>
    public static async Task BasicUsageExample()
    {
        // 1. 配置服务
        var services = new ServiceCollection();
        
        // 添加日志
        //services.AddLogging(builder => builder.AddConsole());
        
        // 添加HTTP客户端服务
        services.AddHttpClientService(options =>
        {
            options.BaseUrl = "https://jsonplaceholder.typicode.com";
            options.TimeoutSeconds = 30;
            options.MaxRetryCount = 3;
            options.RetryDelayMilliseconds = 1000;
            options.DefaultHeaders.Add("Accept", "application/json");
            options.UserAgent = "Riley.Core.Example/1.0";
        });

        var serviceProvider = services.BuildServiceProvider();
        var httpClient = serviceProvider.GetRequiredService<IHttpClientService>();

        try
        {
            // 2. GET请求示例
            Console.WriteLine("=== GET请求示例 ===");
            var getUserResponse = await httpClient.GetAsync<User>("/users/1");
            if (getUserResponse.IsSuccess)
            {
                Console.WriteLine($"用户信息: {getUserResponse.Data?.Name} ({getUserResponse.Data?.Email})");
            }
            else
            {
                Console.WriteLine($"请求失败: {getUserResponse.Message}");
            }

            // 3. GET请求获取列表
            Console.WriteLine("\n=== GET请求列表示例 ===");
            var getUsersResponse = await httpClient.GetAsync<List<User>>("/users");
            if (getUsersResponse.IsSuccess && getUsersResponse.Data != null)
            {
                Console.WriteLine($"获取到 {getUsersResponse.Data.Count} 个用户");
                foreach (var user in getUsersResponse.Data.Take(3))
                {
                    Console.WriteLine($"- {user.Name} ({user.Email})");
                }
            }

            // 4. POST请求示例
            Console.WriteLine("\n=== POST请求示例 ===");
            var newPost = new Post
            {
                UserId = 1,
                Title = "测试标题",
                Body = "这是一个测试内容"
            };

            var createPostResponse = await httpClient.PostAsync<Post>("/posts", newPost);
            if (createPostResponse.IsSuccess)
            {
                Console.WriteLine($"创建成功，Post ID: {createPostResponse.Data?.Id}");
            }

            // 5. PUT请求示例
            Console.WriteLine("\n=== PUT请求示例 ===");
            var updatePost = new Post
            {
                Id = 1,
                UserId = 1,
                Title = "更新的标题",
                Body = "更新的内容"
            };

            var updateResponse = await httpClient.PutAsync<Post>("/posts/1", updatePost);
            if (updateResponse.IsSuccess)
            {
                Console.WriteLine($"更新成功: {updateResponse.Data?.Title}");
            }

            // 6. DELETE请求示例
            Console.WriteLine("\n=== DELETE请求示例 ===");
            var deleteResponse = await httpClient.DeleteAsync("/posts/1");
            if (deleteResponse.IsSuccess)
            {
                Console.WriteLine("删除成功");
            }

            // 7. 带自定义请求头的示例
            Console.WriteLine("\n=== 自定义请求头示例 ===");
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer your-token-here" },
                { "X-Custom-Header", "custom-value" }
            };

            var authResponse = await httpClient.GetAsync<User>("/users/1", headers);
            if (authResponse.IsSuccess)
            {
                Console.WriteLine("带认证的请求成功");
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
    /// 多客户端配置示例
    /// </summary>
    public static async Task MultipleClientsExample()
    {
        var services = new ServiceCollection();
        //services.AddLogging(builder => builder.AddConsole());

        // 配置多个命名客户端
        services.AddHttpClientService("jsonplaceholder", options =>
        {
            options.BaseUrl = "https://jsonplaceholder.typicode.com";
            options.TimeoutSeconds = 30;
        });

        services.AddHttpClientService("github", options =>
        {
            options.BaseUrl = "https://api.github.com";
            options.TimeoutSeconds = 60;
            options.DefaultHeaders.Add("Accept", "application/vnd.github.v3+json");
            options.UserAgent = "Riley.Core.GitHub.Client/1.0";
        });

        var serviceProvider = services.BuildServiceProvider();
        var clientFactory = serviceProvider.GetRequiredService<IHttpClientServiceFactory>();

        try
        {
            // 使用不同的客户端
            var jsonPlaceholderClient = clientFactory.CreateClient("jsonplaceholder");
            var githubClient = clientFactory.CreateClient("github");

            Console.WriteLine("=== 多客户端示例 ===");
            
            // 使用JSONPlaceholder API
            var userResponse = await jsonPlaceholderClient.GetAsync<User>("/users/1");
            if (userResponse.IsSuccess)
            {
                Console.WriteLine($"JSONPlaceholder用户: {userResponse.Data?.Name}");
            }

            // 使用GitHub API
            var repoResponse = await githubClient.GetStringAsync("/repos/microsoft/dotnet");
            if (repoResponse.IsSuccess)
            {
                Console.WriteLine("GitHub API调用成功");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"多客户端示例出错: {ex.Message}");
        }
        finally
        {
            serviceProvider.Dispose();
        }
    }

    /// <summary>
    /// 文件上传下载示例
    /// </summary>
    public static async Task FileOperationsExample()
    {
        var services = new ServiceCollection();
        //services.AddLogging(builder => builder.AddConsole());
        
        services.AddHttpClientService(options =>
        {
            options.BaseUrl = "https://httpbin.org";
            options.TimeoutSeconds = 120; // 文件操作需要更长时间
        });

        var serviceProvider = services.BuildServiceProvider();
        var httpClient = serviceProvider.GetRequiredService<IHttpClientService>();

        try
        {
            Console.WriteLine("=== 文件操作示例 ===");

            // 创建测试文件
            var testFilePath = Path.Combine(Path.GetTempPath(), "test-upload.txt");
            await File.WriteAllTextAsync(testFilePath, "这是一个测试文件内容");

            // 文件上传示例
            Console.WriteLine("上传文件...");
            var uploadResponse = await httpClient.UploadFileAsync<dynamic>(
                "/post", 
                testFilePath, 
                "file",
                new Dictionary<string, string> { { "description", "测试文件" } }
            );

            if (uploadResponse.IsSuccess)
            {
                Console.WriteLine("文件上传成功");
            }
            else
            {
                Console.WriteLine($"文件上传失败: {uploadResponse.Message}");
            }

            // 文件下载示例（下载一个小的测试文件）
            Console.WriteLine("下载文件...");
            var downloadPath = Path.Combine(Path.GetTempPath(), "downloaded-file.json");
            var downloadResponse = await httpClient.DownloadFileAsync("/json", downloadPath);

            if (downloadResponse.IsSuccess)
            {
                Console.WriteLine($"文件下载成功: {downloadPath}");
                var content = await File.ReadAllTextAsync(downloadPath);
                Console.WriteLine($"文件内容预览: {content[..Math.Min(100, content.Length)]}...");
            }
            else
            {
                Console.WriteLine($"文件下载失败: {downloadResponse.Message}");
            }

            // 清理临时文件
            if (File.Exists(testFilePath)) File.Delete(testFilePath);
            if (File.Exists(downloadPath)) File.Delete(downloadPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"文件操作示例出错: {ex.Message}");
        }
        finally
        {
            serviceProvider.Dispose();
        }
    }

    /// <summary>
    /// 错误处理示例
    /// </summary>
    public static async Task ErrorHandlingExample()
    {
        var services = new ServiceCollection();
        //services.AddLogging(builder => builder.AddConsole());
        
        services.AddHttpClientService(options =>
        {
            options.BaseUrl = "https://jsonplaceholder.typicode.com";
            options.MaxRetryCount = 2;
            options.RetryDelayMilliseconds = 500;
        });

        var serviceProvider = services.BuildServiceProvider();
        var httpClient = serviceProvider.GetRequiredService<IHttpClientService>();

        try
        {
            Console.WriteLine("=== 错误处理示例 ===");

            // 1. 404错误示例
            Console.WriteLine("测试404错误...");
            var notFoundResponse = await httpClient.GetAsync<User>("/users/99999");
            Console.WriteLine($"404请求结果: Success={notFoundResponse.IsSuccess}, Status={notFoundResponse.StatusCode}");

            // 2. 网络错误示例（使用不存在的域名）
            var errorServices = new ServiceCollection();
            //errorServices.AddLogging(builder => builder.AddConsole());
            errorServices.AddHttpClientService(options =>
            {
                options.BaseUrl = "https://non-existent-domain-12345.com";
                options.TimeoutSeconds = 5;
                options.MaxRetryCount = 1;
            });

            using var errorServiceProvider = errorServices.BuildServiceProvider();
            var errorClient = errorServiceProvider.GetRequiredService<IHttpClientService>();

            Console.WriteLine("测试网络错误...");
            var networkErrorResponse = await errorClient.GetAsync<string>("/test");
            Console.WriteLine($"网络错误结果: Success={networkErrorResponse.IsSuccess}");
            if (!networkErrorResponse.IsSuccess)
            {
                Console.WriteLine($"错误信息: {networkErrorResponse.Message}");
            }

            // 3. 反序列化错误示例
            Console.WriteLine("测试反序列化错误...");
            // 尝试将HTML响应反序列化为User对象
            var deserializeErrorResponse = await httpClient.GetAsync<User>("/posts/1"); // 这会返回Post对象，但我们期望User
            Console.WriteLine($"反序列化结果: Success={deserializeErrorResponse.IsSuccess}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"错误处理示例出错: {ex.Message}");
        }
        finally
        {
            serviceProvider.Dispose();
        }
    }
}

// 示例数据模型
public class User
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("phone")]
    public string Phone { get; set; } = string.Empty;

    [JsonPropertyName("website")]
    public string Website { get; set; } = string.Empty;
}

public class Post
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("userId")]
    public int UserId { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;
}
