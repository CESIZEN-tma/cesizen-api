using System;
using System.Collections.Generic;
using api.CZ.Data.AbstractModels;

namespace api.scaffold;

public partial class PasswordsInfo : SoftDeletableEntity
{

    public int AttemptCount { get; set; }

    public DateTime LastLogin { get; set; }

    public DateTime LastReset { get; set; }
    
    public virtual ICollection<PasswordHistory> PasswordHistories { get; set; } = new List<PasswordHistory>();

    public virtual ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();
}
