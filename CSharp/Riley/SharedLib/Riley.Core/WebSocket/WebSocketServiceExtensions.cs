using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Riley.Core.WebSocket;

/// <summary>
/// WebSocket服务扩展方法
/// </summary>
public static class WebSocketServiceExtensions
{
    /// <summary>
    /// 添加WebSocket客户端服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configureOptions">配置选项</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddWebSocketClient(this IServiceCollection services, Action<WebSocketOptions>? configureOptions = null)
    {
        // 配置选项
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.Configure<WebSocketOptions>(options => { });
        }

        // 注册WebSocket客户端
        services.AddTransient<IWebSocketClient, WebSocketClient>();

        return services;
    }

    /// <summary>
    /// 添加WebSocket客户端服务（单例模式）
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configureOptions">配置选项</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddWebSocketClientSingleton(this IServiceCollection services, Action<WebSocketOptions>? configureOptions = null)
    {
        // 配置选项
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.Configure<WebSocketOptions>(options => { });
        }

        // 注册WebSocket客户端为单例
        services.AddSingleton<IWebSocketClient, WebSocketClient>();

        return services;
    }

    /// <summary>
    /// 添加命名WebSocket客户端服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="name">客户端名称</param>
    /// <param name="configureOptions">配置选项</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddWebSocketClient(this IServiceCollection services, string name, Action<WebSocketOptions>? configureOptions = null)
    {
        // 配置命名选项
        if (configureOptions != null)
        {
            services.Configure<WebSocketOptions>(name, configureOptions);
        }

        // 注册工厂服务
        services.AddTransient<IWebSocketClientFactory, WebSocketClientFactory>();

        return services;
    }

    /// <summary>
    /// 添加多个WebSocket客户端配置
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configurations">客户端配置字典</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddWebSocketClients(this IServiceCollection services, Dictionary<string, Action<WebSocketOptions>> configurations)
    {
        foreach (var config in configurations)
        {
            services.AddWebSocketClient(config.Key, config.Value);
        }

        return services;
    }
}

/// <summary>
/// WebSocket客户端工厂接口
/// </summary>
public interface IWebSocketClientFactory
{
    /// <summary>
    /// 创建WebSocket客户端
    /// </summary>
    /// <param name="name">客户端名称</param>
    /// <returns>WebSocket客户端</returns>
    IWebSocketClient CreateClient(string name);
}

/// <summary>
/// WebSocket客户端工厂实现
/// </summary>
public class WebSocketClientFactory : IWebSocketClientFactory
{
    private readonly IOptionsMonitor<WebSocketOptions> _optionsMonitor;
    private readonly IServiceProvider _serviceProvider;

    public WebSocketClientFactory(
        IOptionsMonitor<WebSocketOptions> optionsMonitor,
        IServiceProvider serviceProvider)
    {
        _optionsMonitor = optionsMonitor;
        _serviceProvider = serviceProvider;
    }

    public IWebSocketClient CreateClient(string name)
    {
        var options = Microsoft.Extensions.Options.Options.Create(_optionsMonitor.Get(name));
        var logger = _serviceProvider.GetService<Microsoft.Extensions.Logging.ILogger<WebSocketClient>>();
        
        return new WebSocketClient(options, logger);
    }
}
