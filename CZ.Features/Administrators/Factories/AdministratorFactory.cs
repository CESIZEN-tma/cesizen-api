using api.CZ.Core.Factories;
using api.CZ.Features.Administrators.Models;

namespace api.CZ.Features.Administrators.Factories;

public class AdministratorFactory : BaseFactory<Administrator>, IAdministratorFactory
{
    protected override Administrator CreateInstance(params object[] parameters)
    {
        if (parameters.Length == 0)
        {
            return new Administrator
            {
                Id = Guid.NewGuid(),
                CreationTime = DateTime.UtcNow
            };
        }

        return parameters switch
        {
            [string email] => new Administrator
            {
                Id = Guid.NewGuid(),
                Email = email,
                CreationTime = DateTime.UtcNow
            },

            [string email, string firstName, string lastName, string hash] => new Administrator
            {
                Id = Guid.NewGuid(),
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                PasswordHash = hash,
                CreationTime = DateTime.UtcNow
            },

            _ => throw new ArgumentException(
                "Unhandled parameters. Expected: () or (email) or (email, firstName, lastName, hash)")
        };
    }
}
