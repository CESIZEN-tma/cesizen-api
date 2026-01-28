using System.ComponentModel.DataAnnotations;

namespace api.CZ.Features.InformationPages.DTOs;

public class CreateInformationPageDto
{
    [Required]
    [StringLength(100)]
    public required string Title { get; set; }

    [Required]
    public required string Description { get; set; }

    [Required]
    public required string Content { get; set; }

    [Required]
    [StringLength(255)]
    public required string ContentType { get; set; }

    [Required]
    [StringLength(255)]
    public required string Status { get; set; }

    public bool Active { get; set; } = false;
}
