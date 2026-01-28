using api.CZ.Features.PasswordsInfos.DTOs;

namespace api.CZ.Features.PasswordsInfos.Services;

public interface IPasswordsInfoService
{
    Task<IEnumerable<GetPasswordsInfoDto>> GetAllAsync();
    Task<GetPasswordsInfoDto?> GetByIdAsync(Guid id);
    Task<GetPasswordsInfoDto?> CreateAsync(CreatePasswordsInfoDto dto);
    Task<GetPasswordsInfoDto?> UpdateAsync(Guid id, UpdatePasswordsInfoDto dto);
    Task<bool> DeleteAsync(Guid id);
}
