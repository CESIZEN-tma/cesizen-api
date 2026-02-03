using api.CZ.Core.Services;
using api.CZ.Features.Sessions.Models;

namespace api.CZ.Features.Sessions.Services;

/// <summary>
/// Session management service for user sessions.
/// Inherits common session operations from IBaseSessionService.
/// Provides user-specific method names for clarity.
/// </summary>
public interface ISessionService : IBaseSessionService<Session>
{
    // User-specific method names (delegate to base interface methods)
    Task<bool> RevokeAllUserSessions(Guid userId);
    Task<List<Session>> GetActiveSessionsByUserId(Guid userId);
    Task<bool> RevokeSessionForUser(Guid sessionId, Guid userId);
}
