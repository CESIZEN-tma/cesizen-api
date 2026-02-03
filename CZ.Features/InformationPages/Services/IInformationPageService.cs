using api.CZ.Features.InformationPages.DTOs;

namespace api.CZ.Features.InformationPages.Services;

public interface IInformationPageService
{
    Task<IEnumerable<GetInformationPageDto>> GetAllAsync();
    Task<GetInformationPageDto?> GetByIdAsync(Guid id);
    Task<GetInformationPageDto?> CreateAsync(CreateInformationPageDto dto, Guid adminId);
    Task<GetInformationPageDto?> UpdateAsync(Guid id, UpdateInformationPageDto dto, Guid adminId);
    Task<bool> DeleteAsync(Guid id, Guid adminId);
}
