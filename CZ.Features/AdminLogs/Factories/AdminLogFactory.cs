using api.CZ.Core.Factories;
using api.CZ.Features.AdminLogs.Enums;
using api.CZ.Features.AdminLogs.Models;

namespace api.CZ.Features.AdminLogs.Factories;

public class AdminLogFactory : BaseFactory<AdminLog>, IAdminLogFactory
{
    protected override AdminLog CreateInstance(params object[] parameters)
    {
        return new AdminLog
        {
            Id = Guid.NewGuid(),
            CreationTime = DateTime.UtcNow
        };
    }

    public AdminLog Create(Guid adminId, AdminActionCode actionCode, string entityType, Guid targetedEntityId, string description)
    {
        var log = CreateInstance();
        log.IdAdministrator = adminId;
        log.ActionCode = actionCode.ToString();
        log.EntityType = entityType;
        log.TargetedEntityId = targetedEntityId;
        log.Description = description;
        return log;
    }
}
