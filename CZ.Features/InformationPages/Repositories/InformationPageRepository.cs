using api.CZ.Data.EFCore;
using api.CZ.Data.Repositories;
using api.CZ.Features.InformationPages.Models;

namespace api.CZ.Features.InformationPages.Repositories;

public class InformationPageRepository : BaseRepository<InformationPage>, IInformationPageRepository
{
    public InformationPageRepository(CesiZenDbContext context) : base(context)
    {
    }
}
