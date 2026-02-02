using api.CZ.Features.PasswordsInfos.DTOs;
using api.CZ.Features.PasswordsInfos.Models;

namespace api.CZ.Features.PasswordsInfos.Extensions;

public static class PasswordsInfoExtensions
{
    public static GetPasswordsInfoDto ToDto(this PasswordsInfo passwordsInfo)
    {
        return new GetPasswordsInfoDto
        {
            Id = passwordsInfo.Id,
            AttemptCount = passwordsInfo.AttemptCount,
            LastLogin = passwordsInfo.LastLogin,
            LastReset = passwordsInfo.LastReset,
            CreationTime = passwordsInfo.CreationTime,
            UpdateTime = passwordsInfo.UpdateTime
        };
    }
}
