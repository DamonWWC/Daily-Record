namespace Riley.Core.Http;

/// <summary>
/// HTTP客户端服务接口
/// </summary>
public interface IHttpClientService
{
    /// <summary>
    /// 发送GET请求
    /// </summary>
    /// <typeparam name="T">响应数据类型</typeparam>
    /// <param name="url">请求URL</param>
    /// <param name="headers">请求头</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>API响应</returns>
    Task<ApiResponse<T>> GetAsync<T>(string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送GET请求（返回字符串）
    /// </summary>
    /// <param name="url">请求URL</param>
    /// <param name="headers">请求头</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>API响应</returns>
    Task<ApiResponse<string>> GetStringAsync(string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送POST请求
    /// </summary>
    /// <typeparam name="T">响应数据类型</typeparam>
    /// <param name="url">请求URL</param>
    /// <param name="data">请求数据</param>
    /// <param name="headers">请求头</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>API响应</returns>
    Task<ApiResponse<T>> PostAsync<T>(string url, object? data = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送POST请求（无返回数据）
    /// </summary>
    /// <param name="url">请求URL</param>
    /// <param name="data">请求数据</param>
    /// <param name="headers">请求头</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>API响应</returns>
    Task<ApiResponse> PostAsync(string url, object? data = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送PUT请求
    /// </summary>
    /// <typeparam name="T">响应数据类型</typeparam>
    /// <param name="url">请求URL</param>
    /// <param name="data">请求数据</param>
    /// <param name="headers">请求头</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>API响应</returns>
    Task<ApiResponse<T>> PutAsync<T>(string url, object? data = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送PUT请求（无返回数据）
    /// </summary>
    /// <param name="url">请求URL</param>
    /// <param name="data">请求数据</param>
    /// <param name="headers">请求头</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>API响应</returns>
    Task<ApiResponse> PutAsync(string url, object? data = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送DELETE请求
    /// </summary>
    /// <typeparam name="T">响应数据类型</typeparam>
    /// <param name="url">请求URL</param>
    /// <param name="headers">请求头</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>API响应</returns>
    Task<ApiResponse<T>> DeleteAsync<T>(string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送DELETE请求（无返回数据）
    /// </summary>
    /// <param name="url">请求URL</param>
    /// <param name="headers">请求头</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>API响应</returns>
    Task<ApiResponse> DeleteAsync(string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送PATCH请求
    /// </summary>
    /// <typeparam name="T">响应数据类型</typeparam>
    /// <param name="url">请求URL</param>
    /// <param name="data">请求数据</param>
    /// <param name="headers">请求头</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>API响应</returns>
    Task<ApiResponse<T>> PatchAsync<T>(string url, object? data = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送PATCH请求（无返回数据）
    /// </summary>
    /// <param name="url">请求URL</param>
    /// <param name="data">请求数据</param>
    /// <param name="headers">请求头</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>API响应</returns>
    Task<ApiResponse> PatchAsync(string url, object? data = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <typeparam name="T">响应数据类型</typeparam>
    /// <param name="url">请求URL</param>
    /// <param name="filePath">文件路径</param>
    /// <param name="fieldName">文件字段名</param>
    /// <param name="additionalData">附加数据</param>
    /// <param name="headers">请求头</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>API响应</returns>
    Task<ApiResponse<T>> UploadFileAsync<T>(string url, string filePath, string fieldName = "file", Dictionary<string, string>? additionalData = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 下载文件
    /// </summary>
    /// <param name="url">请求URL</param>
    /// <param name="filePath">保存路径</param>
    /// <param name="headers">请求头</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>API响应</returns>
    Task<ApiResponse> DownloadFileAsync(string url, string filePath, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
}
