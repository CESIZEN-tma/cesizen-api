namespace api.CZ.Features.Configurations.DTOs;

public class GetConfigurationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public int Inhalation { get; set; }
    public int Retention1 { get; set; }
    public int Exhalation { get; set; }
    public int Retention2 { get; set; }
    public int DurationMinutes { get; set; }
    public int Difficulty { get; set; }
    public string Objective { get; set; } = null!;
    public string GuidanceType { get; set; } = null!;
    public DateTime CreationTime { get; set; }
    public DateTime? UpdateTime { get; set; }
    public Guid IdAdministrators { get; set; }
}
