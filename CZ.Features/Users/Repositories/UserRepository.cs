using api.CZ.Data.EFCore;
using api.CZ.Data.Repositories;
using api.CZ.Features.Users.Models;

namespace api.CZ.Features.Users.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(CesiZenDbContext context) : base(context)
    {
      
    }
}
