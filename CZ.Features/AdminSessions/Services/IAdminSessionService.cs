using api.CZ.Features.AdminSessions.Models;

namespace api.CZ.Features.AdminSessions.Services;

public interface IAdminSessionService
{
    Task<AdminSession?> GetByRefreshToken(string refreshToken);
    Task<AdminSession> CreateSession(Guid adminId, string refreshToken, DateTime expiresAt);
    Task<bool> ConsumeSession(string refreshToken);
    Task<bool> RevokeAllAdminSessions(Guid adminId);
    Task<bool> RevokeSession(Guid sessionId);
    Task CleanupExpiredSessions();
    Task<List<AdminSession>> GetActiveSessionsByAdminId(Guid adminId);
    Task<bool> RevokeSessionForAdmin(Guid sessionId, Guid adminId);
    Task<bool> RevokeAllSessionsExceptCurrent(Guid adminId, Guid currentSessionId);
}
