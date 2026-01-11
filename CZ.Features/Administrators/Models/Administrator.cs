using System;
using System.Collections.Generic;
using Session = api.CZ.Features.Sessions.Models.Session;

using api.scaffoldBis;

namespace api.CZ.Features.Administrators.Models;

public partial class Administrator
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

    public DateTime CreationTime { get; set; }

    public DateTime? UpdateTime { get; set; }

    public DateTime? DeletionTime { get; set; }

    public Guid? IdNavigationMenu { get; set; }

    public virtual ICollection<AdminLog> AdminLogs { get; set; } = new List<AdminLog>();

    public virtual ICollection<Configuration> Configurations { get; set; } = new List<Configuration>();

    public virtual NavigationMenu? IdNavigationMenuNavigation { get; set; }

    public virtual ICollection<InformationPage> InformationPages { get; set; } = new List<InformationPage>();

    public virtual ICollection<Session> IdSessions { get; set; } = new List<Session>();
}
