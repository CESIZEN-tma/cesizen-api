using api.CZ.Features.PasswordResetTokens.Models;

namespace api.CZ.Features.PasswordResetTokens.Services;

public interface IPasswordResetTokenService
{
    Task<PasswordResetToken?> GetEntityByToken(string token);
    Task<PasswordResetToken> NewToken(Guid userId);
    Task<bool> Consume(string token);
}
