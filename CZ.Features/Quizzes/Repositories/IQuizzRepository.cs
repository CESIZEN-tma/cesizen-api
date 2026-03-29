using api.CZ.Data.Repositories;
using api.CZ.Features.Quizzes.Models;

namespace api.CZ.Features.Quizzes.Repositories;

public interface IQuizzRepository : IBaseRepository<Quizz>
{
    Task<Quizz?> GetWithQuestionsAsync(Guid id);
    Task<IEnumerable<Quizz>> ListWithQuestionCountAsync();
}
