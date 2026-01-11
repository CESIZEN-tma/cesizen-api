using System;
using System.Collections.Generic;
using api.CZ.Features.Administrators.Models;
using api.CZ.Features.Users.Models;

namespace api.CZ.Features.Sessions.Models;

public class Session
{
    public Guid Id { get; set; }

    public string Token { get; set; } = null!;

    public bool Consumed { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime? UpdateTime { get; set; }

    public DateTime? DeletionTime { get; set; }

    public Guid IdUsers { get; set; }

    public virtual User IdUsersNavigation { get; set; } = null!;

    public virtual ICollection<Administrator> Ids { get; set; } = new List<Administrator>();
}
