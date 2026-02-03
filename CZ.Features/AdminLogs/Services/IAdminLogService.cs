using api.CZ.Features.AdminLogs.DTOs;
using api.CZ.Features.AdminLogs.Enums;

namespace api.CZ.Features.AdminLogs.Services;

public interface IAdminLogService
{
    Task LogActionAsync(Guid adminId, AdminActionCode actionCode, string entityType, Guid targetedEntityId, string description);
    Task<IEnumerable<GetAdminLogDto>> GetFilteredLogsAsync(AdminLogFilterDto filter);
    Task<IEnumerable<GetAdminLogDto>> GetRecentLogsAsync(int count = 50);
    Task<IEnumerable<GetAdminLogDto>> GetLogsByAdministratorAsync(Guid adminId);
    Task<IEnumerable<GetAdminLogDto>> GetLogsByEntityAsync(string entityType, Guid entityId);
}
