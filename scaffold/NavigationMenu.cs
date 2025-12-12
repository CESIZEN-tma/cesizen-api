using System;
using System.Collections.Generic;
using api.CZ.Data.AbstractModels;

namespace api.scaffold;

public partial class NavigationMenu : SoftDeletableEntity
{

    public int Position { get; set; }

    public string Label { get; set; } = null!;

    public string Url { get; set; } = null!;

    public bool CurrentlyEditing { get; set; }
    
    public virtual ICollection<Administrator> Administrators { get; set; } = new List<Administrator>();
}
