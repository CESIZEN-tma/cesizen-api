using api.CZ.Features.AdminLogs.Services;
using api.CZ.Features.Configurations.DTOs;
using api.CZ.Features.Configurations.Extensions;
using api.CZ.Features.Configurations.Models;
using api.CZ.Features.Configurations.Repositories;

namespace api.CZ.Features.Configurations.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly IConfigurationRepository _repository;
    private readonly IAdminActionLogger _actionLogger;

    public ConfigurationService(IConfigurationRepository repository, IAdminActionLogger actionLogger)
    {
        _repository = repository;
        _actionLogger = actionLogger;
    }

    public async Task<IEnumerable<GetConfigurationDto>> GetAllAsync()
    {
        var configurations = await _repository.ListAsync(c => c.DeletionTime == null);
        return configurations.Select(c => c.ToDto());
    }

    public async Task<GetConfigurationDto?> GetByIdAsync(Guid id)
    {
        var configuration = await _repository.FindAsync(id);

        if (configuration == null || configuration.DeletionTime != null)
            return null;

        return configuration.ToDto();
    }

    public async Task<GetConfigurationDto?> CreateAsync(CreateConfigurationDto dto, Guid adminId)
    {
        var configuration = new Configuration
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Inhalation = dto.Inhalation,
            Retention1 = dto.Retention1,
            Exhalation = dto.Exhalation,
            Retention2 = dto.Retention2,
            DurationMinutes = dto.DurationMinutes,
            Difficulty = dto.Difficulty,
            Objective = dto.Objective,
            GuidanceType = dto.GuidanceType,
            IdAdministrators = adminId,
            CreationTime = DateTime.UtcNow
        };

        await _repository.AddAsync(configuration);

        // Log the create action
        await _actionLogger.LogCreateAsync(adminId, "Configuration", configuration.Id,
            $"Created configuration '{configuration.Name}'");

        return configuration.ToDto();
    }

    public async Task<GetConfigurationDto?> UpdateAsync(Guid id, UpdateConfigurationDto dto, Guid adminId)
    {
        var configuration = await _repository.FindAsync(id);

        if (configuration == null || configuration.DeletionTime != null)
            return null;

        configuration.Name = dto.Name;
        configuration.Inhalation = dto.Inhalation;
        configuration.Retention1 = dto.Retention1;
        configuration.Exhalation = dto.Exhalation;
        configuration.Retention2 = dto.Retention2;
        configuration.DurationMinutes = dto.DurationMinutes;
        configuration.Difficulty = dto.Difficulty;
        configuration.Objective = dto.Objective;
        configuration.GuidanceType = dto.GuidanceType;
        configuration.UpdateTime = DateTime.UtcNow;

        await _repository.UpdateAsync(configuration);

        // Log the update action
        await _actionLogger.LogUpdateAsync(adminId, "Configuration", configuration.Id,
            $"Updated configuration '{configuration.Name}'");

        return configuration.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id, Guid adminId)
    {
        var configuration = await _repository.FindAsync(id);

        if (configuration == null || configuration.DeletionTime != null)
            return false;

        var configName = configuration.Name;

        configuration.DeletionTime = DateTime.UtcNow;
        configuration.UpdateTime = DateTime.UtcNow;

        await _repository.SoftDeleteAsync(configuration);

        // Log the delete action
        await _actionLogger.LogDeleteAsync(adminId, "Configuration", configuration.Id,
            $"Deleted configuration '{configName}'");

        return true;
    }
}
