using System.ComponentModel.DataAnnotations;

namespace api.CZ.Features.Authentifications.DTOs;

public class LoginDto
{
    [EmailAddress]
    [Required]
    public required string Email { get; set; }
    [Required]
    public string Password { get; set; }
}