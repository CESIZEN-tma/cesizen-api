using api.CZ.Features.Users.UserDtos;
using api.CZ.Features.Users.Models;

namespace api.CZ.Features.Users.Extensions;

public static class UserExtensions
{
    public static GetUserProfileDto ToProfileDto(this User user)
    {
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
}
