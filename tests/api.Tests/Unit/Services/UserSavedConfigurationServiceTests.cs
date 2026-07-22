using FluentAssertions;
using Moq;
using api.CZ.Features.Quizzes.Models;
using api.CZ.Features.Quizzes.Repositories;
using api.CZ.Features.UserSavedConfigurations.DTOs;
using api.CZ.Features.UserSavedConfigurations.Models;
using api.CZ.Features.UserSavedConfigurations.Repositories;
using api.CZ.Features.UserSavedConfigurations.Services;

namespace api.Tests.Unit.Services;

public class UserSavedConfigurationServiceTests
{
    private readonly Mock<IUserSavedConfigurationRepository> _mockRepository;
    private readonly Mock<IQuizzRepository> _mockQuizzRepository;
    private readonly Mock<IQuestionRepository> _mockQuestionRepository;
    private readonly Mock<IResponsesOptionRepository> _mockOptionRepository;
    private readonly UserSavedConfigurationService _sut;

    public UserSavedConfigurationServiceTests()
    {
        _mockRepository = new Mock<IUserSavedConfigurationRepository>();
        _mockQuizzRepository = new Mock<IQuizzRepository>();
        _mockQuestionRepository = new Mock<IQuestionRepository>();
        _mockOptionRepository = new Mock<IResponsesOptionRepository>();

        _sut = new UserSavedConfigurationService(
            _mockRepository.Object, _mockQuizzRepository.Object, _mockQuestionRepository.Object, _mockOptionRepository.Object);
    }

    private static UserSavedConfiguration BuildConfig(Guid? id = null, Guid? userId = null)
    {
        return new UserSavedConfiguration
        {
            Id = id ?? Guid.NewGuid(),
            IdUser = userId ?? Guid.NewGuid(),
            Name = "Config",
            Inhalation = 4,
            Retention1 = 4,
            Exhalation = 4,
            Retention2 = 4,
            DurationMinutes = 5,
            Difficulty = 1,
            Objective = "Relaxation",
            GuidanceType = "Visual",
            CreationTime = DateTime.UtcNow
        };
    }

    private static CreateUserSavedConfigurationDto BuildCreateDto()
    {
        return new CreateUserSavedConfigurationDto
        {
            Name = "New Config",
            Inhalation = 4,
            Retention1 = 4,
            Exhalation = 4,
            Retention2 = 4,
            DurationMinutes = 5,
            Difficulty = 1,
            Objective = "Relaxation",
            GuidanceType = "Visual"
        };
    }

    [Fact]
    public async Task GetByUserAsync_ReturnsMappedDtos()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var configs = new List<UserSavedConfiguration> { BuildConfig(userId: userId), BuildConfig(userId: userId) };
        _mockRepository.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(configs);

