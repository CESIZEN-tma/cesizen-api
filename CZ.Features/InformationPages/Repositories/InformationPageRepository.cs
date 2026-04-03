using api.CZ.Data.EFCore;
using api.CZ.Data.Repositories;
using api.CZ.Features.InformationPages.Models;
using Microsoft.EntityFrameworkCore;

namespace api.CZ.Features.InformationPages.Repositories;

public class InformationPageRepository : BaseRepository<InformationPage>, IInformationPageRepository
{
    private readonly CesiZenDbContext _context;

    public InformationPageRepository(CesiZenDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<InformationPage>> ListWithTagsAsync()
    {
        return await _context.InformationPages
            .Include(p => p.IdInformationTags)
            .Where(p => p.DeletionTime == null)
            .ToListAsync();
    }

    public async Task<InformationPage?> FindWithTagsAsync(Guid id)
    {
        return await _context.InformationPages
            .Include(p => p.IdInformationTags)
            .FirstOrDefaultAsync(p => p.Id == id && p.DeletionTime == null);
    }
}
