using System.Net;
using System.Text.Json;

namespace OrderManagement.Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        var response = new ApiErrorResponse
        {
            Message = GetMessage(exception),
            StatusCode = GetStatusCode(exception),
            Timestamp = DateTime.UtcNow,
            Errors = GetErrors(exception)
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = response.StatusCode;

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }

    private static string GetMessage(Exception exception) => exception switch
    {
        FluentValidation.ValidationException => "One or more validation errors occurred.",
        KeyNotFoundException => exception.Message,
        UnauthorizedAccessException => "You are not authorized to perform this action.",
        ArgumentException => exception.Message,
        InvalidOperationException => exception.Message,
        _ => "An error occurred while processing your request."
    };

    private static int GetStatusCode(Exception exception) => exception switch
    {
        FluentValidation.ValidationException => (int)HttpStatusCode.BadRequest,
        KeyNotFoundException => (int)HttpStatusCode.NotFound,
        UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
        ArgumentException => (int)HttpStatusCode.BadRequest,
        InvalidOperationException => (int)HttpStatusCode.BadRequest,
        _ => (int)HttpStatusCode.InternalServerError
    };

    private static IDictionary<string, string[]>? GetErrors(Exception exception)
    {
        if (exception is FluentValidation.ValidationException validationException)
        {
            return validationException.Errors
                .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
        }

        return null;
    }
}

public class ApiErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public DateTime Timestamp { get; set; }
    public IDictionary<string, string[]>? Errors { get; set; }
}