        // Act
        var result = (await _sut.GetByUserAsync(userId)).ToList();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingConfig_ReturnsDto()
    {
        // Arrange
        var config = BuildConfig();
        _mockRepository.Setup(r => r.FindAsync(config.Id, It.IsAny<CancellationToken>())).ReturnsAsync(config);

        // Act
        var result = await _sut.GetByIdAsync(config.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(config.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentConfig_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.FindAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((UserSavedConfiguration?)null);

        // Act
        var result = await _sut.GetByIdAsync(id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_SoftDeletedConfig_ReturnsNull()
    {
        // Arrange
        var config = BuildConfig();
        config.DeletionTime = DateTime.UtcNow;
        _mockRepository.Setup(r => r.FindAsync(config.Id, It.IsAny<CancellationToken>())).ReturnsAsync(config);

        // Act
        var result = await _sut.GetByIdAsync(config.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ValidDto_CreatesConfigForUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = BuildCreateDto();
        UserSavedConfiguration? added = null;
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<UserSavedConfiguration>(), It.IsAny<CancellationToken>()))
            .Callback<UserSavedConfiguration, CancellationToken>((c, _) => added = c)
            .ReturnsAsync((UserSavedConfiguration c, CancellationToken _) => c);

        // Act
        var result = await _sut.CreateAsync(dto, userId);

        // Assert
        result.Should().NotBeNull();
        added.Should().NotBeNull();
        added!.IdUser.Should().Be(userId);
        added.Name.Should().Be(dto.Name);
    }

    [Fact]
    public async Task UpdateAsync_ExistingConfig_UpdatesFields()
    {
        // Arrange
        var config = BuildConfig();
        var dto = new UpdateUserSavedConfigurationDto
        {
            Name = "Updated",
            Inhalation = 6,
            Retention1 = 2,
            Exhalation = 6,
            Retention2 = 2,
            DurationMinutes = 10,
            Difficulty = 5,
            Objective = "Focus",
            GuidanceType = "Audio"
        };
        _mockRepository.Setup(r => r.FindAsync(config.Id, It.IsAny<CancellationToken>())).ReturnsAsync(config);

        // Act
        var result = await _sut.UpdateAsync(config.Id, dto);

        // Assert
        result.Should().NotBeNull();
        config.Name.Should().Be("Updated");
        config.Inhalation.Should().Be(6);
        config.Objective.Should().Be("Focus");
        config.UpdateTime.Should().NotBeNull();
        _mockRepository.Verify(r => r.UpdateAsync(config, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentConfig_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.FindAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((UserSavedConfiguration?)null);

        // Act
        var result = await _sut.UpdateAsync(id, new UpdateUserSavedConfigurationDto
        {
            Name = "x", Objective = "x", GuidanceType = "x"
        });

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ExistingConfig_SoftDeletesAndReturnsTrue()
    {
        // Arrange
        var config = BuildConfig();
        _mockRepository.Setup(r => r.FindAsync(config.Id, It.IsAny<CancellationToken>())).ReturnsAsync(config);

        // Act
        var result = await _sut.DeleteAsync(config.Id);

        // Assert
        result.Should().BeTrue();
        config.DeletionTime.Should().NotBeNull();
        _mockRepository.Verify(r => r.SoftDeleteAsync(config, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentConfig_ReturnsFalse()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.FindAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((UserSavedConfiguration?)null);

        // Act
        var result = await _sut.DeleteAsync(id);

        // Assert
        result.Should().BeFalse();
    }

    // --- CreateFromQuizResponsesAsync ---

    private static Quizz BuildQuiz(Guid? id = null, string nom = "Stress Quiz")
    {
        return new Quizz { Id = id ?? Guid.NewGuid(), Nom = nom, Active = true, CreationTime = DateTime.UtcNow };
    }

    private static Question BuildQuestion(Guid quizId, Guid? id = null)
    {
        return new Question
        {
            Id = id ?? Guid.NewGuid(),
            Text = "Question",
            Position = 1,
            IdQuizz = quizId,
            CreationTime = DateTime.UtcNow
        };
    }

    private static ResponsesOption BuildOption(Guid questionId, string targetedField, string operation, string value, Guid? id = null)
    {
        return new ResponsesOption
        {
            Id = id ?? Guid.NewGuid(),
            Label = "Option",
            Position = 1,
            TargetedField = targetedField,
            Operation = operation,
            Value = value,
            IdQuestions = questionId,
            CreationTime = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task CreateFromQuizResponsesAsync_QuizNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new QuizResponsesDto { QuizId = Guid.NewGuid() };
        _mockQuizzRepository.Setup(r => r.GetWithQuestionsAsync(dto.QuizId)).ReturnsAsync((Quizz?)null);

        // Act
        var act = () => _sut.CreateFromQuizResponsesAsync(Guid.NewGuid(), dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*not found*");
    }

    [Fact]
    public async Task CreateFromQuizResponsesAsync_QuizSoftDeleted_ThrowsInvalidOperationException()
    {
        // Arrange
        var quiz = BuildQuiz();
        quiz.DeletionTime = DateTime.UtcNow;
        var dto = new QuizResponsesDto { QuizId = quiz.Id };
        _mockQuizzRepository.Setup(r => r.GetWithQuestionsAsync(quiz.Id)).ReturnsAsync(quiz);

        // Act
        var act = () => _sut.CreateFromQuizResponsesAsync(Guid.NewGuid(), dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CreateFromQuizResponsesAsync_QuestionNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var quiz = BuildQuiz();
        var questionId = Guid.NewGuid();
        var dto = new QuizResponsesDto
        {
            QuizId = quiz.Id,
            Responses = { new QuestionResponseDto { QuestionId = questionId, SelectedOptionId = Guid.NewGuid() } }
        };

        _mockQuizzRepository.Setup(r => r.GetWithQuestionsAsync(quiz.Id)).ReturnsAsync(quiz);
        _mockQuestionRepository.Setup(r => r.FindAsync(questionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Question?)null);

        // Act
        var act = () => _sut.CreateFromQuizResponsesAsync(Guid.NewGuid(), dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Question*not found*");
    }

    [Fact]
    public async Task CreateFromQuizResponsesAsync_QuestionDoesNotBelongToQuiz_Throws()
    {
        // Arrange
        var quiz = BuildQuiz();
        var otherQuizId = Guid.NewGuid();
        var question = BuildQuestion(otherQuizId);
        var dto = new QuizResponsesDto
        {
            QuizId = quiz.Id,
            Responses = { new QuestionResponseDto { QuestionId = question.Id, SelectedOptionId = Guid.NewGuid() } }
        };

        _mockQuizzRepository.Setup(r => r.GetWithQuestionsAsync(quiz.Id)).ReturnsAsync(quiz);
        _mockQuestionRepository.Setup(r => r.FindAsync(question.Id, It.IsAny<CancellationToken>())).ReturnsAsync(question);

        // Act
        var act = () => _sut.CreateFromQuizResponsesAsync(Guid.NewGuid(), dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*does not belong to quiz*");
    }

    [Fact]
    public async Task CreateFromQuizResponsesAsync_OptionNotFound_Throws()
    {
        // Arrange
        var quiz = BuildQuiz();
        var question = BuildQuestion(quiz.Id);
        var optionId = Guid.NewGuid();
        var dto = new QuizResponsesDto
        {
            QuizId = quiz.Id,
            Responses = { new QuestionResponseDto { QuestionId = question.Id, SelectedOptionId = optionId } }
        };

        _mockQuizzRepository.Setup(r => r.GetWithQuestionsAsync(quiz.Id)).ReturnsAsync(quiz);
        _mockQuestionRepository.Setup(r => r.FindAsync(question.Id, It.IsAny<CancellationToken>())).ReturnsAsync(question);
        _mockOptionRepository.Setup(r => r.FindAsync(optionId, It.IsAny<CancellationToken>())).ReturnsAsync((ResponsesOption?)null);

        // Act
        var act = () => _sut.CreateFromQuizResponsesAsync(Guid.NewGuid(), dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*option*not found*");
    }

    [Fact]
    public async Task CreateFromQuizResponsesAsync_OptionDoesNotBelongToQuestion_Throws()
    {
        // Arrange
        var quiz = BuildQuiz();
        var question = BuildQuestion(quiz.Id);
        var otherQuestionId = Guid.NewGuid();
        var option = BuildOption(otherQuestionId, "inhalation", "SET", "8");
        var dto = new QuizResponsesDto
        {
            QuizId = quiz.Id,
            Responses = { new QuestionResponseDto { QuestionId = question.Id, SelectedOptionId = option.Id } }
        };

        _mockQuizzRepository.Setup(r => r.GetWithQuestionsAsync(quiz.Id)).ReturnsAsync(quiz);
        _mockQuestionRepository.Setup(r => r.FindAsync(question.Id, It.IsAny<CancellationToken>())).ReturnsAsync(question);
        _mockOptionRepository.Setup(r => r.FindAsync(option.Id, It.IsAny<CancellationToken>())).ReturnsAsync(option);

        // Act
        var act = () => _sut.CreateFromQuizResponsesAsync(Guid.NewGuid(), dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*does not belong to question*");
    }

    [Fact]
    public async Task CreateFromQuizResponsesAsync_ValidResponses_AppliesSetAddAndMultiplyOperations()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var quiz = BuildQuiz();
        var question1 = BuildQuestion(quiz.Id);
        var question2 = BuildQuestion(quiz.Id);
        var question3 = BuildQuestion(quiz.Id);

        var setOption = BuildOption(question1.Id, "inhalation", "SET", "8");
        var addOption = BuildOption(question2.Id, "difficulty", "ADD", "2");
        var multiplyOption = BuildOption(question3.Id, "durationminutes", "MULTIPLY", "3");

        var dto = new QuizResponsesDto
        {
            QuizId = quiz.Id,
            Responses =
            {
                new QuestionResponseDto { QuestionId = question1.Id, SelectedOptionId = setOption.Id },
                new QuestionResponseDto { QuestionId = question2.Id, SelectedOptionId = addOption.Id },
                new QuestionResponseDto { QuestionId = question3.Id, SelectedOptionId = multiplyOption.Id },
            }
        };

        _mockQuizzRepository.Setup(r => r.GetWithQuestionsAsync(quiz.Id)).ReturnsAsync(quiz);
        _mockQuestionRepository.Setup(r => r.FindAsync(question1.Id, It.IsAny<CancellationToken>())).ReturnsAsync(question1);
        _mockQuestionRepository.Setup(r => r.FindAsync(question2.Id, It.IsAny<CancellationToken>())).ReturnsAsync(question2);
        _mockQuestionRepository.Setup(r => r.FindAsync(question3.Id, It.IsAny<CancellationToken>())).ReturnsAsync(question3);
        _mockOptionRepository.Setup(r => r.FindAsync(setOption.Id, It.IsAny<CancellationToken>())).ReturnsAsync(setOption);
        _mockOptionRepository.Setup(r => r.FindAsync(addOption.Id, It.IsAny<CancellationToken>())).ReturnsAsync(addOption);
        _mockOptionRepository.Setup(r => r.FindAsync(multiplyOption.Id, It.IsAny<CancellationToken>())).ReturnsAsync(multiplyOption);

        UserSavedConfiguration? added = null;
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<UserSavedConfiguration>(), It.IsAny<CancellationToken>()))
            .Callback<UserSavedConfiguration, CancellationToken>((c, _) => added = c)
            .ReturnsAsync((UserSavedConfiguration c, CancellationToken _) => c);

        // Act
        var result = await _sut.CreateFromQuizResponsesAsync(userId, dto);

        // Assert: defaults are Inhalation=4, Difficulty=1, DurationMinutes=5
        result.Should().NotBeNull();
        added.Should().NotBeNull();
        added!.IdUser.Should().Be(userId);
        added.Inhalation.Should().Be(8);       // SET 8
        added.Difficulty.Should().Be(3);       // 1 + 2
        added.DurationMinutes.Should().Be(15); // 5 * 3
        added.Name.Should().Contain(quiz.Nom);
    }

    [Fact]
    public async Task CreateFromQuizResponsesAsync_SetOperationOnStringField_AppliesValue()
    {
        // Arrange
        var quiz = BuildQuiz();
        var question = BuildQuestion(quiz.Id);
        var option = BuildOption(question.Id, "objective", "SET", "Focus");
        var dto = new QuizResponsesDto
        {
            QuizId = quiz.Id,
            Responses = { new QuestionResponseDto { QuestionId = question.Id, SelectedOptionId = option.Id } }
        };

        _mockQuizzRepository.Setup(r => r.GetWithQuestionsAsync(quiz.Id)).ReturnsAsync(quiz);
        _mockQuestionRepository.Setup(r => r.FindAsync(question.Id, It.IsAny<CancellationToken>())).ReturnsAsync(question);
        _mockOptionRepository.Setup(r => r.FindAsync(option.Id, It.IsAny<CancellationToken>())).ReturnsAsync(option);

        UserSavedConfiguration? added = null;
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<UserSavedConfiguration>(), It.IsAny<CancellationToken>()))
            .Callback<UserSavedConfiguration, CancellationToken>((c, _) => added = c)
            .ReturnsAsync((UserSavedConfiguration c, CancellationToken _) => c);

        // Act
        await _sut.CreateFromQuizResponsesAsync(Guid.NewGuid(), dto);

        // Assert
        added!.Objective.Should().Be("Focus");
    }

    [Fact]
    public async Task CreateFromQuizResponsesAsync_AddOperationOnStringField_Throws()
    {
        // Arrange
        var quiz = BuildQuiz();
        var question = BuildQuestion(quiz.Id);
        var option = BuildOption(question.Id, "objective", "ADD", "Focus");
        var dto = new QuizResponsesDto
        {
            QuizId = quiz.Id,
            Responses = { new QuestionResponseDto { QuestionId = question.Id, SelectedOptionId = option.Id } }
        };

        _mockQuizzRepository.Setup(r => r.GetWithQuestionsAsync(quiz.Id)).ReturnsAsync(quiz);
        _mockQuestionRepository.Setup(r => r.FindAsync(question.Id, It.IsAny<CancellationToken>())).ReturnsAsync(question);
        _mockOptionRepository.Setup(r => r.FindAsync(option.Id, It.IsAny<CancellationToken>())).ReturnsAsync(option);

        // Act
        var act = () => _sut.CreateFromQuizResponsesAsync(Guid.NewGuid(), dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*not supported for string fields*");
    }

    [Fact]
    public async Task CreateFromQuizResponsesAsync_UnparseableNumericValue_Throws()
    {
        // Arrange
        var quiz = BuildQuiz();
        var question = BuildQuestion(quiz.Id);
        var option = BuildOption(question.Id, "inhalation", "SET", "not-a-number");
        var dto = new QuizResponsesDto
        {
            QuizId = quiz.Id,
            Responses = { new QuestionResponseDto { QuestionId = question.Id, SelectedOptionId = option.Id } }
        };

        _mockQuizzRepository.Setup(r => r.GetWithQuestionsAsync(quiz.Id)).ReturnsAsync(quiz);
        _mockQuestionRepository.Setup(r => r.FindAsync(question.Id, It.IsAny<CancellationToken>())).ReturnsAsync(question);
        _mockOptionRepository.Setup(r => r.FindAsync(option.Id, It.IsAny<CancellationToken>())).ReturnsAsync(option);

        // Act
        var act = () => _sut.CreateFromQuizResponsesAsync(Guid.NewGuid(), dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Cannot parse*");
    }

    [Fact]
    public async Task CreateFromQuizResponsesAsync_UnknownNumericOperation_Throws()
    {
        // Arrange
        var quiz = BuildQuiz();
        var question = BuildQuestion(quiz.Id);
        var option = BuildOption(question.Id, "inhalation", "SUBTRACT", "2");
        var dto = new QuizResponsesDto
        {
            QuizId = quiz.Id,
            Responses = { new QuestionResponseDto { QuestionId = question.Id, SelectedOptionId = option.Id } }
        };

        _mockQuizzRepository.Setup(r => r.GetWithQuestionsAsync(quiz.Id)).ReturnsAsync(quiz);
        _mockQuestionRepository.Setup(r => r.FindAsync(question.Id, It.IsAny<CancellationToken>())).ReturnsAsync(question);
        _mockOptionRepository.Setup(r => r.FindAsync(option.Id, It.IsAny<CancellationToken>())).ReturnsAsync(option);

        // Act
        var act = () => _sut.CreateFromQuizResponsesAsync(Guid.NewGuid(), dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Unknown operation*");
    }

    [Fact]
    public async Task CreateFromQuizResponsesAsync_UnrecognizedTargetedField_IsIgnored()
    {
        // Arrange
        var quiz = BuildQuiz();
        var question = BuildQuestion(quiz.Id);
        var option = BuildOption(question.Id, "somethingelse", "SET", "999");
        var dto = new QuizResponsesDto
        {
            QuizId = quiz.Id,
            Responses = { new QuestionResponseDto { QuestionId = question.Id, SelectedOptionId = option.Id } }
        };

        _mockQuizzRepository.Setup(r => r.GetWithQuestionsAsync(quiz.Id)).ReturnsAsync(quiz);
        _mockQuestionRepository.Setup(r => r.FindAsync(question.Id, It.IsAny<CancellationToken>())).ReturnsAsync(question);
        _mockOptionRepository.Setup(r => r.FindAsync(option.Id, It.IsAny<CancellationToken>())).ReturnsAsync(option);

        UserSavedConfiguration? added = null;
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<UserSavedConfiguration>(), It.IsAny<CancellationToken>()))
            .Callback<UserSavedConfiguration, CancellationToken>((c, _) => added = c)
            .ReturnsAsync((UserSavedConfiguration c, CancellationToken _) => c);

        // Act
        await _sut.CreateFromQuizResponsesAsync(Guid.NewGuid(), dto);

        // Assert: defaults untouched
        added!.Inhalation.Should().Be(4);
        added.Difficulty.Should().Be(1);
    }
}
