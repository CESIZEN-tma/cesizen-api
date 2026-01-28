using api.CZ.Features.InformationPages.Models;

namespace api.CZ.Features.InformationTags.Models;

public partial class InformationTag
{
    public Guid Id { get; set; }
    public string Label { get; set; } = null!;
    public DateTime CreationTime { get; set; }
    public DateTime? UpdateTime { get; set; }
    public DateTime? DeletionTime { get; set; }

    public virtual ICollection<InformationPage> Ids { get; set; } = new List<InformationPage>();
}
