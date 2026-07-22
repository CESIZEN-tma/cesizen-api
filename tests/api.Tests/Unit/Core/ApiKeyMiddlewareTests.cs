using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using api.CZ.Core.Middlewares;

namespace api.Tests.Unit.Core;

public class ApiKeyMiddlewareTests
{
    private const string ValidKey = "test-api-key-12345";

    private static ApiKeyMiddleware BuildMiddleware(RequestDelegate next, string? apiKey = ValidKey)
    {
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["ApiKey"]).Returns(apiKey);
        var mockLogger = new Mock<ILogger<ApiKeyMiddleware>>();

        return new ApiKeyMiddleware(next, mockConfig.Object, mockLogger.Object);
    }

    [Fact]
    public void Constructor_NoApiKeyConfiguredAnywhere_ThrowsInvalidOperationException()
    {
        // Arrange
        var previous = Environment.GetEnvironmentVariable("API_KEY");
        Environment.SetEnvironmentVariable("API_KEY", null);

        try
        {
            // Act
            var act = () => BuildMiddleware(_ => Task.CompletedTask, apiKey: null);

            // Assert
            act.Should().Throw<InvalidOperationException>().WithMessage("*not configured*");
        }
        finally
        {
            Environment.SetEnvironmentVariable("API_KEY", previous);
        }
    }

    [Fact]
    public void Constructor_EmptyApiKey_ThrowsInvalidOperationException()
    {
        // Act
        var act = () => BuildMiddleware(_ => Task.CompletedTask, apiKey: "   ");

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*cannot be empty*");
    }

    [Fact]
    public async Task InvokeAsync_PublicPath_BypassesKeyCheckAndCallsNext()
    {
        // Arrange
        var nextCalled = false;
        var middleware = BuildMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/public/docs";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_SwaggerPath_BypassesKeyCheckAndCallsNext()
    {
        // Arrange
        var nextCalled = false;
        var middleware = BuildMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        var context = new DefaultHttpContext();
        context.Request.Path = "/swagger/index.html";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_MissingApiKeyHeader_Returns401AndDoesNotCallNext()
    {
        // Arrange
        var nextCalled = false;
        var middleware = BuildMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/quizzes";
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task InvokeAsync_WrongApiKey_Returns401AndDoesNotCallNext()
    {
        // Arrange
        var nextCalled = false;
        var middleware = BuildMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/quizzes";
        context.Request.Headers["x-api-key"] = "wrong-key";
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task InvokeAsync_KeyOfDifferentLength_Returns401()
    {
        // Arrange
        var middleware = BuildMiddleware(_ => Task.CompletedTask);
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/quizzes";
        context.Request.Headers["x-api-key"] = ValidKey + "extra";
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task InvokeAsync_CorrectApiKey_CallsNextWithoutModifyingResponse()
    {
        // Arrange
        var nextCalled = false;
        var middleware = BuildMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/quizzes";
        context.Request.Headers["x-api-key"] = ValidKey;

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(200);
    }
}
