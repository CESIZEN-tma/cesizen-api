using api.CZ.Features.Administrators.Models;

namespace api.CZ.Features.NavigationMenus.Models;

public partial class NavigationMenu
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public int Position { get; set; }
    public string Label { get; set; } = null!;
    public string? Url { get; set; }
    public bool CurrentlyEditing { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? UpdateTime { get; set; }
    public DateTime? DeletionTime { get; set; }

    public virtual NavigationMenu? Parent { get; set; }
    public virtual ICollection<NavigationMenu> Children { get; set; } = new List<NavigationMenu>();
    public virtual ICollection<Administrator> Administrators { get; set; } = new List<Administrator>();
}
