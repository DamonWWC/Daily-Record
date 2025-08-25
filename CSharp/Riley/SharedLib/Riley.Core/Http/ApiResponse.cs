using System.Net;
using System.Text.Json.Serialization;

namespace Riley.Core.Http;

/// <summary>
/// 通用API响应模型
/// </summary>
/// <typeparam name="T">响应数据类型</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// HTTP状态码
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// 响应数据
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// 错误消息
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 异常信息
    /// </summary>
    public Exception? Exception { get; set; }

    /// <summary>
    /// 响应头
    /// </summary>
    public Dictionary<string, IEnumerable<string>>? Headers { get; set; }

    /// <summary>
    /// 创建成功响应
    /// </summary>
    public static ApiResponse<T> Success(T data, HttpStatusCode statusCode = HttpStatusCode.OK, Dictionary<string, IEnumerable<string>>? headers = null)
    {
        return new ApiResponse<T>
        {
            IsSuccess = true,
            StatusCode = statusCode,
            Data = data,
            Headers = headers
        };
    }

    /// <summary>
    /// 创建失败响应
    /// </summary>
    public static ApiResponse<T> Failure(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError, Exception? exception = null)
    {
        return new ApiResponse<T>
        {
            IsSuccess = false,
            StatusCode = statusCode,
            Message = message,
            Exception = exception
        };
    }
}

/// <summary>
/// 无数据的API响应模型
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    /// <summary>
    /// 创建成功响应
    /// </summary>
    public static new ApiResponse Success(HttpStatusCode statusCode = HttpStatusCode.OK, Dictionary<string, IEnumerable<string>>? headers = null)
    {
        return new ApiResponse
        {
            IsSuccess = true,
            StatusCode = statusCode,
            Headers = headers
        };
    }

    /// <summary>
    /// 创建失败响应
    /// </summary>
    public static new ApiResponse Failure(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError, Exception? exception = null)
    {
        return new ApiResponse
        {
            IsSuccess = false,
            StatusCode = statusCode,
            Message = message,
            Exception = exception
        };
    }
}
