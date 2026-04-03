using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using api.CZ.Core.Services;
using api.CZ.Features.AdminEmailConfirmationTokens.Services;
using api.CZ.Features.AdminPasswordResetTokens.Services;
using api.CZ.Features.AdminSessions.Models;
using api.CZ.Features.AdminSessions.Services;
using api.CZ.Features.Administrators.Factories;
using api.CZ.Features.Administrators.Repositories;
using api.CZ.Features.Authentifications.Services;
using api.Tests.Builders;
using Simply.Auth.Core.Abstractions;

namespace api.Tests.Unit.Services;

public class AdminAuthentificationServiceSessionTests
{
    private readonly Mock<IAdministratorRepository> _mockAdminRepository;
    private readonly Mock<ISimplyAuthService> _mockSimplyAuthService;
    private readonly Mock<IAdministratorFactory> _mockAdminFactory;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<IAdminEmailConfirmationTokenService> _mockEmailConfirmationTokenService;
    private readonly Mock<IAdminPasswordResetTokenService> _mockPasswordResetTokenService;
    private readonly Mock<IAdminSessionService> _mockSessionService;
    private readonly Mock<ILogger<AdminAuthentificationService>> _mockLogger;
    private readonly AdminAuthentificationService _sut;

    public AdminAuthentificationServiceSessionTests()
    {
        _mockAdminRepository = new Mock<IAdministratorRepository>();
        _mockSimplyAuthService = new Mock<ISimplyAuthService>();
        _mockAdminFactory = new Mock<IAdministratorFactory>();
        _mockEmailService = new Mock<IEmailService>();
        _mockEmailConfirmationTokenService = new Mock<IAdminEmailConfirmationTokenService>();
        _mockPasswordResetTokenService = new Mock<IAdminPasswordResetTokenService>();
        _mockSessionService = new Mock<IAdminSessionService>();
        _mockLogger = new Mock<ILogger<AdminAuthentificationService>>();

        _sut = new AdminAuthentificationService(
            _mockAdminRepository.Object,
            _mockSimplyAuthService.Object,
            _mockAdminFactory.Object,
            _mockEmailService.Object,
            _mockEmailConfirmationTokenService.Object,
            _mockPasswordResetTokenService.Object,
            _mockSessionService.Object,
            _mockLogger.Object);
    }

    #region GetActiveSessions Tests

    [Fact]
    public async Task GetActiveSessions_ValidAdmin_ReturnsSessionList()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var refreshToken = "current-refresh-token";
        var currentSession = TestDataBuilder.AdminSessions.BuildValid(adminId);
        currentSession.Token = refreshToken;

        var sessions = new List<AdminSession>
        {
            currentSession,
            TestDataBuilder.AdminSessions.BuildValid(adminId),
            TestDataBuilder.AdminSessions.BuildValid(adminId)
        };

        _mockSessionService.Setup(s => s.GetActiveSessionsByAdminId(adminId))
            .ReturnsAsync(sessions);

        _mockSessionService.Setup(s => s.GetByRefreshToken(refreshToken))
            .ReturnsAsync(currentSession);

        // Act
        var result = await _sut.GetActiveSessions(adminId, refreshToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
        result.Value!.Single(s => s.IsCurrentSession).Id.Should().Be(currentSession.Id);
    }

    [Fact]
    public async Task GetActiveSessions_NoSessions_ReturnsEmptyList()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var refreshToken = "current-refresh-token";

        _mockSessionService.Setup(s => s.GetActiveSessionsByAdminId(adminId))
            .ReturnsAsync(new List<AdminSession>());

        _mockSessionService.Setup(s => s.GetByRefreshToken(refreshToken))
            .ReturnsAsync((AdminSession?)null);

        // Act
        var result = await _sut.GetActiveSessions(adminId, refreshToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    #endregion

    #region RevokeSession Tests

    [Fact]
    public async Task RevokeSession_ValidSession_ReturnsSuccess()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        _mockSessionService.Setup(s => s.RevokeSessionForAdmin(sessionId, adminId))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.RevokeSession(adminId, sessionId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockSessionService.Verify(s => s.RevokeSessionForAdmin(sessionId, adminId), Times.Once);
    }

    [Fact]
    public async Task RevokeSession_SessionNotFound_ReturnsFailure()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        _mockSessionService.Setup(s => s.RevokeSessionForAdmin(sessionId, adminId))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.RevokeSession(adminId, sessionId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Session not found.");
    }

    #endregion

    #region RevokeAllOtherSessions Tests

    [Fact]
    public async Task RevokeAllOtherSessions_ValidSession_RevokesAllExceptCurrent()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var refreshToken = "current-refresh-token";
        var currentSession = TestDataBuilder.AdminSessions.BuildValid(adminId);
        currentSession.Token = refreshToken;

        _mockSessionService.Setup(s => s.GetByRefreshToken(refreshToken))
            .ReturnsAsync(currentSession);

        _mockSessionService.Setup(s => s.RevokeAllSessionsExceptCurrent(adminId, currentSession.Id))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.RevokeAllOtherSessions(adminId, refreshToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockSessionService.Verify(s => s.RevokeAllSessionsExceptCurrent(adminId, currentSession.Id), Times.Once);
    }

    [Fact]
    public async Task RevokeAllOtherSessions_CurrentSessionNotFound_ReturnsFailure()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var refreshToken = "invalid-refresh-token";

        _mockSessionService.Setup(s => s.GetByRefreshToken(refreshToken))
            .ReturnsAsync((AdminSession?)null);

        // Act
        var result = await _sut.RevokeAllOtherSessions(adminId, refreshToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Current session not found.");
    }

    #endregion
}
