using System.Net;
using System.Text.Json;

namespace api.CZ.Core.Middlewares;



/// <summary>
/// Middleware to catch unhandled exceptions.
/// Security net, use Result<T> for other errors.
/// </summary>
public sealed class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionMiddleware(
        RequestDelegate next, 
        ILogger<ExceptionMiddleware> logger, 
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
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
        // Structured logs
        _logger.LogError(exception, 
            "Unhandled exception(s) : {ExceptionType} on {Method} {Path}. TraceId: {TraceId}",
            exception.GetType().Name,
            context.Request.Method,
            context.Request.Path,
            context.TraceIdentifier);

        var (statusCode, title) = GetErrorDetails(exception);

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var problemDetails = new
        {
            Type = $"https://httpstatuses.com/{(int)statusCode}",
            Title = title,
            Status = (int)statusCode,
            TraceId = context.TraceIdentifier,
            // Development details
            Detail = _environment.IsDevelopment() ? exception.Message : null,
            Exception = _environment.IsDevelopment() ? new
            {
                Type = exception.GetType().FullName,
                Message = exception.Message,
                StackTrace = exception.StackTrace
            } : null
        };

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment()
        });

        await context.Response.WriteAsync(json);
    }

    private static (HttpStatusCode StatusCode, string Title) GetErrorDetails(Exception exception)
    {
        // If new customs exceptions, add below
        return exception switch
        {
            OperationCanceledException => (HttpStatusCode.RequestTimeout, "La requête a expiré"),
            _ => (HttpStatusCode.InternalServerError, "Une erreur technique est survenue")
        };
    }
}