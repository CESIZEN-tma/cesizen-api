using System;
using System.Collections.Generic;
using api.CZ.Data.AbstractModels;

namespace api.scaffold;

public partial class UserSavedConfiguration : SoftDeletableEntity
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
    
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
