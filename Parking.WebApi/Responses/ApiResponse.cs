namespace Parking.WebApi.Responses;

public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
    public int StatusCode { get; set; }

    public static ApiResponse<T> Success(T data, string? message = null, int statusCode = 200)
        => new()
        {
            IsSuccess = true,
            Data = data,
            Message = message,
            StatusCode = statusCode
        };

    public static ApiResponse<object> Success(string? message = null, int statusCode = 200)
        => new ApiResponse<object>
        {
            IsSuccess = true,
            Data = null,
            Message = message,
            StatusCode = statusCode
        };

    public static ApiResponse<T> Fail(List<string> errors, string? message = null, int statusCode = 400)
        => new()
        {
            IsSuccess = false,
            Data = default,
            Message = message ?? "عملیات ناموفق بود",
            Errors = errors,
            StatusCode = statusCode
        };

    public static ApiResponse<T> Fail(string error, string? message = null, int statusCode = 400)
        => Fail([error], message, statusCode);
}

public class ApiResponse : ApiResponse<object> { }