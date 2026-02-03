using api.CZ.Features.PasswordHistories.DTOs;

namespace api.CZ.Features.PasswordHistories.Services;

public interface IPasswordHistoryService
{
    Task<IEnumerable<GetPasswordHistoryDto>> GetAllAsync();
    Task<IEnumerable<GetPasswordHistoryDto>> GetByPasswordInfoIdAsync(Guid passwordInfoId);
    Task<GetPasswordHistoryDto?> GetByIdAsync(Guid id);
    Task<GetPasswordHistoryDto?> CreateAsync(CreatePasswordHistoryDto dto);
    Task<bool> DeleteAsync(Guid id);
}
