using api.CZ.Data.EFCore;
using api.CZ.Data.Repositories;
using api.CZ.Features.EmailConfirmationTokens.Models;

namespace api.CZ.Features.EmailConfirmationTokens.Repositories;

public class EmailConfirmationTokenRepository :  BaseRepository<EmailConfirmationToken>, IEmailConfirmationTokenRepository
{
    public EmailConfirmationTokenRepository(CesiZenDbContext context) : base(context)
    {
      
    }
}
