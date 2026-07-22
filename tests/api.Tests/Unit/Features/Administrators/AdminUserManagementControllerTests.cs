using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using api.CZ.Features.AdminLogs.Enums;
using api.CZ.Features.AdminLogs.Services;
using api.CZ.Features.Administrators;
using api.CZ.Features.Sessions.Models;
using api.CZ.Features.Sessions.Services;
using api.CZ.Features.Users.DTOs;
using api.CZ.Features.Users.Services;
using api.Tests.Builders;

namespace api.Tests.Unit.Features.Administrators;

public class AdminUserManagementControllerTests
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<ISessionService> _mockSessionService;
    private readonly Mock<IAdminActionLogger> _mockActionLogger;
    private readonly AdminUserManagementController _controller;
    private readonly Guid _testAdminId;

    public AdminUserManagementControllerTests()
    {
        _mockUserService = new Mock<IUserService>();
        _mockSessionService = new Mock<ISessionService>();
        _mockActionLogger = new Mock<IAdminActionLogger>();
        var mockLogger = new Mock<ILogger<AdminUserManagementController>>();

        _controller = new AdminUserManagementController(
            _mockUserService.Object, _mockSessionService.Object, _mockActionLogger.Object, mockLogger.Object);
        _testAdminId = Guid.NewGuid();

        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, _testAdminId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    [Fact]
    public async Task GetAllUsers_ReturnsOkWithUsers()
    {
        // Arrange
        var users = new List<GetUserAdminDto> { new() { Id = Guid.NewGuid(), Email = "a@b.com", FirstName = "A", LastName = "B" } };
        _mockUserService.Setup(s => s.GetAllForAdminAsync()).ReturnsAsync(users);

        // Act
        var result = await _controller.GetAllUsers();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        ((OkObjectResult)result).Value.Should().Be(users);
    }

    [Fact]
    public async Task UpdateUserStatus_DisablingOwnAccount_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.UpdateUserStatus(_testAdminId, new UpdateUserStatusDto { Active = false });

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        _mockUserService.Verify(s => s.UpdateUserStatusAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserStatus_EnablingOwnAccount_IsAllowed()
    {
        // Arrange
        _mockUserService.Setup(s => s.UpdateUserStatusAsync(_testAdminId, true, _testAdminId)).ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateUserStatus(_testAdminId, new UpdateUserStatusDto { Active = true });

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UpdateUserStatus_ExistingUser_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserService.Setup(s => s.UpdateUserStatusAsync(userId, false, _testAdminId)).ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateUserStatus(userId, new UpdateUserStatusDto { Active = false });

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UpdateUserStatus_NonExistentUser_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserService.Setup(s => s.UpdateUserStatusAsync(userId, true, _testAdminId)).ReturnsAsync(false);

        // Act
        var result = await _controller.UpdateUserStatus(userId, new UpdateUserStatusDto { Active = true });

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetUserSessions_ExistingUser_ReturnsOkWithSessions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profile = new GetUserProfileDto { Id = userId, Email = "a@b.com", FirstName = "A", LastName = "B" };
        var sessions = TestDataBuilder.Sessions.BuildMany(2, userId);

        _mockUserService.Setup(s => s.GetProfileAsync(userId)).ReturnsAsync(profile);
        _mockSessionService.Setup(s => s.GetActiveSessionsByUserId(userId)).ReturnsAsync(sessions);

        // Act
        var result = await _controller.GetUserSessions(userId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetUserSessions_NonExistentUser_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserService.Setup(s => s.GetProfileAsync(userId)).ReturnsAsync((GetUserProfileDto?)null);

        // Act
        var result = await _controller.GetUserSessions(userId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task RevokeAllUserSessions_OwnAccount_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.RevokeAllUserSessions(_testAdminId);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        _mockSessionService.Verify(s => s.RevokeAllUserSessions(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task RevokeAllUserSessions_NonExistentUser_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserService.Setup(s => s.GetProfileAsync(userId)).ReturnsAsync((GetUserProfileDto?)null);

        // Act
        var result = await _controller.RevokeAllUserSessions(userId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task RevokeAllUserSessions_ExistingUser_RevokesAndLogsAction()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profile = new GetUserProfileDto { Id = userId, Email = "user@b.com", FirstName = "A", LastName = "B" };
        _mockUserService.Setup(s => s.GetProfileAsync(userId)).ReturnsAsync(profile);
        _mockSessionService.Setup(s => s.RevokeAllUserSessions(userId)).ReturnsAsync(true);

        // Act
        var result = await _controller.RevokeAllUserSessions(userId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockActionLogger.Verify(l => l.LogCustomActionAsync(
            _testAdminId, AdminActionCode.USER_SESSION_REVOKED, "User", userId, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task RevokeUserSession_NonExistentUser_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserService.Setup(s => s.GetProfileAsync(userId)).ReturnsAsync((GetUserProfileDto?)null);

        // Act
        var result = await _controller.RevokeUserSession(userId, Guid.NewGuid());

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task RevokeUserSession_SessionNotFoundForUser_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var profile = new GetUserProfileDto { Id = userId, Email = "user@b.com", FirstName = "A", LastName = "B" };
        _mockUserService.Setup(s => s.GetProfileAsync(userId)).ReturnsAsync(profile);
        _mockSessionService.Setup(s => s.RevokeSessionForUser(sessionId, userId)).ReturnsAsync(false);

        // Act
        var result = await _controller.RevokeUserSession(userId, sessionId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        _mockActionLogger.Verify(l => l.LogCustomActionAsync(
            It.IsAny<Guid>(), It.IsAny<AdminActionCode>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RevokeUserSession_ValidSession_RevokesAndLogsAction()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var profile = new GetUserProfileDto { Id = userId, Email = "user@b.com", FirstName = "A", LastName = "B" };
        _mockUserService.Setup(s => s.GetProfileAsync(userId)).ReturnsAsync(profile);
        _mockSessionService.Setup(s => s.RevokeSessionForUser(sessionId, userId)).ReturnsAsync(true);

        // Act
        var result = await _controller.RevokeUserSession(userId, sessionId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockActionLogger.Verify(l => l.LogCustomActionAsync(
            _testAdminId, AdminActionCode.USER_SESSION_REVOKED, "User", userId, It.IsAny<string>()), Times.Once);
    }
}
