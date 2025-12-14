using System;
using System.Collections.Generic;
using api.CZ.Data.AbstractModels;

namespace api.scaffold;

public partial class User : SoftDeletableEntity
{

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateTime MemberSince { get; set; }

    public string? ThumbnailUrl { get; set; }

    public DateTime LockedUntil { get; set; }

    public bool AccountActivated { get; set; }

    public bool Active { get; set; }
    
    public Guid IdUserSavedConfigurations { get; set; }

    public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();

    public virtual ICollection<EmailConfirmationToken> EmailConfirmationTokens { get; set; } = new List<EmailConfirmationToken>();

    public virtual UserSavedConfiguration IdUserSavedConfigurationsNavigation { get; set; } = null!;

    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
}
