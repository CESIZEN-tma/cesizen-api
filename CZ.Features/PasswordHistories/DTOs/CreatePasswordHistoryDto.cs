using System.ComponentModel.DataAnnotations;

namespace api.CZ.Features.PasswordHistories.DTOs;

public class CreatePasswordHistoryDto
{
    [Required]
    public required string PasswordHash { get; set; }

    [Required]
    public Guid IdPasswordsInfos { get; set; }
}
