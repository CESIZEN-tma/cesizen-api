using api.CZ.Features.EmailConfirmationTokens.Models;

namespace api.CZ.Features.EmailConfirmationTokens.Services;

public interface IEmailConfirmationTokenService
{
    Task<EmailConfirmationToken> NewToken(Guid userId);
    Task<EmailConfirmationToken?> GetEntityByToken(string token);
    Task<bool> Consume(string token);
}