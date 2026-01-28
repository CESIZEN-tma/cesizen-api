using api.CZ.Data.EFCore;
using api.CZ.Data.Repositories;
using api.CZ.Features.PasswordHistories.Models;

namespace api.CZ.Features.PasswordHistories.Repositories;

public class PasswordHistoryRepository : BaseRepository<PasswordHistory>, IPasswordHistoryRepository
{
    public PasswordHistoryRepository(CesiZenDbContext context) : base(context)
    {
    }
}
