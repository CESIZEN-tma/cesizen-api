namespace api.CZ.Features.Users.DTOs;

public class GetUserAdminDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public bool Active { get; set; }
    public bool AccountActivated { get; set; }
    public DateTime MemberSince { get; set; }
    public DateTime? LockedUntil { get; set; }
}
