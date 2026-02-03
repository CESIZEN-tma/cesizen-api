namespace api.CZ.Features.Bookmarks.DTOs;

public class GetBookmarkDto
{
    public Guid UserId { get; set; }
    public Guid ConfigurationId { get; set; }
    public string ConfigurationName { get; set; } = null!;
    public DateTime CreationTime { get; set; }
}
