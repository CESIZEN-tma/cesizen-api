using System;
using System.Collections.Generic;
using api.CZ.Data.AbstractModels;
using api.CZ.Features.Administrators.Models;

namespace api.scaffold;

public partial class Configuration : SoftDeletableEntity
{

    public string Name { get; set; } = null!;

    public int Inhalation { get; set; }

    public int Retention1 { get; set; }

    public int Exhalation { get; set; }

    public int Retention2 { get; set; }

    public int DurationMinutes { get; set; }

    public int Difficulty { get; set; }

    public string Objective { get; set; } = null!;

    public string GuidanceType { get; set; } = null!;
    
    public Guid IdAdministrators { get; set; }

    public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();

    public virtual Administrator IdAdministratorsNavigation { get; set; } = null!;
}
