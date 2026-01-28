using api.CZ.Data.EFCore;
using api.CZ.Data.Repositories;
using api.CZ.Features.Quizzes.Models;

namespace api.CZ.Features.Quizzes.Repositories;

public class ResponsesOptionRepository : BaseRepository<ResponsesOption>, IResponsesOptionRepository
{
    public ResponsesOptionRepository(CesiZenDbContext context) : base(context)
    {
    }
}
