using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net;

namespace Riley.Core.Http;

/// <summary>
/// HTTP客户端服务扩展方法
/// </summary>
public static class HttpClientServiceExtensions
{
    /// <summary>
    /// 添加HTTP客户端服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configureOptions">配置选项</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddHttpClientService(this IServiceCollection services, Action<HttpClientOptions>? configureOptions = null)
    {
        // 配置选项
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.Configure<HttpClientOptions>(options => { });
        }

        // 注册HttpClient
        services.AddHttpClient<IHttpClientService, HttpClientService>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<HttpClientOptions>>().Value;
            
            // HttpClient的配置会在HttpClientService构造函数中处理
        })
        .ConfigurePrimaryHttpMessageHandler((serviceProvider) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<HttpClientOptions>>().Value;
            
            var handler = new HttpClientHandler();
            
            // 配置SSL证书验证
            if (options.IgnoreSslErrors)
            {
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            }
            
            return handler;
        });

        return services;
    }

    /// <summary>
    /// 添加HTTP客户端服务（带命名客户端）
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="name">客户端名称</param>
    /// <param name="configureOptions">配置选项</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddHttpClientService(this IServiceCollection services, string name, Action<HttpClientOptions>? configureOptions = null)
    {
        // 配置命名选项
        if (configureOptions != null)
        {
            services.Configure<HttpClientOptions>(name, configureOptions);
        }

        // 注册命名HttpClient
        services.AddHttpClient(name, (serviceProvider, client) =>
        {
            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<HttpClientOptions>>();
            var options = optionsMonitor.Get(name);
            
            // 基本配置
            if (!string.IsNullOrEmpty(options.BaseUrl))
            {
                client.BaseAddress = new Uri(options.BaseUrl);
            }
            
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            
            foreach (var header in options.DefaultHeaders)
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
            }
            
            client.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
        })
        .ConfigurePrimaryHttpMessageHandler((serviceProvider) =>
        {
            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<HttpClientOptions>>();
            var options = optionsMonitor.Get(name);
            
            var handler = new HttpClientHandler();
            
            if (options.IgnoreSslErrors)
            {
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            }
            
            return handler;
        });

        // 注册工厂服务
        services.AddTransient<IHttpClientServiceFactory, HttpClientServiceFactory>();

        return services;
    }

    /// <summary>
    /// 添加多BaseUrl HTTP客户端服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configureOptions">配置选项</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddMultiBaseUrlHttpClientService(this IServiceCollection services, Action<MultiBaseUrlOptions>? configureOptions = null)
    {
        // 配置选项
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.Configure<MultiBaseUrlOptions>(options => { });
        }

        // 注册HttpClient
        services.AddHttpClient<IMultiBaseUrlHttpClientService, MultiBaseUrlHttpClientService>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<MultiBaseUrlOptions>>().Value;
            
            // 不设置BaseAddress，因为会根据服务名称动态选择
            // HttpClient的其他配置会在MultiBaseUrlHttpClientService构造函数中处理
        })
        .ConfigurePrimaryHttpMessageHandler((serviceProvider) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<MultiBaseUrlOptions>>().Value;
            
            var handler = new HttpClientHandler();
            
            // 配置SSL证书验证
            if (options.IgnoreSslErrors)
            {
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            }
            
            return handler;
        });

        return services;
    }

    /// <summary>
    /// 添加多BaseUrl HTTP客户端服务（带命名客户端）
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="name">客户端名称</param>
    /// <param name="configureOptions">配置选项</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddMultiBaseUrlHttpClientService(this IServiceCollection services, string name, Action<MultiBaseUrlOptions>? configureOptions = null)
    {
        // 配置命名选项
        if (configureOptions != null)
        {
            services.Configure<MultiBaseUrlOptions>(name, configureOptions);
        }

        // 注册命名HttpClient
        services.AddHttpClient(name, (serviceProvider, client) =>
        {
            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<MultiBaseUrlOptions>>();
            var options = optionsMonitor.Get(name);
            
            // 不设置BaseAddress，因为会根据服务名称动态选择
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            
            foreach (var header in options.DefaultHeaders)
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
            }
            
            client.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
        })
        .ConfigurePrimaryHttpMessageHandler((serviceProvider) =>
        {
            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<MultiBaseUrlOptions>>();
            var options = optionsMonitor.Get(name);
            
            var handler = new HttpClientHandler();
            
            if (options.IgnoreSslErrors)
            {
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            }
            
            return handler;
        });

        // 注册多BaseUrl工厂服务
        services.AddTransient<IMultiBaseUrlHttpClientServiceFactory, MultiBaseUrlHttpClientServiceFactory>();

        return services;
    }
}

/// <summary>
/// 多BaseUrl HTTP客户端服务工厂接口
/// </summary>
public interface IMultiBaseUrlHttpClientServiceFactory
{
    /// <summary>
    /// 创建多BaseUrl HTTP客户端服务
    /// </summary>
    /// <param name="name">客户端名称</param>
    /// <returns>多BaseUrl HTTP客户端服务</returns>
    IMultiBaseUrlHttpClientService CreateClient(string name);
}

/// <summary>
/// 多BaseUrl HTTP客户端服务工厂实现
/// </summary>
public class MultiBaseUrlHttpClientServiceFactory : IMultiBaseUrlHttpClientServiceFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptionsMonitor<MultiBaseUrlOptions> _optionsMonitor;
    private readonly IServiceProvider _serviceProvider;

    public MultiBaseUrlHttpClientServiceFactory(
        IHttpClientFactory httpClientFactory,
        IOptionsMonitor<MultiBaseUrlOptions> optionsMonitor,
        IServiceProvider serviceProvider)
    {
        _httpClientFactory = httpClientFactory;
        _optionsMonitor = optionsMonitor;
        _serviceProvider = serviceProvider;
    }

    public IMultiBaseUrlHttpClientService CreateClient(string name)
    {
        var httpClient = _httpClientFactory.CreateClient(name);
        var options = Microsoft.Extensions.Options.Options.Create(_optionsMonitor.Get(name));
        var logger = _serviceProvider.GetService<Microsoft.Extensions.Logging.ILogger<MultiBaseUrlHttpClientService>>();
        
        return new MultiBaseUrlHttpClientService(httpClient, options, logger);
    }
}

/// <summary>
/// HTTP客户端服务工厂接口
/// </summary>
public interface IHttpClientServiceFactory
{
    /// <summary>
    /// 创建HTTP客户端服务
    /// </summary>
    /// <param name="name">客户端名称</param>
    /// <returns>HTTP客户端服务</returns>
    IHttpClientService CreateClient(string name);
}

/// <summary>
/// HTTP客户端服务工厂实现
/// </summary>
public class HttpClientServiceFactory : IHttpClientServiceFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptionsMonitor<HttpClientOptions> _optionsMonitor;
    private readonly IServiceProvider _serviceProvider;

    public HttpClientServiceFactory(
        IHttpClientFactory httpClientFactory,
        IOptionsMonitor<HttpClientOptions> optionsMonitor,
        IServiceProvider serviceProvider)
    {
        _httpClientFactory = httpClientFactory;
        _optionsMonitor = optionsMonitor;
        _serviceProvider = serviceProvider;
    }

    public IHttpClientService CreateClient(string name)
    {
        var httpClient = _httpClientFactory.CreateClient(name);
        var options = Microsoft.Extensions.Options.Options.Create(_optionsMonitor.Get(name));
        var logger = _serviceProvider.GetService<Microsoft.Extensions.Logging.ILogger<HttpClientService>>();
        
        return new HttpClientService(httpClient, options, logger);
    }
}
