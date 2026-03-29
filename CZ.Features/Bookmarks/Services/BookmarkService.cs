using api.CZ.Features.Bookmarks.DTOs;
using api.CZ.Features.Bookmarks.Extensions;
using api.CZ.Features.Bookmarks.Factories;
using api.CZ.Features.Bookmarks.Repositories;

namespace api.CZ.Features.Bookmarks.Services;

public class BookmarkService : IBookmarkService
{
    private readonly IBookmarkRepository _repository;
    private readonly IBookmarkFactory _factory;

    public BookmarkService(IBookmarkRepository repository, IBookmarkFactory factory)
    {
        _repository = repository;
        _factory = factory;
    }

    public async Task<IEnumerable<GetBookmarkDto>> GetUserBookmarksAsync(Guid userId)
    {
        var bookmarks = await _repository.GetUserBookmarksAsync(userId);

        return bookmarks.Select(b => b.ToDto());
    }

    public async Task<GetBookmarkDto?> CreateBookmarkAsync(Guid userId, CreateBookmarkDto dto)
    {
        var existing = await _repository.GetUserBookmarkIncludingDeletedAsync(userId, dto.ConfigurationId);

        if (existing != null)
        {
            if (existing.DeletionTime == null)
                throw new InvalidOperationException("Bookmark already exists");

            existing.DeletionTime = null;
            existing.UpdateTime = DateTime.UtcNow;
            await _repository.UpdateAsync(existing);
            return existing.ToDto();
        }

        var bookmark = _factory.Create(userId, dto.ConfigurationId);
        await _repository.AddAsync(bookmark);

        return bookmark.ToDto();
    }

    public async Task<bool> DeleteBookmarkAsync(Guid userId, Guid configurationId)
    {
        var bookmark = await _repository.GetUserBookmarkAsync(userId, configurationId);

        if (bookmark == null)
            return false;

        bookmark.DeletionTime = DateTime.UtcNow;
        await _repository.SoftDeleteAsync(bookmark);

        return true;
    }
}
