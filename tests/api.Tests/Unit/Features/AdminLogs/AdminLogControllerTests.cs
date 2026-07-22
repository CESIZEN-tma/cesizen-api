using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using api.CZ.Features.AdminLogs;
using api.CZ.Features.AdminLogs.DTOs;
using api.CZ.Features.AdminLogs.Services;

namespace api.Tests.Unit.Features.AdminLogs;

public class AdminLogControllerTests
{
    private readonly Mock<IAdminLogService> _mockService;
    private readonly AdminLogController _controller;

    public AdminLogControllerTests()
    {
        _mockService = new Mock<IAdminLogService>();
        var mockLogger = new Mock<ILogger<AdminLogController>>();
        _controller = new AdminLogController(_mockService.Object, mockLogger.Object);
    }

    private static GetAdminLogDto BuildLog()
    {
        return new GetAdminLogDto
        {
            Id = Guid.NewGuid(),
            ActionCode = "ADMIN_CREATED",
            EntityType = "Administrator",
            TargetedEntityId = Guid.NewGuid(),
            Description = "did something",
            CreationTime = DateTime.UtcNow,
            AdministratorId = Guid.NewGuid(),
            AdministratorEmail = "admin@b.com",
            AdministratorName = "Admin Name"
        };
    }

    [Fact]
    public async Task GetFilteredLogs_ReturnsOkWithLogs()
    {
        // Arrange
        var filter = new AdminLogFilterDto();
        _mockService.Setup(s => s.GetFilteredLogsAsync(filter)).ReturnsAsync(new List<GetAdminLogDto> { BuildLog() });

        // Act
        var result = await _controller.GetFilteredLogs(filter);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetRecentLogs_DefaultCount_PassesFiftyToService()
    {
        // Arrange
        _mockService.Setup(s => s.GetRecentLogsAsync(50)).ReturnsAsync(new List<GetAdminLogDto> { BuildLog() });

        // Act
        var result = await _controller.GetRecentLogs();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockService.Verify(s => s.GetRecentLogsAsync(50), Times.Once);
    }

    [Fact]
    public async Task GetRecentLogs_CustomCount_PassesItToService()
    {
        // Arrange
        _mockService.Setup(s => s.GetRecentLogsAsync(10)).ReturnsAsync(new List<GetAdminLogDto> { BuildLog() });

        // Act
        var result = await _controller.GetRecentLogs(10);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockService.Verify(s => s.GetRecentLogsAsync(10), Times.Once);
    }

    [Fact]
    public async Task GetLogsByAdministrator_ReturnsOkWithLogs()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        _mockService.Setup(s => s.GetLogsByAdministratorAsync(adminId)).ReturnsAsync(new List<GetAdminLogDto> { BuildLog() });

        // Act
        var result = await _controller.GetLogsByAdministrator(adminId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetLogsByEntity_ReturnsOkWithLogs()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        _mockService.Setup(s => s.GetLogsByEntityAsync("Quiz", entityId)).ReturnsAsync(new List<GetAdminLogDto> { BuildLog() });

        // Act
        var result = await _controller.GetLogsByEntity("Quiz", entityId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockService.Verify(s => s.GetLogsByEntityAsync("Quiz", entityId), Times.Once);
    }
}
