using api.CZ.Data.EFCore;
using api.CZ.Data.Repositories;
using api.CZ.Features.AdminEmailConfirmationTokens.Models;

namespace api.CZ.Features.AdminEmailConfirmationTokens.Repositories;

public class AdminEmailConfirmationTokenRepository : BaseRepository<AdminEmailConfirmationToken>, IAdminEmailConfirmationTokenRepository
{
    public AdminEmailConfirmationTokenRepository(CesiZenDbContext context) : base(context)
    {
    }
}
