using api.CZ.Core.Factories;
using api.CZ.Features.Quizzes.Models;

namespace api.CZ.Features.Quizzes.Factories;

public class QuizzFactory : BaseFactory<Quizz>, IQuizzFactory
{
    protected override Quizz CreateInstance(params object[] parameters)
    {
        if (parameters.Length == 0)
        {
            return new Quizz
            {
                Id = Guid.NewGuid(),
                CreationTime = DateTime.UtcNow,
                Active = true
            };
        }

        return parameters switch
        {
            [string nom] => new Quizz
            {
                Id = Guid.NewGuid(),
                Nom = nom,
                Active = true,
                CreationTime = DateTime.UtcNow
            },
            _ => throw new ArgumentException("Expected parameters: () or (nom)")
        };
    }
}
