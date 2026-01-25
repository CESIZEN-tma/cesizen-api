using api.CZ.Features.AdminPasswordResetTokens.Models;

namespace api.CZ.Features.AdminPasswordResetTokens.Services;

public interface IAdminPasswordResetTokenService
{
    Task<AdminPasswordResetToken?> GetEntityByToken(string token);
    Task<AdminPasswordResetToken> NewToken(Guid adminId);
    Task<bool> Consume(string token);
}
