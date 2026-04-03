using api.CZ.Data.EFCore;
using api.CZ.Data.Repositories;
using api.CZ.Features.AdminSessions.Models;

namespace api.CZ.Features.AdminSessions.Repositories;

public class AdminSessionRepository : BaseRepository<AdminSession>, IAdminSessionRepository
{
    public AdminSessionRepository(CesiZenDbContext context) : base(context)
    {
    }
}
