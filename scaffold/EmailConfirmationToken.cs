using System;
using System.Collections.Generic;
using api.CZ.Data.AbstractModels;

namespace api.scaffold;

public partial class EmailConfirmationToken : SoftDeletableEntity
{

    public string Token { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public bool Consumed { get; set; }

    public bool? ConsumedAt { get; set; }

    public Guid IdUsers { get; set; }

    public virtual User IdUsersNavigation { get; set; } = null!;
}
