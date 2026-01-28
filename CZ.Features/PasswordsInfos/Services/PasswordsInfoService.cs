using api.CZ.Features.PasswordsInfos.DTOs;
using api.CZ.Features.PasswordsInfos.Models;
using api.CZ.Features.PasswordsInfos.Repositories;

namespace api.CZ.Features.PasswordsInfos.Services;

public class PasswordsInfoService : IPasswordsInfoService
{
    private readonly IPasswordsInfoRepository _repository;

    public PasswordsInfoService(IPasswordsInfoRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<GetPasswordsInfoDto>> GetAllAsync()
    {
        var infos = await _repository.ListAsync(p => p.DeletionTime == null);

        return infos.Select(p => new GetPasswordsInfoDto
        {
            Id = p.Id,
            AttemptCount = p.AttemptCount,
            LastLogin = p.LastLogin,
            LastReset = p.LastReset,
            CreationTime = p.CreationTime,
            UpdateTime = p.UpdateTime
        });
    }

    public async Task<GetPasswordsInfoDto?> GetByIdAsync(Guid id)
    {
        var info = await _repository.FindAsync(id);

        if (info == null || info.DeletionTime != null)
            return null;

        return new GetPasswordsInfoDto
        {
            Id = info.Id,
            AttemptCount = info.AttemptCount,
            LastLogin = info.LastLogin,
            LastReset = info.LastReset,
            CreationTime = info.CreationTime,
            UpdateTime = info.UpdateTime
        };
    }

    public async Task<GetPasswordsInfoDto?> CreateAsync(CreatePasswordsInfoDto dto)
    {
        var info = new PasswordsInfo
        {
            Id = Guid.NewGuid(),
            AttemptCount = dto.AttemptCount,
            LastLogin = DateTime.UtcNow,
            LastReset = DateTime.UtcNow,
            CreationTime = DateTime.UtcNow
        };

        await _repository.AddAsync(info);

        return new GetPasswordsInfoDto
        {
            Id = info.Id,
            AttemptCount = info.AttemptCount,
            LastLogin = info.LastLogin,
            LastReset = info.LastReset,
            CreationTime = info.CreationTime,
            UpdateTime = info.UpdateTime
        };
    }

    public async Task<GetPasswordsInfoDto?> UpdateAsync(Guid id, UpdatePasswordsInfoDto dto)
    {
        var info = await _repository.FindAsync(id);

        if (info == null || info.DeletionTime != null)
            return null;

        info.AttemptCount = dto.AttemptCount;

        if (dto.LastLogin.HasValue)
            info.LastLogin = dto.LastLogin.Value;

        if (dto.LastReset.HasValue)
            info.LastReset = dto.LastReset.Value;

        info.UpdateTime = DateTime.UtcNow;

        await _repository.UpdateAsync(info);

        return new GetPasswordsInfoDto
        {
            Id = info.Id,
            AttemptCount = info.AttemptCount,
            LastLogin = info.LastLogin,
            LastReset = info.LastReset,
            CreationTime = info.CreationTime,
            UpdateTime = info.UpdateTime
        };
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var info = await _repository.FindAsync(id);

        if (info == null || info.DeletionTime != null)
            return false;

        info.DeletionTime = DateTime.UtcNow;
        info.UpdateTime = DateTime.UtcNow;

        await _repository.SoftDeleteAsync(info);

        return true;
    }
}
