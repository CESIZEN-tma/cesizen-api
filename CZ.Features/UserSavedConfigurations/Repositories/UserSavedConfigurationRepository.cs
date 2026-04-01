using api.CZ.Data.EFCore;
using api.CZ.Data.Repositories;
using api.CZ.Features.UserSavedConfigurations.Models;
using Microsoft.EntityFrameworkCore;

namespace api.CZ.Features.UserSavedConfigurations.Repositories;

public class UserSavedConfigurationRepository : BaseRepository<UserSavedConfiguration>, IUserSavedConfigurationRepository
{
    private readonly CesiZenDbContext _context;

    public UserSavedConfigurationRepository(CesiZenDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<UserSavedConfiguration>> GetByUserIdAsync(Guid userId)
    {
        return await _context.UserSavedConfigurations
            .Where(c => c.IdUser == userId && c.DeletionTime == null)
            .ToListAsync();
    }
}
