using System;
using System.Collections.Generic;

namespace api.scaffoldBis;

public partial class PasswordResetToken
{
    public Guid Id { get; set; }

    public string Token { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public bool Consumed { get; set; }

    public DateTime? ConsumedAt { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime? UpdateTime { get; set; }

    public DateTime? DeletionTime { get; set; }

    public Guid IdPasswordsInfos { get; set; }

    public virtual PasswordsInfo IdPasswordsInfosNavigation { get; set; } = null!;
}
