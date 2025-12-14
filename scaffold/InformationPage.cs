using System;
using System.Collections.Generic;
using api.CZ.Data.AbstractModels;
using api.CZ.Features.Administrators.Models;

namespace api.scaffold;

public partial class InformationPage : SoftDeletableEntity
{

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string ContentType { get; set; } = null!;

    public bool CurrentlyEditing { get; set; }

    public string Status { get; set; } = null!;

    public bool Active { get; set; }
    
    public Guid IdAdministrators { get; set; }

    public virtual Administrator IdAdministratorsNavigation { get; set; } = null!;

    public virtual ICollection<InformationTag> IdInformationTags { get; set; } = new List<InformationTag>();
}
