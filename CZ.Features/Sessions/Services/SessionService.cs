using api.CZ.Core.Services;
using api.CZ.Features.Sessions.Factories;
using api.CZ.Features.Sessions.Models;
using api.CZ.Features.Sessions.Repositories;

namespace api.CZ.Features.Sessions.Services;

public class SessionService : BaseSessionService<Session, ISessionRepository, ISessionFactory>, ISessionService
{
    public SessionService(
        ISessionRepository repository,
        ISessionFactory factory,
        ILogger<SessionService> logger)
        : base(
            repository,
            factory,
            logger,
            getSessionId: s => s.Id,
            getEntityId: s => s.IdUsers,
            getToken: s => s.Token,
            getConsumed: s => s.Consumed,
            getExpiresAt: s => s.ExpiresAt,
            setConsumed: (s, consumed) => s.Consumed = consumed,
            setUpdateTime: (s, time) => s.UpdateTime = time,
            createSessionFunc: (userId, token, expiresAt) => factory.Create(userId, token, expiresAt))
    {
    }

    // Delegate methods to base with user-specific naming
    public new async Task<Session?> GetByRefreshToken(string refreshToken) =>
        await base.GetByRefreshToken(refreshToken);

    public new async Task<Session> CreateSession(Guid userId, string refreshToken, DateTime expiresAt) =>
        await base.CreateSession(userId, refreshToken, expiresAt);

    public new async Task<bool> ConsumeSession(string refreshToken) =>
        await base.ConsumeSession(refreshToken);

    public async Task<bool> RevokeAllUserSessions(Guid userId) =>
        await base.RevokeAllEntitySessions(userId);

    public new async Task<bool> RevokeSession(Guid sessionId) =>
        await base.RevokeSession(sessionId);

    public new async Task CleanupExpiredSessions() =>
        await base.CleanupExpiredSessions();

    public async Task<List<Session>> GetActiveSessionsByUserId(Guid userId) =>
        await base.GetActiveSessionsByEntityId(userId);

    public async Task<bool> RevokeSessionForUser(Guid sessionId, Guid userId) =>
        await base.RevokeSessionForEntity(sessionId, userId);

    public new async Task<bool> RevokeAllSessionsExceptCurrent(Guid userId, Guid currentSessionId) =>
        await base.RevokeAllSessionsExceptCurrent(userId, currentSessionId);
}
