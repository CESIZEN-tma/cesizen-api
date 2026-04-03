using api.CZ.Data.EFCore;
using api.CZ.Data.Repositories;
using api.CZ.Features.PasswordsInfos.Models;

namespace api.CZ.Features.PasswordsInfos.Repositories;

public class PasswordsInfoRepository : BaseRepository<PasswordsInfo>, IPasswordsInfoRepository
{
    public PasswordsInfoRepository(CesiZenDbContext context) : base(context)
    {
    }
}
