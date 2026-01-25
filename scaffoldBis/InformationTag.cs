using System;
using System.Collections.Generic;

namespace api.scaffoldBis;

public partial class InformationTag
{
    public Guid Id { get; set; }

    public string Label { get; set; } = null!;

    public DateTime CreationTime { get; set; }

    public DateTime? UpdateTime { get; set; }

    public DateTime? DeletionTime { get; set; }

    public virtual ICollection<InformationPage> Ids { get; set; } = new List<InformationPage>();
}
