using api.CZ.Features.Administrators.Models;

namespace api.CZ.Features.AdminLogs.Models;

public partial class AdminLog
{
    public Guid Id { get; set; }
    public string ActionCode { get; set; } = null!;
    public string EntityType { get; set; } = null!;
    public Guid TargetedEntityId { get; set; }
    public string Description { get; set; } = null!;
    public DateTime CreationTime { get; set; }
    public DateTime? UpdateTime { get; set; }
    public DateTime? DeletionTime { get; set; }
    public Guid IdAdministrator { get; set; }

    public virtual Administrator IdAdministratorNavigation { get; set; } = null!;
}
