using api.CZ.Data.EFCore;
using api.CZ.Data.Repositories;
using api.CZ.Features.NavigationMenus.Models;

namespace api.CZ.Features.NavigationMenus.Repositories;

public class NavigationMenuRepository : BaseRepository<NavigationMenu>, INavigationMenuRepository
{
    public NavigationMenuRepository(CesiZenDbContext context) : base(context)
    {
    }
}
