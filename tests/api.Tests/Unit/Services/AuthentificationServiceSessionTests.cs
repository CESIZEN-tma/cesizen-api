using System.Linq.Expressions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using api.CZ.Core.ResultPattern;
using api.CZ.Core.Services;
using api.CZ.Features.Authentifications.Services;
using api.CZ.Features.EmailConfirmationTokens.Services;
using api.CZ.Features.PasswordHistories.Services;
using api.CZ.Features.PasswordResetTokens.Services;
using api.CZ.Features.Sessions.Models;
using api.CZ.Features.Sessions.Services;
using api.CZ.Features.Users.Factories;
using api.CZ.Features.Users.Models;
using api.CZ.Features.Users.Repositories;
using api.Tests.Builders;
using Simply.Auth.Core.Abstractions;

namespace api.Tests.Unit.Services;

public class AuthentificationServiceSessionTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<ISimplyAuthService> _mockSimplyAuthService;
    private readonly Mock<IUserFactory> _mockUserFactory;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<IEmailConfirmationTokenService> _mockEmailConfirmationTokenService;
    private readonly Mock<IPasswordResetTokenService> _mockPasswordResetTokenService;
    private readonly Mock<ISessionService> _mockSessionService;
    private readonly Mock<IPasswordHistoryManager> _mockPasswordHistoryManager;
    private readonly Mock<ILogger<AuthentificationService>> _mockLogger;
    private readonly AuthentificationService _sut;

    public AuthentificationServiceSessionTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockSimplyAuthService = new Mock<ISimplyAuthService>();
        _mockUserFactory = new Mock<IUserFactory>();
        _mockEmailService = new Mock<IEmailService>();
        _mockEmailConfirmationTokenService = new Mock<IEmailConfirmationTokenService>();
        _mockPasswordResetTokenService = new Mock<IPasswordResetTokenService>();
        _mockSessionService = new Mock<ISessionService>();
        _mockPasswordHistoryManager = new Mock<IPasswordHistoryManager>();
        _mockLogger = new Mock<ILogger<AuthentificationService>>();

        _sut = new AuthentificationService(
            _mockUserRepository.Object,
            _mockSimplyAuthService.Object,
            _mockUserFactory.Object,
            _mockEmailService.Object,
            _mockEmailConfirmationTokenService.Object,
            _mockPasswordResetTokenService.Object,
            _mockSessionService.Object,
            _mockPasswordHistoryManager.Object,
            _mockLogger.Object);
    }

    #region GetActiveSessions Tests

    [Fact]
    public async Task GetActiveSessions_ValidUser_ReturnsSessionList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshToken = "current-refresh-token";
        var currentSession = TestDataBuilder.Sessions.BuildValid(userId);
        currentSession.Token = refreshToken;

        var sessions = new List<Session>
        {
            currentSession,
            TestDataBuilder.Sessions.BuildValid(userId),
            TestDataBuilder.Sessions.BuildValid(userId)
        };

        _mockSessionService.Setup(s => s.GetActiveSessionsByUserId(userId))
            .ReturnsAsync(sessions);

        _mockSessionService.Setup(s => s.GetByRefreshToken(refreshToken))
            .ReturnsAsync(currentSession);

        // Act
        var result = await _sut.GetActiveSessions(userId, refreshToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
        result.Value!.Single(s => s.IsCurrentSession).Id.Should().Be(currentSession.Id);
    }

    [Fact]
    public async Task GetActiveSessions_NoSessions_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshToken = "current-refresh-token";

        _mockSessionService.Setup(s => s.GetActiveSessionsByUserId(userId))
            .ReturnsAsync(new List<Session>());

        _mockSessionService.Setup(s => s.GetByRefreshToken(refreshToken))
            .ReturnsAsync((Session?)null);

        // Act
        var result = await _sut.GetActiveSessions(userId, refreshToken);

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
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        _mockSessionService.Setup(s => s.RevokeSessionForUser(sessionId, userId))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.RevokeSession(userId, sessionId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockSessionService.Verify(s => s.RevokeSessionForUser(sessionId, userId), Times.Once);
    }

    [Fact]
    public async Task RevokeSession_SessionNotFound_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        _mockSessionService.Setup(s => s.RevokeSessionForUser(sessionId, userId))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.RevokeSession(userId, sessionId);

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
        var userId = Guid.NewGuid();
        var refreshToken = "current-refresh-token";
        var currentSession = TestDataBuilder.Sessions.BuildValid(userId);
        currentSession.Token = refreshToken;

        _mockSessionService.Setup(s => s.GetByRefreshToken(refreshToken))
            .ReturnsAsync(currentSession);

        _mockSessionService.Setup(s => s.RevokeAllSessionsExceptCurrent(userId, currentSession.Id))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.RevokeAllOtherSessions(userId, refreshToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockSessionService.Verify(s => s.RevokeAllSessionsExceptCurrent(userId, currentSession.Id), Times.Once);
    }

    [Fact]
    public async Task RevokeAllOtherSessions_CurrentSessionNotFound_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshToken = "invalid-refresh-token";

        _mockSessionService.Setup(s => s.GetByRefreshToken(refreshToken))
            .ReturnsAsync((Session?)null);

        // Act
        var result = await _sut.RevokeAllOtherSessions(userId, refreshToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Current session not found.");
    }

    #endregion
}
