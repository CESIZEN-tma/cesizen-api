using System;
using System.Collections.Generic;
using api.CZ.Data.AbstractModels;

namespace api.scaffold;

public partial class Session : SoftDeletableEntity
{

    public string Token { get; set; } = null!;

    public bool Consumed { get; set; }

    public DateTime ExpiresAt { get; set; }
    
    public Guid IdUsers { get; set; }

    public virtual User IdUsersNavigation { get; set; } = null!;

    public virtual ICollection<Administrator> Ids { get; set; } = new List<Administrator>();
}
