using api.CZ.Data.EFCore;
using api.CZ.Data.Repositories;
using api.CZ.Features.Configurations.Models;

namespace api.CZ.Features.Configurations.Repositories;

public class ConfigurationRepository : BaseRepository<Configuration>, IConfigurationRepository
{
    public ConfigurationRepository(CesiZenDbContext context) : base(context)
    {
    }
}
