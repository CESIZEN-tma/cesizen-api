using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using api.CZ.Features.Authentifications.Services;
using api.CZ.Features.Authentifications.DTOs;

namespace api.CZ.Features.Authentifications;

[ApiController]
[Route("admin")]
public class AdminAuthentificationController : ControllerBase
{
    private readonly IAdminAuthentificationService _service;
    private readonly ILogger<AdminAuthentificationController> _logger;

    public AdminAuthentificationController(IAdminAuthentificationService service, ILogger<AdminAuthentificationController> logger)
    {
        _service = service;
        _logger = logger;
    }

    private Guid GetAdminId()
    {
        var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(adminIdClaim!);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _service.RegisterAdmin(dto);

        return result.Match<IActionResult>(
            onSuccess: () => Created(),
            onFailure: error => BadRequest(new { error })
        );
    }

    [HttpPut("confirm-account/{token}")]
    public async Task<IActionResult> ConfirmAccount(string token)
    {
        var result = await _service.ConfirmAccount(token);

        return result.Match<IActionResult>(
            onSuccess: () => Ok(new { message = "Account activated successfully." }),
            onFailure: error => BadRequest(new { error })
        );
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _service.Login(dto);

        return result.Match<IActionResult>(
            onSuccess: tokens => Ok(tokens),
            onFailure: error => Unauthorized(new { error })
        );
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var result = await _service.ForgotPassword(dto);

        return result.Match<IActionResult>(
            onSuccess: () => Ok(new { message = "If an account with that email exists, a password reset link has been sent." }),
            onFailure: error => BadRequest(new { error })
        );
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var result = await _service.ResetPassword(dto);

        return result.Match<IActionResult>(
            onSuccess: () => Ok(new { message = "Password has been reset successfully. You can now login with your new password." }),
            onFailure: error => BadRequest(new { error })
        );
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
    {
        var result = await _service.RefreshToken(dto);

        return result.Match<IActionResult>(
            onSuccess: tokens => Ok(tokens),
            onFailure: error => Unauthorized(new { error })
        );
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenDto dto)
    {
        var result = await _service.Logout(dto.RefreshToken);

        return result.Match<IActionResult>(
            onSuccess: () => Ok(new { message = "Logged out successfully." }),
            onFailure: error => BadRequest(new { error })
        );
    }

    [Authorize(Roles = "Administrator")]
    [HttpGet("sessions")]
    public async Task<IActionResult> GetActiveSessions([FromHeader(Name = "X-Refresh-Token")] string refreshToken)
    {
        var adminId = GetAdminId();
        var result = await _service.GetActiveSessions(adminId, refreshToken);

        return result.Match<IActionResult>(
            onSuccess: sessions => Ok(sessions),
            onFailure: error => BadRequest(new { error })
        );
    }

    [Authorize(Roles = "Administrator")]
    [HttpDelete("sessions/{sessionId:guid}")]
    public async Task<IActionResult> RevokeSession(Guid sessionId)
    {
        var adminId = GetAdminId();
        var result = await _service.RevokeSession(adminId, sessionId);

        return result.Match<IActionResult>(
            onSuccess: () => Ok(new { message = "Session revoked successfully." }),
            onFailure: error => NotFound(new { error })
        );
    }

    [Authorize(Roles = "Administrator")]
    [HttpDelete("sessions")]
    public async Task<IActionResult> RevokeAllOtherSessions([FromHeader(Name = "X-Refresh-Token")] string refreshToken)
    {
        var adminId = GetAdminId();
        var result = await _service.RevokeAllOtherSessions(adminId, refreshToken);

        return result.Match<IActionResult>(
            onSuccess: () => Ok(new { message = "All other sessions revoked successfully." }),
            onFailure: error => BadRequest(new { error })
        );
    }

    [Authorize(Roles = "Administrator")]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordDto dto,
        [FromHeader(Name = "X-Refresh-Token")] string refreshToken)
    {
        var adminId = GetAdminId();
        var result = await _service.ChangePassword(adminId, dto, refreshToken);

        return result.Match<IActionResult>(
            onSuccess: () => Ok(new { message = "Password changed successfully." }),
            onFailure: error => BadRequest(new { error })
        );
    }
}
