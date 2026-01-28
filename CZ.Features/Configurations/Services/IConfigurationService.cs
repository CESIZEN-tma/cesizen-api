using api.CZ.Features.Configurations.DTOs;

namespace api.CZ.Features.Configurations.Services;

public interface IConfigurationService
{
    Task<IEnumerable<GetConfigurationDto>> GetAllAsync();
    Task<GetConfigurationDto?> GetByIdAsync(Guid id);
    Task<GetConfigurationDto?> CreateAsync(CreateConfigurationDto dto, Guid adminId);
    Task<GetConfigurationDto?> UpdateAsync(Guid id, UpdateConfigurationDto dto, Guid adminId);
    Task<bool> DeleteAsync(Guid id, Guid adminId);
}
