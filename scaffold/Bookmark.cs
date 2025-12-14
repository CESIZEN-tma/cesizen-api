using System;
using System.Collections.Generic;
using api.CZ.Data.AbstractModels;

namespace api.scaffold;

public partial class Bookmark  : SoftDeletableEntity
{

    public Guid IdConfigurations { get; set; }
    
    public virtual Configuration IdConfigurationsNavigation { get; set; } = null!;

    public virtual User IdNavigation { get; set; } = null!;
}
