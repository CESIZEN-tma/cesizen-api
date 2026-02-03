using api.CZ.Features.Administrators.DTOs;
using api.CZ.Features.Administrators.Models;

namespace api.CZ.Features.Administrators.Extensions;

public static class AdministratorExtensions
{
    public static GetAdministratorDto ToDto(this Administrator administrator)
    {
        return new GetAdministratorDto
        {
            Id = administrator.Id,
            Email = administrator.Email,
            FirstName = administrator.FirstName,
            LastName = administrator.LastName,
            MemberSince = administrator.MemberSince,
            ThumbnailUrl = administrator.ThumbnailUrl,
            AccountActivated = administrator.AccountActivated,
            CreationTime = administrator.CreationTime,
            UpdateTime = administrator.UpdateTime
        };
    }
}
