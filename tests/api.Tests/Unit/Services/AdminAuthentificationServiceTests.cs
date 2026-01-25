using System.Linq.Expressions;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using api.CZ.Core.ResultPattern;
using api.CZ.Core.Services;
using api.CZ.Features.AdminEmailConfirmationTokens.Models;
using api.CZ.Features.AdminEmailConfirmationTokens.Services;
using api.CZ.Features.AdminPasswordResetTokens.Models;
using api.CZ.Features.AdminPasswordResetTokens.Services;
using api.CZ.Features.AdminSessions.Models;
using api.CZ.Features.AdminSessions.Services;
using api.CZ.Features.Administrators.Factories;
using api.CZ.Features.Administrators.Models;
using api.CZ.Features.Administrators.Repositories;
using api.CZ.Features.Authentifications.DTOs;
using api.CZ.Features.Authentifications.Services;
using api.Tests.Builders;
using Simply.Auth.AspNetCore.Models;
using Simply.Auth.Core.Abstractions;
using Simply.Auth.Core.Enums;
using Simply.Auth.Core.Models;

namespace api.Tests.Unit.Services;

public class AdminAuthentificationServiceTests
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

    public AdminAuthentificationServiceTests()
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

    #region RegisterAdmin Tests

    [Fact]
    public async Task RegisterAdmin_ValidDto_ReturnsSuccess()
    {
        // Arrange
        var dto = new RegisterDto
        {
            Email = "admin@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = "John",
            LastName = "Doe"
        };

        var newAdmin = TestDataBuilder.Administrators.Build();
        var confirmationToken = TestDataBuilder.AdminEmailConfirmationTokens.Build(newAdmin.Id);

        _mockAdminRepository.Setup(r => r.AnyAsync(
                It.IsAny<Expression<Func<Administrator, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockSimplyAuthService.Setup(s => s.HashPassword(dto.Password))
            .Returns("hashed-password");

        _mockAdminFactory.Setup(f => f.Create(It.IsAny<object[]>()))
            .Returns(newAdmin);

        _mockAdminRepository.Setup(r => r.AddAsync(It.IsAny<Administrator>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newAdmin);

        _mockEmailConfirmationTokenService.Setup(s => s.NewToken(It.IsAny<Guid>()))
            .ReturnsAsync(confirmationToken);

        _mockEmailService.Setup(e => e.SendRegisteringConfirmationEmail(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<TimeSpan?>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _sut.RegisterAdmin(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockAdminRepository.Verify(r => r.AddAsync(It.IsAny<Administrator>(), default), Times.Once);
        _mockEmailConfirmationTokenService.Verify(s => s.NewToken(It.IsAny<Guid>()), Times.Once);
        _mockEmailService.Verify(e => e.SendRegisteringConfirmationEmail(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<TimeSpan?>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAdmin_PasswordMismatch_ReturnsFailure()
    {
        // Arrange
        var dto = new RegisterDto
        {
            Email = "admin@example.com",
            Password = "Password123!",
            ConfirmPassword = "DifferentPassword!",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = await _sut.RegisterAdmin(dto);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Password must be identical.");
        _mockAdminRepository.Verify(r => r.AddAsync(It.IsAny<Administrator>(), default), Times.Never);
    }

    [Fact]
    public async Task RegisterAdmin_EmailAlreadyExists_ReturnsFailure()
    {
        // Arrange
        var dto = new RegisterDto
        {
            Email = "existing@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = "John",
            LastName = "Doe"
        };

        _mockAdminRepository.Setup(r => r.AnyAsync(
                It.IsAny<Expression<Func<Administrator, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.RegisterAdmin(dto);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Email already exists");
        _mockAdminRepository.Verify(r => r.AddAsync(It.IsAny<Administrator>(), default), Times.Never);
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_ValidCredentials_ReturnsSuccessWithTokens()
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
        result.Value.Should().NotBeNull();
        result.Value!.AccessToken.Should().Be(tokens.AccessToken);
        result.Value.RefreshToken.Should().Be(tokens.RefreshToken);
        _mockSessionService.Verify(s => s.CreateSession(
            admin.Id,
            tokens.RefreshToken,
            tokens.RefreshTokenExpiration), Times.Once);
    }

    [Fact]
    public async Task Login_AdminNotFound_ReturnsFailure()
    {
        // Arrange
        var dto = new LoginDto
        {
            Email = "nonexistent@example.com",
            Password = "Password123!"
        };

        _mockAdminRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Administrator, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Administrator?)null);

        // Act
        var result = await _sut.Login(dto);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Invalid credentials");
    }

    [Fact]
    public async Task Login_AccountNotActivated_ReturnsFailure()
    {
        // Arrange
        var dto = new LoginDto
        {
            Email = "admin@example.com",
            Password = "Password123!"
        };

        var admin = TestDataBuilder.Administrators.BuildUnactivated();
        admin.Email = dto.Email;

        _mockAdminRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Administrator, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);

        // Act
        var result = await _sut.Login(dto);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Le compte doit être activé.");
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsFailure()
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
        result.Error.Should().Be("Invalid credentials");
    }

    [Fact]
    public async Task Login_PasswordNeedsRehash_RehashesAndReturnsSuccess()
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
            .Returns(SimplyVerificationResult.SuccessRehashNeeded);

        _mockSimplyAuthService.Setup(s => s.HashPassword(dto.Password))
            .Returns("new-hashed-password");

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
            a.PasswordHash == "new-hashed-password"), default), Times.Once);
    }

    #endregion

    #region ConfirmAccount Tests

    [Fact]
    public async Task ConfirmAccount_ValidToken_ReturnsSuccess()
    {
        // Arrange
        var tokenString = "valid-confirmation-token";
        var admin = TestDataBuilder.Administrators.BuildUnactivated();
        var confirmationToken = TestDataBuilder.AdminEmailConfirmationTokens.BuildValid(admin.Id);
        confirmationToken.Token = tokenString;

        _mockEmailConfirmationTokenService.Setup(s => s.GetEntityByToken(tokenString))
            .ReturnsAsync(confirmationToken);

        _mockAdminRepository.Setup(r => r.FindAsync(confirmationToken.IdAdministrators, It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);

        _mockAdminRepository.Setup(r => r.UpdateAsync(It.IsAny<Administrator>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockEmailConfirmationTokenService.Setup(s => s.Consume(tokenString))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.ConfirmAccount(tokenString);

        // Assert
        result.IsSuccess.Should().BeTrue();
        admin.AccountActivated.Should().BeTrue();
        _mockAdminRepository.Verify(r => r.UpdateAsync(admin, default), Times.Once);
        _mockEmailConfirmationTokenService.Verify(s => s.Consume(tokenString), Times.Once);
    }

    [Fact]
    public async Task ConfirmAccount_InvalidToken_ReturnsFailure()
    {
        // Arrange
        var tokenString = "invalid-token";

        _mockEmailConfirmationTokenService.Setup(s => s.GetEntityByToken(tokenString))
            .ReturnsAsync((AdminEmailConfirmationToken?)null);

        // Act
        var result = await _sut.ConfirmAccount(tokenString);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Invalid token.");
    }

    [Fact]
    public async Task ConfirmAccount_ConsumedToken_ReturnsFailure()
    {
        // Arrange
        var tokenString = "consumed-token";
        var confirmationToken = TestDataBuilder.AdminEmailConfirmationTokens.BuildConsumed();
        confirmationToken.Token = tokenString;

        _mockEmailConfirmationTokenService.Setup(s => s.GetEntityByToken(tokenString))
            .ReturnsAsync(confirmationToken);

        // Act
        var result = await _sut.ConfirmAccount(tokenString);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Token already used.");
    }

    [Fact]
    public async Task ConfirmAccount_ExpiredToken_ReturnsFailure()
    {
        // Arrange
        var tokenString = "expired-token";
        var confirmationToken = TestDataBuilder.AdminEmailConfirmationTokens.BuildExpired();
        confirmationToken.Token = tokenString;
        confirmationToken.Consumed = false;

        _mockEmailConfirmationTokenService.Setup(s => s.GetEntityByToken(tokenString))
            .ReturnsAsync(confirmationToken);

        // Act
        var result = await _sut.ConfirmAccount(tokenString);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Token expired.");
    }

    [Fact]
    public async Task ConfirmAccount_AdminNotFound_ReturnsFailure()
    {
        // Arrange
        var tokenString = "valid-token";
        var confirmationToken = TestDataBuilder.AdminEmailConfirmationTokens.BuildValid();
        confirmationToken.Token = tokenString;

        _mockEmailConfirmationTokenService.Setup(s => s.GetEntityByToken(tokenString))
            .ReturnsAsync(confirmationToken);

        _mockAdminRepository.Setup(r => r.FindAsync(confirmationToken.IdAdministrators, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Administrator?)null);

        // Act
        var result = await _sut.ConfirmAccount(tokenString);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Administrator not found.");
    }

    [Fact]
    public async Task ConfirmAccount_AlreadyActivatedAccount_ReturnsSuccessAndConsumesToken()
    {
        // Arrange
        var tokenString = "valid-token";
        var admin = TestDataBuilder.Administrators.Build();
        admin.AccountActivated = true;

        var confirmationToken = TestDataBuilder.AdminEmailConfirmationTokens.BuildValid(admin.Id);
        confirmationToken.Token = tokenString;

        _mockEmailConfirmationTokenService.Setup(s => s.GetEntityByToken(tokenString))
            .ReturnsAsync(confirmationToken);

        _mockAdminRepository.Setup(r => r.FindAsync(confirmationToken.IdAdministrators, It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);

        _mockEmailConfirmationTokenService.Setup(s => s.Consume(tokenString))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.ConfirmAccount(tokenString);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockAdminRepository.Verify(r => r.UpdateAsync(It.IsAny<Administrator>(), default), Times.Never);
        _mockEmailConfirmationTokenService.Verify(s => s.Consume(tokenString), Times.Once);
    }

    #endregion

    #region ForgotPassword Tests

    [Fact]
    public async Task ForgotPassword_ValidAdmin_SendsResetEmail()
    {
        // Arrange
        var dto = new ForgotPasswordDto { Email = "admin@example.com" };
        var admin = TestDataBuilder.Administrators.Build(a =>
        {
            a.Email = dto.Email;
            a.AccountActivated = true;
        });

        var resetToken = TestDataBuilder.AdminPasswordResetTokens.Build(admin.Id);

        _mockAdminRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Administrator, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);

        _mockPasswordResetTokenService.Setup(s => s.NewToken(admin.Id))
            .ReturnsAsync(resetToken);

        _mockEmailService.Setup(e => e.SendPasswordResetEmail(
                resetToken.Token,
                admin.FirstName,
                admin.LastName,
                admin.Email,
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<TimeSpan?>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _sut.ForgotPassword(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockPasswordResetTokenService.Verify(s => s.NewToken(admin.Id), Times.Once);
        _mockEmailService.Verify(e => e.SendPasswordResetEmail(
            resetToken.Token,
            admin.FirstName,
            admin.LastName,
            admin.Email,
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<TimeSpan?>()), Times.Once);
    }

    [Fact]
    public async Task ForgotPassword_AdminNotFound_ReturnsSuccessWithoutSendingEmail()
    {
        // Arrange
        var dto = new ForgotPasswordDto { Email = "nonexistent@example.com" };

        _mockAdminRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Administrator, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Administrator?)null);

        // Act
        var result = await _sut.ForgotPassword(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockPasswordResetTokenService.Verify(s => s.NewToken(It.IsAny<Guid>()), Times.Never);
        _mockEmailService.Verify(e => e.SendPasswordResetEmail(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<TimeSpan?>()), Times.Never);
    }

    [Fact]
    public async Task ForgotPassword_AccountNotActivated_ReturnsSuccessWithoutSendingEmail()
    {
        // Arrange
        var dto = new ForgotPasswordDto { Email = "admin@example.com" };
        var admin = TestDataBuilder.Administrators.BuildUnactivated();
        admin.Email = dto.Email;

        _mockAdminRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Administrator, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);

        // Act
        var result = await _sut.ForgotPassword(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockPasswordResetTokenService.Verify(s => s.NewToken(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task ForgotPassword_EmailSendFails_ReturnsFailure()
    {
        // Arrange
        var dto = new ForgotPasswordDto { Email = "admin@example.com" };
        var admin = TestDataBuilder.Administrators.Build(a =>
        {
            a.Email = dto.Email;
            a.AccountActivated = true;
        });

        var resetToken = TestDataBuilder.AdminPasswordResetTokens.Build(admin.Id);

        _mockAdminRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Administrator, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);

        _mockPasswordResetTokenService.Setup(s => s.NewToken(admin.Id))
            .ReturnsAsync(resetToken);

        _mockEmailService.Setup(e => e.SendPasswordResetEmail(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<TimeSpan?>()))
            .ReturnsAsync(Result.Failure("Email service error"));

        // Act
        var result = await _sut.ForgotPassword(dto);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Failed to send reset email. Please try again later.");
    }

    #endregion

    #region ResetPassword Tests

    [Fact]
    public async Task ResetPassword_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var dto = new ResetPasswordDto
        {
            Token = "valid-reset-token",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };

        var admin = TestDataBuilder.Administrators.Build();
        var resetToken = TestDataBuilder.AdminPasswordResetTokens.BuildValid(admin.Id);
        resetToken.Token = dto.Token;

        _mockPasswordResetTokenService.Setup(s => s.GetEntityByToken(dto.Token))
            .ReturnsAsync(resetToken);

        _mockAdminRepository.Setup(r => r.FindAsync(resetToken.IdAdministrators, It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);

        _mockSimplyAuthService.Setup(s => s.HashPassword(dto.NewPassword))
            .Returns("new-hashed-password");

        _mockAdminRepository.Setup(r => r.UpdateAsync(It.IsAny<Administrator>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockPasswordResetTokenService.Setup(s => s.Consume(dto.Token))
            .ReturnsAsync(true);

        _mockEmailService.Setup(e => e.SendPasswordResetConfirmationEmail(
                admin.FirstName,
                admin.LastName,
                admin.Email,
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _sut.ResetPassword(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockAdminRepository.Verify(r => r.UpdateAsync(It.Is<Administrator>(a =>
            a.PasswordHash == "new-hashed-password"), default), Times.Once);
        _mockPasswordResetTokenService.Verify(s => s.Consume(dto.Token), Times.Once);
        _mockEmailService.Verify(e => e.SendPasswordResetConfirmationEmail(
            admin.FirstName,
            admin.LastName,
            admin.Email,
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ResetPassword_PasswordMismatch_ReturnsFailure()
    {
        // Arrange
        var dto = new ResetPasswordDto
        {
            Token = "valid-reset-token",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "DifferentPassword!"
        };

        // Act
        var result = await _sut.ResetPassword(dto);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Passwords must match.");
    }

    [Fact]
    public async Task ResetPassword_InvalidToken_ReturnsFailure()
    {
        // Arrange
        var dto = new ResetPasswordDto
        {
            Token = "invalid-token",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };

        _mockPasswordResetTokenService.Setup(s => s.GetEntityByToken(dto.Token))
            .ReturnsAsync((AdminPasswordResetToken?)null);

        // Act
        var result = await _sut.ResetPassword(dto);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Invalid or expired reset token.");
    }

    [Fact]
    public async Task ResetPassword_ConsumedToken_ReturnsFailure()
    {
        // Arrange
        var dto = new ResetPasswordDto
        {
            Token = "consumed-token",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };

        var resetToken = TestDataBuilder.AdminPasswordResetTokens.BuildConsumed();
        resetToken.Token = dto.Token;

        _mockPasswordResetTokenService.Setup(s => s.GetEntityByToken(dto.Token))
            .ReturnsAsync(resetToken);

        // Act
        var result = await _sut.ResetPassword(dto);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("This reset link has already been used.");
    }

    [Fact]
    public async Task ResetPassword_ExpiredToken_ReturnsFailure()
    {
        // Arrange
        var dto = new ResetPasswordDto
        {
            Token = "expired-token",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };

        var resetToken = TestDataBuilder.AdminPasswordResetTokens.BuildExpired();
        resetToken.Token = dto.Token;
        resetToken.Consumed = false;

        _mockPasswordResetTokenService.Setup(s => s.GetEntityByToken(dto.Token))
            .ReturnsAsync(resetToken);

        // Act
        var result = await _sut.ResetPassword(dto);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("This reset link has expired. Please request a new one.");
    }

    [Fact]
    public async Task ResetPassword_AdminNotFound_ReturnsFailure()
    {
        // Arrange
        var dto = new ResetPasswordDto
        {
            Token = "valid-token",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };

        var resetToken = TestDataBuilder.AdminPasswordResetTokens.BuildValid();
        resetToken.Token = dto.Token;

        _mockPasswordResetTokenService.Setup(s => s.GetEntityByToken(dto.Token))
            .ReturnsAsync(resetToken);

        _mockAdminRepository.Setup(r => r.FindAsync(resetToken.IdAdministrators, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Administrator?)null);

        // Act
        var result = await _sut.ResetPassword(dto);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Administrator not found.");
    }

    #endregion

    #region RefreshToken Tests

    [Fact]
    public async Task RefreshToken_ValidRefreshToken_ReturnsNewTokens()
    {
        // Arrange
        var dto = new RefreshTokenDto { RefreshToken = "valid-refresh-token" };
        var admin = TestDataBuilder.Administrators.Build(a => a.AccountActivated = true);
        var session = TestDataBuilder.AdminSessions.BuildValid(admin.Id);
        session.Token = dto.RefreshToken;

        var newTokens = new SimplyTokenPair(
            "new-access-token",
            "new-refresh-token",
            DateTime.UtcNow.AddHours(1),
            DateTime.UtcNow.AddDays(30));

        var newSession = TestDataBuilder.AdminSessions.Build(admin.Id);

        _mockSessionService.Setup(s => s.GetByRefreshToken(dto.RefreshToken))
            .ReturnsAsync(session);

        _mockAdminRepository.Setup(r => r.FindAsync(session.IdAdministrators, It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);

        _mockSessionService.Setup(s => s.ConsumeSession(dto.RefreshToken))
            .ReturnsAsync(true);

        _mockSimplyAuthService.Setup(s => s.GenerateTokens(admin.Id.ToString(), It.IsAny<Claim[]>()))
            .Returns(newTokens);

        _mockSessionService.Setup(s => s.CreateSession(
                admin.Id,
                newTokens.RefreshToken,
                newTokens.RefreshTokenExpiration))
            .ReturnsAsync(newSession);

        // Act
        var result = await _sut.RefreshToken(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.AccessToken.Should().Be(newTokens.AccessToken);
        result.Value.RefreshToken.Should().Be(newTokens.RefreshToken);
        _mockSessionService.Verify(s => s.ConsumeSession(dto.RefreshToken), Times.Once);
        _mockSessionService.Verify(s => s.CreateSession(
            admin.Id,
            newTokens.RefreshToken,
            newTokens.RefreshTokenExpiration), Times.Once);
    }

    [Fact]
    public async Task RefreshToken_InvalidSession_ReturnsFailure()
    {
        // Arrange
        var dto = new RefreshTokenDto { RefreshToken = "invalid-refresh-token" };

        _mockSessionService.Setup(s => s.GetByRefreshToken(dto.RefreshToken))
            .ReturnsAsync((AdminSession?)null);

        // Act
        var result = await _sut.RefreshToken(dto);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Invalid or expired refresh token.");
    }

    [Fact]
    public async Task RefreshToken_AdminNotFound_ReturnsFailure()
    {
        // Arrange
        var dto = new RefreshTokenDto { RefreshToken = "valid-refresh-token" };
        var session = TestDataBuilder.AdminSessions.BuildValid();
        session.Token = dto.RefreshToken;

        _mockSessionService.Setup(s => s.GetByRefreshToken(dto.RefreshToken))
            .ReturnsAsync(session);

        _mockAdminRepository.Setup(r => r.FindAsync(session.IdAdministrators, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Administrator?)null);

        // Act
        var result = await _sut.RefreshToken(dto);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Administrator not found.");
    }

    [Fact]
    public async Task RefreshToken_AccountNotActivated_ReturnsFailure()
    {
        // Arrange
        var dto = new RefreshTokenDto { RefreshToken = "valid-refresh-token" };
        var admin = TestDataBuilder.Administrators.BuildUnactivated();
        var session = TestDataBuilder.AdminSessions.BuildValid(admin.Id);
        session.Token = dto.RefreshToken;

        _mockSessionService.Setup(s => s.GetByRefreshToken(dto.RefreshToken))
            .ReturnsAsync(session);

        _mockAdminRepository.Setup(r => r.FindAsync(session.IdAdministrators, It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);

        // Act
        var result = await _sut.RefreshToken(dto);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Account is not activated.");
    }

    #endregion

    #region Logout Tests

    [Fact]
    public async Task Logout_ValidRefreshToken_ReturnsSuccess()
    {
        // Arrange
        var refreshToken = "valid-refresh-token";

        _mockSessionService.Setup(s => s.ConsumeSession(refreshToken))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.Logout(refreshToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockSessionService.Verify(s => s.ConsumeSession(refreshToken), Times.Once);
    }

    [Fact]
    public async Task Logout_InvalidRefreshToken_ReturnsSuccess()
    {
        // Arrange
        var refreshToken = "invalid-refresh-token";

        _mockSessionService.Setup(s => s.ConsumeSession(refreshToken))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.Logout(refreshToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockSessionService.Verify(s => s.ConsumeSession(refreshToken), Times.Once);
    }

    [Fact]
    public async Task Logout_AlreadyConsumedToken_ReturnsSuccess()
    {
        // Arrange
        var refreshToken = "consumed-refresh-token";

        _mockSessionService.Setup(s => s.ConsumeSession(refreshToken))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.Logout(refreshToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    #endregion
}
