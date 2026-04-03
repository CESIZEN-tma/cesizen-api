using System.ComponentModel.DataAnnotations;

namespace api.CZ.Features.UserSavedConfigurations.DTOs;

public class CreateUserSavedConfigurationDto
{
    [Required]
    [StringLength(255)]
    public required string Name { get; set; }

    [Range(1, 60)]
    public int Inhalation { get; set; }

    [Range(0, 60)]
    public int Retention1 { get; set; }

    [Range(1, 60)]
    public int Exhalation { get; set; }

    [Range(0, 60)]
    public int Retention2 { get; set; }

    [Range(1, 120)]
    public int DurationMinutes { get; set; }

    [Range(1, 10)]
    public int Difficulty { get; set; }

    [Required]
    [StringLength(50)]
    public required string Objective { get; set; }

    [Required]
    [StringLength(50)]
    public required string GuidanceType { get; set; }
}
