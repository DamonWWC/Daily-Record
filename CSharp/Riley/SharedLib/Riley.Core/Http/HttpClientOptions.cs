namespace Riley.Core.Http;

/// <summary>
/// HTTP客户端配置选项
/// </summary>
public class HttpClientOptions
{
    /// <summary>
    /// 基础URL
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// 请求超时时间（秒）
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// 默认请求头
    /// </summary>
    public Dictionary<string, string> DefaultHeaders { get; set; } = new();

    /// <summary>
    /// 是否忽略SSL证书错误
    /// </summary>
    public bool IgnoreSslErrors { get; set; } = false;

    /// <summary>
    /// 最大重试次数
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// 重试延迟（毫秒）
    /// </summary>
    public int RetryDelayMilliseconds { get; set; } = 1000;

    /// <summary>
    /// 用户代理
    /// </summary>
    public string UserAgent { get; set; } = "Riley.Core.HttpClient/1.0";
}
