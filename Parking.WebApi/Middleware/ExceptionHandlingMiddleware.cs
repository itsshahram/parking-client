using System.Net;
using System.Text.Json;
using Parking.WebApi.Application.Common.Exceptions;
using Parking.WebApi.Responses;

namespace Parking.WebApi.Middleware;

public class ExceptionHandlingMiddleware(
    RequestDelegate next, 
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "خطای غیرمنتظره رخ داده است: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = HttpStatusCode.InternalServerError;
        var message = "خطای داخلی سرور رخ داده است.";
        List<string>? errors = null;

        switch (exception)
        {
            case ValidationException validationException:
                statusCode = HttpStatusCode.BadRequest;
                message = "خطاهای اعتبارسنجی";
                errors = validationException.Errors;
                break;

            case CustomNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                message = exception.Message;
                errors = new List<string> { exception.Message };
                break;

            case AlreadyExistsException:
                statusCode = HttpStatusCode.Conflict;
                message = exception.Message;
                errors = new List<string> { exception.Message };
                break;

            case AlreadyPaidException:
                statusCode = HttpStatusCode.BadRequest;
                message = exception.Message;
                errors = new List<string> { exception.Message };
                break;

            case CardIsInUseException:
                statusCode = HttpStatusCode.BadRequest;
                message = exception.Message;
                errors = new List<string> { exception.Message };
                break;

            case InActiveCardException:
                statusCode = HttpStatusCode.BadRequest;
                message = exception.Message;
                errors = new List<string> { exception.Message };
                break;
            
            case ArgumentException argumentException:
                statusCode = HttpStatusCode.BadRequest;
                message = argumentException.Message;
                errors = new List<string> { argumentException.Message };
                break;

            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Unauthorized;
                message = exception.Message;
                errors = new List<string> { exception.Message };
                break;

            case InvalidOperationException:
                statusCode = HttpStatusCode.InternalServerError;
                message = exception.Message;
                errors = new List<string> { exception.Message };
                break;

            default:
                errors = new List<string> { exception.Message };
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = ApiResponse<object>.Fail(errors!, message, (int)statusCode);

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}
