using api.CZ.Features.AdminLogs.Services;
using api.CZ.Features.Quizzes.DTOs;
using api.CZ.Features.Quizzes.Factories;
using api.CZ.Features.Quizzes.Models;
using api.CZ.Features.Quizzes.Repositories;

namespace api.CZ.Features.Quizzes.Services;

public class QuizzService : IQuizzService
{
    private readonly IQuizzRepository _quizzRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IResponsesOptionRepository _optionRepository;
    private readonly IQuizzFactory _quizzFactory;
    private readonly IAdminActionLogger _actionLogger;

    public QuizzService(
        IQuizzRepository quizzRepository,
        IQuestionRepository questionRepository,
        IResponsesOptionRepository optionRepository,
        IQuizzFactory quizzFactory,
        IAdminActionLogger actionLogger)
    {
        _quizzRepository = quizzRepository;
        _questionRepository = questionRepository;
        _optionRepository = optionRepository;
        _quizzFactory = quizzFactory;
        _actionLogger = actionLogger;
    }

    public async Task<IEnumerable<GetQuizzDto>> GetAllAsync()
    {
        var quizzes = await _quizzRepository.ListAsync(q => q.DeletionTime == null);

        return quizzes.Select(q => new GetQuizzDto
        {
            Id = q.Id,
            Nom = q.Nom,
            Active = q.Active,
            CreationTime = q.CreationTime,
            QuestionCount = q.Questions.Count
        });
    }

    public async Task<GetQuizzDetailDto?> GetByIdAsync(Guid id)
    {
        var quizz = await _quizzRepository.GetWithQuestionsAsync(id);

        if (quizz == null)
            return null;

        return new GetQuizzDetailDto
        {
            Id = quizz.Id,
            Nom = quizz.Nom,
            Active = quizz.Active,
            CreationTime = quizz.CreationTime,
            Questions = quizz.Questions
                .OrderBy(q => q.Position)
                .Select(q => new QuestionDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    Position = q.Position,
                    Options = q.ResponsesOptions
                        .OrderBy(o => o.Position)
                        .Select(o => new ResponseOptionDto
                        {
                            Id = o.Id,
                            Label = o.Label,
                            Position = o.Position,
                            TargetedField = o.TargetedField,
                            Operation = o.Operation,
                            Value = o.Value
                        }).ToList()
                }).ToList()
        };
    }

    public async Task<GetQuizzDetailDto> CreateAsync(CreateQuizzDto dto, Guid adminId)
    {
        var quizz = _quizzFactory.Create(dto.Nom);
        quizz.Active = dto.Active;

        await _quizzRepository.AddAsync(quizz);

        foreach (var questionDto in dto.Questions)
        {
            var question = new Question
            {
                Id = Guid.NewGuid(),
                Text = questionDto.Text,
                Position = questionDto.Position,
                IdQuizz = quizz.Id,
                CreationTime = DateTime.UtcNow
            };

            await _questionRepository.AddAsync(question);

            foreach (var optionDto in questionDto.Options)
            {
                var option = new ResponsesOption
                {
                    Id = Guid.NewGuid(),
                    Label = optionDto.Label,
                    Position = optionDto.Position,
                    TargetedField = optionDto.TargetedField,
                    Operation = optionDto.Operation,
                    Value = optionDto.Value,
                    IdQuestions = question.Id,
                    CreationTime = DateTime.UtcNow
                };

                await _optionRepository.AddAsync(option);
            }
        }

        // Log the create action
        await _actionLogger.LogCreateAsync(adminId, "Quiz", quizz.Id,
            $"Created quiz '{quizz.Nom}' with {dto.Questions.Count} questions");

        return (await GetByIdAsync(quizz.Id))!;
    }

    public async Task<GetQuizzDto?> UpdateAsync(Guid id, UpdateQuizzDto dto, Guid adminId)
    {
        var quizz = await _quizzRepository.FindAsync(id);

        if (quizz == null || quizz.DeletionTime != null)
            return null;

        quizz.Nom = dto.Nom;
        quizz.Active = dto.Active;
        quizz.UpdateTime = DateTime.UtcNow;

        await _quizzRepository.UpdateAsync(quizz);

        // Log the update action
        await _actionLogger.LogUpdateAsync(adminId, "Quiz", quizz.Id,
            $"Updated quiz '{quizz.Nom}'");

        return new GetQuizzDto
        {
            Id = quizz.Id,
            Nom = quizz.Nom,
            Active = quizz.Active,
            CreationTime = quizz.CreationTime,
            QuestionCount = quizz.Questions.Count
        };
    }

    public async Task<bool> DeleteAsync(Guid id, Guid adminId)
    {
        var quizz = await _quizzRepository.FindAsync(id);

        if (quizz == null || quizz.DeletionTime != null)
            return false;

        var quizzName = quizz.Nom;

        quizz.DeletionTime = DateTime.UtcNow;
        quizz.UpdateTime = DateTime.UtcNow;

        await _quizzRepository.SoftDeleteAsync(quizz);

        // Log the delete action
        await _actionLogger.LogDeleteAsync(adminId, "Quiz", quizz.Id,
            $"Deleted quiz '{quizzName}'");

        return true;
    }
}
