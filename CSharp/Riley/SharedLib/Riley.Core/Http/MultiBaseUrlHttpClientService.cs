using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Riley.Core.Http;

/// <summary>
/// 支持多BaseUrl的HTTP客户端服务实现
/// </summary>
public class MultiBaseUrlHttpClientService : IMultiBaseUrlHttpClientService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly MultiBaseUrlOptions _options;
    private readonly ILogger<MultiBaseUrlHttpClientService>? _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private bool _disposed = false;

    public MultiBaseUrlHttpClientService(HttpClient httpClient, IOptions<MultiBaseUrlOptions> options, ILogger<MultiBaseUrlHttpClientService>? logger = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;

        ConfigureHttpClient();
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };
    }

    private void ConfigureHttpClient()
    {
        // 设置超时
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);

        // 设置默认请求头
        foreach (var header in _options.DefaultHeaders)
        {
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
        }

        // 设置用户代理
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(_options.UserAgent);
    }

    #region IHttpClientService 实现（使用默认服务）

    public async Task<ApiResponse<T>> GetAsync<T>(string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        return await GetAsync<T>(_options.DefaultService, url, headers, cancellationToken);
    }

    public async Task<ApiResponse<string>> GetStringAsync(string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        return await GetStringAsync(_options.DefaultService, url, headers, cancellationToken);
    }

    public async Task<ApiResponse<T>> PostAsync<T>(string url, object? data = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        return await PostAsync<T>(_options.DefaultService, url, data, headers, cancellationToken);
    }

    public async Task<ApiResponse> PostAsync(string url, object? data = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        return await PostAsync(_options.DefaultService, url, data, headers, cancellationToken);
    }

    public async Task<ApiResponse<T>> PutAsync<T>(string url, object? data = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        return await PutAsync<T>(_options.DefaultService, url, data, headers, cancellationToken);
    }

    public async Task<ApiResponse> PutAsync(string url, object? data = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        return await PutAsync(_options.DefaultService, url, data, headers, cancellationToken);
    }

    public async Task<ApiResponse<T>> DeleteAsync<T>(string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        return await DeleteAsync<T>(_options.DefaultService, url, headers, cancellationToken);
    }

    public async Task<ApiResponse> DeleteAsync(string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        return await DeleteAsync(_options.DefaultService, url, headers, cancellationToken);
    }

    public async Task<ApiResponse<T>> PatchAsync<T>(string url, object? data = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        return await PatchAsync<T>(_options.DefaultService, url, data, headers, cancellationToken);
    }

    public async Task<ApiResponse> PatchAsync(string url, object? data = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        return await PatchAsync(_options.DefaultService, url, data, headers, cancellationToken);
    }

    public async Task<ApiResponse<T>> UploadFileAsync<T>(string url, string filePath, string fieldName = "file", Dictionary<string, string>? additionalData = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        return await UploadFileAsync<T>(_options.DefaultService, url, filePath, fieldName, additionalData, headers, cancellationToken);
    }

    public async Task<ApiResponse> DownloadFileAsync(string url, string filePath, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        return await DownloadFileAsync(_options.DefaultService, url, filePath, headers, cancellationToken);
    }

    #endregion

    #region IMultiBaseUrlHttpClientService 实现（指定服务）

    public async Task<ApiResponse<T>> GetAsync<T>(string serviceName, string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        return await SendRequestAsync<T>(serviceName, HttpMethod.Get, url, null, headers, cancellationToken);
    }

    public async Task<ApiResponse<string>> GetStringAsync(string serviceName, string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        return await SendRequestAsync<string>(serviceName, HttpMethod.Get, url, null, headers, cancellationToken);
    }

    public async Task<ApiResponse<T>> PostAsync<T>(string serviceName, string url, object? data = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        return await SendRequestAsync<T>(serviceName, HttpMethod.Post, url, data, headers, cancellationToken);
    }

    public async Task<ApiResponse> PostAsync(string serviceName, string url, object? data = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        var response = await SendRequestAsync<object>(serviceName, HttpMethod.Post, url, data, headers, cancellationToken);
        return new ApiResponse
        {
            IsSuccess = response.IsSuccess,
            StatusCode = response.StatusCode,
            Message = response.Message,
            Exception = response.Exception,
            Headers = response.Headers
        };
    }

    public async Task<ApiResponse<T>> PutAsync<T>(string serviceName, string url, object? data = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        return await SendRequestAsync<T>(serviceName, HttpMethod.Put, url, data, headers, cancellationToken);
    }

    public async Task<ApiResponse> PutAsync(string serviceName, string url, object? data = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        var response = await SendRequestAsync<object>(serviceName, HttpMethod.Put, url, data, headers, cancellationToken);
        return new ApiResponse
        {
            IsSuccess = response.IsSuccess,
            StatusCode = response.StatusCode,
            Message = response.Message,
            Exception = response.Exception,
            Headers = response.Headers
        };
    }

    public async Task<ApiResponse<T>> DeleteAsync<T>(string serviceName, string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        return await SendRequestAsync<T>(serviceName, HttpMethod.Delete, url, null, headers, cancellationToken);
    }

    public async Task<ApiResponse> DeleteAsync(string serviceName, string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        var response = await SendRequestAsync<object>(serviceName, HttpMethod.Delete, url, null, headers, cancellationToken);
        return new ApiResponse
        {
            IsSuccess = response.IsSuccess,
            StatusCode = response.StatusCode,
            Message = response.Message,
            Exception = response.Exception,
            Headers = response.Headers
        };
    }

    public async Task<ApiResponse<T>> PatchAsync<T>(string serviceName, string url, object? data = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        return await SendRequestAsync<T>(serviceName, HttpMethod.Patch, url, data, headers, cancellationToken);
    }

    public async Task<ApiResponse> PatchAsync(string serviceName, string url, object? data = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        var response = await SendRequestAsync<object>(serviceName, HttpMethod.Patch, url, data, headers, cancellationToken);
        return new ApiResponse
        {
            IsSuccess = response.IsSuccess,
            StatusCode = response.StatusCode,
            Message = response.Message,
            Exception = response.Exception,
            Headers = response.Headers
        };
    }

    public async Task<ApiResponse<T>> UploadFileAsync<T>(string serviceName, string url, string filePath, string fieldName = "file", Dictionary<string, string>? additionalData = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return ApiResponse<T>.Failure($"File not found: {filePath}", HttpStatusCode.BadRequest);
            }

            var fullUrl = BuildFullUrl(serviceName, url);
            using var content = new MultipartFormDataContent();
            
            // 添加文件
            var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(filePath, cancellationToken));
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            content.Add(fileContent, fieldName, Path.GetFileName(filePath));

            // 添加额外数据
            if (additionalData != null)
            {
                foreach (var item in additionalData)
                {
                    content.Add(new StringContent(item.Value), item.Key);
                }
            }

            using var request = new HttpRequestMessage(HttpMethod.Post, fullUrl) { Content = content };
            
            // 添加请求头
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            return await ExecuteRequestAsync<T>(request, serviceName, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Upload file failed: {Service} {Url}", serviceName, url);
            return ApiResponse<T>.Failure($"Upload file failed: {ex.Message}", HttpStatusCode.InternalServerError, ex);
        }
    }

    public async Task<ApiResponse> DownloadFileAsync(string serviceName, string url, string filePath, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var fullUrl = BuildFullUrl(serviceName, url);
            using var request = new HttpRequestMessage(HttpMethod.Get, fullUrl);
            
            // 添加请求头
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            var response = await ExecuteWithRetryAsync(async () => await _httpClient.SendAsync(request, cancellationToken));

            if (response.IsSuccessStatusCode)
            {
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                await response.Content.CopyToAsync(fileStream, cancellationToken);

                _logger?.LogInformation("File downloaded successfully: {Service} {FilePath}", serviceName, filePath);
                return ApiResponse.Success(response.StatusCode, GetResponseHeaders(response));
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger?.LogWarning("Download file failed with status {StatusCode}: {Service} {Content}", response.StatusCode, serviceName, errorContent);
                return ApiResponse.Failure($"Download failed: {response.StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Download file failed: {Service} {Url}", serviceName, url);
            return ApiResponse.Failure($"Download file failed: {ex.Message}", HttpStatusCode.InternalServerError, ex);
        }
    }

    public IEnumerable<string> GetServiceNames()
    {
        return _options.GetServiceNames();
    }

    public bool HasService(string serviceName)
    {
        return _options.HasService(serviceName);
    }

    public string GetBaseUrl(string? serviceName = null)
    {
        return _options.GetBaseUrl(serviceName);
    }

    #endregion

    #region 私有方法

    private async Task<ApiResponse<T>> SendRequestAsync<T>(string serviceName, HttpMethod method, string url, object? data, Dictionary<string, string>? headers, CancellationToken cancellationToken)
    {
        try
        {
            var fullUrl = BuildFullUrl(serviceName, url);
            using var request = new HttpRequestMessage(method, fullUrl);

            // 设置请求内容
            if (data != null && (method == HttpMethod.Post || method == HttpMethod.Put || method == HttpMethod.Patch))
            {
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            // 添加请求头
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            return await ExecuteRequestAsync<T>(request, serviceName, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "HTTP request failed: {Service} {Method} {Url}", serviceName, method, url);
            return ApiResponse<T>.Failure($"Request failed: {ex.Message}", HttpStatusCode.InternalServerError, ex);
        }
    }

    private string BuildFullUrl(string serviceName, string url)
    {
        var baseUrl = _options.GetBaseUrl(serviceName);
        
        if (string.IsNullOrEmpty(baseUrl))
        {
            throw new InvalidOperationException($"BaseUrl not configured for service: {serviceName}");
        }

        // 确保baseUrl以/结尾，url不以/开头
        baseUrl = baseUrl.TrimEnd('/');
        url = url.TrimStart('/');
        
        return $"{baseUrl}/{url}";
    }

    private async Task<ApiResponse<T>> ExecuteRequestAsync<T>(HttpRequestMessage request, string serviceName, CancellationToken cancellationToken)
    {
        try
        {
            _logger?.LogDebug("Sending HTTP request: {Service} {Method} {Url}", serviceName, request.Method, request.RequestUri);

            var response = await ExecuteWithRetryAsync(async () => await _httpClient.SendAsync(request, cancellationToken));
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseHeaders = GetResponseHeaders(response);

            _logger?.LogDebug("HTTP response received: {Service} {StatusCode}, Content Length: {Length}", serviceName, response.StatusCode, content.Length);

            if (response.IsSuccessStatusCode)
            {
                T? data = default;
                if (!string.IsNullOrEmpty(content))
                {
                    if (typeof(T) == typeof(string))
                    {
                        data = (T)(object)content;
                    }
                    else if (typeof(T) != typeof(object))
                    {
                        try
                        {
                            data = JsonSerializer.Deserialize<T>(content, _jsonOptions);
                        }
                        catch (JsonException ex)
                        {
                            _logger?.LogWarning(ex, "Failed to deserialize response content to {Type} for service {Service}", typeof(T).Name, serviceName);
                            return ApiResponse<T>.Failure($"Failed to deserialize response: {ex.Message}", response.StatusCode, ex);
                        }
                    }
                }

                return ApiResponse<T>.Success(data!, response.StatusCode, responseHeaders);
            }
            else
            {
                _logger?.LogWarning("HTTP request failed with status {StatusCode}: {Service} {Content}", response.StatusCode, serviceName, content);
                return ApiResponse<T>.Failure($"Request failed with status {response.StatusCode}: {content}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "HTTP request execution failed: {Service} {Method} {Url}", serviceName, request.Method, request.RequestUri);
            return ApiResponse<T>.Failure($"Request execution failed: {ex.Message}", HttpStatusCode.InternalServerError, ex);
        }
    }

    private async Task<HttpResponseMessage> ExecuteWithRetryAsync(Func<Task<HttpResponseMessage>> requestFunc)
    {
        var retryCount = 0;
        Exception? lastException = null;

        while (retryCount <= _options.MaxRetryCount)
        {
            try
            {
                var response = await requestFunc();
                
                // 如果是服务器错误或网络错误，进行重试
                if (retryCount < _options.MaxRetryCount && 
                    (response.StatusCode >= HttpStatusCode.InternalServerError || 
                     response.StatusCode == HttpStatusCode.RequestTimeout ||
                     response.StatusCode == HttpStatusCode.TooManyRequests))
                {
                    retryCount++;
                    _logger?.LogWarning("Request failed with {StatusCode}, retrying {RetryCount}/{MaxRetry}", 
                        response.StatusCode, retryCount, _options.MaxRetryCount);
                    
                    await Task.Delay(_options.RetryDelayMilliseconds * retryCount);
                    continue;
                }

                return response;
            }
            catch (Exception ex) when (retryCount < _options.MaxRetryCount && 
                                     (ex is HttpRequestException || ex is TaskCanceledException))
            {
                lastException = ex;
                retryCount++;
                _logger?.LogWarning(ex, "Request failed with exception, retrying {RetryCount}/{MaxRetry}", 
                    retryCount, _options.MaxRetryCount);
                
                await Task.Delay(_options.RetryDelayMilliseconds * retryCount);
            }
        }

        // 如果所有重试都失败了，抛出最后的异常
        throw lastException ?? new HttpRequestException("Request failed after all retries");
    }

    private static Dictionary<string, IEnumerable<string>> GetResponseHeaders(HttpResponseMessage response)
    {
        var headers = new Dictionary<string, IEnumerable<string>>();
        
        foreach (var header in response.Headers)
        {
            headers[header.Key] = header.Value;
        }
        
        if (response.Content.Headers != null)
        {
            foreach (var header in response.Content.Headers)
            {
                headers[header.Key] = header.Value;
            }
        }

        return headers;
    }

    #endregion

    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient?.Dispose();
            _disposed = true;
        }
    }
}
