using api.CZ.Core.Factories;
using api.CZ.Features.Sessions.Models;

namespace api.CZ.Features.Sessions.Factories;

public class SessionFactory : BaseFactory<Session>, ISessionFactory
{
    protected override Session CreateInstance(params object[] parameters)
    {
        return parameters switch
        {
            [Guid userId, string refreshToken, DateTime expiresAt] => new Session
            {
                Id = Guid.NewGuid(),
                Token = refreshToken,
                Consumed = false,
                ExpiresAt = expiresAt,
                IdUsers = userId,
                CreationTime = DateTime.UtcNow
            },

            [Guid userId, string refreshToken, TimeSpan validity] => new Session
            {
                Id = Guid.NewGuid(),
                Token = refreshToken,
                Consumed = false,
                ExpiresAt = DateTime.UtcNow.Add(validity),
                IdUsers = userId,
                CreationTime = DateTime.UtcNow
            },

            _ => throw new ArgumentException(
                "Unhandled parameters. Expected: (userId, refreshToken, expiresAt) or (userId, refreshToken, validity)")
        };
    }
}
