using System;
using System.Collections.Generic;
using api.CZ.Features.Users.Models;

namespace api.CZ.Features.EmailConfirmationTokens.Models;

public partial class EmailConfirmationToken
{
    public Guid Id { get; set; }

    public string Token { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public bool Consumed { get; set; }

    public DateTime? ConsumedAt { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime? UpdateTime { get; set; }

    public DateTime? DeletionTime { get; set; }

    public Guid IdUsers { get; set; }

    public virtual User IdUsersNavigation { get; set; } = null!;
}
