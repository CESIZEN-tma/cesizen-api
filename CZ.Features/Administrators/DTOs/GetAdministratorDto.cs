namespace api.CZ.Features.Administrators.DTOs;

public class GetAdministratorDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateTime MemberSince { get; set; }
    public string? ThumbnailUrl { get; set; }
    public bool AccountActivated { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? UpdateTime { get; set; }
}
