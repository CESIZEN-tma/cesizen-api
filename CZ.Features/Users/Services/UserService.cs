using api.CZ.Features.Users.Repositories;
using api.CZ.Features.Users.DTOs;
using api.CZ.Features.Users.Extensions;
using api.CZ.Features.AdminLogs.Services;
using api.CZ.Features.AdminLogs.Enums;

namespace api.CZ.Features.Users.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly IAdminActionLogger _adminActionLogger;

    public UserService(IUserRepository repository, IAdminActionLogger adminActionLogger)
    {
        _repository = repository;
        _adminActionLogger = adminActionLogger;
    }

    public async Task<GetUserProfileDto?> GetProfileAsync(Guid userId)
    {
        var user = await _repository.FindAsync(userId);

        if (user == null || user.DeletionTime != null)
            return null;

        return user.ToProfileDto();
    }

    public async Task<GetUserProfileDto?> UpdateProfileAsync(Guid userId, UpdateUserProfileDto dto)
    {
        var user = await _repository.FindAsync(userId);

        if (user == null || user.DeletionTime != null)
            return null;

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.ThumbnailUrl = dto.ThumbnailUrl;
        user.UpdateTime = DateTime.UtcNow;

        await _repository.UpdateAsync(user);

        return user.ToProfileDto();
    }

    public async Task<bool> DeleteAccountAsync(Guid userId)
    {
        var user = await _repository.FindAsync(userId);

        if (user == null || user.DeletionTime != null)
            return false;

        user.DeletionTime = DateTime.UtcNow;
        user.Active = false;
        user.UpdateTime = DateTime.UtcNow;

        await _repository.SoftDeleteAsync(user);

        return true;
    }

    public async Task<IEnumerable<GetUserAdminDto>> GetAllForAdminAsync()
    {
        var users = await _repository.ListAsync(u => u.DeletionTime == null);

        return users.Select(u => new GetUserAdminDto
        {
            Id = u.Id,
            Email = u.Email,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Active = u.Active,
            AccountActivated = u.AccountActivated,
            MemberSince = u.MemberSince,
            LockedUntil = u.LockedUntil
        });
    }

    public async Task<bool> UpdateUserStatusAsync(Guid userId, bool active, Guid adminId)
    {
        var user = await _repository.FindAsync(userId);

        if (user == null || user.DeletionTime != null)
            return false;

        // No change needed
        if (user.Active == active)
            return true;

        user.Active = active;
        user.UpdateTime = DateTime.UtcNow;

        await _repository.UpdateAsync(user);

        // Log admin action
        await _adminActionLogger.LogCustomActionAsync(
            adminId,
            active ? AdminActionCode.USER_ENABLED : AdminActionCode.USER_DISABLED,
            "User",
            userId,
            $"{(active ? "Enabled" : "Disabled")} user account {user.Email}"
        );

        return true;
    }
}
