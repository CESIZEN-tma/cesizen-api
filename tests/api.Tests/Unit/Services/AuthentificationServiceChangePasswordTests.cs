using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using api.CZ.Core.ResultPattern;
using api.CZ.Core.Services;
using api.CZ.Features.Authentifications.DTOs;
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
using Simply.Auth.Core.Enums;

namespace api.Tests.Unit.Services;

public class AuthentificationServiceChangePasswordTests
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

    public AuthentificationServiceChangePasswordTests()
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

    [Fact]
    public async Task ChangePassword_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var user = TestDataBuilder.Users.Build();
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };
        var refreshToken = "current-refresh-token";
        var currentSession = TestDataBuilder.Sessions.BuildValid(user.Id);
        currentSession.Token = refreshToken;

        _mockUserRepository.Setup(r => r.FindAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockSimplyAuthService.Setup(s => s.VerifyPassword(dto.CurrentPassword, user.PasswordHash))
            .Returns(SimplyVerificationResult.Success);

        _mockSimplyAuthService.Setup(s => s.HashPassword(dto.NewPassword))
            .Returns("new-hashed-password");

        _mockSessionService.Setup(s => s.GetByRefreshToken(refreshToken))
            .ReturnsAsync(currentSession);

        _mockSessionService.Setup(s => s.RevokeAllSessionsExceptCurrent(user.Id, currentSession.Id))
            .ReturnsAsync(true);

        _mockEmailService.Setup(e => e.SendPasswordResetConfirmationEmail(
                user.FirstName,
                user.LastName,
                user.Email,
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _sut.ChangePassword(user.Id, dto, refreshToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockUserRepository.Verify(r => r.UpdateAsync(It.Is<User>(u =>
            u.PasswordHash == "new-hashed-password"), default), Times.Once);
        _mockSessionService.Verify(s => s.RevokeAllSessionsExceptCurrent(user.Id, currentSession.Id), Times.Once);
        _mockEmailService.Verify(e => e.SendPasswordResetConfirmationEmail(
            user.FirstName,
            user.LastName,
            user.Email,
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ChangePassword_PasswordMismatch_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "DifferentPassword!"
        };
        var refreshToken = "current-refresh-token";

        // Act
        var result = await _sut.ChangePassword(userId, dto, refreshToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("New passwords must match.");
        _mockUserRepository.Verify(r => r.FindAsync(It.IsAny<Guid>(), default), Times.Never);
    }

    [Fact]
    public async Task ChangePassword_UserNotFound_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };
        var refreshToken = "current-refresh-token";

        _mockUserRepository.Setup(r => r.FindAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.ChangePassword(userId, dto, refreshToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("User not found.");
    }

    [Fact]
    public async Task ChangePassword_IncorrectCurrentPassword_ReturnsFailure()
    {
        // Arrange
        var user = TestDataBuilder.Users.Build();
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "WrongPassword!",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };
        var refreshToken = "current-refresh-token";

        _mockUserRepository.Setup(r => r.FindAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockSimplyAuthService.Setup(s => s.VerifyPassword(dto.CurrentPassword, user.PasswordHash))
            .Returns(SimplyVerificationResult.Failed);

        // Act
        var result = await _sut.ChangePassword(user.Id, dto, refreshToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Current password is incorrect.");
        _mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<User>(), default), Times.Never);
    }

    [Fact]
    public async Task ChangePassword_NoCurrentSession_StillChangesPasswordWithoutRevokingOthers()
    {
        // Arrange
        var user = TestDataBuilder.Users.Build();
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };
        var refreshToken = "invalid-refresh-token";

        _mockUserRepository.Setup(r => r.FindAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockSimplyAuthService.Setup(s => s.VerifyPassword(dto.CurrentPassword, user.PasswordHash))
            .Returns(SimplyVerificationResult.Success);

        _mockSimplyAuthService.Setup(s => s.HashPassword(dto.NewPassword))
            .Returns("new-hashed-password");

        _mockSessionService.Setup(s => s.GetByRefreshToken(refreshToken))
            .ReturnsAsync((Session?)null);

        _mockEmailService.Setup(e => e.SendPasswordResetConfirmationEmail(
                user.FirstName,
                user.LastName,
                user.Email,
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _sut.ChangePassword(user.Id, dto, refreshToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockUserRepository.Verify(r => r.UpdateAsync(It.Is<User>(u =>
            u.PasswordHash == "new-hashed-password"), default), Times.Once);
        _mockSessionService.Verify(s => s.RevokeAllSessionsExceptCurrent(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }
}
