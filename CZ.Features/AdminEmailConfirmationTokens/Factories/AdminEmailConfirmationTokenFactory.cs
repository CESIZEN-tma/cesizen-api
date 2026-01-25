using api.CZ.Core.Factories;
using api.CZ.Features.AdminEmailConfirmationTokens.Models;

namespace api.CZ.Features.AdminEmailConfirmationTokens.Factories;

public class AdminEmailConfirmationTokenFactory : BaseFactory<AdminEmailConfirmationToken>, IAdminEmailConfirmationTokenFactory
{
    protected override AdminEmailConfirmationToken CreateInstance(params object[] parameters)
    {
        return parameters switch
        {
            [Guid adminId] => new AdminEmailConfirmationToken
            {
                Id = Guid.NewGuid(),
                Token = GenerateToken(),
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                Consumed = false,
                IdAdministrators = adminId,
                CreationTime = DateTime.UtcNow
            },

            [Guid adminId, TimeSpan validity] => new AdminEmailConfirmationToken
            {
                Id = Guid.NewGuid(),
                Token = GenerateToken(),
                ExpiresAt = DateTime.UtcNow.Add(validity),
                Consumed = false,
                IdAdministrators = adminId,
                CreationTime = DateTime.UtcNow
            },

            _ => throw new ArgumentException(
                "Unhandled parameters. Expected: (adminId) or (adminId, validity)")
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
