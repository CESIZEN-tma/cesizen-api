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
}