using DotNetEnv;
using Microsoft.AspNetCore.Authorization;

namespace api.CZ.Core.Middlewares;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _apiKey;
    private readonly ILogger<ApiKeyMiddleware> _logger;
    private const string ApiKeyHeaderName = "x-api-key";
    
    public ApiKeyMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        ILogger<ApiKeyMiddleware> logger)
    {
        _next = next;
        _apiKey = configuration["ApiKey"] ?? Environment.GetEnvironmentVariable("API_KEY")
            ?? throw new InvalidOperationException("API Key not configured");
        _logger = logger;
        
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            throw new InvalidOperationException("API Key cannot be empty");
        }
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        // Allow anonymous endpoints
        var endpoint = context.GetEndpoint();
        var isPublic = context.Request.Path.ToString().ToLower().Contains("public");
        var isSwagger = context.Request.Path.ToString().ToLower().Contains("swagger");
        if (isSwagger || isPublic)
        {
            await _next(context);
            return;
        }
        
        // Extract API key from header
        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
        {
            _logger.LogWarning("API key missing from request. IP: {IP}", 
                context.Connection.RemoteIpAddress);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new 
            { 
                error = "API Key is missing" 
            });
            return;
        }
        
        // Validate API key (constant-time comparison)
        if (!IsApiKeyValid(extractedApiKey!))
        {
            _logger.LogWarning("Invalid API key attempt. IP: {IP}", 
                context.Connection.RemoteIpAddress);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new 
            { 
                error = "Invalid API Key" 
            });
            return;
        }
        
        await _next(context);
    }
    
    private bool IsApiKeyValid(string providedKey)
    {
        // Constant-time comparison to prevent timing attacks
        if (providedKey.Length != _apiKey.Length)
            return false;
            
        var result = 0;
        for (int i = 0; i < _apiKey.Length; i++)
        {
            result |= providedKey[i] ^ _apiKey[i];
        }
        
        return result == 0;
    }
}