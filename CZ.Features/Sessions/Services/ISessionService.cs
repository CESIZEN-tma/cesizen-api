using api.CZ.Features.Sessions.Models;

namespace api.CZ.Features.Sessions.Services;

public interface ISessionService
{
    Task<Session?> GetByRefreshToken(string refreshToken);
    Task<Session> CreateSession(Guid userId, string refreshToken, DateTime expiresAt);
    Task<bool> ConsumeSession(string refreshToken);
    Task<bool> RevokeAllUserSessions(Guid userId);
    Task<bool> RevokeSession(Guid sessionId);
    Task CleanupExpiredSessions();
    Task<List<Session>> GetActiveSessionsByUserId(Guid userId);
    Task<bool> RevokeSessionForUser(Guid sessionId, Guid userId);
    Task<bool> RevokeAllSessionsExceptCurrent(Guid userId, Guid currentSessionId);
}
