using System.ComponentModel.DataAnnotations;

namespace api.CZ.Features.Authentifications.DTOs;

public class ResetPasswordDto
{
    [Required]
    public required string Token { get; set; }

    [Required]
    [MinLength(8)]
    public required string NewPassword { get; set; }

    [Required]
    public required string ConfirmPassword { get; set; }
}
