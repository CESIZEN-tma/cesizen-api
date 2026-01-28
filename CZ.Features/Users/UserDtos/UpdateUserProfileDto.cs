using System.ComponentModel.DataAnnotations;

namespace api.CZ.Features.Users.UserDtos;

public class UpdateUserProfileDto
{
    [Required]
    [StringLength(255)]
    public required string FirstName { get; set; }

    [Required]
    [StringLength(255)]
    public required string LastName { get; set; }

    [Url]
    public string? ThumbnailUrl { get; set; }
}
