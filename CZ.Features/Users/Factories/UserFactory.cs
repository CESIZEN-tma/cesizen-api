using api.CZ.Core.Factories;
using api.CZ.Features.Users.Models;

namespace api.CZ.Features.Users.Factories;

public class UserFactory : BaseFactory<User>, IUserFactory
{
    protected override User CreateInstance(params object[] parameters)
    {
        // Default création
        if (parameters.Length == 0)
        {
            return new User
            {
                Id = Guid.NewGuid(),
                CreationTime = DateTime.Now,
                Active = true
            };
        }

        // Creation with typed parameters
        return parameters switch
        {
            [string email] => new User()
            {
                Id = Guid.NewGuid(),
                Email = email,
                CreationTime = DateTime.UtcNow,
                Active = true
            },
            
            [string email, string firstName, string lastName, string hash] => new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                PasswordHash = hash,
                CreationTime = DateTime.UtcNow,
                Active = true
            },
            
            _ => throw new ArgumentException(
                $"Unhandled parameters. Await : () or (email) or (email, firstName, lastName)")
        };
    }
}