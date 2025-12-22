namespace Parking.WebApi.Application.Common.Models;

public class Result
{
    public bool IsSuccess { get; }
    public string? Message { get; }
    public List<string> Errors { get; }

    protected Result(bool isSuccess, string? message, List<string>? errors = null)
    {
        IsSuccess = isSuccess;
        Message = message;
        Errors = errors ?? new List<string>();
    }

    public static Result Success(string? message = null)
        => new(true, message);

    public static Result Failure(string error, string? message = null)
        => new(false, message, new List<string> { error });

    public static Result Failure(List<string> errors, string? message = null)
        => new(false, message, errors);
}

public class Result<T> : Result
{
    public T? Data { get; }

    private Result(bool isSuccess, T? data, string? message, List<string>? errors = null)
        : base(isSuccess, message, errors)
    {
        Data = data;
    }

    public static Result<T> Success(T data, string? message = null)
        => new(true, data, message);

    public static new Result<T> Failure(string error, string? message = null)
        => new(false, default, message, new List<string> { error });

    public static new Result<T> Failure(List<string> errors, string? message = null)
        => new(false, default, message, errors);
}
