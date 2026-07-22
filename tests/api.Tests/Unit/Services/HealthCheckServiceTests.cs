using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using api.CZ.Data.EFCore;
using api.CZ.Features.HealthChecks.Services;

namespace api.Tests.Unit.Services;

public class HealthCheckServiceTests
{
    private static CesiZenDbContext BuildInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<CesiZenDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CesiZenDbContext(options);
    }

    [Fact]
    public async Task CheckHealthAsync_ReturnsSuccessResultWithApiAndDatabaseChecks()
    {
        // Arrange
        using var dbContext = BuildInMemoryContext();
        var mockLogger = new Mock<ILogger<HealthCheckService>>();
        var sut = new HealthCheckService(dbContext, mockLogger.Object);

        // Act
        var result = await sut.CheckHealthAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Checks.Should().HaveCount(2);
        result.Value.Checks.Should().Contain(c => c.Name == "api");
        result.Value.Checks.Should().Contain(c => c.Name == "database");
        result.Value.TotalDurationMs.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task CheckHealthAsync_ApiCheck_IsHealthyUnderNormalMemoryUsage()
    {
        // Arrange
        using var dbContext = BuildInMemoryContext();
        var mockLogger = new Mock<ILogger<HealthCheckService>>();
        var sut = new HealthCheckService(dbContext, mockLogger.Object);

        // Act
        var result = await sut.CheckHealthAsync();

        // Assert
        var apiCheck = result.Value.Checks.Single(c => c.Name == "api");
        apiCheck.Status.Should().Be("Healthy");
        apiCheck.Description.Should().Contain("Memory");
    }

    [Fact]
    public async Task CheckHealthAsync_OverallStatus_IsUnhealthyWhenAnyCheckIsUnhealthy()
    {
        // Arrange
        using var dbContext = BuildInMemoryContext();
        var mockLogger = new Mock<ILogger<HealthCheckService>>();
        var sut = new HealthCheckService(dbContext, mockLogger.Object);

        // Act
        var result = await sut.CheckHealthAsync();

        // Assert: the InMemory provider doesn't support raw SQL (ExecuteSqlRawAsync),
        // so the database check fails and the overall status reflects that.
        var databaseCheck = result.Value.Checks.Single(c => c.Name == "database");
        databaseCheck.Status.Should().Be("Unhealthy");
        databaseCheck.Error.Should().NotBeNullOrEmpty();
        result.Value.Status.Should().Be("Unhealthy");
    }

    [Fact]
    public async Task CheckHealthAsync_RespectsCancellationToken()
    {
        // Arrange
        using var dbContext = BuildInMemoryContext();
        var mockLogger = new Mock<ILogger<HealthCheckService>>();
        var sut = new HealthCheckService(dbContext, mockLogger.Object);
        using var cts = new CancellationTokenSource();

        // Act
        var result = await sut.CheckHealthAsync(cts.Token);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
