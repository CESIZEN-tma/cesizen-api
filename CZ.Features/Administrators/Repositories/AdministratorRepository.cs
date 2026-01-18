using api.CZ.Data.EFCore;
using api.CZ.Data.Repositories;
using api.CZ.Features.Administrators.Models;

namespace api.CZ.Features.Administrators.Repositories;

public class AdministratorRepository : BaseRepository<Administrator>, IAdministratorRepository
{
    public AdministratorRepository(CesiZenDbContext context) : base(context)
    {
    }
}
