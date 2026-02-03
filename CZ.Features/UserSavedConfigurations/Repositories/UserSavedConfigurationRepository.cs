using api.CZ.Data.EFCore;
using api.CZ.Data.Repositories;
using api.CZ.Features.UserSavedConfigurations.Models;

namespace api.CZ.Features.UserSavedConfigurations.Repositories;

public class UserSavedConfigurationRepository : BaseRepository<UserSavedConfiguration>, IUserSavedConfigurationRepository
{
    public UserSavedConfigurationRepository(CesiZenDbContext context) : base(context)
    {
    }
}
