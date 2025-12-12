using System;
using System.Collections.Generic;
using api.CZ.Data.AbstractModels;
using api.scaffold;

namespace api.CZ.Features.Administrators.Models;

public partial class Administrator : SoftDeletableEntity
{

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateTime MemberSince { get; set; }

    public string? ThumbnailUrl { get; set; }

    public DateTime LockedUntil { get; set; }

    public bool AccountActivated { get; set; }
    
    public Guid IdAdminLogs { get; set; }

    public Guid IdNavigationMenu { get; set; }

    public virtual ICollection<Configuration> Configurations { get; set; } = new List<Configuration>();

    public virtual AdminLog IdAdminLogsNavigation { get; set; } = null!;

    public virtual NavigationMenu IdNavigationMenuNavigation { get; set; } = null!;

    public virtual ICollection<InformationPage> InformationPages { get; set; } = new List<InformationPage>();

    public virtual ICollection<Session> IdSessions { get; set; } = new List<Session>();
}
