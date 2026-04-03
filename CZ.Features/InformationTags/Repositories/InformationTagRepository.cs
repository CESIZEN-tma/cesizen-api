using api.CZ.Data.EFCore;
using api.CZ.Data.Repositories;
using api.CZ.Features.InformationTags.Models;

namespace api.CZ.Features.InformationTags.Repositories;

public class InformationTagRepository : BaseRepository<InformationTag>, IInformationTagRepository
{
    public InformationTagRepository(CesiZenDbContext context) : base(context)
    {
    }
}
