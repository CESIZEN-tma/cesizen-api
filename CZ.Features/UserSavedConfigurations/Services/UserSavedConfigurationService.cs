using api.CZ.Features.UserSavedConfigurations.DTOs;
using api.CZ.Features.UserSavedConfigurations.Models;
using api.CZ.Features.UserSavedConfigurations.Repositories;
using api.CZ.Features.Quizzes.Repositories;

namespace api.CZ.Features.UserSavedConfigurations.Services;

public class UserSavedConfigurationService : IUserSavedConfigurationService
{
    private readonly IUserSavedConfigurationRepository _repository;
    private readonly IQuizzRepository _quizzRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IResponsesOptionRepository _responsesOptionRepository;

    public UserSavedConfigurationService(
        IUserSavedConfigurationRepository repository,
        IQuizzRepository quizzRepository,
        IQuestionRepository questionRepository,
        IResponsesOptionRepository responsesOptionRepository)
    {
        _repository = repository;
        _quizzRepository = quizzRepository;
        _questionRepository = questionRepository;
        _responsesOptionRepository = responsesOptionRepository;
    }

    public async Task<IEnumerable<GetUserSavedConfigurationDto>> GetAllAsync()
    {
        var configurations = await _repository.ListAsync(c => c.DeletionTime == null);

        return configurations.Select(c => new GetUserSavedConfigurationDto
        {
            Id = c.Id,
            Name = c.Name,
            Inhalation = c.Inhalation,
            Retention1 = c.Retention1,
            Exhalation = c.Exhalation,
            Retention2 = c.Retention2,
            DurationMinutes = c.DurationMinutes,
            Difficulty = c.Difficulty,
            Objective = c.Objective,
            GuidanceType = c.GuidanceType,
            CreationTime = c.CreationTime,
            UpdateTime = c.UpdateTime
        });
    }

    public async Task<GetUserSavedConfigurationDto?> GetByIdAsync(Guid id)
    {
        var configuration = await _repository.FindAsync(id);

        if (configuration == null || configuration.DeletionTime != null)
            return null;

        return new GetUserSavedConfigurationDto
        {
            Id = configuration.Id,
            Name = configuration.Name,
            Inhalation = configuration.Inhalation,
            Retention1 = configuration.Retention1,
            Exhalation = configuration.Exhalation,
            Retention2 = configuration.Retention2,
            DurationMinutes = configuration.DurationMinutes,
            Difficulty = configuration.Difficulty,
            Objective = configuration.Objective,
            GuidanceType = configuration.GuidanceType,
            CreationTime = configuration.CreationTime,
            UpdateTime = configuration.UpdateTime
        };
    }

    public async Task<GetUserSavedConfigurationDto?> CreateAsync(CreateUserSavedConfigurationDto dto)
    {
        var configuration = new UserSavedConfiguration
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Inhalation = dto.Inhalation,
            Retention1 = dto.Retention1,
            Exhalation = dto.Exhalation,
            Retention2 = dto.Retention2,
            DurationMinutes = dto.DurationMinutes,
            Difficulty = dto.Difficulty,
            Objective = dto.Objective,
            GuidanceType = dto.GuidanceType,
            CreationTime = DateTime.UtcNow
        };

        await _repository.AddAsync(configuration);

