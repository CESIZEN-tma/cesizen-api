using api.CZ.Data.EFCore;
using api.CZ.Data.Repositories;
using api.CZ.Features.Quizzes.Models;
using Microsoft.EntityFrameworkCore;

namespace api.CZ.Features.Quizzes.Repositories;

public class QuizzRepository : BaseRepository<Quizz>, IQuizzRepository
{
    private readonly CesiZenDbContext _context;

    public QuizzRepository(CesiZenDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Quizz?> GetWithQuestionsAsync(Guid id)
    {
        return await _context.Quizzs
            .Include(q => q.Questions)
                .ThenInclude(q => q.ResponsesOptions)
            .FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<IEnumerable<Quizz>> ListWithQuestionCountAsync()
    {
        return await _context.Quizzs
            .Include(q => q.Questions)
            .ToListAsync();
    }
}
