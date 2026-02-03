using System.ComponentModel.DataAnnotations;

namespace api.CZ.Features.Bookmarks.DTOs;

public class CreateBookmarkDto
{
    [Required]
    public Guid ConfigurationId { get; set; }
}
