namespace api.CZ.Features.AdminLogs.DTOs;

public class EntityLineageDto
{
    public string EntityType { get; set; } = null!;
    public Guid EntityId { get; set; }
    public int TotalEvents { get; set; }
    public List<LineageEventDto> Events { get; set; } = [];
}

public class LineageEventDto
{
    public int Step { get; set; }
    public Guid LogId { get; set; }
    public string ActionCode { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime OccurredAt { get; set; }
    public Guid AdministratorId { get; set; }
    public string AdministratorEmail { get; set; } = null!;
    public string AdministratorName { get; set; } = null!;
}
