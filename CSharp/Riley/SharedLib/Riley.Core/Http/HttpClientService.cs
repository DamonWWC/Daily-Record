using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Riley.Core.Http;

/// <summary>
/// HTTP客户端服务实现
/// </summary>
public class HttpClientService : IHttpClientService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly HttpClientOptions _options;
    private readonly ILogger<HttpClientService>? _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private bool _disposed = false;

    public HttpClientService(HttpClient httpClient, IOptions<HttpClientOptions> options, ILogger<HttpClientService>? logger = null)
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
        // 设置基础URL
        if (!string.IsNullOrEmpty(_options.BaseUrl))
        {
            _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        }

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

    public async Task<ApiResponse<T>> GetAsync<T>(string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        return await SendRequestAsync<T>(HttpMethod.Get, url, null, headers, cancellationToken);
    }

    public async Task<ApiResponse<string>> GetStringAsync(string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        return await SendRequestAsync<string>(HttpMethod.Get, url, null, headers, cancellationToken);
    }

    public async Task<ApiResponse<T>> PostAsync<T>(string url, object? data = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        return await SendRequestAsync<T>(HttpMethod.Post, url, data, headers, cancellationToken);
    }

    public async Task<ApiResponse> PostAsync(string url, object? data = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        var response = await SendRequestAsync<object>(HttpMethod.Post, url, data, headers, cancellationToken);
        return new ApiResponse
        {
            IsSuccess = response.IsSuccess,
            StatusCode = response.StatusCode,
            Message = response.Message,
            Exception = response.Exception,
            Headers = response.Headers
        };
    }

    public async Task<ApiResponse<T>> PutAsync<T>(string url, object? data = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        return await SendRequestAsync<T>(HttpMethod.Put, url, data, headers, cancellationToken);
    }

    public async Task<ApiResponse> PutAsync(string url, object? data = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        var response = await SendRequestAsync<object>(HttpMethod.Put, url, data, headers, cancellationToken);
        return new ApiResponse
        {
            IsSuccess = response.IsSuccess,
            StatusCode = response.StatusCode,
            Message = response.Message,
            Exception = response.Exception,
            Headers = response.Headers
        };
    }

    public async Task<ApiResponse<T>> DeleteAsync<T>(string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        return await SendRequestAsync<T>(HttpMethod.Delete, url, null, headers, cancellationToken);
    }

    public async Task<ApiResponse> DeleteAsync(string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        var response = await SendRequestAsync<object>(HttpMethod.Delete, url, null, headers, cancellationToken);
        return new ApiResponse
        {
            IsSuccess = response.IsSuccess,
            StatusCode = response.StatusCode,
            Message = response.Message,
            Exception = response.Exception,
            Headers = response.Headers
        };
    }

    public async Task<ApiResponse<T>> PatchAsync<T>(string url, object? data = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        return await SendRequestAsync<T>(HttpMethod.Patch, url, data, headers, cancellationToken);
    }

    public async Task<ApiResponse> PatchAsync(string url, object? data = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        var response = await SendRequestAsync<object>(HttpMethod.Patch, url, data, headers, cancellationToken);
        return new ApiResponse
        {
            IsSuccess = response.IsSuccess,
            StatusCode = response.StatusCode,
            Message = response.Message,
            Exception = response.Exception,
            Headers = response.Headers
        };
    }

    public async Task<ApiResponse<T>> UploadFileAsync<T>(string url, string filePath, string fieldName = "file", Dictionary<string, string>? additionalData = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return ApiResponse<T>.Failure($"File not found: {filePath}", HttpStatusCode.BadRequest);
            }

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

            using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
            
            // 添加请求头
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            return await ExecuteRequestAsync<T>(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Upload file failed: {Url}", url);
            return ApiResponse<T>.Failure($"Upload file failed: {ex.Message}", HttpStatusCode.InternalServerError, ex);
        }
    }

    public async Task<ApiResponse> DownloadFileAsync(string url, string filePath, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            
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

                _logger?.LogInformation("File downloaded successfully: {FilePath}", filePath);
                return ApiResponse.Success(response.StatusCode, GetResponseHeaders(response));
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger?.LogWarning("Download file failed with status {StatusCode}: {Content}", response.StatusCode, errorContent);
                return ApiResponse.Failure($"Download failed: {response.StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Download file failed: {Url}", url);
            return ApiResponse.Failure($"Download file failed: {ex.Message}", HttpStatusCode.InternalServerError, ex);
        }
    }

    private async Task<ApiResponse<T>> SendRequestAsync<T>(HttpMethod method, string url, object? data, Dictionary<string, string>? headers, CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(method, url);

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

            return await ExecuteRequestAsync<T>(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "HTTP request failed: {Method} {Url}", method, url);
            return ApiResponse<T>.Failure($"Request failed: {ex.Message}", HttpStatusCode.InternalServerError, ex);
        }
    }

    private async Task<ApiResponse<T>> ExecuteRequestAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            _logger?.LogDebug("Sending HTTP request: {Method} {Url}", request.Method, request.RequestUri);

            var response = await ExecuteWithRetryAsync(async () => await _httpClient.SendAsync(request, cancellationToken));
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseHeaders = GetResponseHeaders(response);

            _logger?.LogDebug("HTTP response received: {StatusCode}, Content Length: {Length}", response.StatusCode, content.Length);

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
                            _logger?.LogWarning(ex, "Failed to deserialize response content to {Type}", typeof(T).Name);
                            return ApiResponse<T>.Failure($"Failed to deserialize response: {ex.Message}", response.StatusCode, ex);
                        }
                    }
                }

                return ApiResponse<T>.Success(data!, response.StatusCode, responseHeaders);
            }
            else
            {
                _logger?.LogWarning("HTTP request failed with status {StatusCode}: {Content}", response.StatusCode, content);
                return ApiResponse<T>.Failure($"Request failed with status {response.StatusCode}: {content}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "HTTP request execution failed: {Method} {Url}", request.Method, request.RequestUri);
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

    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient?.Dispose();
            _disposed = true;
        }
    }
}
