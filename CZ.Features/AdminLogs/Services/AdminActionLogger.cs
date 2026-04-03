using api.CZ.Features.AdminLogs.Enums;

namespace api.CZ.Features.AdminLogs.Services;

public class AdminActionLogger : IAdminActionLogger
{
    private readonly IAdminLogService _adminLogService;
    private readonly ILogger<AdminActionLogger> _logger;

    public AdminActionLogger(
        IAdminLogService adminLogService,
        ILogger<AdminActionLogger> logger)
    {
        _adminLogService = adminLogService;
        _logger = logger;
    }

    public async Task LogCreateAsync(Guid adminId, string entityType, Guid entityId, string description)
    {
        var actionCode = GetCreateActionCode(entityType);
        await _adminLogService.LogActionAsync(adminId, actionCode, entityType, entityId, description);
        _logger.LogInformation("Admin {AdminId} created {EntityType} {EntityId}: {Description}",
            adminId, entityType, entityId, description);
    }

    public async Task LogUpdateAsync(Guid adminId, string entityType, Guid entityId, string description)
    {
        var actionCode = GetUpdateActionCode(entityType);
        await _adminLogService.LogActionAsync(adminId, actionCode, entityType, entityId, description);
        _logger.LogInformation("Admin {AdminId} updated {EntityType} {EntityId}: {Description}",
            adminId, entityType, entityId, description);
    }

    public async Task LogDeleteAsync(Guid adminId, string entityType, Guid entityId, string description)
    {
        var actionCode = GetDeleteActionCode(entityType);
        await _adminLogService.LogActionAsync(adminId, actionCode, entityType, entityId, description);
        _logger.LogInformation("Admin {AdminId} deleted {EntityType} {EntityId}: {Description}",
            adminId, entityType, entityId, description);
    }

    public async Task LogCustomActionAsync(Guid adminId, AdminActionCode actionCode, string entityType, Guid entityId, string description)
    {
        await _adminLogService.LogActionAsync(adminId, actionCode, entityType, entityId, description);
        _logger.LogInformation("Admin {AdminId} performed {ActionCode} on {EntityType} {EntityId}: {Description}",
            adminId, actionCode, entityType, entityId, description);
    }

    private AdminActionCode GetCreateActionCode(string entityType)
    {
        return entityType.ToUpperInvariant() switch
        {
            "ADMINISTRATOR" => AdminActionCode.ADMIN_CREATED,
            "INFORMATIONPAGE" => AdminActionCode.INFO_PAGE_CREATED,
            "INFORMATIONTAG" => AdminActionCode.INFO_TAG_CREATED,
            "NAVIGATIONMENU" => AdminActionCode.NAV_MENU_CREATED,
            "CONFIGURATION" => AdminActionCode.CONFIG_CREATED,
            "QUIZ" or "QUIZZ" => AdminActionCode.QUIZ_CREATED,
            _ => AdminActionCode.BULK_OPERATION
        };
    }

    private AdminActionCode GetUpdateActionCode(string entityType)
    {
        return entityType.ToUpperInvariant() switch
        {
            "ADMINISTRATOR" => AdminActionCode.ADMIN_UPDATED,
            "INFORMATIONPAGE" => AdminActionCode.INFO_PAGE_UPDATED,
            "INFORMATIONTAG" => AdminActionCode.INFO_TAG_UPDATED,
            "NAVIGATIONMENU" => AdminActionCode.NAV_MENU_UPDATED,
            "CONFIGURATION" => AdminActionCode.CONFIG_UPDATED,
            "QUIZ" or "QUIZZ" => AdminActionCode.QUIZ_UPDATED,
            _ => AdminActionCode.BULK_OPERATION
        };
    }

    private AdminActionCode GetDeleteActionCode(string entityType)
    {
        return entityType.ToUpperInvariant() switch
        {
            "ADMINISTRATOR" => AdminActionCode.ADMIN_DELETED,
            "INFORMATIONPAGE" => AdminActionCode.INFO_PAGE_DELETED,
            "INFORMATIONTAG" => AdminActionCode.INFO_TAG_DELETED,
            "NAVIGATIONMENU" => AdminActionCode.NAV_MENU_DELETED,
            "CONFIGURATION" => AdminActionCode.CONFIG_DELETED,
            "QUIZ" or "QUIZZ" => AdminActionCode.QUIZ_DELETED,
            _ => AdminActionCode.BULK_OPERATION
        };
    }
}
