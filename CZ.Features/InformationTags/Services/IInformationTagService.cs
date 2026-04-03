using api.CZ.Features.InformationTags.DTOs;

namespace api.CZ.Features.InformationTags.Services;

public interface IInformationTagService
{
    Task<IEnumerable<GetInformationTagDto>> GetAllAsync();
    Task<GetInformationTagDto?> GetByIdAsync(Guid id);
    Task<GetInformationTagDto?> CreateAsync(CreateInformationTagDto dto, Guid adminId);
    Task<GetInformationTagDto?> UpdateAsync(Guid id, UpdateInformationTagDto dto, Guid adminId);
    Task<bool> DeleteAsync(Guid id, Guid adminId);
}
