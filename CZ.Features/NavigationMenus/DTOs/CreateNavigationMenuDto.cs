using System.ComponentModel.DataAnnotations;

namespace api.CZ.Features.NavigationMenus.DTOs;

public class CreateNavigationMenuDto
{
    [Required]
    [Range(0, 100)]
    public int Position { get; set; }

    [Required]
    [StringLength(100)]
    public required string Label { get; set; }

    [Required]
    [Url]
    public required string Url { get; set; }
}
