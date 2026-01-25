using api.CZ.Features.AdminEmailConfirmationTokens.Factories;
using api.CZ.Features.AdminEmailConfirmationTokens.Models;
using api.CZ.Features.AdminEmailConfirmationTokens.Repositories;

namespace api.CZ.Features.AdminEmailConfirmationTokens.Services;

public class AdminEmailConfirmationTokenService : IAdminEmailConfirmationTokenService
{
    private readonly IAdminEmailConfirmationTokenRepository _repository;
    private readonly IAdminEmailConfirmationTokenFactory _factory;

    public AdminEmailConfirmationTokenService(
        IAdminEmailConfirmationTokenRepository repository,
        IAdminEmailConfirmationTokenFactory factory)
    {
        _repository = repository;
        _factory = factory;
    }

    public async Task<AdminEmailConfirmationToken?> GetEntityByToken(string token)
    {
        var foundToken = await _repository.FirstOrDefaultAsync(t => t.Token == token);
        return foundToken != null && !foundToken.Consumed ? foundToken : null;
    }

    public async Task<AdminEmailConfirmationToken> NewToken(Guid adminId)
    {
        var existingToken = await _repository.FirstOrDefaultAsync(t => t.IdAdministrators == adminId);
        if (existingToken != null && existingToken.ExpiresAt < DateTime.UtcNow)
        {
            await _repository.DeleteAsync(existingToken);
        }

        var newToken = _factory.Create(adminId);
        await _repository.AddAsync(newToken);

        return newToken;
    }

    public async Task<bool> Consume(string token)
    {
        var confirmationToken = await _repository.FirstOrDefaultAsync(t => t.Token == token);

        if (confirmationToken == null)
            return false;

        if (confirmationToken.Consumed)
            return false;

        if (confirmationToken.ExpiresAt < DateTime.UtcNow)
            return false;

        confirmationToken.Consumed = true;
        confirmationToken.ConsumedAt = DateTime.UtcNow;
        confirmationToken.UpdateTime = DateTime.UtcNow;

        await _repository.UpdateAsync(confirmationToken);

        return true;
    }
}
