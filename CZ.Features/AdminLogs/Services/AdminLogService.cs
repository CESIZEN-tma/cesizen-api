using api.CZ.Features.AdminLogs.DTOs;
using api.CZ.Features.AdminLogs.Enums;
using api.CZ.Features.AdminLogs.Factories;
using api.CZ.Features.AdminLogs.Repositories;

namespace api.CZ.Features.AdminLogs.Services;

public class AdminLogService : IAdminLogService
{
    private readonly IAdminLogRepository _repository;
    private readonly IAdminLogFactory _factory;

    public AdminLogService(IAdminLogRepository repository, IAdminLogFactory factory)
    {
        _repository = repository;
        _factory = factory;
    }

    public async Task LogActionAsync(Guid adminId, AdminActionCode actionCode, string entityType, Guid targetedEntityId, string description)
    {
        var log = _factory.Create(adminId, actionCode, entityType, targetedEntityId, description);
        await _repository.AddAsync(log);
    }

    public async Task<IEnumerable<GetAdminLogDto>> GetFilteredLogsAsync(AdminLogFilterDto filter)
    {
        var logs = await _repository.GetFilteredLogsAsync(filter);

        return logs.Select(l => new GetAdminLogDto
        {
            Id = l.Id,
            ActionCode = l.ActionCode,
            EntityType = l.EntityType,
            TargetedEntityId = l.TargetedEntityId,
            Description = l.Description,
            CreationTime = l.CreationTime,
            AdministratorId = l.IdAdministrator,
            AdministratorEmail = l.IdAdministratorNavigation.Email,
            AdministratorName = $"{l.IdAdministratorNavigation.FirstName} {l.IdAdministratorNavigation.LastName}"
        });
    }

    public async Task<IEnumerable<GetAdminLogDto>> GetRecentLogsAsync(int count = 50)
    {
        var logs = await _repository.GetRecentLogsAsync(count);

        return logs.Select(l => new GetAdminLogDto
        {
            Id = l.Id,
            ActionCode = l.ActionCode,
            EntityType = l.EntityType,
            TargetedEntityId = l.TargetedEntityId,
            Description = l.Description,
            CreationTime = l.CreationTime,
            AdministratorId = l.IdAdministrator,
            AdministratorEmail = l.IdAdministratorNavigation.Email,
            AdministratorName = $"{l.IdAdministratorNavigation.FirstName} {l.IdAdministratorNavigation.LastName}"
        });
    }

    public async Task<IEnumerable<GetAdminLogDto>> GetLogsByAdministratorAsync(Guid adminId)
    {
        var logs = await _repository.GetLogsByAdministratorAsync(adminId);

        return logs.Select(l => new GetAdminLogDto
        {
            Id = l.Id,
            ActionCode = l.ActionCode,
            EntityType = l.EntityType,
            TargetedEntityId = l.TargetedEntityId,
            Description = l.Description,
            CreationTime = l.CreationTime,
            AdministratorId = l.IdAdministrator,
            AdministratorEmail = l.IdAdministratorNavigation.Email,
            AdministratorName = $"{l.IdAdministratorNavigation.FirstName} {l.IdAdministratorNavigation.LastName}"
        });
    }

    public async Task<IEnumerable<GetAdminLogDto>> GetLogsByEntityAsync(string entityType, Guid entityId)
    {
        var logs = await _repository.GetLogsByEntityAsync(entityType, entityId);

        return logs.Select(l => new GetAdminLogDto
        {
            Id = l.Id,
            ActionCode = l.ActionCode,
            EntityType = l.EntityType,
            TargetedEntityId = l.TargetedEntityId,
            Description = l.Description,
            CreationTime = l.CreationTime,
            AdministratorId = l.IdAdministrator,
            AdministratorEmail = l.IdAdministratorNavigation.Email,
            AdministratorName = $"{l.IdAdministratorNavigation.FirstName} {l.IdAdministratorNavigation.LastName}"
        });
    }

    public async Task<EntityLineageDto> GetEntityLineageAsync(string entityType, Guid entityId)
    {
        var logs = await _repository.GetLogsByEntityAsync(entityType, entityId);

        var events = logs
            .OrderBy(l => l.CreationTime)
            .Select((l, index) => new LineageEventDto
            {
                Step = index + 1,
                LogId = l.Id,
                ActionCode = l.ActionCode,
                Description = l.Description,
                OccurredAt = l.CreationTime,
                AdministratorId = l.IdAdministrator,
                AdministratorEmail = l.IdAdministratorNavigation.Email,
                AdministratorName = $"{l.IdAdministratorNavigation.FirstName} {l.IdAdministratorNavigation.LastName}"
            })
            .ToList();

        return new EntityLineageDto
        {
            EntityType = entityType,
            EntityId = entityId,
            TotalEvents = events.Count,
            Events = events
        };
    }
}
