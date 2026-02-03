using api.CZ.Features.Configurations.DTOs;
using api.CZ.Features.Configurations.Models;

namespace api.CZ.Features.Configurations.Extensions;

public static class ConfigurationExtensions
{
    public static GetConfigurationDto ToDto(this Configuration configuration)
    {
        return new GetConfigurationDto
        {
            Id = configuration.Id,
            Name = configuration.Name,
            Inhalation = configuration.Inhalation,
            Retention1 = configuration.Retention1,
            Exhalation = configuration.Exhalation,
            Retention2 = configuration.Retention2,
            DurationMinutes = configuration.DurationMinutes,
            Difficulty = configuration.Difficulty,
            Objective = configuration.Objective,
            GuidanceType = configuration.GuidanceType,
            CreationTime = configuration.CreationTime,
            UpdateTime = configuration.UpdateTime,
            IdAdministrators = configuration.IdAdministrators
        };
    }
}
