using api.CZ.Features.AdminLogs.Enums;
using api.CZ.Features.AdminLogs.Models;

namespace api.CZ.Features.AdminLogs.Factories;

public interface IAdminLogFactory
{
    AdminLog Create(Guid adminId, AdminActionCode actionCode, string entityType, Guid targetedEntityId, string description);
}
