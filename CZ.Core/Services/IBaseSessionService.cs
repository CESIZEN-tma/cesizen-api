namespace api.CZ.Core.Services;

/// <summary>
/// Base interface for session management services supporting different entity types (Users, Administrators)
/// </summary>
/// <typeparam name="TSession">The session entity type (Session or AdminSession)</typeparam>
public interface IBaseSessionService<TSession> where TSession : class
{
    /// <summary>
    /// Retrieves a valid, non-consumed session by refresh token
    /// </summary>
    Task<TSession?> GetByRefreshToken(string refreshToken);

    /// <summary>
    /// Creates a new session for the specified entity
    /// </summary>
    Task<TSession> CreateSession(Guid entityId, string refreshToken, DateTime expiresAt);

    /// <summary>
    /// Marks a session as consumed (used)
    /// </summary>
    Task<bool> ConsumeSession(string refreshToken);

    /// <summary>
    /// Revokes all active sessions for a specific entity
    /// </summary>
    Task<bool> RevokeAllEntitySessions(Guid entityId);

    /// <summary>
    /// Revokes a specific session by its ID
    /// </summary>
    Task<bool> RevokeSession(Guid sessionId);

    /// <summary>
    /// Cleans up expired sessions by marking them as consumed
    /// </summary>
    Task CleanupExpiredSessions();

    /// <summary>
    /// Gets all active (non-consumed, non-expired) sessions for an entity
    /// </summary>
    Task<List<TSession>> GetActiveSessionsByEntityId(Guid entityId);

    /// <summary>
    /// Revokes a specific session for a specific entity (security check)
    /// </summary>
    Task<bool> RevokeSessionForEntity(Guid sessionId, Guid entityId);

    /// <summary>
    /// Revokes all sessions except the current one for a specific entity
    /// </summary>
    Task<bool> RevokeAllSessionsExceptCurrent(Guid entityId, Guid currentSessionId);
}
