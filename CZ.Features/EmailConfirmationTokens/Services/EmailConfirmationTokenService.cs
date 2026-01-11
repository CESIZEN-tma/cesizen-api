using api.CZ.Features.EmailConfirmationTokens.Factories;
using api.CZ.Features.EmailConfirmationTokens.Models;
using api.CZ.Features.EmailConfirmationTokens.Repositories;

namespace api.CZ.Features.EmailConfirmationTokens.Services;

public class EmailConfirmationTokenService : IEmailConfirmationTokenService
{
    private readonly IEmailConfirmationTokenRepository _repository;
    private readonly IEmailConfirmationTokenFactory _factory;

    public EmailConfirmationTokenService(
        IEmailConfirmationTokenRepository repository,
        IEmailConfirmationTokenFactory factory)
    {
        _repository = repository;
        _factory = factory;
    }


    public async Task<EmailConfirmationToken?> GetEntityByToken(string token)
    {
        var foundToken = await _repository.FirstOrDefaultAsync(t => t.Token == token);
        return foundToken != null && !foundToken.Consumed ? foundToken : null;
    }
        
    public async Task<EmailConfirmationToken> NewToken(Guid userId)
    {
        // Delete old token if not consumed and not expired
        var existingToken = await _repository.FirstOrDefaultAsync(t => t.IdUsers == userId);
        if (existingToken != null && existingToken.ExpiresAt < DateTime.UtcNow)
        {
            await _repository.DeleteAsync(existingToken);
        }

        var newToken = _factory.Create(userId);
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