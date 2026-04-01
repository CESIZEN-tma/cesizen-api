using api.CZ.Features.NavigationMenus.DTOs;
using api.CZ.Features.NavigationMenus.Models;

namespace api.CZ.Features.NavigationMenus.Extensions;

public static class NavigationMenuExtensions
{
    public static GetNavigationMenuDto ToDto(this NavigationMenu menu)
    {
        return new GetNavigationMenuDto
        {
            Id = menu.Id,
            ParentId = menu.ParentId,
            Position = menu.Position,
            Label = menu.Label,
            Url = menu.Url,
            CurrentlyEditing = menu.CurrentlyEditing,
            CreationTime = menu.CreationTime,
            UpdateTime = menu.UpdateTime
        };
    }
}
