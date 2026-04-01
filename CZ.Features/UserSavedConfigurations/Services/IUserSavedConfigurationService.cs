using api.CZ.Features.UserSavedConfigurations.DTOs;

namespace api.CZ.Features.UserSavedConfigurations.Services;

public interface IUserSavedConfigurationService
{
    Task<IEnumerable<GetUserSavedConfigurationDto>> GetByUserAsync(Guid userId);
    Task<GetUserSavedConfigurationDto?> GetByIdAsync(Guid id);
    Task<GetUserSavedConfigurationDto?> CreateAsync(CreateUserSavedConfigurationDto dto, Guid userId);
    Task<GetUserSavedConfigurationDto?> UpdateAsync(Guid id, UpdateUserSavedConfigurationDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<GetUserSavedConfigurationDto?> CreateFromQuizResponsesAsync(Guid userId, QuizResponsesDto dto);
}
