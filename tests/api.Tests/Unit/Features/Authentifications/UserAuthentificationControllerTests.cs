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

public class UserAuthentificationControllerTests
{
    private readonly Mock<IAuthentificationService> _mockService;
    private readonly UserAuthentificationController _controller;
    private readonly Guid _testUserId;

    public UserAuthentificationControllerTests()
    {
        _mockService = new Mock<IAuthentificationService>();
        var mockLogger = new Mock<ILogger<UserAuthentificationController>>();
        _controller = new UserAuthentificationController(_mockService.Object, mockLogger.Object);
        _testUserId = Guid.NewGuid();

        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, _testUserId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    private static SimplyAuthResponse BuildTokens()
    {
        return new SimplyAuthResponse
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token",
            AccessTokenExpiration = DateTime.UtcNow.AddMinutes(15),
            RefreshTokenExpiration = DateTime.UtcNow.AddDays(15)
        };
    }

    [Fact]
    public async Task Register_ValidDto_ReturnsCreated()
    {
        // Arrange
        var dto = new RegisterDto { Email = "a@b.com", Password = "Password1!", ConfirmPassword = "Password1!", FirstName = "A", LastName = "B" };
        _mockService.Setup(s => s.RegisterUser(dto)).ReturnsAsync(Result.Success());

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
        _mockService.Setup(s => s.RegisterUser(dto)).ReturnsAsync(Result.Failure("Email already used"));

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
    public async Task ConfirmAccount_InvalidToken_ReturnsBadRequest()
    {
        // Arrange
        _mockService.Setup(s => s.ConfirmAccount("bad-token")).ReturnsAsync(Result.Failure("Invalid token"));

        // Act
        var result = await _controller.ConfirmAccount("bad-token");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Login_InvalidClientType_ThrowsException()
    {
        // Arrange
        var dto = new LoginDto { Email = "a@b.com", Password = "pwd" };

        // Act
        var act = () => _controller.Login("desktop", dto);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Invalid client type");
        _mockService.Verify(s => s.Login(It.IsAny<LoginDto>()), Times.Never);
    }

    [Fact]
    public async Task Login_WebClientSuccess_SetsCookieAndReturnsAccessTokenOnly()
    {
        // Arrange
        var dto = new LoginDto { Email = "a@b.com", Password = "pwd" };
        var tokens = BuildTokens();
        _mockService.Setup(s => s.Login(dto)).ReturnsAsync(Result.Success(tokens));

        // Act
        var result = await _controller.Login("web", dto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _controller.Response.Headers.SetCookie.ToString().Should().Contain("refresh_token");
    }

    [Fact]
    public async Task Login_MobileClientSuccess_ReturnsBothTokensInBody()
    {
        // Arrange
        var dto = new LoginDto { Email = "a@b.com", Password = "pwd" };
        var tokens = BuildTokens();
        _mockService.Setup(s => s.Login(dto)).ReturnsAsync(Result.Success(tokens));

        // Act
        var result = await _controller.Login("mobile", dto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _controller.Response.Headers.SetCookie.ToString().Should().BeEmpty();
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
    public async Task ResetPassword_InvalidToken_ReturnsBadRequest()
    {
        // Arrange
        var dto = new ResetPasswordDto { Token = "bad", NewPassword = "NewPassword1!", ConfirmPassword = "NewPassword1!" };
        _mockService.Setup(s => s.ResetPassword(dto)).ReturnsAsync(Result.Failure("Invalid or expired token"));

        // Act
        var result = await _controller.ResetPassword(dto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task RefreshToken_ValidToken_ReturnsOkWithTokens()
    {
        // Arrange
        var dto = new RefreshTokenDto { RefreshToken = "valid-refresh" };
        var tokens = BuildTokens();
        _mockService.Setup(s => s.RefreshToken(dto)).ReturnsAsync(Result.Success(tokens));

        // Act
        var result = await _controller.RefreshToken(dto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RefreshToken_InvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        var dto = new RefreshTokenDto { RefreshToken = "invalid" };
        _mockService.Setup(s => s.RefreshToken(dto)).ReturnsAsync(Result.Failure<SimplyAuthResponse>("Invalid refresh token"));

        // Act
        var result = await _controller.RefreshToken(dto);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Logout_ValidToken_ReturnsOk()
    {
        // Arrange
        var dto = new RefreshTokenDto { RefreshToken = "token" };
        _mockService.Setup(s => s.Logout("token")).ReturnsAsync(Result.Success());

        // Act
        var result = await _controller.Logout(dto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetActiveSessions_ValidUser_ReturnsOkWithSessions()
    {
        // Arrange
        var sessions = new List<SessionDto> { new() { Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddDays(1), IsCurrentSession = true } };
        _mockService.Setup(s => s.GetActiveSessions(_testUserId, "refresh")).ReturnsAsync(Result.Success(sessions));

        // Act
        var result = await _controller.GetActiveSessions("refresh");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockService.Verify(s => s.GetActiveSessions(_testUserId, "refresh"), Times.Once);
    }

    [Fact]
    public async Task RevokeSession_ExistingSession_ReturnsOk()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        _mockService.Setup(s => s.RevokeSession(_testUserId, sessionId)).ReturnsAsync(Result.Success());

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
        _mockService.Setup(s => s.RevokeSession(_testUserId, sessionId)).ReturnsAsync(Result.Failure("Session not found"));

        // Act
        var result = await _controller.RevokeSession(sessionId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task RevokeAllOtherSessions_ValidRequest_ReturnsOk()
    {
        // Arrange
        _mockService.Setup(s => s.RevokeAllOtherSessions(_testUserId, "refresh")).ReturnsAsync(Result.Success());

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
        _mockService.Setup(s => s.ChangePassword(_testUserId, dto, "refresh")).ReturnsAsync(Result.Success());

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
        _mockService.Setup(s => s.ChangePassword(_testUserId, dto, "refresh")).ReturnsAsync(Result.Failure("Current password is incorrect"));

        // Act
        var result = await _controller.ChangePassword(dto, "refresh");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }
}
