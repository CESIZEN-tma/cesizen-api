using api.CZ.Features.PasswordHistories.DTOs;
using api.CZ.Features.PasswordHistories.Models;
using api.CZ.Features.PasswordHistories.Repositories;

namespace api.CZ.Features.PasswordHistories.Services;

public class PasswordHistoryService : IPasswordHistoryService
{
    private readonly IPasswordHistoryRepository _repository;

    public PasswordHistoryService(IPasswordHistoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<GetPasswordHistoryDto>> GetAllAsync()
    {
        var histories = await _repository.ListAsync(h => h.DeletionTime == null);

        return histories.Select(h => new GetPasswordHistoryDto
        {
            Id = h.Id,
            ChangedAt = h.ChangedAt,
            CreationTime = h.CreationTime,
            UpdateTime = h.UpdateTime,
            IdPasswordsInfos = h.IdPasswordsInfos
        });
    }

    public async Task<IEnumerable<GetPasswordHistoryDto>> GetByPasswordInfoIdAsync(Guid passwordInfoId)
    {
        var histories = await _repository.ListAsync(h =>
            h.IdPasswordsInfos == passwordInfoId && h.DeletionTime == null);

        return histories.OrderByDescending(h => h.ChangedAt).Select(h => new GetPasswordHistoryDto
        {
            Id = h.Id,
            ChangedAt = h.ChangedAt,
            CreationTime = h.CreationTime,
            UpdateTime = h.UpdateTime,
            IdPasswordsInfos = h.IdPasswordsInfos
        });
    }

    public async Task<GetPasswordHistoryDto?> GetByIdAsync(Guid id)
    {
        var history = await _repository.FindAsync(id);

        if (history == null || history.DeletionTime != null)
            return null;

        return new GetPasswordHistoryDto
        {
            Id = history.Id,
            ChangedAt = history.ChangedAt,
            CreationTime = history.CreationTime,
            UpdateTime = history.UpdateTime,
            IdPasswordsInfos = history.IdPasswordsInfos
        };
    }

    public async Task<GetPasswordHistoryDto?> CreateAsync(CreatePasswordHistoryDto dto)
    {
        var history = new PasswordHistory
        {
            Id = Guid.NewGuid(),
            PasswordHash = dto.PasswordHash,
            ChangedAt = DateTime.UtcNow,
            IdPasswordsInfos = dto.IdPasswordsInfos,
            CreationTime = DateTime.UtcNow
        };

        await _repository.AddAsync(history);

        return new GetPasswordHistoryDto
        {
            Id = history.Id,
            ChangedAt = history.ChangedAt,
            CreationTime = history.CreationTime,
            UpdateTime = history.UpdateTime,
            IdPasswordsInfos = history.IdPasswordsInfos
        };
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var history = await _repository.FindAsync(id);

        if (history == null || history.DeletionTime != null)
            return false;

        history.DeletionTime = DateTime.UtcNow;
        history.UpdateTime = DateTime.UtcNow;

        await _repository.SoftDeleteAsync(history);

        return true;
    }
}
