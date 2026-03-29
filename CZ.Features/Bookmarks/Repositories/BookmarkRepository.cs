using api.CZ.Data.EFCore;
using api.CZ.Data.Repositories;
using api.CZ.Features.Bookmarks.Models;
using Microsoft.EntityFrameworkCore;

namespace api.CZ.Features.Bookmarks.Repositories;

public class BookmarkRepository : BaseRepository<Bookmark>, IBookmarkRepository
{
    private readonly CesiZenDbContext _context;

    public BookmarkRepository(CesiZenDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<Bookmark>> GetUserBookmarksAsync(Guid userId)
    {
        return await _context.Bookmarks
            .Include(b => b.IdConfigurationsNavigation)
            .Where(b => b.Id == userId && b.DeletionTime == null)
            .ToListAsync();
    }

    public async Task<Bookmark?> GetUserBookmarkAsync(Guid userId, Guid configurationId)
    {
        return await _context.Bookmarks
            .Include(b => b.IdConfigurationsNavigation)
            .FirstOrDefaultAsync(b => b.Id == userId && b.IdConfigurations == configurationId && b.DeletionTime == null);
    }

    public async Task<Bookmark?> GetUserBookmarkIncludingDeletedAsync(Guid userId, Guid configurationId)
    {
        return await _context.Bookmarks
            .Include(b => b.IdConfigurationsNavigation)
            .FirstOrDefaultAsync(b => b.Id == userId && b.IdConfigurations == configurationId);
    }
}
