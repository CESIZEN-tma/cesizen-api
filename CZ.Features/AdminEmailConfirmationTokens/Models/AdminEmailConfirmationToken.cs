using api.CZ.Features.Administrators.Models;

namespace api.CZ.Features.AdminEmailConfirmationTokens.Models;

public class AdminEmailConfirmationToken
{
    public Guid Id { get; set; }

    public string Token { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public bool Consumed { get; set; }

    public DateTime? ConsumedAt { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime? UpdateTime { get; set; }

    public DateTime? DeletionTime { get; set; }

    public Guid IdAdministrators { get; set; }

    public virtual Administrator IdAdministratorsNavigation { get; set; } = null!;
}
