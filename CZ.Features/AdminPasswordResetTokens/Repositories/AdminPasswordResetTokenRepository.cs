using api.CZ.Data.EFCore;
using api.CZ.Data.Repositories;
using api.CZ.Features.AdminPasswordResetTokens.Models;

namespace api.CZ.Features.AdminPasswordResetTokens.Repositories;

public class AdminPasswordResetTokenRepository : BaseRepository<AdminPasswordResetToken>, IAdminPasswordResetTokenRepository
{
    public AdminPasswordResetTokenRepository(CesiZenDbContext context) : base(context)
    {
    }
}
