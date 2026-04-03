using api.CZ.Core.Factories;
using api.CZ.Features.AdminPasswordResetTokens.Models;

namespace api.CZ.Features.AdminPasswordResetTokens.Factories;

public class AdminPasswordResetTokenFactory : BaseFactory<AdminPasswordResetToken>, IAdminPasswordResetTokenFactory
{
    protected override AdminPasswordResetToken CreateInstance(params object[] parameters)
    {
        return parameters switch
        {
            [Guid adminId] => new AdminPasswordResetToken
            {
                Id = Guid.NewGuid(),
                Token = GenerateToken(),
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                Consumed = false,
                IdAdministrators = adminId,
                CreationTime = DateTime.UtcNow
            },

            [Guid adminId, TimeSpan validity] => new AdminPasswordResetToken
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
