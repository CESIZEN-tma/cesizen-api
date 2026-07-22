using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using api.CZ.Core.ResultPattern;
using api.CZ.Features.HealthChecks;
using api.CZ.Features.HealthChecks.Services;

namespace api.Tests.Unit.Features.HealthChecks;

public class HealthCheckControllerTests
{
    private readonly Mock<IHealthCheckService> _mockService;
    private readonly HealthCheckController _controller;

    public HealthCheckControllerTests()
    {
        _mockService = new Mock<IHealthCheckService>();
        var mockLogger = new Mock<ILogger<HealthCheckController>>();
        _controller = new HealthCheckController(_mockService.Object, mockLogger.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [Fact]
    public async Task CheckHealth_HealthyResult_ReturnsOk()
    {
        // Arrange
        var response = new HealthCheckResponse("Healthy", 12.5, new List<HealthCheckEntry>
        {
            new("api", "Healthy", 1, "Memory: 50 MB"),
            new("database", "Healthy", 10, "Connection and read OK")
        });
        _mockService.Setup(s => s.CheckHealthAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(response));

        // Act
        var result = await _controller.CheckHealth(CancellationToken.None);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(response);
    }

    [Fact]
    public async Task CheckHealth_UnhealthyResult_ReturnsServiceUnavailable()
    {
        // Arrange
        var response = new HealthCheckResponse("Unhealthy", 12.5, new List<HealthCheckEntry>
        {
            new("api", "Healthy", 1, "Memory: 50 MB"),
            new("database", "Unhealthy", 10, Error: "Unable to connect to database")
        });
        _mockService.Setup(s => s.CheckHealthAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(response));

        // Act
        var result = await _controller.CheckHealth(CancellationToken.None);

        // Assert
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
        objectResult.Value.Should().Be(response);
    }

    [Fact]
    public async Task CheckHealth_ServiceFailure_ReturnsServiceUnavailableWithErrorMessage()
    {
        // Arrange
        _mockService.Setup(s => s.CheckHealthAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<HealthCheckResponse>("Unexpected error"));

        // Act
        var result = await _controller.CheckHealth(CancellationToken.None);

        // Assert
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
    }
}
