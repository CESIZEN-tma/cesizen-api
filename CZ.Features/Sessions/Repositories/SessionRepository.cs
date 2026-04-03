using api.CZ.Data.EFCore;
using api.CZ.Data.Repositories;
using api.CZ.Features.Sessions.Models;

namespace api.CZ.Features.Sessions.Repositories;

public class SessionRepository : BaseRepository<Session>, ISessionRepository
{
    public SessionRepository(CesiZenDbContext context) : base(context)
    {
    }
}
