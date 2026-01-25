using System.ComponentModel.DataAnnotations;

namespace api.CZ.Features.Authentifications.DTOs;

public class ForgotPasswordDto
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
}
