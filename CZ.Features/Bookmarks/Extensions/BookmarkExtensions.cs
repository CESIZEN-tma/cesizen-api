using api.CZ.Features.Bookmarks.DTOs;
using api.CZ.Features.Bookmarks.Models;

namespace api.CZ.Features.Bookmarks.Extensions;

public static class BookmarkExtensions
{
    public static GetBookmarkDto ToDto(this Bookmark bookmark)
    {
        return new GetBookmarkDto
        {
            UserId = bookmark.Id,
            ConfigurationId = bookmark.IdConfigurations,
            ConfigurationName = bookmark.IdConfigurationsNavigation?.Name ?? "Unknown",
            CreationTime = bookmark.CreationTime
        };
    }
}
