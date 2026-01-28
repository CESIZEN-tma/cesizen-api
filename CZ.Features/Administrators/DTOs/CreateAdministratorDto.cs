using System.ComponentModel.DataAnnotations;

namespace api.CZ.Features.Administrators.DTOs;

public class CreateAdministratorDto
{
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public required string Email { get; set; }

    [Required]
    [StringLength(255, MinimumLength = 8)]
    public required string Password { get; set; }

    [Required]
    [StringLength(255)]
    public required string FirstName { get; set; }

    [Required]
    [StringLength(255)]
    public required string LastName { get; set; }

    [Url]
    public string? ThumbnailUrl { get; set; }
}
