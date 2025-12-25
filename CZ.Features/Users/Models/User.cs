using System;
using System.Collections.Generic;
using api.CZ.Features.EmailConfirmationTokens.Models;
using api.scaffoldBis;

namespace api.CZ.Features.Users.Models;

public partial class User
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateTime MemberSince { get; set; }

    public string? ThumbnailUrl { get; set; }

    public DateTime? LockedUntil { get; set; }

    public bool AccountActivated { get; set; }

    public bool Active { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime? UpdateTime { get; set; }

    public DateTime? DeletionTime { get; set; }

    public Guid? IdUserSavedConfigurations { get; set; }

    public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();

    public virtual ICollection<EmailConfirmationToken> EmailConfirmationTokens { get; set; } = new List<EmailConfirmationToken>();

    public virtual UserSavedConfiguration? IdUserSavedConfigurationsNavigation { get; set; }

    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
}
