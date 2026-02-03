namespace api.CZ.Features.PasswordHistories.DTOs;

public class GetPasswordHistoryDto
{
    public Guid Id { get; set; }
    public DateTime ChangedAt { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? UpdateTime { get; set; }
    public Guid IdPasswordsInfos { get; set; }
}
