using api.CZ.Features.PasswordsInfos.Models;

namespace api.CZ.Features.PasswordHistories.Models;

public partial class PasswordHistory
{
    public Guid Id { get; set; }
    public string PasswordHash { get; set; } = null!;
    public DateTime ChangedAt { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? UpdateTime { get; set; }
    public DateTime? DeletionTime { get; set; }
    public Guid IdPasswordsInfos { get; set; }

    public virtual PasswordsInfo IdPasswordsInfosNavigation { get; set; } = null!;
}
