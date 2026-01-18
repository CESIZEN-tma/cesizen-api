using api.CZ.Features.AdminEmailConfirmationTokens.Models;

namespace api.CZ.Features.AdminEmailConfirmationTokens.Services;

public interface IAdminEmailConfirmationTokenService
{
    Task<AdminEmailConfirmationToken> NewToken(Guid adminId);
    Task<AdminEmailConfirmationToken?> GetEntityByToken(string token);
    Task<bool> Consume(string token);
}
