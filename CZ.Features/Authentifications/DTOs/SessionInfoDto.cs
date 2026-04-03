namespace api.CZ.Features.Authentifications.DTOs;

public class SessionInfoDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
