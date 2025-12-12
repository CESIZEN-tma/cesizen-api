using System;
using System.Collections.Generic;
using api.CZ.Data.AbstractModels;

namespace api.scaffold;

public partial class PasswordHistory : SoftDeletableEntity
{

    public string PasswordHash { get; set; } = null!;

    public DateTime ChangedAt { get; set; }
    
    public Guid IdPasswordsInfos { get; set; }

    public virtual PasswordsInfo IdPasswordsInfosNavigation { get; set; } = null!;
}
