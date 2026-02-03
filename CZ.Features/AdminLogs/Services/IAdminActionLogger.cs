using api.CZ.Features.AdminLogs.Enums;

namespace api.CZ.Features.AdminLogs.Services;

/// <summary>
/// Service for logging administrator actions automatically.
/// Inject this service into any service where administrators perform CRUD operations.
/// </summary>
public interface IAdminActionLogger
{
    /// <summary>
    /// Log a create action
    /// </summary>
    Task LogCreateAsync(Guid adminId, string entityType, Guid entityId, string description);

    /// <summary>
    /// Log an update action
    /// </summary>
    Task LogUpdateAsync(Guid adminId, string entityType, Guid entityId, string description);

    /// <summary>
    /// Log a delete action
    /// </summary>
    Task LogDeleteAsync(Guid adminId, string entityType, Guid entityId, string description);

    /// <summary>
    /// Log a custom action
    /// </summary>
    Task LogCustomActionAsync(Guid adminId, AdminActionCode actionCode, string entityType, Guid entityId, string description);
}
