using System;
using System.Collections.Generic;
using api.CZ.Data.AbstractModels;
using api.CZ.Features.Administrators.Models;

namespace api.scaffold;

public partial class AdminLog : SoftDeletableEntity
{

    public string ActionCode { get; set; } = null!;

    public string EntityType { get; set; } = null!;

    public Guid TargetedEntityId { get; set; }

    public string Description { get; set; } = null!;
    
    public virtual ICollection<Administrator> Administrators { get; set; } = new List<Administrator>();
}
