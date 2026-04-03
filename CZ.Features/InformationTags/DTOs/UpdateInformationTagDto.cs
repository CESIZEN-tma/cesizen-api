using System.ComponentModel.DataAnnotations;

namespace api.CZ.Features.InformationTags.DTOs;

public class UpdateInformationTagDto
{
    [Required]
    [StringLength(255)]
    public required string Label { get; set; }
}
