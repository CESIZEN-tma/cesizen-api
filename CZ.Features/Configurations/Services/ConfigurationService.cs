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
            $"Created configuration '{configuration.Name}' [Inhalation={configuration.Inhalation}s, Retention1={configuration.Retention1}s, Exhalation={configuration.Exhalation}s, Retention2={configuration.Retention2}s, Duration={configuration.DurationMinutes}min, Difficulty={configuration.Difficulty}/5, Objective='{configuration.Objective}', GuidanceType={configuration.GuidanceType}]");

        return configuration.ToDto();
    }

    public async Task<GetConfigurationDto?> UpdateAsync(Guid id, UpdateConfigurationDto dto, Guid adminId)
    {
        var configuration = await _repository.FindAsync(id);

        if (configuration == null || configuration.DeletionTime != null)
            return null;

        var changes = new List<string>();
        if (configuration.Name != dto.Name) changes.Add($"Name: '{configuration.Name}' → '{dto.Name}'");
        if (configuration.Inhalation != dto.Inhalation) changes.Add($"Inhalation: {configuration.Inhalation}s → {dto.Inhalation}s");
        if (configuration.Retention1 != dto.Retention1) changes.Add($"Retention1: {configuration.Retention1}s → {dto.Retention1}s");
        if (configuration.Exhalation != dto.Exhalation) changes.Add($"Exhalation: {configuration.Exhalation}s → {dto.Exhalation}s");
        if (configuration.Retention2 != dto.Retention2) changes.Add($"Retention2: {configuration.Retention2}s → {dto.Retention2}s");
        if (configuration.DurationMinutes != dto.DurationMinutes) changes.Add($"Duration: {configuration.DurationMinutes}min → {dto.DurationMinutes}min");
        if (configuration.Difficulty != dto.Difficulty) changes.Add($"Difficulty: {configuration.Difficulty} → {dto.Difficulty}");
        if (configuration.Objective != dto.Objective) changes.Add($"Objective: '{configuration.Objective}' → '{dto.Objective}'");
        if (configuration.GuidanceType != dto.GuidanceType) changes.Add($"GuidanceType: {configuration.GuidanceType} → {dto.GuidanceType}");
        var changesDescription = changes.Count > 0 ? string.Join(", ", changes) : "no changes";

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
            $"Updated configuration '{dto.Name}': {changesDescription}");

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
