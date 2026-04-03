using api.CZ.Features.PasswordHistories.Models;
using api.CZ.Features.PasswordHistories.Repositories;
using api.CZ.Features.PasswordsInfos.Models;
using api.CZ.Features.PasswordsInfos.Repositories;
using api.CZ.Features.Users.Repositories;
using Simply.Auth.Core.Abstractions;

namespace api.CZ.Features.PasswordHistories.Services;

public class PasswordHistoryManager : IPasswordHistoryManager
{
    private const int MaxPasswordHistory = 5;

    private readonly IUserRepository _userRepository;
    private readonly IPasswordsInfoRepository _passwordsInfoRepository;
    private readonly IPasswordHistoryRepository _passwordHistoryRepository;
    private readonly ISimplyAuthService _authService;
    private readonly ILogger<PasswordHistoryManager> _logger;

    public PasswordHistoryManager(
        IUserRepository userRepository,
        IPasswordsInfoRepository passwordsInfoRepository,
        IPasswordHistoryRepository passwordHistoryRepository,
        ISimplyAuthService authService,
        ILogger<PasswordHistoryManager> logger)
    {
        _userRepository = userRepository;
        _passwordsInfoRepository = passwordsInfoRepository;
        _passwordHistoryRepository = passwordHistoryRepository;
        _authService = authService;
        _logger = logger;
    }

    public async Task<bool> IsPasswordReusedAsync(Guid userId, string newPassword)
    {
        var user = await _userRepository.FindAsync(userId);

        if (user == null || !user.IdPasswordsInfos.HasValue)
        {
            return false;
        }

        var histories = await _passwordHistoryRepository.ListAsync(h =>
            h.IdPasswordsInfos == user.IdPasswordsInfos.Value &&
            h.DeletionTime == null);

        var recentHistories = histories
            .OrderByDescending(h => h.ChangedAt)
            .Take(MaxPasswordHistory)
            .ToList();

        foreach (var history in recentHistories)
        {
            var verificationResult = _authService.VerifyPassword(newPassword, history.PasswordHash);

            if (verificationResult != Simply.Auth.Core.Enums.SimplyVerificationResult.Failed)
            {
                _logger.LogWarning("User {UserId} attempted to reuse a recent password", userId);
                return true;
            }
        }

        return false;
    }

    public async Task AddPasswordToHistoryAsync(Guid userId, string passwordHash)
    {
        var user = await _userRepository.FindAsync(userId);

        if (user == null)
        {
            _logger.LogError("Cannot add password to history: User {UserId} not found", userId);
            return;
        }

        await EnsurePasswordsInfoExistsAsync(userId);

        if (!user.IdPasswordsInfos.HasValue)
        {
            _logger.LogError("Cannot add password to history: PasswordsInfo not created for user {UserId}", userId);
            return;
        }

        var newHistory = new PasswordHistory
        {
            Id = Guid.NewGuid(),
            PasswordHash = passwordHash,
            ChangedAt = DateTime.UtcNow,
            IdPasswordsInfos = user.IdPasswordsInfos.Value,
            CreationTime = DateTime.UtcNow
        };

        await _passwordHistoryRepository.AddAsync(newHistory);

        await CleanupOldPasswordHistoriesAsync(user.IdPasswordsInfos.Value);

        _logger.LogInformation("Password added to history for user {UserId}", userId);
    }

    public async Task EnsurePasswordsInfoExistsAsync(Guid userId)
    {
        var user = await _userRepository.FindAsync(userId);

        if (user == null)
        {
            _logger.LogError("Cannot ensure PasswordsInfo: User {UserId} not found", userId);
            return;
        }

        if (user.IdPasswordsInfos.HasValue)
        {
            var existingInfo = await _passwordsInfoRepository.FindAsync(user.IdPasswordsInfos.Value);
            if (existingInfo != null && existingInfo.DeletionTime == null)
            {
                return;
            }
        }

        var passwordsInfo = new PasswordsInfo
        {
            Id = Guid.NewGuid(),
            AttemptCount = 0,
            LastLogin = DateTime.UtcNow,
            LastReset = DateTime.UtcNow,
            CreationTime = DateTime.UtcNow
        };

        await _passwordsInfoRepository.AddAsync(passwordsInfo);

        user.IdPasswordsInfos = passwordsInfo.Id;
        user.UpdateTime = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("PasswordsInfo created for user {UserId}", userId);
    }

    private async Task CleanupOldPasswordHistoriesAsync(Guid passwordsInfoId)
    {
        var histories = await _passwordHistoryRepository.ListAsync(h =>
            h.IdPasswordsInfos == passwordsInfoId &&
            h.DeletionTime == null);

        var orderedHistories = histories
            .OrderByDescending(h => h.ChangedAt)
            .ToList();

        var historiesToDelete = orderedHistories
            .Skip(MaxPasswordHistory)
            .ToList();

        foreach (var history in historiesToDelete)
        {
            history.DeletionTime = DateTime.UtcNow;
            history.UpdateTime = DateTime.UtcNow;
            await _passwordHistoryRepository.SoftDeleteAsync(history);
        }

        if (historiesToDelete.Any())
        {
            _logger.LogInformation("Cleaned up {Count} old password histories for PasswordsInfo {Id}",
                historiesToDelete.Count, passwordsInfoId);
        }
    }
}
