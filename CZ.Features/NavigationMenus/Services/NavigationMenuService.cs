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

        var dtos = menus.OrderBy(m => m.Position)
                        .Select(m => m.ToDto())
                        .ToList();

        var lookup = dtos.ToDictionary(m => m.Id);
        var roots = new List<GetNavigationMenuDto>();

        foreach (var dto in dtos)
        {
            if (dto.ParentId == null)
            {
                roots.Add(dto);
            }
            else if (lookup.TryGetValue(dto.ParentId.Value, out var parent))
            {
                parent.Children.Add(dto);
            }
        }

        return roots;
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
        // When a sub-menu is created under a parent, clear the parent's URL
        if (dto.ParentId.HasValue)
        {
            var parent = await _repository.FindAsync(dto.ParentId.Value);
            if (parent != null && parent.Url != null)
            {
                parent.Url = null;
                parent.UpdateTime = DateTime.UtcNow;
                await _repository.UpdateAsync(parent);
            }
        }

        var menu = new NavigationMenu
        {
            Id = Guid.NewGuid(),
            ParentId = dto.ParentId,
            Position = dto.Position,
            Label = dto.Label,
            Url = dto.ParentId.HasValue ? null : dto.Url,
            CurrentlyEditing = false,
            CreationTime = DateTime.UtcNow
        };

        await _repository.AddAsync(menu);

        // Log the create action
        await _actionLogger.LogCreateAsync(adminId, "NavigationMenu", menu.Id,
            $"Created navigation menu '{menu.Label}' [Position={menu.Position}, Level={(menu.ParentId.HasValue ? $"sub-menu of {menu.ParentId}" : "root")}{(menu.Url != null ? $", URL={menu.Url}" : "")}]");

        return menu.ToDto();
    }

    public async Task<GetNavigationMenuDto?> UpdateAsync(Guid id, UpdateNavigationMenuDto dto, Guid adminId)
    {
        var menu = await _repository.FindAsync(id);

        if (menu == null || menu.DeletionTime != null)
            return null;

        var changes = new List<string>();
        if (menu.Label != dto.Label) changes.Add($"Label: '{menu.Label}' → '{dto.Label}'");
        if (menu.Position != dto.Position) changes.Add($"Position: {menu.Position} → {dto.Position}");
        if (menu.ParentId != dto.ParentId) changes.Add($"Parent: {menu.ParentId?.ToString() ?? "root"} → {dto.ParentId?.ToString() ?? "root"}");
        if (menu.Url != dto.Url) changes.Add($"URL: '{menu.Url ?? "none"}' → '{dto.Url ?? "none"}'");
        var changesDescription = changes.Count > 0 ? string.Join(", ", changes) : "no changes";

        menu.ParentId = dto.ParentId;
        menu.Position = dto.Position;
        menu.Label = dto.Label;
        menu.Url = dto.Url;
        menu.CurrentlyEditing = dto.CurrentlyEditing;
        menu.UpdateTime = DateTime.UtcNow;

        await _repository.UpdateAsync(menu);

        // Log the update action
        await _actionLogger.LogUpdateAsync(adminId, "NavigationMenu", menu.Id,
            $"Updated navigation menu '{dto.Label}': {changesDescription}");

        return menu.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id, Guid adminId)
    {
        var menu = await _repository.FindAsync(id);

        if (menu == null || menu.DeletionTime != null)
            return false;

        var menuLabel = menu.Label;

        // Cascade soft-delete children
        var children = await _repository.ListAsync(m => m.ParentId == id && m.DeletionTime == null);
        foreach (var child in children)
        {
            child.DeletionTime = DateTime.UtcNow;
            child.UpdateTime = DateTime.UtcNow;
            await _repository.SoftDeleteAsync(child);
        }

        menu.DeletionTime = DateTime.UtcNow;
        menu.UpdateTime = DateTime.UtcNow;

        await _repository.SoftDeleteAsync(menu);

        await _actionLogger.LogDeleteAsync(adminId, "NavigationMenu", menu.Id,
            $"Deleted navigation menu '{menuLabel}' and {children.Count} sub-menu(s)");

        return true;
    }

    public async Task UpdatePositionsAsync(List<UpdateMenuPositionDto> positions, Guid adminId)
    {
        foreach (var pos in positions)
        {
            var menu = await _repository.FindAsync(pos.Id);
            if (menu == null || menu.DeletionTime != null) continue;

            menu.Position = pos.Position;
            menu.UpdateTime = DateTime.UtcNow;
            await _repository.UpdateAsync(menu);
        }

        var posDetails = string.Join(", ", positions.Select(p => $"{p.Id}→pos{p.Position}"));
        await _actionLogger.LogUpdateAsync(adminId, "NavigationMenu", Guid.Empty,
            $"Reordered {positions.Count} menu item(s): {posDetails}");
    }
}
