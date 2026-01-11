using System.ComponentModel.DataAnnotations;

namespace api.CZ.Features.Authentifications.DTOs;

public class RefreshTokenDto
{
    [Required]
    public required string RefreshToken { get; set; }
}
