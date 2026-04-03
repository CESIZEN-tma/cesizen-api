namespace api.CZ.Features.NavigationMenus.DTOs;

public class GetNavigationMenuDto
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public int Position { get; set; }
    public string Label { get; set; } = null!;
    public string? Url { get; set; }
    public bool CurrentlyEditing { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? UpdateTime { get; set; }
    public List<GetNavigationMenuDto> Children { get; set; } = new();
}
