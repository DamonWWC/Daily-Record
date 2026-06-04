namespace Riley.Core.Http;

/// <summary>
/// 多BaseUrl配置选项
/// </summary>
public class MultiBaseUrlOptions : HttpClientOptions
{
    /// <summary>
    /// BaseUrl映射表 (key: 服务名称, value: BaseUrl)
    /// </summary>
    public Dictionary<string, string> BaseUrls { get; set; } = new();

    /// <summary>
    /// 默认服务名称（当未指定服务时使用）
    /// </summary>
    public string DefaultService { get; set; } = "default";

    /// <summary>
    /// 获取指定服务的BaseUrl
    /// </summary>
    /// <param name="serviceName">服务名称</param>
    /// <returns>BaseUrl</returns>
    public string GetBaseUrl(string? serviceName = null)
    {
        serviceName ??= DefaultService;
        
        if (BaseUrls.TryGetValue(serviceName, out var baseUrl))
        {
            return baseUrl;
        }

        // 如果找不到指定服务的BaseUrl，返回默认的BaseUrl
        return BaseUrl;
    }

    /// <summary>
    /// 设置服务的BaseUrl
    /// </summary>
    /// <param name="serviceName">服务名称</param>
    /// <param name="baseUrl">BaseUrl</param>
    public void SetBaseUrl(string serviceName, string baseUrl)
    {
        BaseUrls[serviceName] = baseUrl;
    }

    /// <summary>
    /// 移除服务的BaseUrl配置
    /// </summary>
    /// <param name="serviceName">服务名称</param>
    /// <returns>是否移除成功</returns>
    public bool RemoveBaseUrl(string serviceName)
    {
        return BaseUrls.Remove(serviceName);
    }

    /// <summary>
    /// 获取所有已配置的服务名称
    /// </summary>
    /// <returns>服务名称列表</returns>
    public IEnumerable<string> GetServiceNames()
    {
        return BaseUrls.Keys;
    }

    /// <summary>
    /// 检查是否配置了指定服务
    /// </summary>
    /// <param name="serviceName">服务名称</param>
    /// <returns>是否已配置</returns>
    public bool HasService(string serviceName)
    {
        return BaseUrls.ContainsKey(serviceName);
    }
}
