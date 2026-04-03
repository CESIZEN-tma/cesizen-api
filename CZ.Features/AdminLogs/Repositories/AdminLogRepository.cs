using Microsoft.EntityFrameworkCore;
using api.CZ.Data.EFCore;
using api.CZ.Data.Repositories;
using api.CZ.Features.AdminLogs.DTOs;
using api.CZ.Features.AdminLogs.Models;

namespace api.CZ.Features.AdminLogs.Repositories;

public class AdminLogRepository : BaseRepository<AdminLog>, IAdminLogRepository
{
    private readonly CesiZenDbContext _ctx;

    public AdminLogRepository(CesiZenDbContext context) : base(context)
    {
        _ctx = context;
    }

    public async Task<List<AdminLog>> GetFilteredLogsAsync(AdminLogFilterDto filter)
    {
        var query = _ctx.AdminLogs
            .Include(l => l.IdAdministratorNavigation)
            .Where(l => l.DeletionTime == null);

        if (filter.AdministratorId.HasValue)
            query = query.Where(l => l.IdAdministrator == filter.AdministratorId.Value);

        if (!string.IsNullOrEmpty(filter.ActionCode))
            query = query.Where(l => l.ActionCode == filter.ActionCode);

        if (!string.IsNullOrEmpty(filter.EntityType))
            query = query.Where(l => l.EntityType == filter.EntityType);

        if (filter.StartDate.HasValue)
            query = query.Where(l => l.CreationTime >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(l => l.CreationTime <= filter.EndDate.Value);

        var logs = await query
            .OrderByDescending(l => l.CreationTime)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return logs;
    }

    public async Task<List<AdminLog>> GetRecentLogsAsync(int count)
    {
        return await _ctx.AdminLogs
            .Include(l => l.IdAdministratorNavigation)
            .Where(l => l.DeletionTime == null)
            .OrderByDescending(l => l.CreationTime)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<AdminLog>> GetLogsByAdministratorAsync(Guid adminId)
    {
        return await _ctx.AdminLogs
            .Include(l => l.IdAdministratorNavigation)
            .Where(l => l.IdAdministrator == adminId && l.DeletionTime == null)
            .OrderByDescending(l => l.CreationTime)
            .ToListAsync();
    }

    public async Task<List<AdminLog>> GetLogsByEntityAsync(string entityType, Guid entityId)
    {
        return await _ctx.AdminLogs
            .Include(l => l.IdAdministratorNavigation)
            .Where(l => l.EntityType == entityType && l.TargetedEntityId == entityId && l.DeletionTime == null)
            .OrderByDescending(l => l.CreationTime)
            .ToListAsync();
    }
}
