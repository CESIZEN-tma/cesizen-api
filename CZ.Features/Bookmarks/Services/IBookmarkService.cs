using api.CZ.Features.Bookmarks.DTOs;

namespace api.CZ.Features.Bookmarks.Services;

public interface IBookmarkService
{
    Task<IEnumerable<GetBookmarkDto>> GetUserBookmarksAsync(Guid userId);
    Task<GetBookmarkDto?> CreateBookmarkAsync(Guid userId, CreateBookmarkDto dto);
    Task<bool> DeleteBookmarkAsync(Guid userId, Guid configurationId);
}
