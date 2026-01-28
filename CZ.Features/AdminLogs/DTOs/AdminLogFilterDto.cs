namespace api.CZ.Features.AdminLogs.DTOs;

public class AdminLogFilterDto
{
    public Guid? AdministratorId { get; set; }
    public string? ActionCode { get; set; }
    public string? EntityType { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
