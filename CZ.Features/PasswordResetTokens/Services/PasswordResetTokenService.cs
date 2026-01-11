using api.CZ.Features.PasswordResetTokens.Factories;
using api.CZ.Features.PasswordResetTokens.Models;
using api.CZ.Features.PasswordResetTokens.Repositories;

namespace api.CZ.Features.PasswordResetTokens.Services;

public class PasswordResetTokenService : IPasswordResetTokenService
{
    private readonly IPasswordResetTokenRepository _repository;
    private readonly IPasswordResetTokenFactory _factory;

    public PasswordResetTokenService(
        IPasswordResetTokenRepository repository,
        IPasswordResetTokenFactory factory)
    {
        _repository = repository;
        _factory = factory;
    }

    public async Task<PasswordResetToken?> GetEntityByToken(string token)
    {
        var foundToken = await _repository.FirstOrDefaultAsync(t => t.Token == token);
        return foundToken != null && !foundToken.Consumed ? foundToken : null;
    }

    public async Task<PasswordResetToken> NewToken(Guid userId)
    {
        // Delete old non-consumed, non-expired tokens for this user
        var existingToken = await _repository.FirstOrDefaultAsync(t =>
            t.IdUsers == userId && !t.Consumed && t.ExpiresAt > DateTime.UtcNow);

        if (existingToken != null)
        {
            await _repository.DeleteAsync(existingToken);
        }

        var newToken = _factory.Create(userId);
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
