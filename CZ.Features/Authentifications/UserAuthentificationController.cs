using Microsoft.AspNetCore.Mvc;
using api.CZ.Features.Authentifications.Services;
using api.CZ.Features.Authentifications.DTOs;

namespace api.CZ.Features.Authentifications;

[ApiController]
[Route("user")]
public class UserAuthentificationController : ControllerBase
{
    private readonly IAuthentificationService _service;
    private readonly ILogger<UserAuthentificationController> _logger;

    public UserAuthentificationController(IAuthentificationService service, ILogger<UserAuthentificationController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _service.RegisterUser(dto);

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
}