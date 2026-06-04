namespace Riley.Core.Http;

/// <summary>
/// 支持多BaseUrl的HTTP客户端服务接口
/// </summary>
public interface IMultiBaseUrlHttpClientService : IHttpClientService
{
    /// <summary>
    /// 发送GET请求（指定服务）
    /// </summary>
    /// <typeparam name="T">响应数据类型</typeparam>
    /// <param name="serviceName">服务名称</param>
    /// <param name="url">请求URL</param>
    /// <param name="headers">请求头</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>API响应</returns>
    Task<ApiResponse<T>> GetAsync<T>(string serviceName, string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送GET请求（返回字符串，指定服务）
    /// </summary>
    /// <param name="serviceName">服务名称</param>
    /// <param name="url">请求URL</param>
    /// <param name="headers">请求头</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>API响应</returns>
    Task<ApiResponse<string>> GetStringAsync(string serviceName, string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送POST请求（指定服务）
    /// </summary>
    /// <typeparam name="T">响应数据类型</typeparam>
    /// <param name="serviceName">服务名称</param>
    /// <param name="url">请求URL</param>
    /// <param name="data">请求数据</param>
    /// <param name="headers">请求头</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>API响应</returns>
    Task<ApiResponse<T>> PostAsync<T>(string serviceName, string url, object? data = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送POST请求（无返回数据，指定服务）
    /// </summary>
    /// <param name="serviceName">服务名称</param>
    /// <param name="url">请求URL</param>
    /// <param name="data">请求数据</param>
    /// <param name="headers">请求头</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>API响应</returns>
    Task<ApiResponse> PostAsync(string serviceName, string url, object? data = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送PUT请求（指定服务）
    /// </summary>
    /// <typeparam name="T">响应数据类型</typeparam>
    /// <param name="serviceName">服务名称</param>
    /// <param name="url">请求URL</param>
    /// <param name="data">请求数据</param>
    /// <param name="headers">请求头</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>API响应</returns>
    Task<ApiResponse<T>> PutAsync<T>(string serviceName, string url, object? data = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送PUT请求（无返回数据，指定服务）
    /// </summary>
    /// <param name="serviceName">服务名称</param>
    /// <param name="url">请求URL</param>
    /// <param name="data">请求数据</param>
    /// <param name="headers">请求头</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>API响应</returns>
    Task<ApiResponse> PutAsync(string serviceName, string url, object? data = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送DELETE请求（指定服务）
    /// </summary>
    /// <typeparam name="T">响应数据类型</typeparam>
    /// <param name="serviceName">服务名称</param>
    /// <param name="url">请求URL</param>
    /// <param name="headers">请求头</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>API响应</returns>
    Task<ApiResponse<T>> DeleteAsync<T>(string serviceName, string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送DELETE请求（无返回数据，指定服务）
    /// </summary>
    /// <param name="serviceName">服务名称</param>
    /// <param name="url">请求URL</param>
    /// <param name="headers">请求头</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>API响应</returns>
    Task<ApiResponse> DeleteAsync(string serviceName, string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送PATCH请求（指定服务）
    /// </summary>
    /// <typeparam name="T">响应数据类型</typeparam>
    /// <param name="serviceName">服务名称</param>
    /// <param name="url">请求URL</param>
    /// <param name="data">请求数据</param>
    /// <param name="headers">请求头</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>API响应</returns>
    Task<ApiResponse<T>> PatchAsync<T>(string serviceName, string url, object? data = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送PATCH请求（无返回数据，指定服务）
    /// </summary>
    /// <param name="serviceName">服务名称</param>
    /// <param name="url">请求URL</param>
    /// <param name="data">请求数据</param>
    /// <param name="headers">请求头</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>API响应</returns>
    Task<ApiResponse> PatchAsync(string serviceName, string url, object? data = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 上传文件（指定服务）
    /// </summary>
    /// <typeparam name="T">响应数据类型</typeparam>
    /// <param name="serviceName">服务名称</param>
    /// <param name="url">请求URL</param>
    /// <param name="filePath">文件路径</param>
    /// <param name="fieldName">文件字段名</param>
    /// <param name="additionalData">附加数据</param>
    /// <param name="headers">请求头</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>API响应</returns>
    Task<ApiResponse<T>> UploadFileAsync<T>(string serviceName, string url, string filePath, string fieldName = "file", Dictionary<string, string>? additionalData = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 下载文件（指定服务）
    /// </summary>
    /// <param name="serviceName">服务名称</param>
    /// <param name="url">请求URL</param>
    /// <param name="filePath">保存路径</param>
    /// <param name="headers">请求头</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>API响应</returns>
    Task<ApiResponse> DownloadFileAsync(string serviceName, string url, string filePath, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取所有已配置的服务名称
    /// </summary>
    /// <returns>服务名称列表</returns>
    IEnumerable<string> GetServiceNames();

    /// <summary>
    /// 检查是否配置了指定服务
    /// </summary>
    /// <param name="serviceName">服务名称</param>
    /// <returns>是否已配置</returns>
    bool HasService(string serviceName);

    /// <summary>
    /// 获取指定服务的BaseUrl
    /// </summary>
    /// <param name="serviceName">服务名称</param>
    /// <returns>BaseUrl</returns>
    string GetBaseUrl(string? serviceName = null);
}
