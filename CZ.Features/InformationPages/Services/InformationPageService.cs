using api.CZ.Features.AdminLogs.Services;
using api.CZ.Features.InformationPages.DTOs;
using api.CZ.Features.InformationPages.Extensions;
using api.CZ.Features.InformationPages.Models;
using api.CZ.Features.InformationPages.Repositories;

namespace api.CZ.Features.InformationPages.Services;

public class InformationPageService : IInformationPageService
{
    private readonly IInformationPageRepository _repository;
    private readonly IAdminActionLogger _actionLogger;

    public InformationPageService(IInformationPageRepository repository, IAdminActionLogger actionLogger)
    {
        _repository = repository;
        _actionLogger = actionLogger;
    }

    public async Task<IEnumerable<GetInformationPageDto>> GetAllAsync()
    {
        var pages = await _repository.ListAsync(p => p.DeletionTime == null);

        return pages.Select(p => p.ToDto());
    }

    public async Task<GetInformationPageDto?> GetByIdAsync(Guid id)
    {
        var page = await _repository.FindAsync(id);

        if (page == null || page.DeletionTime != null)
            return null;

        return page.ToDto();
    }

    public async Task<GetInformationPageDto?> CreateAsync(CreateInformationPageDto dto, Guid adminId)
    {
        var page = new InformationPage
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            Content = dto.Content,
            ContentType = dto.ContentType,
            CurrentlyEditing = false,
            Status = dto.Status,
            Active = dto.Active,
            IdAdministrators = adminId,
            CreationTime = DateTime.UtcNow
        };

        await _repository.AddAsync(page);

        // Log the create action
        await _actionLogger.LogCreateAsync(adminId, "InformationPage", page.Id,
            $"Created information page '{page.Title}'");

        return page.ToDto();
    }

    public async Task<GetInformationPageDto?> UpdateAsync(Guid id, UpdateInformationPageDto dto, Guid adminId)
    {
        var page = await _repository.FindAsync(id);

        if (page == null || page.DeletionTime != null)
            return null;

        page.Title = dto.Title;
        page.Description = dto.Description;
        page.Content = dto.Content;
        page.ContentType = dto.ContentType;
        page.CurrentlyEditing = dto.CurrentlyEditing;
        page.Status = dto.Status;
        page.Active = dto.Active;
        page.UpdateTime = DateTime.UtcNow;

        await _repository.UpdateAsync(page);

        // Log the update action
        await _actionLogger.LogUpdateAsync(adminId, "InformationPage", page.Id,
            $"Updated information page '{page.Title}'");

        return page.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id, Guid adminId)
    {
        var page = await _repository.FindAsync(id);

        if (page == null || page.DeletionTime != null)
            return false;

        var pageTitle = page.Title;

        page.DeletionTime = DateTime.UtcNow;
        page.UpdateTime = DateTime.UtcNow;
        page.Active = false;

        await _repository.SoftDeleteAsync(page);

        // Log the delete action
        await _actionLogger.LogDeleteAsync(adminId, "InformationPage", page.Id,
            $"Deleted information page '{pageTitle}'");

        return true;
    }
}
