using System.ComponentModel.DataAnnotations;

namespace api.CZ.Features.PasswordsInfos.DTOs;

public class UpdatePasswordsInfoDto
{
    [Range(0, int.MaxValue)]
    public int AttemptCount { get; set; }

    public DateTime? LastLogin { get; set; }

    public DateTime? LastReset { get; set; }
}
