using api.CZ.Core.Factories;
using api.CZ.Features.PasswordResetTokens.Models;

namespace api.CZ.Features.PasswordResetTokens.Factories;

public class PasswordResetTokenFactory : BaseFactory<PasswordResetToken>, IPasswordResetTokenFactory
{
    protected override PasswordResetToken CreateInstance(params object[] parameters)
    {
        return parameters switch
        {
            [Guid userId] => new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                Token = GenerateToken(),
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                Consumed = false,
                IdUsers = userId,
                CreationTime = DateTime.UtcNow
            },

            [Guid userId, TimeSpan validity] => new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                Token = GenerateToken(),
                ExpiresAt = DateTime.UtcNow.Add(validity),
                Consumed = false,
                IdUsers = userId,
                CreationTime = DateTime.UtcNow
            },

            _ => throw new ArgumentException(
                "Unhandled parameters. Expected: (userId) or (userId, validity)")
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
