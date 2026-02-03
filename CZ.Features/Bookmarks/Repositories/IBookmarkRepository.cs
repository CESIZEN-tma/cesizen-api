using api.CZ.Data.Repositories;
using api.CZ.Features.Bookmarks.Models;

namespace api.CZ.Features.Bookmarks.Repositories;

public interface IBookmarkRepository : IBaseRepository<Bookmark>
{
    Task<List<Bookmark>> GetUserBookmarksAsync(Guid userId);
    Task<Bookmark?> GetUserBookmarkAsync(Guid userId, Guid configurationId);
}
