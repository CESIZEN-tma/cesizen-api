using api.CZ.Features.Administrators.Models;

namespace api.CZ.Features.AdminSessions.Models;

public class AdminSession
{
    public Guid Id { get; set; }

    public string Token { get; set; } = null!;

    public bool Consumed { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime? UpdateTime { get; set; }

    public DateTime? DeletionTime { get; set; }

    public Guid IdAdministrators { get; set; }

    public virtual Administrator IdAdministratorsNavigation { get; set; } = null!;
}
