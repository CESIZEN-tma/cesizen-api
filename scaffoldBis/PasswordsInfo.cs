using System;
using System.Collections.Generic;

namespace api.scaffoldBis;

public partial class PasswordsInfo
{
    public Guid Id { get; set; }

    public int AttemptCount { get; set; }

    public DateTime LastLogin { get; set; }

    public DateTime LastReset { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime? UpdateTime { get; set; }

    public DateTime? DeletionTime { get; set; }

    public virtual ICollection<PasswordHistory> PasswordHistories { get; set; } = new List<PasswordHistory>();

    public virtual ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();
}
