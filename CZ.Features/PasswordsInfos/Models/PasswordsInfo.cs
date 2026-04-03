using api.CZ.Features.PasswordHistories.Models;
using api.CZ.Features.Users.Models;

namespace api.CZ.Features.PasswordsInfos.Models;

public partial class PasswordsInfo
{
    public Guid Id { get; set; }
    public int AttemptCount { get; set; }
    public DateTime LastLogin { get; set; }
    public DateTime LastReset { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? UpdateTime { get; set; }
    public DateTime? DeletionTime { get; set; }

    public virtual ICollection<PasswordHistory> PasswordHistories { get; set; } = new List<PasswordHistory>();
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
