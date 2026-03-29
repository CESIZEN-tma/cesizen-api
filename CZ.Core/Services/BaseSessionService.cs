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
    protected readonly Expression<Func<TSession, Guid>> GetSessionIdExpr;
    protected readonly Expression<Func<TSession, Guid>> GetEntityIdExpr;
    protected readonly Expression<Func<TSession, string>> GetTokenExpr;
    protected readonly Expression<Func<TSession, bool>> GetConsumedExpr;
    protected readonly Expression<Func<TSession, DateTime>> GetExpiresAtExpr;
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
        Expression<Func<TSession, Guid>> getSessionId,
        Expression<Func<TSession, Guid>> getEntityId,
        Expression<Func<TSession, string>> getToken,
        Expression<Func<TSession, bool>> getConsumed,
        Expression<Func<TSession, DateTime>> getExpiresAt,
        Action<TSession, bool> setConsumed,
        Action<TSession, DateTime> setUpdateTime,
        Func<Guid, string, DateTime, TSession> createSessionFunc)
    {
        Repository = repository;
        Factory = factory;
        Logger = logger;
        GetSessionIdExpr = getSessionId;
        GetEntityIdExpr = getEntityId;
        GetTokenExpr = getToken;
        GetConsumedExpr = getConsumed;
        GetExpiresAtExpr = getExpiresAt;
        GetSessionId = getSessionId.Compile();
        GetEntityId = getEntityId.Compile();
        GetToken = getToken.Compile();
        GetConsumed = getConsumed.Compile();
        GetExpiresAt = getExpiresAt.Compile();
        SetConsumed = setConsumed;
        SetUpdateTime = setUpdateTime;
        CreateSessionFunc = createSessionFunc;
    }

    public async Task<TSession?> GetByRefreshToken(string refreshToken)
    {
        var tokenParam = GetTokenExpr.Parameters[0];
        var consumedParam = GetConsumedExpr.Parameters[0];
        var expiresParam = GetExpiresAtExpr.Parameters[0];

        var parameter = Expression.Parameter(typeof(TSession), "s");

        var tokenBody = Expression.Invoke(GetTokenExpr, parameter);
        var tokenCondition = Expression.Equal(tokenBody, Expression.Constant(refreshToken));

        var consumedBody = Expression.Invoke(GetConsumedExpr, parameter);
        var consumedCondition = Expression.Not(consumedBody);

        var expiresBody = Expression.Invoke(GetExpiresAtExpr, parameter);
        var expiresCondition = Expression.GreaterThan(expiresBody, Expression.Constant(DateTime.UtcNow));

        var combinedCondition = Expression.AndAlso(
            Expression.AndAlso(tokenCondition, consumedCondition),
            expiresCondition);

        var lambda = Expression.Lambda<Func<TSession, bool>>(combinedCondition, parameter);

        var sessions = await Repository.ListAsync(lambda);

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
        var parameter = Expression.Parameter(typeof(TSession), "s");
        var tokenBody = Expression.Invoke(GetTokenExpr, parameter);
        var condition = Expression.Equal(tokenBody, Expression.Constant(refreshToken));
        var lambda = Expression.Lambda<Func<TSession, bool>>(condition, parameter);

        var session = await Repository.FirstOrDefaultAsync(lambda);

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
        var parameter = Expression.Parameter(typeof(TSession), "s");

        var entityBody = Expression.Invoke(GetEntityIdExpr, parameter);
        var entityCondition = Expression.Equal(entityBody, Expression.Constant(entityId));

        var consumedBody = Expression.Invoke(GetConsumedExpr, parameter);
        var consumedCondition = Expression.Not(consumedBody);

        var expiresBody = Expression.Invoke(GetExpiresAtExpr, parameter);
        var expiresCondition = Expression.GreaterThan(expiresBody, Expression.Constant(DateTime.UtcNow));

        var combinedCondition = Expression.AndAlso(
            Expression.AndAlso(entityCondition, consumedCondition),
            expiresCondition);

        var lambda = Expression.Lambda<Func<TSession, bool>>(combinedCondition, parameter);

        var sessions = await Repository.ListAsync(lambda);

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
        var parameter = Expression.Parameter(typeof(TSession), "s");

        var expiresBody = Expression.Invoke(GetExpiresAtExpr, parameter);
        var expiresCondition = Expression.LessThan(expiresBody, Expression.Constant(DateTime.UtcNow));

        var consumedBody = Expression.Invoke(GetConsumedExpr, parameter);
        var consumedCondition = Expression.Not(consumedBody);

        var combinedCondition = Expression.AndAlso(expiresCondition, consumedCondition);

        var lambda = Expression.Lambda<Func<TSession, bool>>(combinedCondition, parameter);

        var expiredSessions = await Repository.ListAsync(lambda);

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
        var parameter = Expression.Parameter(typeof(TSession), "s");

        var entityBody = Expression.Invoke(GetEntityIdExpr, parameter);
        var entityCondition = Expression.Equal(entityBody, Expression.Constant(entityId));

        var consumedBody = Expression.Invoke(GetConsumedExpr, parameter);
        var consumedCondition = Expression.Not(consumedBody);

        var expiresBody = Expression.Invoke(GetExpiresAtExpr, parameter);
        var expiresCondition = Expression.GreaterThan(expiresBody, Expression.Constant(DateTime.UtcNow));

        var combinedCondition = Expression.AndAlso(
            Expression.AndAlso(entityCondition, consumedCondition),
            expiresCondition);

        var lambda = Expression.Lambda<Func<TSession, bool>>(combinedCondition, parameter);

        var sessions = await Repository.ListAsync(lambda);

        Logger.LogInformation("Retrieved {Count} active sessions for entity {EntityId}", sessions.Count(), entityId);

        return sessions.ToList();
    }

    public async Task<bool> RevokeSessionForEntity(Guid sessionId, Guid entityId)
    {
        var parameter = Expression.Parameter(typeof(TSession), "s");
        var entityBody = Expression.Invoke(GetEntityIdExpr, parameter);
        var entityCondition = Expression.Equal(entityBody, Expression.Constant(entityId));
        var lambda = Expression.Lambda<Func<TSession, bool>>(entityCondition, parameter);

        var sessions = await Repository.ListAsync(lambda);

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
        var parameter = Expression.Parameter(typeof(TSession), "s");

        var entityBody = Expression.Invoke(GetEntityIdExpr, parameter);
        var entityCondition = Expression.Equal(entityBody, Expression.Constant(entityId));

        var consumedBody = Expression.Invoke(GetConsumedExpr, parameter);
        var consumedCondition = Expression.Not(consumedBody);

        var expiresBody = Expression.Invoke(GetExpiresAtExpr, parameter);
        var expiresCondition = Expression.GreaterThan(expiresBody, Expression.Constant(DateTime.UtcNow));

        var combinedCondition = Expression.AndAlso(
            Expression.AndAlso(entityCondition, consumedCondition),
            expiresCondition);

        var lambda = Expression.Lambda<Func<TSession, bool>>(combinedCondition, parameter);

        var sessions = await Repository.ListAsync(lambda);

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
