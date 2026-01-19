using api.CZ.Features.Sessions.Factories;
using api.CZ.Features.Sessions.Models;
using api.CZ.Features.Sessions.Repositories;

namespace api.CZ.Features.Sessions.Services;

public class SessionService : ISessionService
{
    private readonly ISessionRepository _repository;
    private readonly ISessionFactory _factory;
    private readonly ILogger<SessionService> _logger;

    public SessionService(
        ISessionRepository repository,
        ISessionFactory factory,
        ILogger<SessionService> logger)
    {
        _repository = repository;
        _factory = factory;
        _logger = logger;
    }

    public async Task<Session?> GetByRefreshToken(string refreshToken)
    {
        var session = await _repository.FirstOrDefaultAsync(s =>
            s.Token == refreshToken &&
            !s.Consumed &&
            s.ExpiresAt > DateTime.UtcNow);

        return session;
    }

    public async Task<Session> CreateSession(Guid userId, string refreshToken, DateTime expiresAt)
    {
        var session = _factory.Create(userId, refreshToken, expiresAt);
        await _repository.AddAsync(session);

        _logger.LogInformation("Session created for user {UserId}, expires at {ExpiresAt}",
            userId, expiresAt);

        return session;
    }

    public async Task<bool> ConsumeSession(string refreshToken)
    {
        var session = await _repository.FirstOrDefaultAsync(s => s.Token == refreshToken);

        if (session == null)
        {
            _logger.LogWarning("Attempted to consume non-existent session with token");
            return false;
        }

        if (session.Consumed)
        {
            _logger.LogWarning("Attempted to consume already consumed session {SessionId}", session.Id);
            return false;
        }

        if (session.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Attempted to consume expired session {SessionId}", session.Id);
            return false;
        }

        session.Consumed = true;
        session.UpdateTime = DateTime.UtcNow;

        await _repository.UpdateAsync(session);

        _logger.LogInformation("Session {SessionId} consumed successfully", session.Id);

        return true;
    }

    public async Task<bool> RevokeAllUserSessions(Guid userId)
    {
        var sessions = await _repository.ListAsync(s =>
            s.IdUsers == userId &&
            !s.Consumed &&
            s.ExpiresAt > DateTime.UtcNow);

        if (!sessions.Any())
        {
            _logger.LogInformation("No active sessions found for user {UserId}", userId);
            return true;
        }

        foreach (var session in sessions)
        {
            session.Consumed = true;
            session.UpdateTime = DateTime.UtcNow;
            await _repository.UpdateAsync(session);
        }

        _logger.LogInformation("Revoked {Count} active sessions for user {UserId}",
            sessions.Count(), userId);

        return true;
    }

    public async Task<bool> RevokeSession(Guid sessionId)
    {
        var session = await _repository.FindAsync(sessionId);

        if (session == null)
        {
            _logger.LogWarning("Attempted to revoke non-existent session {SessionId}", sessionId);
            return false;
        }

        if (session.Consumed)
        {
            _logger.LogInformation("Session {SessionId} already consumed", sessionId);
            return true;
        }

        session.Consumed = true;
        session.UpdateTime = DateTime.UtcNow;

        await _repository.UpdateAsync(session);

        _logger.LogInformation("Session {SessionId} revoked successfully", sessionId);

        return true;
    }

    public async Task CleanupExpiredSessions()
    {
        var expiredSessions = await _repository.ListAsync(s =>
            s.ExpiresAt < DateTime.UtcNow &&
            !s.Consumed);

        if (!expiredSessions.Any())
        {
            _logger.LogDebug("No expired sessions to cleanup");
            return;
        }

        foreach (var session in expiredSessions)
        {
            session.Consumed = true;
            session.UpdateTime = DateTime.UtcNow;
            await _repository.UpdateAsync(session);
        }

        _logger.LogInformation("Cleaned up {Count} expired sessions", expiredSessions.Count());
    }

    public async Task<List<Session>> GetActiveSessionsByUserId(Guid userId)
    {
        var sessions = await _repository.ListAsync(s =>
            s.IdUsers == userId &&
            !s.Consumed &&
            s.ExpiresAt > DateTime.UtcNow);

        _logger.LogInformation("Retrieved {Count} active sessions for user {UserId}", sessions.Count(), userId);

        return sessions.ToList();
    }

    public async Task<bool> RevokeSessionForUser(Guid sessionId, Guid userId)
    {
        var session = await _repository.FirstOrDefaultAsync(s =>
            s.Id == sessionId &&
            s.IdUsers == userId);

        if (session == null)
        {
            _logger.LogWarning("Attempted to revoke non-existent session {SessionId} for user {UserId}", sessionId, userId);
            return false;
        }

        if (session.Consumed)
        {
            _logger.LogInformation("Session {SessionId} already consumed", sessionId);
            return true;
        }

        session.Consumed = true;
        session.UpdateTime = DateTime.UtcNow;

        await _repository.UpdateAsync(session);

        _logger.LogInformation("Session {SessionId} revoked for user {UserId}", sessionId, userId);

        return true;
    }

    public async Task<bool> RevokeAllSessionsExceptCurrent(Guid userId, Guid currentSessionId)
    {
        var sessions = await _repository.ListAsync(s =>
            s.IdUsers == userId &&
            s.Id != currentSessionId &&
            !s.Consumed &&
            s.ExpiresAt > DateTime.UtcNow);

        if (!sessions.Any())
        {
            _logger.LogInformation("No other active sessions found for user {UserId}", userId);
            return true;
        }

        foreach (var session in sessions)
        {
            session.Consumed = true;
            session.UpdateTime = DateTime.UtcNow;
            await _repository.UpdateAsync(session);
        }

        _logger.LogInformation("Revoked {Count} sessions (excluding current) for user {UserId}",
            sessions.Count(), userId);

        return true;
    }
}
