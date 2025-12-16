using System;
using System.Collections.Generic;
using api.CZ.Features.Users.Models;

namespace api.scaffoldBis;

public partial class Bookmark
{
    public Guid Id { get; set; }

    public Guid IdConfigurations { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime? UpdateTime { get; set; }

    public DateTime? DeletionTime { get; set; }

    public virtual Configuration IdConfigurationsNavigation { get; set; } = null!;

    public virtual User IdNavigation { get; set; } = null!;
}
