using System.ComponentModel.DataAnnotations;

namespace api.CZ.Features.Authentifications.DTOs;

public class ChangePasswordDto
{
    [Required]
    public required string CurrentPassword { get; set; }

    [Required]
    [MinLength(8)]
    public required string NewPassword { get; set; }

    [Required]
    public required string ConfirmPassword { get; set; }
}
