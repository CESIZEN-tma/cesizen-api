namespace api.CZ.Features.NavigationMenus.DTOs;

public class GetNavigationMenuDto
{
    public Guid Id { get; set; }
    public int Position { get; set; }
    public string Label { get; set; } = null!;
    public string Url { get; set; } = null!;
    public bool CurrentlyEditing { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? UpdateTime { get; set; }
}
