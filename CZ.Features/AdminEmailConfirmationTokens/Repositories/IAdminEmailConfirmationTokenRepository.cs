using api.CZ.Data.Repositories;
using api.CZ.Features.AdminEmailConfirmationTokens.Models;

namespace api.CZ.Features.AdminEmailConfirmationTokens.Repositories;

public interface IAdminEmailConfirmationTokenRepository : IBaseRepository<AdminEmailConfirmationToken>
{
}