        return new GetUserSavedConfigurationDto
        {
            Id = configuration.Id,
            Name = configuration.Name,
            Inhalation = configuration.Inhalation,
            Retention1 = configuration.Retention1,
            Exhalation = configuration.Exhalation,
            Retention2 = configuration.Retention2,
            DurationMinutes = configuration.DurationMinutes,
            Difficulty = configuration.Difficulty,
            Objective = configuration.Objective,
            GuidanceType = configuration.GuidanceType,
            CreationTime = configuration.CreationTime,
            UpdateTime = configuration.UpdateTime
        };
    }

    public async Task<GetUserSavedConfigurationDto?> UpdateAsync(Guid id, UpdateUserSavedConfigurationDto dto)
    {
        var configuration = await _repository.FindAsync(id);

        if (configuration == null || configuration.DeletionTime != null)
            return null;

        configuration.Name = dto.Name;
        configuration.Inhalation = dto.Inhalation;
        configuration.Retention1 = dto.Retention1;
        configuration.Exhalation = dto.Exhalation;
        configuration.Retention2 = dto.Retention2;
        configuration.DurationMinutes = dto.DurationMinutes;
        configuration.Difficulty = dto.Difficulty;
        configuration.Objective = dto.Objective;
        configuration.GuidanceType = dto.GuidanceType;
        configuration.UpdateTime = DateTime.UtcNow;

        await _repository.UpdateAsync(configuration);

        return new GetUserSavedConfigurationDto
        {
            Id = configuration.Id,
            Name = configuration.Name,
            Inhalation = configuration.Inhalation,
            Retention1 = configuration.Retention1,
            Exhalation = configuration.Exhalation,
            Retention2 = configuration.Retention2,
            DurationMinutes = configuration.DurationMinutes,
            Difficulty = configuration.Difficulty,
            Objective = configuration.Objective,
            GuidanceType = configuration.GuidanceType,
            CreationTime = configuration.CreationTime,
            UpdateTime = configuration.UpdateTime
        };
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var configuration = await _repository.FindAsync(id);

        if (configuration == null || configuration.DeletionTime != null)
            return false;

        configuration.DeletionTime = DateTime.UtcNow;
        configuration.UpdateTime = DateTime.UtcNow;

        await _repository.SoftDeleteAsync(configuration);

        return true;
    }

    public async Task<GetUserSavedConfigurationDto?> CreateFromQuizResponsesAsync(Guid userId, QuizResponsesDto dto)
    {
        // Validate quiz exists
        var quiz = await _quizzRepository.GetWithQuestionsAsync(dto.QuizId);
        if (quiz == null || quiz.DeletionTime != null)
            throw new InvalidOperationException($"Quiz with ID {dto.QuizId} not found.");

        // Validate all responses
        foreach (var response in dto.Responses)
        {
            // Validate question belongs to quiz
            var question = await _questionRepository.FindAsync(response.QuestionId);
            if (question == null || question.DeletionTime != null)
                throw new InvalidOperationException($"Question with ID {response.QuestionId} not found.");

            if (question.IdQuizz != dto.QuizId)
                throw new InvalidOperationException($"Question {response.QuestionId} does not belong to quiz {dto.QuizId}.");

            // Validate option belongs to question
            var option = await _responsesOptionRepository.FindAsync(response.SelectedOptionId);
            if (option == null || option.DeletionTime != null)
                throw new InvalidOperationException($"Response option with ID {response.SelectedOptionId} not found.");

            if (option.IdQuestions != response.QuestionId)
                throw new InvalidOperationException($"Response option {response.SelectedOptionId} does not belong to question {response.QuestionId}.");
        }

        // Start with default configuration values
        var config = new
        {
            Inhalation = 4,
            Retention1 = 4,
            Exhalation = 4,
            Retention2 = 4,
            DurationMinutes = 5,
            Difficulty = 1,
            Objective = "Relaxation",
            GuidanceType = "Visual"
        };

        // Apply operations from selected response options
        int inhalation = config.Inhalation;
        int retention1 = config.Retention1;
        int exhalation = config.Exhalation;
        int retention2 = config.Retention2;
        int durationMinutes = config.DurationMinutes;
        int difficulty = config.Difficulty;
        string objective = config.Objective;
        string guidanceType = config.GuidanceType;

        foreach (var response in dto.Responses)
        {
            var option = await _responsesOptionRepository.FindAsync(response.SelectedOptionId);
            if (option == null) continue;

            var targetedField = option.TargetedField.ToLowerInvariant();
            var operation = option.Operation.ToUpperInvariant();
            var value = option.Value;

            switch (targetedField)
            {
                case "inhalation":
                    inhalation = ApplyNumericOperation(inhalation, operation, value);
                    break;
                case "retention1":
                    retention1 = ApplyNumericOperation(retention1, operation, value);
                    break;
                case "exhalation":
                    exhalation = ApplyNumericOperation(exhalation, operation, value);
                    break;
                case "retention2":
                    retention2 = ApplyNumericOperation(retention2, operation, value);
                    break;
                case "durationminutes":
                    durationMinutes = ApplyNumericOperation(durationMinutes, operation, value);
                    break;
                case "difficulty":
                    difficulty = ApplyNumericOperation(difficulty, operation, value);
                    break;
                case "objective":
                    objective = ApplyStringOperation(objective, operation, value);
                    break;
                case "guidancetype":
                    guidanceType = ApplyStringOperation(guidanceType, operation, value);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown targeted field: {option.TargetedField}");
            }
        }

        // Create UserSavedConfiguration with generated values
        var userSavedConfiguration = new UserSavedConfiguration
        {
            Id = Guid.NewGuid(),
            Name = $"Generated from {quiz.Nom} - {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}",
            Inhalation = inhalation,
            Retention1 = retention1,
            Exhalation = exhalation,
            Retention2 = retention2,
            DurationMinutes = durationMinutes,
            Difficulty = difficulty,
            Objective = objective,
            GuidanceType = guidanceType,
            CreationTime = DateTime.UtcNow
        };

        await _repository.AddAsync(userSavedConfiguration);

        return new GetUserSavedConfigurationDto
        {
            Id = userSavedConfiguration.Id,
            Name = userSavedConfiguration.Name,
            Inhalation = userSavedConfiguration.Inhalation,
            Retention1 = userSavedConfiguration.Retention1,
            Exhalation = userSavedConfiguration.Exhalation,
            Retention2 = userSavedConfiguration.Retention2,
            DurationMinutes = userSavedConfiguration.DurationMinutes,
            Difficulty = userSavedConfiguration.Difficulty,
            Objective = userSavedConfiguration.Objective,
            GuidanceType = userSavedConfiguration.GuidanceType,
            CreationTime = userSavedConfiguration.CreationTime,
            UpdateTime = userSavedConfiguration.UpdateTime
        };
    }

    private int ApplyNumericOperation(int currentValue, string operation, string value)
    {
        if (!int.TryParse(value, out int numericValue))
            throw new InvalidOperationException($"Cannot parse value '{value}' as integer.");

        return operation switch
        {
            "SET" => numericValue,
            "ADD" => currentValue + numericValue,
            "MULTIPLY" => currentValue * numericValue,
            _ => throw new InvalidOperationException($"Unknown operation: {operation}")
        };
    }

    private string ApplyStringOperation(string currentValue, string operation, string value)
    {
        return operation switch
        {
            "SET" => value,
            _ => throw new InvalidOperationException($"Operation {operation} is not supported for string fields.")
        };
    }
}
