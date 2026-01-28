namespace api.CZ.Features.Users.UserDtos;

public class GetUserProfileDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateTime MemberSince { get; set; }
    public string? ThumbnailUrl { get; set; }
    public bool AccountActivated { get; set; }
    public bool Active { get; set; }
}
