using api.CZ.Data.Repositories;
using api.CZ.Features.AdminLogs.DTOs;
using api.CZ.Features.AdminLogs.Models;

namespace api.CZ.Features.AdminLogs.Repositories;

public interface IAdminLogRepository : IBaseRepository<AdminLog>
{
    Task<List<AdminLog>> GetFilteredLogsAsync(AdminLogFilterDto filter);
    Task<List<AdminLog>> GetRecentLogsAsync(int count);
    Task<List<AdminLog>> GetLogsByAdministratorAsync(Guid adminId);
    Task<List<AdminLog>> GetLogsByEntityAsync(string entityType, Guid entityId);
}
