using System.Linq.Expressions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
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
using Simply.Auth.Core.Models;

namespace api.Tests.Unit.Services;

public class AuthentificationServiceLockoutTests
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

    public AuthentificationServiceLockoutTests()
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
    public async Task Login_LockedAccount_ReturnsFailureWithRemainingTime()
    {
        // Arrange
        var dto = new LoginDto
        {
            Email = "user@example.com",
            Password = "Password123!"
        };

        var lockedUntil = DateTime.UtcNow.AddMinutes(10);
        var user = TestDataBuilder.Users.BuildLocked(lockedUntil);
        user.Email = dto.Email;

        _mockUserRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.Login(dto);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Account is locked");
        result.Error.Should().Contain("minute");
    }

    [Fact]
    public async Task Login_ExpiredLockout_AllowsLogin()
    {
        // Arrange
        var dto = new LoginDto
        {
            Email = "user@example.com",
            Password = "Password123!"
        };

        var user = TestDataBuilder.Users.Build(u =>
        {
            u.Email = dto.Email;
            u.AccountActivated = true;
            u.LockedUntil = DateTime.UtcNow.AddMinutes(-5); // Lockout expired
            u.FailedLoginAttempts = 5;
        });

        var tokens = new SimplyTokenPair(
            "access-token",
            "refresh-token",
            DateTime.UtcNow.AddHours(1),
            DateTime.UtcNow.AddDays(30));

        var session = TestDataBuilder.Sessions.Build(user.Id);

        _mockUserRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockSimplyAuthService.Setup(s => s.VerifyPassword(dto.Password, user.PasswordHash))
            .Returns(SimplyVerificationResult.Success);

        _mockSimplyAuthService.Setup(s => s.GenerateTokens(user.Id.ToString()))
            .Returns(tokens);

        _mockSessionService.Setup(s => s.CreateSession(
                user.Id,
                tokens.RefreshToken,
                tokens.RefreshTokenExpiration))
            .ReturnsAsync(session);

        // Act
        var result = await _sut.Login(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Login_FailedPassword_IncrementsFailedAttempts()
    {
        // Arrange
        var dto = new LoginDto
        {
            Email = "user@example.com",
            Password = "WrongPassword!"
        };

        var user = TestDataBuilder.Users.Build(u =>
        {
            u.Email = dto.Email;
            u.AccountActivated = true;
            u.FailedLoginAttempts = 2;
        });

        _mockUserRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockSimplyAuthService.Setup(s => s.VerifyPassword(dto.Password, user.PasswordHash))
            .Returns(SimplyVerificationResult.Failed);

        // Act
        var result = await _sut.Login(dto);

        // Assert
        result.IsFailure.Should().BeTrue();
        _mockUserRepository.Verify(r => r.UpdateAsync(It.Is<User>(u =>
            u.FailedLoginAttempts == 3), default), Times.Once);
    }

    [Fact]
    public async Task Login_FifthFailedAttempt_LocksAccount()
    {
        // Arrange
        var dto = new LoginDto
        {
            Email = "user@example.com",
            Password = "WrongPassword!"
        };

        var user = TestDataBuilder.Users.Build(u =>
        {
            u.Email = dto.Email;
            u.AccountActivated = true;
            u.FailedLoginAttempts = 4; // 5th attempt will lock
        });

        _mockUserRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockSimplyAuthService.Setup(s => s.VerifyPassword(dto.Password, user.PasswordHash))
            .Returns(SimplyVerificationResult.Failed);

        // Act
        var result = await _sut.Login(dto);

        // Assert
        result.IsFailure.Should().BeTrue();
        _mockUserRepository.Verify(r => r.UpdateAsync(It.Is<User>(u =>
            u.FailedLoginAttempts == 5 &&
            u.LockedUntil.HasValue &&
            u.LockedUntil.Value > DateTime.UtcNow), default), Times.Once);
    }

    [Fact]
    public async Task Login_SuccessfulLogin_ResetsFailedAttempts()
    {
        // Arrange
        var dto = new LoginDto
        {
            Email = "user@example.com",
            Password = "Password123!"
        };

        var user = TestDataBuilder.Users.Build(u =>
        {
            u.Email = dto.Email;
            u.AccountActivated = true;
            u.FailedLoginAttempts = 3;
        });

        var tokens = new SimplyTokenPair(
            "access-token",
            "refresh-token",
            DateTime.UtcNow.AddHours(1),
            DateTime.UtcNow.AddDays(30));

        var session = TestDataBuilder.Sessions.Build(user.Id);

        _mockUserRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockSimplyAuthService.Setup(s => s.VerifyPassword(dto.Password, user.PasswordHash))
            .Returns(SimplyVerificationResult.Success);

        _mockSimplyAuthService.Setup(s => s.GenerateTokens(user.Id.ToString()))
            .Returns(tokens);

        _mockSessionService.Setup(s => s.CreateSession(
                user.Id,
                tokens.RefreshToken,
                tokens.RefreshTokenExpiration))
            .ReturnsAsync(session);

        // Act
        var result = await _sut.Login(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockUserRepository.Verify(r => r.UpdateAsync(It.Is<User>(u =>
            u.FailedLoginAttempts == 0 &&
            u.LockedUntil == null), default), Times.Once);
    }

    [Fact]
    public async Task Login_SuccessfulLoginWithNoFailedAttempts_DoesNotUpdateUser()
    {
        // Arrange
        var dto = new LoginDto
        {
            Email = "user@example.com",
            Password = "Password123!"
        };

        var user = TestDataBuilder.Users.Build(u =>
        {
            u.Email = dto.Email;
            u.AccountActivated = true;
            u.FailedLoginAttempts = 0;
            u.LockedUntil = null;
        });

        var tokens = new SimplyTokenPair(
            "access-token",
            "refresh-token",
            DateTime.UtcNow.AddHours(1),
            DateTime.UtcNow.AddDays(30));

        var session = TestDataBuilder.Sessions.Build(user.Id);

        _mockUserRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockSimplyAuthService.Setup(s => s.VerifyPassword(dto.Password, user.PasswordHash))
            .Returns(SimplyVerificationResult.Success);

        _mockSimplyAuthService.Setup(s => s.GenerateTokens(user.Id.ToString()))
            .Returns(tokens);

        _mockSessionService.Setup(s => s.CreateSession(
                user.Id,
                tokens.RefreshToken,
                tokens.RefreshTokenExpiration))
            .ReturnsAsync(session);

        // Act
        var result = await _sut.Login(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Should not call update for resetting failed attempts since they're already 0
        _mockUserRepository.Verify(r => r.UpdateAsync(It.Is<User>(u =>
            u.FailedLoginAttempts == 0), default), Times.Never);
    }
}
