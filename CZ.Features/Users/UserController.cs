using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using api.CZ.Features.Users.Services;
using api.CZ.Features.Users.DTOs;

namespace api.CZ.Features.Users;

[ApiController]
[Route("api/users")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _service;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService service, ILogger<UserController> logger)
    {
        _service = service;
        _logger = logger;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetUserId();
        var profile = await _service.GetProfileAsync(userId);

        if (profile == null)
            return NotFound(new { error = "User not found" });

        return Ok(profile);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileDto dto)
    {
        var userId = GetUserId();
        var profile = await _service.UpdateProfileAsync(userId, dto);

        if (profile == null)
            return NotFound(new { error = "User not found" });

        return Ok(profile);
    }

    [HttpDelete("account")]
    public async Task<IActionResult> DeleteAccount()
    {
        var userId = GetUserId();
        var deleted = await _service.DeleteAccountAsync(userId);

        if (!deleted)
            return NotFound(new { error = "User not found" });

        return Ok(new { message = "Account deleted successfully" });
    }
}
