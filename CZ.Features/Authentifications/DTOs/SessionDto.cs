namespace api.CZ.Features.Authentifications.DTOs;

public class SessionDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsCurrentSession { get; set; }
}
