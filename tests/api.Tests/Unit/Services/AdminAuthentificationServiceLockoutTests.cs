using System.Linq.Expressions;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using api.CZ.Core.Services;
using api.CZ.Features.AdminEmailConfirmationTokens.Services;
using api.CZ.Features.AdminPasswordResetTokens.Services;
using api.CZ.Features.AdminSessions.Models;
using api.CZ.Features.AdminSessions.Services;
using api.CZ.Features.Administrators.Factories;
using api.CZ.Features.Administrators.Models;
using api.CZ.Features.Administrators.Repositories;
using api.CZ.Features.Authentifications.DTOs;
using api.CZ.Features.Authentifications.Services;
using api.Tests.Builders;
using Simply.Auth.Core.Abstractions;
using Simply.Auth.Core.Enums;
using Simply.Auth.Core.Models;

namespace api.Tests.Unit.Services;

public class AdminAuthentificationServiceLockoutTests
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

    public AdminAuthentificationServiceLockoutTests()
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

    [Fact]
    public async Task Login_LockedAccount_ReturnsFailureWithRemainingTime()
    {
        // Arrange
        var dto = new LoginDto
        {
            Email = "admin@example.com",
            Password = "Password123!"
        };

        var lockedUntil = DateTime.UtcNow.AddMinutes(10);
        var admin = TestDataBuilder.Administrators.BuildLocked(lockedUntil);
        admin.Email = dto.Email;

        _mockAdminRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Administrator, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);

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
            Email = "admin@example.com",
            Password = "Password123!"
        };

        var admin = TestDataBuilder.Administrators.Build(a =>
        {
            a.Email = dto.Email;
            a.AccountActivated = true;
            a.LockedUntil = DateTime.UtcNow.AddMinutes(-5); // Lockout expired
            a.FailedLoginAttempts = 5;
        });

        var tokens = new SimplyTokenPair(
            "access-token",
            "refresh-token",
            DateTime.UtcNow.AddHours(1),
            DateTime.UtcNow.AddDays(30));

        var session = TestDataBuilder.AdminSessions.Build(admin.Id);

        _mockAdminRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Administrator, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);

        _mockSimplyAuthService.Setup(s => s.VerifyPassword(dto.Password, admin.PasswordHash))
            .Returns(SimplyVerificationResult.Success);

        _mockSimplyAuthService.Setup(s => s.GenerateTokens(admin.Id.ToString(), It.IsAny<Claim[]>()))
            .Returns(tokens);

        _mockSessionService.Setup(s => s.CreateSession(
                admin.Id,
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
            Email = "admin@example.com",
            Password = "WrongPassword!"
        };

        var admin = TestDataBuilder.Administrators.Build(a =>
        {
            a.Email = dto.Email;
            a.AccountActivated = true;
            a.FailedLoginAttempts = 2;
        });

        _mockAdminRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Administrator, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);

        _mockSimplyAuthService.Setup(s => s.VerifyPassword(dto.Password, admin.PasswordHash))
            .Returns(SimplyVerificationResult.Failed);

        // Act
        var result = await _sut.Login(dto);

        // Assert
        result.IsFailure.Should().BeTrue();
        _mockAdminRepository.Verify(r => r.UpdateAsync(It.Is<Administrator>(a =>
            a.FailedLoginAttempts == 3), default), Times.Once);
    }

    [Fact]
    public async Task Login_FifthFailedAttempt_LocksAccount()
    {
        // Arrange
        var dto = new LoginDto
        {
            Email = "admin@example.com",
            Password = "WrongPassword!"
        };

        var admin = TestDataBuilder.Administrators.Build(a =>
        {
            a.Email = dto.Email;
            a.AccountActivated = true;
            a.FailedLoginAttempts = 4; // 5th attempt will lock
        });

        _mockAdminRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Administrator, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);

        _mockSimplyAuthService.Setup(s => s.VerifyPassword(dto.Password, admin.PasswordHash))
            .Returns(SimplyVerificationResult.Failed);

        // Act
        var result = await _sut.Login(dto);

        // Assert
        result.IsFailure.Should().BeTrue();
        _mockAdminRepository.Verify(r => r.UpdateAsync(It.Is<Administrator>(a =>
            a.FailedLoginAttempts == 5 &&
            a.LockedUntil.HasValue &&
            a.LockedUntil.Value > DateTime.UtcNow), default), Times.Once);
    }

    [Fact]
    public async Task Login_SuccessfulLogin_ResetsFailedAttempts()
    {
        // Arrange
        var dto = new LoginDto
        {
            Email = "admin@example.com",
            Password = "Password123!"
        };

        var admin = TestDataBuilder.Administrators.Build(a =>
        {
            a.Email = dto.Email;
            a.AccountActivated = true;
            a.FailedLoginAttempts = 3;
        });

        var tokens = new SimplyTokenPair(
            "access-token",
            "refresh-token",
            DateTime.UtcNow.AddHours(1),
            DateTime.UtcNow.AddDays(30));

        var session = TestDataBuilder.AdminSessions.Build(admin.Id);

        _mockAdminRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Administrator, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);

        _mockSimplyAuthService.Setup(s => s.VerifyPassword(dto.Password, admin.PasswordHash))
            .Returns(SimplyVerificationResult.Success);

        _mockSimplyAuthService.Setup(s => s.GenerateTokens(admin.Id.ToString(), It.IsAny<Claim[]>()))
            .Returns(tokens);

        _mockSessionService.Setup(s => s.CreateSession(
                admin.Id,
                tokens.RefreshToken,
                tokens.RefreshTokenExpiration))
            .ReturnsAsync(session);

        // Act
        var result = await _sut.Login(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockAdminRepository.Verify(r => r.UpdateAsync(It.Is<Administrator>(a =>
            a.FailedLoginAttempts == 0 &&
            a.LockedUntil == null), default), Times.Once);
    }

    [Fact]
    public async Task Login_SuccessfulLoginWithNoFailedAttempts_DoesNotUpdateAdmin()
    {
        // Arrange
        var dto = new LoginDto
        {
            Email = "admin@example.com",
            Password = "Password123!"
        };

        var admin = TestDataBuilder.Administrators.Build(a =>
        {
            a.Email = dto.Email;
            a.AccountActivated = true;
            a.FailedLoginAttempts = 0;
            a.LockedUntil = null;
        });

        var tokens = new SimplyTokenPair(
            "access-token",
            "refresh-token",
            DateTime.UtcNow.AddHours(1),
            DateTime.UtcNow.AddDays(30));

        var session = TestDataBuilder.AdminSessions.Build(admin.Id);

        _mockAdminRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Administrator, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);

        _mockSimplyAuthService.Setup(s => s.VerifyPassword(dto.Password, admin.PasswordHash))
            .Returns(SimplyVerificationResult.Success);

        _mockSimplyAuthService.Setup(s => s.GenerateTokens(admin.Id.ToString(), It.IsAny<Claim[]>()))
            .Returns(tokens);

        _mockSessionService.Setup(s => s.CreateSession(
                admin.Id,
                tokens.RefreshToken,
                tokens.RefreshTokenExpiration))
            .ReturnsAsync(session);

        // Act
        var result = await _sut.Login(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Should not call update for resetting failed attempts since they're already 0
        _mockAdminRepository.Verify(r => r.UpdateAsync(It.Is<Administrator>(a =>
            a.FailedLoginAttempts == 0), default), Times.Never);
    }
}
