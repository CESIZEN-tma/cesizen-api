using api.CZ.Data.EFCore;
using api.CZ.Data.Repositories;
using api.CZ.Features.PasswordResetTokens.Models;

namespace api.CZ.Features.PasswordResetTokens.Repositories;

public class PasswordResetTokenRepository : BaseRepository<PasswordResetToken>, IPasswordResetTokenRepository
{
    public PasswordResetTokenRepository(CesiZenDbContext context) : base(context)
    {
    }
}
