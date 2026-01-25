using api.CZ.Core.Factories;
using api.CZ.Features.EmailConfirmationTokens.Models;

namespace api.CZ.Features.EmailConfirmationTokens.Factories;

public class EmailConfirmationTokenFactory : BaseFactory<EmailConfirmationToken>, IEmailConfirmationTokenFactory
{
    protected override EmailConfirmationToken CreateInstance(params object[] parameters)
    {
        return parameters switch
        {
            [Guid userId] => new EmailConfirmationToken
            {
                Id = Guid.NewGuid(),
                Token = GenerateToken(),
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                Consumed = false,
                IdUsers = userId,
                CreationTime = DateTime.UtcNow
            },
            
            [Guid userId, TimeSpan validity] => new EmailConfirmationToken
            {
                Id = Guid.NewGuid(),
                Token = GenerateToken(),
                ExpiresAt = DateTime.UtcNow.Add(validity),
                Consumed = false,
                IdUsers = userId,
                CreationTime = DateTime.UtcNow
            },
            
            _ => throw new ArgumentException(
                "Unhandled parameters. Await : (userId) or (userId, validity)")
        };
    }
    
    private static string GenerateToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("/", "_")
            .Replace("+", "-")
            .TrimEnd('=');
    }
}