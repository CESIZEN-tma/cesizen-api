using api.CZ.Data.EFCore;
using api.CZ.Data.Repositories;
using api.CZ.Features.Quizzes.Models;

namespace api.CZ.Features.Quizzes.Repositories;

public class QuestionRepository : BaseRepository<Question>, IQuestionRepository
{
    public QuestionRepository(CesiZenDbContext context) : base(context)
    {
    }
}
