using api.CZ.Data.Repositories;
using api.CZ.Features.EmailConfirmationTokens.Models;

namespace api.CZ.Features.EmailConfirmationTokens.Repositories;

public interface IEmailConfirmationTokenRepository : IBaseRepository<EmailConfirmationToken>
{
}
