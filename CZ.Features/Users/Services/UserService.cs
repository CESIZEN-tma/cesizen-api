using api.CZ.Features.Users.Repositories;
using api.CZ.Features.Users.UserDtos;
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

        return new GetUserProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            MemberSince = user.MemberSince,
            ThumbnailUrl = user.ThumbnailUrl,
            AccountActivated = user.AccountActivated,
            Active = user.Active
        };
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

        return new GetUserProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            MemberSince = user.MemberSince,
            ThumbnailUrl = user.ThumbnailUrl,
            AccountActivated = user.AccountActivated,
            Active = user.Active
        };
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
