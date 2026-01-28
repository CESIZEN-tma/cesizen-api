using System;
using System.Collections.Generic;
using Bookmark = api.CZ.Features.Bookmarks.Models.Bookmark;
using api.CZ.Features.EmailConfirmationTokens.Models;
using api.CZ.Features.PasswordResetTokens.Models;
using api.CZ.Features.PasswordsInfos.Models;
using api.CZ.Features.UserSavedConfigurations.Models;
using Session = api.CZ.Features.Sessions.Models.Session;

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

    public int FailedLoginAttempts { get; set; }

    public bool AccountActivated { get; set; }

    public bool Active { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime? UpdateTime { get; set; }

    public DateTime? DeletionTime { get; set; }

    public Guid? IdUserSavedConfigurations { get; set; }

    public Guid? IdPasswordsInfos { get; set; }

    public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();

    public virtual ICollection<EmailConfirmationToken> EmailConfirmationTokens { get; set; } = new List<EmailConfirmationToken>();

    public virtual UserSavedConfiguration? IdUserSavedConfigurationsNavigation { get; set; }

    public virtual PasswordsInfo? IdPasswordsInfosNavigation { get; set; }

    public virtual ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();

    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
}
