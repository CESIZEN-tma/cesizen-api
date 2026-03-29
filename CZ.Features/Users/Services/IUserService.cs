using api.CZ.Features.Users.DTOs;

namespace api.CZ.Features.Users.Services;

public interface IUserService
{
    Task<GetUserProfileDto?> GetProfileAsync(Guid userId);
    Task<GetUserProfileDto?> UpdateProfileAsync(Guid userId, UpdateUserProfileDto dto);
    Task<bool> DeleteAccountAsync(Guid userId);
    Task<bool> UpdateUserStatusAsync(Guid userId, bool active, Guid adminId);
    Task<IEnumerable<GetUserAdminDto>> GetAllForAdminAsync();
}
