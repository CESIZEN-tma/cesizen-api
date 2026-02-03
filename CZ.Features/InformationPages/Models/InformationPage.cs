using api.CZ.Features.Administrators.Models;
using api.CZ.Features.InformationTags.Models;

namespace api.CZ.Features.InformationPages.Models;

public partial class InformationPage
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public bool CurrentlyEditing { get; set; }
    public string Status { get; set; } = null!;
    public bool Active { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? UpdateTime { get; set; }
    public DateTime? DeletionTime { get; set; }
    public Guid IdAdministrators { get; set; }

    public virtual Administrator IdAdministratorsNavigation { get; set; } = null!;
    public virtual ICollection<InformationTag> IdInformationTags { get; set; } = new List<InformationTag>();
}
