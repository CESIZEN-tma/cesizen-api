using System;
using System.Collections.Generic;
using api.CZ.Data.AbstractModels;

namespace api.scaffold;

public partial class InformationTag : SoftDeletableEntity
{

    public string Label { get; set; } = null!;
    
    public virtual ICollection<InformationPage> Ids { get; set; } = new List<InformationPage>();
}
