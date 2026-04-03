using api.CZ.Core.Factories;
using api.CZ.Features.AdminSessions.Models;

namespace api.CZ.Features.AdminSessions.Factories;

public class AdminSessionFactory : BaseFactory<AdminSession>, IAdminSessionFactory
{
    protected override AdminSession CreateInstance(params object[] parameters)
    {
        return parameters switch
        {
            [Guid adminId, string refreshToken, DateTime expiresAt] => new AdminSession
            {
                Id = Guid.NewGuid(),
                Token = refreshToken,
                Consumed = false,
                ExpiresAt = expiresAt,
                IdAdministrators = adminId,
                CreationTime = DateTime.UtcNow
            },

            [Guid adminId, string refreshToken, TimeSpan validity] => new AdminSession
            {
                Id = Guid.NewGuid(),
                Token = refreshToken,
                Consumed = false,
                ExpiresAt = DateTime.UtcNow.Add(validity),
                IdAdministrators = adminId,
                CreationTime = DateTime.UtcNow
            },

            _ => throw new ArgumentException(
                "Unhandled parameters. Expected: (adminId, refreshToken, expiresAt) or (adminId, refreshToken, validity)")
        };
    }
}
