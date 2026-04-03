using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using api.CZ.Core.ResultPattern;
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

namespace api.Tests.Unit.Services;

public class AdminAuthentificationServiceChangePasswordTests
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

    public AdminAuthentificationServiceChangePasswordTests()
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
    public async Task ChangePassword_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var admin = TestDataBuilder.Administrators.Build();
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };
        var refreshToken = "current-refresh-token";
        var currentSession = TestDataBuilder.AdminSessions.BuildValid(admin.Id);
        currentSession.Token = refreshToken;

        _mockAdminRepository.Setup(r => r.FindAsync(admin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);

        _mockSimplyAuthService.Setup(s => s.VerifyPassword(dto.CurrentPassword, admin.PasswordHash))
            .Returns(SimplyVerificationResult.Success);

        _mockSimplyAuthService.Setup(s => s.HashPassword(dto.NewPassword))
            .Returns("new-hashed-password");

        _mockSessionService.Setup(s => s.GetByRefreshToken(refreshToken))
            .ReturnsAsync(currentSession);

        _mockSessionService.Setup(s => s.RevokeAllSessionsExceptCurrent(admin.Id, currentSession.Id))
            .ReturnsAsync(true);

        _mockEmailService.Setup(e => e.SendPasswordResetConfirmationEmail(
                admin.FirstName,
                admin.LastName,
                admin.Email,
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _sut.ChangePassword(admin.Id, dto, refreshToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockAdminRepository.Verify(r => r.UpdateAsync(It.Is<Administrator>(a =>
            a.PasswordHash == "new-hashed-password"), default), Times.Once);
        _mockSessionService.Verify(s => s.RevokeAllSessionsExceptCurrent(admin.Id, currentSession.Id), Times.Once);
        _mockEmailService.Verify(e => e.SendPasswordResetConfirmationEmail(
            admin.FirstName,
            admin.LastName,
            admin.Email,
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ChangePassword_PasswordMismatch_ReturnsFailure()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "DifferentPassword!"
        };
        var refreshToken = "current-refresh-token";

        // Act
        var result = await _sut.ChangePassword(adminId, dto, refreshToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("New passwords must match.");
        _mockAdminRepository.Verify(r => r.FindAsync(It.IsAny<Guid>(), default), Times.Never);
    }

    [Fact]
    public async Task ChangePassword_AdminNotFound_ReturnsFailure()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };
        var refreshToken = "current-refresh-token";

        _mockAdminRepository.Setup(r => r.FindAsync(adminId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Administrator?)null);

        // Act
        var result = await _sut.ChangePassword(adminId, dto, refreshToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Administrator not found.");
    }

    [Fact]
    public async Task ChangePassword_IncorrectCurrentPassword_ReturnsFailure()
    {
        // Arrange
        var admin = TestDataBuilder.Administrators.Build();
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "WrongPassword!",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };
        var refreshToken = "current-refresh-token";

        _mockAdminRepository.Setup(r => r.FindAsync(admin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);

        _mockSimplyAuthService.Setup(s => s.VerifyPassword(dto.CurrentPassword, admin.PasswordHash))
            .Returns(SimplyVerificationResult.Failed);

        // Act
        var result = await _sut.ChangePassword(admin.Id, dto, refreshToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Current password is incorrect.");
        _mockAdminRepository.Verify(r => r.UpdateAsync(It.IsAny<Administrator>(), default), Times.Never);
    }

    [Fact]
    public async Task ChangePassword_NoCurrentSession_StillChangesPasswordWithoutRevokingOthers()
    {
        // Arrange
        var admin = TestDataBuilder.Administrators.Build();
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };
        var refreshToken = "invalid-refresh-token";

        _mockAdminRepository.Setup(r => r.FindAsync(admin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);

        _mockSimplyAuthService.Setup(s => s.VerifyPassword(dto.CurrentPassword, admin.PasswordHash))
            .Returns(SimplyVerificationResult.Success);

        _mockSimplyAuthService.Setup(s => s.HashPassword(dto.NewPassword))
            .Returns("new-hashed-password");

        _mockSessionService.Setup(s => s.GetByRefreshToken(refreshToken))
            .ReturnsAsync((AdminSession?)null);

        _mockEmailService.Setup(e => e.SendPasswordResetConfirmationEmail(
                admin.FirstName,
                admin.LastName,
                admin.Email,
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _sut.ChangePassword(admin.Id, dto, refreshToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockAdminRepository.Verify(r => r.UpdateAsync(It.Is<Administrator>(a =>
            a.PasswordHash == "new-hashed-password"), default), Times.Once);
        _mockSessionService.Verify(s => s.RevokeAllSessionsExceptCurrent(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }
}
