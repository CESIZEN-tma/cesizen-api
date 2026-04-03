using api.CZ.Core.Services;
using api.CZ.Features.AdminSessions.Factories;
using api.CZ.Features.AdminSessions.Models;
using api.CZ.Features.AdminSessions.Repositories;

namespace api.CZ.Features.AdminSessions.Services;

public class AdminSessionService : BaseSessionService<AdminSession, IAdminSessionRepository, IAdminSessionFactory>, IAdminSessionService
{
    public AdminSessionService(
        IAdminSessionRepository repository,
        IAdminSessionFactory factory,
        ILogger<AdminSessionService> logger)
        : base(
            repository,
            factory,
            logger,
            getSessionId: s => s.Id,
            getEntityId: s => s.IdAdministrators,
            getToken: s => s.Token,
            getConsumed: s => s.Consumed,
            getExpiresAt: s => s.ExpiresAt,
            setConsumed: (s, consumed) => s.Consumed = consumed,
            setUpdateTime: (s, time) => s.UpdateTime = time,
            createSessionFunc: (adminId, token, expiresAt) => factory.Create(adminId, token, expiresAt))
    {
    }

    // Delegate methods to base with admin-specific naming
    public new async Task<AdminSession?> GetByRefreshToken(string refreshToken) =>
        await base.GetByRefreshToken(refreshToken);

    public new async Task<AdminSession> CreateSession(Guid adminId, string refreshToken, DateTime expiresAt) =>
        await base.CreateSession(adminId, refreshToken, expiresAt);

    public new async Task<bool> ConsumeSession(string refreshToken) =>
        await base.ConsumeSession(refreshToken);

    public async Task<bool> RevokeAllAdminSessions(Guid adminId) =>
        await base.RevokeAllEntitySessions(adminId);

    public new async Task<bool> RevokeSession(Guid sessionId) =>
        await base.RevokeSession(sessionId);

    public new async Task CleanupExpiredSessions() =>
        await base.CleanupExpiredSessions();

    public async Task<List<AdminSession>> GetActiveSessionsByAdminId(Guid adminId) =>
        await base.GetActiveSessionsByEntityId(adminId);

    public async Task<bool> RevokeSessionForAdmin(Guid sessionId, Guid adminId) =>
        await base.RevokeSessionForEntity(sessionId, adminId);

    public new async Task<bool> RevokeAllSessionsExceptCurrent(Guid adminId, Guid currentSessionId) =>
        await base.RevokeAllSessionsExceptCurrent(adminId, currentSessionId);
}
