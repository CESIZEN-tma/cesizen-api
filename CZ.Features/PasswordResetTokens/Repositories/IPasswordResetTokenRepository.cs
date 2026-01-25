using api.CZ.Data.Repositories;
using api.CZ.Features.PasswordResetTokens.Models;

namespace api.CZ.Features.PasswordResetTokens.Repositories;

public interface IPasswordResetTokenRepository : IBaseRepository<PasswordResetToken>
{
}
