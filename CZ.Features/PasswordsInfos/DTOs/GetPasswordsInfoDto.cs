namespace api.CZ.Features.PasswordsInfos.DTOs;

public class GetPasswordsInfoDto
{
    public Guid Id { get; set; }
    public int AttemptCount { get; set; }
    public DateTime LastLogin { get; set; }
    public DateTime LastReset { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? UpdateTime { get; set; }
}
