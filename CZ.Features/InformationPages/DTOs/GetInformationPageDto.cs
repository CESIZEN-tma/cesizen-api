namespace api.CZ.Features.InformationPages.DTOs;

public class GetInformationPageDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public bool CurrentlyEditing { get; set; }
    public string Status { get; set; } = null!;
    public bool Active { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? UpdateTime { get; set; }
    public Guid IdAdministrators { get; set; }
    public List<Guid> TagIds { get; set; } = new();
}
