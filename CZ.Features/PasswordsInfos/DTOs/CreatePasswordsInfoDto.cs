using System.ComponentModel.DataAnnotations;

namespace api.CZ.Features.PasswordsInfos.DTOs;

public class CreatePasswordsInfoDto
{
    [Range(0, int.MaxValue)]
    public int AttemptCount { get; set; } = 0;
}
