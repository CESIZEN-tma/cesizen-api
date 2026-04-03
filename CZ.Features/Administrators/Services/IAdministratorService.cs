using api.CZ.Features.Administrators.DTOs;

namespace api.CZ.Features.Administrators.Services;

public interface IAdministratorService
{
    Task<IEnumerable<GetAdministratorDto>> GetAllAsync();
    Task<GetAdministratorDto?> GetByIdAsync(Guid id);
    Task<GetAdministratorDto?> CreateAsync(CreateAdministratorDto dto, Guid creatorAdminId);
    Task<GetAdministratorDto?> UpdateAsync(Guid id, UpdateAdministratorDto dto, Guid adminId);
    Task<bool> DeleteAsync(Guid id, Guid adminId);
}
