namespace api.CZ.Features.AdminLogs.DTOs;

public class GetAdminLogDto
{
    public Guid Id { get; set; }
    public string ActionCode { get; set; } = null!;
    public string EntityType { get; set; } = null!;
    public Guid TargetedEntityId { get; set; }
    public string Description { get; set; } = null!;
    public DateTime CreationTime { get; set; }
    public Guid AdministratorId { get; set; }
    public string AdministratorEmail { get; set; } = null!;
    public string AdministratorName { get; set; } = null!;
}
