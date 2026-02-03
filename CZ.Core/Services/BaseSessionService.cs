using System.Linq.Expressions;
using api.CZ.Data.Repositories;

namespace api.CZ.Core.Services;

/// <summary>
/// Abstract base class providing common session management functionality for User and Admin sessions.
/// Eliminates code duplication between SessionService and AdminSessionService.
/// </summary>
public abstract class BaseSessionService<TSession, TRepository, TFactory> : IBaseSessionService<TSession>
    where TSession : class
    where TRepository : IBaseRepository<TSession>
    where TFactory : class
{
    protected readonly TRepository Repository;
    protected readonly TFactory Factory;
    protected readonly ILogger Logger;
    protected readonly Func<TSession, Guid> GetSessionId;
    protected readonly Func<TSession, Guid> GetEntityId;
    protected readonly Func<TSession, string> GetToken;
    protected readonly Func<TSession, bool> GetConsumed;
    protected readonly Func<TSession, DateTime> GetExpiresAt;
    protected readonly Action<TSession, bool> SetConsumed;
    protected readonly Action<TSession, DateTime> SetUpdateTime;
    protected readonly Func<Guid, string, DateTime, TSession> CreateSessionFunc;

    protected BaseSessionService(
        TRepository repository,
        TFactory factory,
        ILogger logger,
        Func<TSession, Guid> getSessionId,
        Func<TSession, Guid> getEntityId,
        Func<TSession, string> getToken,
        Func<TSession, bool> getConsumed,
        Func<TSession, DateTime> getExpiresAt,
        Action<TSession, bool> setConsumed,
        Action<TSession, DateTime> setUpdateTime,
        Func<Guid, string, DateTime, TSession> createSessionFunc)
    {
        Repository = repository;
        Factory = factory;
        Logger = logger;
        GetSessionId = getSessionId;
        GetEntityId = getEntityId;
        GetToken = getToken;
        GetConsumed = getConsumed;
        GetExpiresAt = getExpiresAt;
        SetConsumed = setConsumed;
        SetUpdateTime = setUpdateTime;
        CreateSessionFunc = createSessionFunc;
    }

    public async Task<TSession?> GetByRefreshToken(string refreshToken)
    {
        var sessions = await Repository.ListAsync(s =>
            GetToken(s) == refreshToken &&
            !GetConsumed(s) &&
            GetExpiresAt(s) > DateTime.UtcNow);

        return sessions.FirstOrDefault();
    }

    public async Task<TSession> CreateSession(Guid entityId, string refreshToken, DateTime expiresAt)
    {
        var session = CreateSessionFunc(entityId, refreshToken, expiresAt);
        await Repository.AddAsync(session);

        Logger.LogInformation("Session created for entity {EntityId}, expires at {ExpiresAt}",
            entityId, expiresAt);

        return session;
    }

    public async Task<bool> ConsumeSession(string refreshToken)
    {
        var session = await Repository.FirstOrDefaultAsync(s => GetToken(s) == refreshToken);

        if (session == null)
        {
            Logger.LogWarning("Attempted to consume non-existent session with token");
            return false;
        }

        if (GetConsumed(session))
        {
            Logger.LogWarning("Attempted to consume already consumed session");
            return false;
        }

        if (GetExpiresAt(session) < DateTime.UtcNow)
        {
            Logger.LogWarning("Attempted to consume expired session");
            return false;
        }

        SetConsumed(session, true);
        SetUpdateTime(session, DateTime.UtcNow);

        await Repository.UpdateAsync(session);

        Logger.LogInformation("Session consumed successfully");

        return true;
    }

    public async Task<bool> RevokeAllEntitySessions(Guid entityId)
    {
        var sessions = await Repository.ListAsync(s =>
            GetEntityId(s) == entityId &&
            !GetConsumed(s) &&
            GetExpiresAt(s) > DateTime.UtcNow);

        if (!sessions.Any())
        {
            Logger.LogInformation("No active sessions found for entity {EntityId}", entityId);
            return true;
        }

        foreach (var session in sessions)
        {
            SetConsumed(session, true);
            SetUpdateTime(session, DateTime.UtcNow);
            await Repository.UpdateAsync(session);
        }

        Logger.LogInformation("Revoked {Count} active sessions for entity {EntityId}",
            sessions.Count(), entityId);

        return true;
    }

    public async Task<bool> RevokeSession(Guid sessionId)
    {
        var session = await Repository.FindAsync(sessionId);

        if (session == null)
        {
            Logger.LogWarning("Attempted to revoke non-existent session {SessionId}", sessionId);
            return false;
        }

        if (GetConsumed(session))
        {
            Logger.LogInformation("Session {SessionId} already consumed", sessionId);
            return true;
        }

        SetConsumed(session, true);
        SetUpdateTime(session, DateTime.UtcNow);

        await Repository.UpdateAsync(session);

        Logger.LogInformation("Session {SessionId} revoked successfully", sessionId);

        return true;
    }

    public async Task CleanupExpiredSessions()
    {
        var expiredSessions = await Repository.ListAsync(s =>
            GetExpiresAt(s) < DateTime.UtcNow &&
            !GetConsumed(s));

        if (!expiredSessions.Any())
        {
            Logger.LogDebug("No expired sessions to cleanup");
            return;
        }

        foreach (var session in expiredSessions)
        {
            SetConsumed(session, true);
            SetUpdateTime(session, DateTime.UtcNow);
            await Repository.UpdateAsync(session);
        }

        Logger.LogInformation("Cleaned up {Count} expired sessions", expiredSessions.Count());
    }

    public async Task<List<TSession>> GetActiveSessionsByEntityId(Guid entityId)
    {
        var sessions = await Repository.ListAsync(s =>
            GetEntityId(s) == entityId &&
            !GetConsumed(s) &&
            GetExpiresAt(s) > DateTime.UtcNow);

        Logger.LogInformation("Retrieved {Count} active sessions for entity {EntityId}", sessions.Count(), entityId);

        return sessions.ToList();
    }

    public async Task<bool> RevokeSessionForEntity(Guid sessionId, Guid entityId)
    {
        var sessions = await Repository.ListAsync(s =>
            GetEntityId(s) == entityId);

        var session = sessions.FirstOrDefault(s => GetSessionId(s) == sessionId);

        if (session == null)
        {
            Logger.LogWarning("Attempted to revoke non-existent session {SessionId} for entity {EntityId}", sessionId, entityId);
            return false;
        }

        if (GetConsumed(session))
        {
            Logger.LogInformation("Session {SessionId} already consumed", sessionId);
            return true;
        }

        SetConsumed(session, true);
        SetUpdateTime(session, DateTime.UtcNow);

        await Repository.UpdateAsync(session);

        Logger.LogInformation("Session {SessionId} revoked for entity {EntityId}", sessionId, entityId);

        return true;
    }

    public async Task<bool> RevokeAllSessionsExceptCurrent(Guid entityId, Guid currentSessionId)
    {
        var sessions = await Repository.ListAsync(s =>
            GetEntityId(s) == entityId &&
            !GetConsumed(s) &&
            GetExpiresAt(s) > DateTime.UtcNow);

        var sessionsToRevoke = sessions.Where(s => GetSessionId(s) != currentSessionId).ToList();

        if (!sessionsToRevoke.Any())
        {
            Logger.LogInformation("No other active sessions found for entity {EntityId}", entityId);
            return true;
        }

        foreach (var session in sessionsToRevoke)
        {
            SetConsumed(session, true);
            SetUpdateTime(session, DateTime.UtcNow);
            await Repository.UpdateAsync(session);
        }

        Logger.LogInformation("Revoked {Count} sessions (excluding current) for entity {EntityId}",
            sessionsToRevoke.Count(), entityId);

        return true;
    }
}
