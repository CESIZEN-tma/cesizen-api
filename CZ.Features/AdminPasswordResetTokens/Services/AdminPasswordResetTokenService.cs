using api.CZ.Features.AdminPasswordResetTokens.Factories;
using api.CZ.Features.AdminPasswordResetTokens.Models;
using api.CZ.Features.AdminPasswordResetTokens.Repositories;

namespace api.CZ.Features.AdminPasswordResetTokens.Services;

public class AdminPasswordResetTokenService : IAdminPasswordResetTokenService
{
    private readonly IAdminPasswordResetTokenRepository _repository;
    private readonly IAdminPasswordResetTokenFactory _factory;

    public AdminPasswordResetTokenService(
        IAdminPasswordResetTokenRepository repository,
        IAdminPasswordResetTokenFactory factory)
    {
        _repository = repository;
        _factory = factory;
    }

    public async Task<AdminPasswordResetToken?> GetEntityByToken(string token)
    {
        var foundToken = await _repository.FirstOrDefaultAsync(t => t.Token == token);
        return foundToken != null && !foundToken.Consumed ? foundToken : null;
    }

    public async Task<AdminPasswordResetToken> NewToken(Guid adminId)
    {
        var existingToken = await _repository.FirstOrDefaultAsync(t =>
            t.IdAdministrators == adminId && !t.Consumed && t.ExpiresAt > DateTime.UtcNow);

        if (existingToken != null)
        {
            await _repository.DeleteAsync(existingToken);
        }

        var newToken = _factory.Create(adminId);
        await _repository.AddAsync(newToken);

        return newToken;
    }

    public async Task<bool> Consume(string token)
    {
        var resetToken = await _repository.FirstOrDefaultAsync(t => t.Token == token);

        if (resetToken == null)
            return false;

        if (resetToken.Consumed)
            return false;

        if (resetToken.ExpiresAt < DateTime.UtcNow)
            return false;

        resetToken.Consumed = true;
        resetToken.ConsumedAt = DateTime.UtcNow;
        resetToken.UpdateTime = DateTime.UtcNow;

        await _repository.UpdateAsync(resetToken);

        return true;
    }
}
