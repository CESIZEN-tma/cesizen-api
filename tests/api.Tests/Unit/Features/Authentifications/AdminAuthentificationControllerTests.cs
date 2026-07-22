using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Simply.Auth.AspNetCore.Models;
using api.CZ.Core.ResultPattern;
using api.CZ.Features.Authentifications;
using api.CZ.Features.Authentifications.DTOs;
using api.CZ.Features.Authentifications.Services;

namespace api.Tests.Unit.Features.Authentifications;

public class AdminAuthentificationControllerTests
{
    private readonly Mock<IAdminAuthentificationService> _mockService;
    private readonly AdminAuthentificationController _controller;
    private readonly Guid _testAdminId;

    public AdminAuthentificationControllerTests()
    {
        _mockService = new Mock<IAdminAuthentificationService>();
        var mockLogger = new Mock<ILogger<AdminAuthentificationController>>();
        _controller = new AdminAuthentificationController(_mockService.Object, mockLogger.Object);
        _testAdminId = Guid.NewGuid();

        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, _testAdminId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    private void SetRefreshTokenCookie(string value)
    {
        _controller.ControllerContext.HttpContext.Request.Headers["Cookie"] = $"refresh_token={value}";
    }

    private static SimplyAuthResponse BuildTokens()
    {
        return new SimplyAuthResponse
        {
            AccessToken = "access-token",
            RefreshToken = "new-refresh-token",
            AccessTokenExpiration = DateTime.UtcNow.AddMinutes(15),
            RefreshTokenExpiration = DateTime.UtcNow.AddDays(15)
        };
    }

    [Fact]
    public async Task Register_ValidDto_ReturnsCreated()
    {
        // Arrange
        var dto = new RegisterDto { Email = "a@b.com", Password = "Password1!", ConfirmPassword = "Password1!", FirstName = "A", LastName = "B" };
        _mockService.Setup(s => s.RegisterAdmin(dto)).ReturnsAsync(Result.Success());

        // Act
        var result = await _controller.Register(dto);

        // Assert
        result.Should().BeOfType<CreatedResult>();
    }

    [Fact]
    public async Task Register_ServiceFails_ReturnsBadRequest()
    {
        // Arrange
        var dto = new RegisterDto { Email = "a@b.com", Password = "Password1!", ConfirmPassword = "Password1!", FirstName = "A", LastName = "B" };
        _mockService.Setup(s => s.RegisterAdmin(dto)).ReturnsAsync(Result.Failure("Email already used"));

        // Act
        var result = await _controller.Register(dto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ConfirmAccount_ValidToken_ReturnsOk()
    {
        // Arrange
        _mockService.Setup(s => s.ConfirmAccount("token123")).ReturnsAsync(Result.Success());

        // Act
        var result = await _controller.ConfirmAccount("token123");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Login_InvalidClientType_ThrowsException()
    {
        // Arrange
        var dto = new LoginDto { Email = "a@b.com", Password = "pwd" };
        _mockService.Setup(s => s.Login(dto)).ReturnsAsync(Result.Success(BuildTokens()));

        // Act
        var act = () => _controller.Login("desktop", dto);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Invalid client type");
    }

    [Fact]
    public async Task Login_WebClientSuccess_SetsCookieAndReturnsAccessTokenOnly()
    {
        // Arrange
        var dto = new LoginDto { Email = "a@b.com", Password = "pwd" };
        _mockService.Setup(s => s.Login(dto)).ReturnsAsync(Result.Success(BuildTokens()));

        // Act
        var result = await _controller.Login("web", dto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _controller.Response.Headers.SetCookie.ToString().Should().Contain("refresh_token");
    }

    [Fact]
    public async Task Login_ServiceFails_ReturnsUnauthorized()
    {
        // Arrange
        var dto = new LoginDto { Email = "a@b.com", Password = "wrong" };
        _mockService.Setup(s => s.Login(dto)).ReturnsAsync(Result.Failure<SimplyAuthResponse>("Invalid credentials"));

        // Act
        var result = await _controller.Login("web", dto);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task ForgotPassword_AnyEmail_ReturnsOk()
    {
        // Arrange
        var dto = new ForgotPasswordDto { Email = "a@b.com" };
        _mockService.Setup(s => s.ForgotPassword(dto)).ReturnsAsync(Result.Success());

        // Act
        var result = await _controller.ForgotPassword(dto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ResetPassword_ValidToken_ReturnsOk()
    {
        // Arrange
        var dto = new ResetPasswordDto { Token = "token", NewPassword = "NewPassword1!", ConfirmPassword = "NewPassword1!" };
        _mockService.Setup(s => s.ResetPassword(dto)).ReturnsAsync(Result.Success());

        // Act
        var result = await _controller.ResetPassword(dto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RefreshToken_NoCookiePresent_ReturnsUnauthorized()
    {
        // Act
        var result = await _controller.RefreshToken();

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
        _mockService.Verify(s => s.RefreshToken(It.IsAny<RefreshTokenDto>()), Times.Never);
    }

    [Fact]
    public async Task RefreshToken_ValidCookie_SetsNewCookieAndReturnsAccessToken()
    {
        // Arrange
        SetRefreshTokenCookie("existing-refresh-token");
        _mockService
            .Setup(s => s.RefreshToken(It.Is<RefreshTokenDto>(d => d.RefreshToken == "existing-refresh-token")))
            .ReturnsAsync(Result.Success(BuildTokens()));

        // Act
        var result = await _controller.RefreshToken();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _controller.Response.Headers.SetCookie.ToString().Should().Contain("refresh_token");
    }

    [Fact]
    public async Task RefreshToken_ServiceRejectsToken_ReturnsUnauthorized()
    {
        // Arrange
        SetRefreshTokenCookie("expired-token");
        _mockService
            .Setup(s => s.RefreshToken(It.IsAny<RefreshTokenDto>()))
            .ReturnsAsync(Result.Failure<SimplyAuthResponse>("Invalid refresh token"));

        // Act
        var result = await _controller.RefreshToken();

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Logout_NoCookiePresent_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.Logout();

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        _mockService.Verify(s => s.Logout(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Logout_ValidCookie_DeletesCookieAndReturnsOk()
    {
        // Arrange
        SetRefreshTokenCookie("token-to-revoke");
        _mockService.Setup(s => s.Logout("token-to-revoke")).ReturnsAsync(Result.Success());

        // Act
        var result = await _controller.Logout();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetActiveSessions_ValidAdmin_ReturnsOkWithSessions()
    {
        // Arrange
        var sessions = new List<SessionDto> { new() { Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddDays(1), IsCurrentSession = true } };
        _mockService.Setup(s => s.GetActiveSessions(_testAdminId, "refresh")).ReturnsAsync(Result.Success(sessions));

        // Act
        var result = await _controller.GetActiveSessions("refresh");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RevokeSession_ExistingSession_ReturnsOk()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        _mockService.Setup(s => s.RevokeSession(_testAdminId, sessionId)).ReturnsAsync(Result.Success());

        // Act
        var result = await _controller.RevokeSession(sessionId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RevokeSession_NonExistentSession_ReturnsNotFound()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        _mockService.Setup(s => s.RevokeSession(_testAdminId, sessionId)).ReturnsAsync(Result.Failure("Session not found"));

        // Act
        var result = await _controller.RevokeSession(sessionId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task RevokeAllOtherSessions_ValidRequest_ReturnsOk()
    {
        // Arrange
        _mockService.Setup(s => s.RevokeAllOtherSessions(_testAdminId, "refresh")).ReturnsAsync(Result.Success());

        // Act
        var result = await _controller.RevokeAllOtherSessions("refresh");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ChangePassword_ValidRequest_ReturnsOk()
    {
        // Arrange
        var dto = new ChangePasswordDto { CurrentPassword = "Old1234!", NewPassword = "New1234!", ConfirmPassword = "New1234!" };
        _mockService.Setup(s => s.ChangePassword(_testAdminId, dto, "refresh")).ReturnsAsync(Result.Success());

        // Act
        var result = await _controller.ChangePassword(dto, "refresh");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ChangePassword_WrongCurrentPassword_ReturnsBadRequest()
    {
        // Arrange
        var dto = new ChangePasswordDto { CurrentPassword = "Wrong", NewPassword = "New1234!", ConfirmPassword = "New1234!" };
        _mockService.Setup(s => s.ChangePassword(_testAdminId, dto, "refresh")).ReturnsAsync(Result.Failure("Current password is incorrect"));

        // Act
        var result = await _controller.ChangePassword(dto, "refresh");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }
}
