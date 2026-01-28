using api.CZ.Features.Administrators.Models;

namespace api.CZ.Features.NavigationMenus.Models;

public partial class NavigationMenu
{
    public Guid Id { get; set; }
    public int Position { get; set; }
    public string Label { get; set; } = null!;
    public string Url { get; set; } = null!;
    public bool CurrentlyEditing { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? UpdateTime { get; set; }
    public DateTime? DeletionTime { get; set; }

    public virtual ICollection<Administrator> Administrators { get; set; } = new List<Administrator>();
}
