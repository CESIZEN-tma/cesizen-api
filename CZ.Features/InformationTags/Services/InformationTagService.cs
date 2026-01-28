using api.CZ.Features.InformationTags.DTOs;
using api.CZ.Features.InformationTags.Models;
using api.CZ.Features.InformationTags.Repositories;
using api.CZ.Features.AdminLogs.Services;

namespace api.CZ.Features.InformationTags.Services;

public class InformationTagService : IInformationTagService
{
    private readonly IInformationTagRepository _repository;
    private readonly IAdminActionLogger _actionLogger;

    public InformationTagService(IInformationTagRepository repository, IAdminActionLogger actionLogger)
    {
        _repository = repository;
        _actionLogger = actionLogger;
    }

    public async Task<IEnumerable<GetInformationTagDto>> GetAllAsync()
    {
        var tags = await _repository.ListAsync(t => t.DeletionTime == null);

        return tags.Select(t => new GetInformationTagDto
        {
            Id = t.Id,
            Label = t.Label,
            CreationTime = t.CreationTime,
            UpdateTime = t.UpdateTime
        });
    }

    public async Task<GetInformationTagDto?> GetByIdAsync(Guid id)
    {
        var tag = await _repository.FindAsync(id);

        if (tag == null || tag.DeletionTime != null)
            return null;

        return new GetInformationTagDto
        {
            Id = tag.Id,
            Label = tag.Label,
            CreationTime = tag.CreationTime,
            UpdateTime = tag.UpdateTime
        };
    }

    public async Task<GetInformationTagDto?> CreateAsync(CreateInformationTagDto dto, Guid adminId)
    {
        var tag = new InformationTag
        {
            Id = Guid.NewGuid(),
            Label = dto.Label,
            CreationTime = DateTime.UtcNow
        };

        await _repository.AddAsync(tag);

        await _actionLogger.LogCreateAsync(adminId, "InformationTag", tag.Id,
            $"Created information tag '{tag.Label}'");

        return new GetInformationTagDto
        {
            Id = tag.Id,
            Label = tag.Label,
            CreationTime = tag.CreationTime,
            UpdateTime = tag.UpdateTime
        };
    }

    public async Task<GetInformationTagDto?> UpdateAsync(Guid id, UpdateInformationTagDto dto, Guid adminId)
    {
        var tag = await _repository.FindAsync(id);

        if (tag == null || tag.DeletionTime != null)
            return null;

        tag.Label = dto.Label;
        tag.UpdateTime = DateTime.UtcNow;

        await _repository.UpdateAsync(tag);

        await _actionLogger.LogUpdateAsync(adminId, "InformationTag", id,
            $"Updated information tag to '{tag.Label}'");

        return new GetInformationTagDto
        {
            Id = tag.Id,
            Label = tag.Label,
            CreationTime = tag.CreationTime,
            UpdateTime = tag.UpdateTime
        };
    }

    public async Task<bool> DeleteAsync(Guid id, Guid adminId)
    {
        var tag = await _repository.FindAsync(id);

        if (tag == null || tag.DeletionTime != null)
            return false;

        var tagLabel = tag.Label;

        tag.DeletionTime = DateTime.UtcNow;
        tag.UpdateTime = DateTime.UtcNow;

        await _repository.SoftDeleteAsync(tag);

        await _actionLogger.LogDeleteAsync(adminId, "InformationTag", id,
            $"Deleted information tag '{tagLabel}'");

        return true;
    }
}
