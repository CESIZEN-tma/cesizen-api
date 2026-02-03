using api.CZ.Features.AdminLogs.Services;
using api.CZ.Features.NavigationMenus.DTOs;
using api.CZ.Features.NavigationMenus.Extensions;
using api.CZ.Features.NavigationMenus.Models;
using api.CZ.Features.NavigationMenus.Repositories;

namespace api.CZ.Features.NavigationMenus.Services;

public class NavigationMenuService : INavigationMenuService
{
    private readonly INavigationMenuRepository _repository;
    private readonly IAdminActionLogger _actionLogger;

    public NavigationMenuService(INavigationMenuRepository repository, IAdminActionLogger actionLogger)
    {
        _repository = repository;
        _actionLogger = actionLogger;
    }

    public async Task<IEnumerable<GetNavigationMenuDto>> GetAllAsync()
    {
        var menus = await _repository.ListAsync(m => m.DeletionTime == null);

        return menus.OrderBy(m => m.Position).Select(m => m.ToDto());
    }

    public async Task<GetNavigationMenuDto?> GetByIdAsync(Guid id)
    {
        var menu = await _repository.FindAsync(id);

        if (menu == null || menu.DeletionTime != null)
            return null;

        return menu.ToDto();
    }

    public async Task<GetNavigationMenuDto?> CreateAsync(CreateNavigationMenuDto dto, Guid adminId)
    {
        var menu = new NavigationMenu
        {
            Id = Guid.NewGuid(),
            Position = dto.Position,
            Label = dto.Label,
            Url = dto.Url,
            CurrentlyEditing = false,
            CreationTime = DateTime.UtcNow
        };

        await _repository.AddAsync(menu);

        // Log the create action
        await _actionLogger.LogCreateAsync(adminId, "NavigationMenu", menu.Id,
            $"Created navigation menu '{menu.Label}' at position {menu.Position}");

        return menu.ToDto();
    }

    public async Task<GetNavigationMenuDto?> UpdateAsync(Guid id, UpdateNavigationMenuDto dto, Guid adminId)
    {
        var menu = await _repository.FindAsync(id);

        if (menu == null || menu.DeletionTime != null)
            return null;

        menu.Position = dto.Position;
        menu.Label = dto.Label;
        menu.Url = dto.Url;
        menu.CurrentlyEditing = dto.CurrentlyEditing;
        menu.UpdateTime = DateTime.UtcNow;

        await _repository.UpdateAsync(menu);

        // Log the update action
        await _actionLogger.LogUpdateAsync(adminId, "NavigationMenu", menu.Id,
            $"Updated navigation menu '{menu.Label}' at position {menu.Position}");

        return menu.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id, Guid adminId)
    {
        var menu = await _repository.FindAsync(id);

        if (menu == null || menu.DeletionTime != null)
            return false;

        var menuLabel = menu.Label;

        menu.DeletionTime = DateTime.UtcNow;
        menu.UpdateTime = DateTime.UtcNow;

        await _repository.SoftDeleteAsync(menu);

        // Log the delete action
        await _actionLogger.LogDeleteAsync(adminId, "NavigationMenu", menu.Id,
            $"Deleted navigation menu '{menuLabel}'");

        return true;
    }
}
