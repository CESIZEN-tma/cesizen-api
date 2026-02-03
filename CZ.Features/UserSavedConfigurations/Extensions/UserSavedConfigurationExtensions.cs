using api.CZ.Features.UserSavedConfigurations.DTOs;
using api.CZ.Features.UserSavedConfigurations.Models;

namespace api.CZ.Features.UserSavedConfigurations.Extensions;

public static class UserSavedConfigurationExtensions
{
    public static GetUserSavedConfigurationDto ToDto(this UserSavedConfiguration config)
    {
        return new GetUserSavedConfigurationDto
        {
            Id = config.Id,
            Name = config.Name,
            Inhalation = config.Inhalation,
            Retention1 = config.Retention1,
            Exhalation = config.Exhalation,
            Retention2 = config.Retention2,
            DurationMinutes = config.DurationMinutes,
            Difficulty = config.Difficulty,
            Objective = config.Objective,
            GuidanceType = config.GuidanceType,
            CreationTime = config.CreationTime,
            UpdateTime = config.UpdateTime
        };
    }
}
