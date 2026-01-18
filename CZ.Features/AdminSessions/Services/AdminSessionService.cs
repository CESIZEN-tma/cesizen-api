using api.CZ.Features.AdminSessions.Factories;
using api.CZ.Features.AdminSessions.Models;
using api.CZ.Features.AdminSessions.Repositories;

namespace api.CZ.Features.AdminSessions.Services;

public class AdminSessionService : IAdminSessionService
{
    private readonly IAdminSessionRepository _repository;
    private readonly IAdminSessionFactory _factory;
    private readonly ILogger<AdminSessionService> _logger;

    public AdminSessionService(
        IAdminSessionRepository repository,
        IAdminSessionFactory factory,
        ILogger<AdminSessionService> logger)
    {
        _repository = repository;
        _factory = factory;
        _logger = logger;
    }

    public async Task<AdminSession?> GetByRefreshToken(string refreshToken)
    {
        var session = await _repository.FirstOrDefaultAsync(s =>
            s.Token == refreshToken &&
            !s.Consumed &&
            s.ExpiresAt > DateTime.UtcNow);

        return session;
    }

    public async Task<AdminSession> CreateSession(Guid adminId, string refreshToken, DateTime expiresAt)
    {
        var session = _factory.Create(adminId, refreshToken, expiresAt);
        await _repository.AddAsync(session);

        _logger.LogInformation("Admin session created for administrator {AdminId}, expires at {ExpiresAt}",
            adminId, expiresAt);

        return session;
    }

    public async Task<bool> ConsumeSession(string refreshToken)
    {
        var session = await _repository.FirstOrDefaultAsync(s => s.Token == refreshToken);

        if (session == null)
        {
            _logger.LogWarning("Attempted to consume non-existent admin session with token");
            return false;
        }

        if (session.Consumed)
        {
            _logger.LogWarning("Attempted to consume already consumed admin session {SessionId}", session.Id);
            return false;
        }

        if (session.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Attempted to consume expired admin session {SessionId}", session.Id);
            return false;
        }

        session.Consumed = true;
        session.UpdateTime = DateTime.UtcNow;

        await _repository.UpdateAsync(session);

        _logger.LogInformation("Admin session {SessionId} consumed successfully", session.Id);

        return true;
    }

    public async Task<bool> RevokeAllAdminSessions(Guid adminId)
    {
        var sessions = await _repository.ListAsync(s =>
            s.IdAdministrators == adminId &&
            !s.Consumed &&
            s.ExpiresAt > DateTime.UtcNow);

        if (!sessions.Any())
        {
            _logger.LogInformation("No active sessions found for administrator {AdminId}", adminId);
            return true;
        }

        foreach (var session in sessions)
        {
            session.Consumed = true;
            session.UpdateTime = DateTime.UtcNow;
            await _repository.UpdateAsync(session);
        }

        _logger.LogInformation("Revoked {Count} active sessions for administrator {AdminId}",
            sessions.Count, adminId);

        return true;
    }

    public async Task<bool> RevokeSession(Guid sessionId)
    {
        var session = await _repository.FindAsync(sessionId);

        if (session == null)
        {
            _logger.LogWarning("Attempted to revoke non-existent admin session {SessionId}", sessionId);
            return false;
        }

        if (session.Consumed)
        {
            _logger.LogInformation("Admin session {SessionId} already consumed", sessionId);
            return true;
        }

        session.Consumed = true;
        session.UpdateTime = DateTime.UtcNow;

        await _repository.UpdateAsync(session);

        _logger.LogInformation("Admin session {SessionId} revoked successfully", sessionId);

        return true;
    }

    public async Task CleanupExpiredSessions()
    {
        var expiredSessions = await _repository.ListAsync(s =>
            s.ExpiresAt < DateTime.UtcNow &&
            !s.Consumed);

        if (!expiredSessions.Any())
        {
            _logger.LogDebug("No expired admin sessions to cleanup");
            return;
        }

        foreach (var session in expiredSessions)
        {
            session.Consumed = true;
            session.UpdateTime = DateTime.UtcNow;
            await _repository.UpdateAsync(session);
        }

        _logger.LogInformation("Cleaned up {Count} expired admin sessions", expiredSessions.Count);
    }
}
