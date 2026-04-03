using api.CZ.Features.Quizzes.DTOs;
using api.CZ.Features.Quizzes.Models;

namespace api.CZ.Features.Quizzes.Extensions;

public static class QuizzExtensions
{
    public static GetQuizzDto ToDto(this Quizz quizz)
    {
        return new GetQuizzDto
        {
            Id = quizz.Id,
            Nom = quizz.Nom,
            Active = quizz.Active,
            CreationTime = quizz.CreationTime,
            QuestionCount = quizz.Questions.Count
        };
    }
}
