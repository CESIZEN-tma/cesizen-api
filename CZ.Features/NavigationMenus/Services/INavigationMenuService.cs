using api.CZ.Features.NavigationMenus.DTOs;

namespace api.CZ.Features.NavigationMenus.Services;

public interface INavigationMenuService
{
    Task<IEnumerable<GetNavigationMenuDto>> GetAllAsync();
    Task<GetNavigationMenuDto?> GetByIdAsync(Guid id);
    Task<GetNavigationMenuDto?> CreateAsync(CreateNavigationMenuDto dto, Guid adminId);
    Task<GetNavigationMenuDto?> UpdateAsync(Guid id, UpdateNavigationMenuDto dto, Guid adminId);
    Task<bool> DeleteAsync(Guid id, Guid adminId);
    Task UpdatePositionsAsync(List<UpdateMenuPositionDto> positions, Guid adminId);
}
