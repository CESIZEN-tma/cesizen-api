using api.CZ.Core.Services;
using api.CZ.Features.AdminSessions.Models;

namespace api.CZ.Features.AdminSessions.Services;

/// <summary>
/// Session management service for administrator sessions.
/// Inherits common session operations from IBaseSessionService.
/// Provides admin-specific method names for clarity.
/// </summary>
public interface IAdminSessionService : IBaseSessionService<AdminSession>
{
    // Admin-specific method names (delegate to base interface methods)
    Task<bool> RevokeAllAdminSessions(Guid adminId);
    Task<List<AdminSession>> GetActiveSessionsByAdminId(Guid adminId);
    Task<bool> RevokeSessionForAdmin(Guid sessionId, Guid adminId);
}
