using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using api.CZ.Features.Users.Services;
using api.CZ.Features.Users.DTOs;
using api.CZ.Features.Sessions.Services;
using api.CZ.Features.Authentifications.DTOs;
using api.CZ.Features.AdminLogs.Services;
using api.CZ.Features.AdminLogs.Enums;

namespace api.CZ.Features.Administrators;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Administrator")]
public class AdminUserManagementController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ISessionService _sessionService;
    private readonly IAdminActionLogger _actionLogger;
    private readonly ILogger<AdminUserManagementController> _logger;

    public AdminUserManagementController(
        IUserService userService,
        ISessionService sessionService,
        IAdminActionLogger actionLogger,
        ILogger<AdminUserManagementController> logger)
    {
        _userService = userService;
        _sessionService = sessionService;
        _actionLogger = actionLogger;
        _logger = logger;
    }

    private Guid GetAdminId()
    {
        var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(adminIdClaim!);
    }

    [HttpPatch("{userId:guid}/status")]
    public async Task<IActionResult> UpdateUserStatus(Guid userId, [FromBody] UpdateUserStatusDto dto)
    {
        var adminId = GetAdminId();

        // Edge case: Prevent admin from disabling themselves if they're also a user
        // (This depends on business logic - can admins be users?)
        if (userId == adminId && !dto.Active)
        {
            return BadRequest(new { error = "You cannot disable your own account" });
        }

        var result = await _userService.UpdateUserStatusAsync(userId, dto.Active, adminId);

        if (!result)
            return NotFound(new { error = "User not found" });

        return Ok(new { message = $"User {(dto.Active ? "enabled" : "disabled")} successfully" });
    }

    [HttpGet("{userId:guid}/sessions")]
    public async Task<IActionResult> GetUserSessions(Guid userId)
    {
        // Verify user exists
        var user = await _userService.GetProfileAsync(userId);
        if (user == null)
            return NotFound(new { error = "User not found" });

        var sessions = await _sessionService.GetActiveSessionsByUserId(userId);

        var sessionDtos = sessions.Select(s => new SessionInfoDto
        {
            Id = s.Id,
            CreatedAt = s.CreationTime,
            ExpiresAt = s.ExpiresAt
        }).ToList();

        return Ok(sessionDtos);
    }

    [HttpDelete("{userId:guid}/sessions")]
    public async Task<IActionResult> RevokeAllUserSessions(Guid userId)
    {
        var adminId = GetAdminId();

        // Edge case: Warn if admin is revoking their own sessions
        if (userId == adminId)
        {
            return BadRequest(new {
                error = "You are about to revoke your own sessions, which will log you out. Use the user authentication endpoint instead for self-management."
            });
        }

        // Verify user exists
        var user = await _userService.GetProfileAsync(userId);
        if (user == null)
            return NotFound(new { error = "User not found" });

        // Revoke all user sessions
        var revoked = await _sessionService.RevokeAllUserSessions(userId);

        // Log admin action
        await _actionLogger.LogCustomActionAsync(
            adminId,
            AdminActionCode.USER_SESSION_REVOKED,
            "User",
            userId,
            $"Revoked all sessions for user {user.Email}"
        );

        _logger.LogInformation("Admin {AdminId} revoked all sessions for user {UserId}", adminId, userId);

        return Ok(new { message = "All user sessions revoked successfully" });
    }

    [HttpDelete("{userId:guid}/sessions/{sessionId:guid}")]
    public async Task<IActionResult> RevokeUserSession(Guid userId, Guid sessionId)
    {
        var adminId = GetAdminId();

        // Verify user exists
        var user = await _userService.GetProfileAsync(userId);
        if (user == null)
            return NotFound(new { error = "User not found" });

        // Revoke specific session (ensure it belongs to the user)
        var revoked = await _sessionService.RevokeSessionForUser(sessionId, userId);

        if (!revoked)
            return NotFound(new { error = "Session not found for this user" });

        // Log admin action
        await _actionLogger.LogCustomActionAsync(
            adminId,
            AdminActionCode.USER_SESSION_REVOKED,
            "User",
            userId,
            $"Revoked session {sessionId} for user {user.Email}"
        );

        _logger.LogInformation("Admin {AdminId} revoked session {SessionId} for user {UserId}",
            adminId, sessionId, userId);

        return Ok(new { message = "User session revoked successfully" });
    }
}
