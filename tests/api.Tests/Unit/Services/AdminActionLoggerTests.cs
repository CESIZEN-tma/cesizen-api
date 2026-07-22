using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using api.CZ.Features.AdminLogs.Enums;
using api.CZ.Features.AdminLogs.Services;

namespace api.Tests.Unit.Services;

public class AdminActionLoggerTests
{
    private readonly Mock<IAdminLogService> _mockAdminLogService;
    private readonly Mock<ILogger<AdminActionLogger>> _mockLogger;
    private readonly AdminActionLogger _sut;

    public AdminActionLoggerTests()
    {
        _mockAdminLogService = new Mock<IAdminLogService>();
        _mockLogger = new Mock<ILogger<AdminActionLogger>>();
        _sut = new AdminActionLogger(_mockAdminLogService.Object, _mockLogger.Object);
    }

    [Theory]
    [InlineData("Administrator", AdminActionCode.ADMIN_CREATED)]
    [InlineData("InformationPage", AdminActionCode.INFO_PAGE_CREATED)]
    [InlineData("InformationTag", AdminActionCode.INFO_TAG_CREATED)]
    [InlineData("NavigationMenu", AdminActionCode.NAV_MENU_CREATED)]
    [InlineData("Configuration", AdminActionCode.CONFIG_CREATED)]
    [InlineData("Quiz", AdminActionCode.QUIZ_CREATED)]
    [InlineData("Quizz", AdminActionCode.QUIZ_CREATED)]
    [InlineData("Unknown", AdminActionCode.BULK_OPERATION)]
    public async Task LogCreateAsync_MapsEntityTypeToCorrectActionCode(string entityType, AdminActionCode expected)
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var entityId = Guid.NewGuid();

        // Act
        await _sut.LogCreateAsync(adminId, entityType, entityId, "description");

        // Assert
        _mockAdminLogService.Verify(s => s.LogActionAsync(adminId, expected, entityType, entityId, "description"), Times.Once);
    }

    [Theory]
    [InlineData("Administrator", AdminActionCode.ADMIN_UPDATED)]
    [InlineData("InformationPage", AdminActionCode.INFO_PAGE_UPDATED)]
    [InlineData("InformationTag", AdminActionCode.INFO_TAG_UPDATED)]
    [InlineData("NavigationMenu", AdminActionCode.NAV_MENU_UPDATED)]
    [InlineData("Configuration", AdminActionCode.CONFIG_UPDATED)]
    [InlineData("Quiz", AdminActionCode.QUIZ_UPDATED)]
    [InlineData("Unknown", AdminActionCode.BULK_OPERATION)]
    public async Task LogUpdateAsync_MapsEntityTypeToCorrectActionCode(string entityType, AdminActionCode expected)
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var entityId = Guid.NewGuid();

        // Act
        await _sut.LogUpdateAsync(adminId, entityType, entityId, "description");

        // Assert
        _mockAdminLogService.Verify(s => s.LogActionAsync(adminId, expected, entityType, entityId, "description"), Times.Once);
    }

    [Theory]
    [InlineData("Administrator", AdminActionCode.ADMIN_DELETED)]
    [InlineData("InformationPage", AdminActionCode.INFO_PAGE_DELETED)]
    [InlineData("InformationTag", AdminActionCode.INFO_TAG_DELETED)]
    [InlineData("NavigationMenu", AdminActionCode.NAV_MENU_DELETED)]
    [InlineData("Configuration", AdminActionCode.CONFIG_DELETED)]
    [InlineData("Quiz", AdminActionCode.QUIZ_DELETED)]
    [InlineData("Unknown", AdminActionCode.BULK_OPERATION)]
    public async Task LogDeleteAsync_MapsEntityTypeToCorrectActionCode(string entityType, AdminActionCode expected)
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var entityId = Guid.NewGuid();

        // Act
        await _sut.LogDeleteAsync(adminId, entityType, entityId, "description");

        // Assert
        _mockAdminLogService.Verify(s => s.LogActionAsync(adminId, expected, entityType, entityId, "description"), Times.Once);
    }

    [Fact]
    public async Task LogCreateAsync_EntityTypeIsCaseInsensitive()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var entityId = Guid.NewGuid();

        // Act
        await _sut.LogCreateAsync(adminId, "aDMINISTRATOr", entityId, "description");

        // Assert
        _mockAdminLogService.Verify(
            s => s.LogActionAsync(adminId, AdminActionCode.ADMIN_CREATED, "aDMINISTRATOr", entityId, "description"),
            Times.Once);
    }

    [Fact]
    public async Task LogCustomActionAsync_PassesActionCodeThroughUnchanged()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var entityId = Guid.NewGuid();

        // Act
        await _sut.LogCustomActionAsync(adminId, AdminActionCode.USER_ENABLED, "User", entityId, "description");

        // Assert
        _mockAdminLogService.Verify(
            s => s.LogActionAsync(adminId, AdminActionCode.USER_ENABLED, "User", entityId, "description"),
            Times.Once);
    }
}
