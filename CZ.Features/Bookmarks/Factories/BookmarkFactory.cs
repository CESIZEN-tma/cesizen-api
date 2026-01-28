using api.CZ.Core.Factories;
using api.CZ.Features.Bookmarks.Models;

namespace api.CZ.Features.Bookmarks.Factories;

public class BookmarkFactory : BaseFactory<Bookmark>, IBookmarkFactory
{
    protected override Bookmark CreateInstance(params object[] parameters)
    {
        if (parameters.Length == 0)
        {
            return new Bookmark
            {
                CreationTime = DateTime.UtcNow
            };
        }

        return parameters switch
        {
            [Guid userId, Guid configurationId] => new Bookmark
            {
                Id = userId,
                IdConfigurations = configurationId,
                CreationTime = DateTime.UtcNow
            },
            _ => throw new ArgumentException("Expected parameters: (userId, configurationId)")
        };
    }
}
