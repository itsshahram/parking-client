
namespace Parking.App.Models;

public class ApiResponse<T>
{
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }
    public List<string> Errors { get; set; } = new List<string>();

    public ApiResponse(int statusCode, string message, T data, List<string>? errors = null)
    {
        StatusCode = statusCode;
        Message = message;
        Data = data;
        Errors = errors ?? new List<string>();
    }
}

