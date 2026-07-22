using FluentAssertions;
using Moq;
using api.CZ.Features.AdminLogs.Services;
using api.CZ.Features.Quizzes.DTOs;
using api.CZ.Features.Quizzes.Factories;
using api.CZ.Features.Quizzes.Models;
using api.CZ.Features.Quizzes.Repositories;
using api.CZ.Features.Quizzes.Services;

namespace api.Tests.Unit.Services;

public class QuizzServiceTests
{
    private readonly Mock<IQuizzRepository> _mockQuizzRepository;
    private readonly Mock<IQuestionRepository> _mockQuestionRepository;
    private readonly Mock<IResponsesOptionRepository> _mockOptionRepository;
    private readonly Mock<IQuizzFactory> _mockFactory;
    private readonly Mock<IAdminActionLogger> _mockActionLogger;
    private readonly QuizzService _sut;

    public QuizzServiceTests()
    {
        _mockQuizzRepository = new Mock<IQuizzRepository>();
        _mockQuestionRepository = new Mock<IQuestionRepository>();
        _mockOptionRepository = new Mock<IResponsesOptionRepository>();
        _mockFactory = new Mock<IQuizzFactory>();
        _mockActionLogger = new Mock<IAdminActionLogger>();

        _sut = new QuizzService(
            _mockQuizzRepository.Object,
            _mockQuestionRepository.Object,
            _mockOptionRepository.Object,
            _mockFactory.Object,
            _mockActionLogger.Object);
    }

    private static Quizz BuildQuizzWithQuestions(Guid? id = null, string nom = "Quizz")
    {
        var quizzId = id ?? Guid.NewGuid();
        var question1Id = Guid.NewGuid();
        var question2Id = Guid.NewGuid();

        return new Quizz
        {
            Id = quizzId,
            Nom = nom,
            Active = true,
            CreationTime = DateTime.UtcNow,
            Questions = new List<Question>
            {
                new()
                {
                    Id = question2Id,
                    Text = "Second question",
                    Position = 2,
                    IdQuizz = quizzId,
                    CreationTime = DateTime.UtcNow,
                    ResponsesOptions = new List<ResponsesOption>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(), Label = "B", Position = 2,
                            TargetedField = "stress", Operation = "+", Value = "1",
                            IdQuestions = question2Id, CreationTime = DateTime.UtcNow
                        },
                        new()
                        {
                            Id = Guid.NewGuid(), Label = "A", Position = 1,
                            TargetedField = "stress", Operation = "+", Value = "0",
                            IdQuestions = question2Id, CreationTime = DateTime.UtcNow
                        }
                    }
                },
                new()
                {
                    Id = question1Id,
                    Text = "First question",
                    Position = 1,
                    IdQuizz = quizzId,
                    CreationTime = DateTime.UtcNow,
                    ResponsesOptions = new List<ResponsesOption>()
                }
            }
        };
    }

    [Fact]
    public async Task GetAllAsync_ReturnsQuizzesWithQuestionCount()
    {
        // Arrange
        var quizz = BuildQuizzWithQuestions();
        _mockQuizzRepository.Setup(r => r.ListWithQuestionCountAsync())
            .ReturnsAsync(new List<Quizz> { quizz });

        // Act
        var result = (await _sut.GetAllAsync()).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].QuestionCount.Should().Be(2);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingQuizz_ReturnsQuestionsAndOptionsOrderedByPosition()
    {
        // Arrange
        var quizz = BuildQuizzWithQuestions();
        _mockQuizzRepository.Setup(r => r.GetWithQuestionsAsync(quizz.Id))
            .ReturnsAsync(quizz);

        // Act
        var result = await _sut.GetByIdAsync(quizz.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Questions.Should().HaveCount(2);
        result.Questions[0].Text.Should().Be("First question");
        result.Questions[1].Text.Should().Be("Second question");
        result.Questions[1].Options.Should().HaveCount(2);
        result.Questions[1].Options[0].Label.Should().Be("A");
        result.Questions[1].Options[1].Label.Should().Be("B");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentQuizz_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockQuizzRepository.Setup(r => r.GetWithQuestionsAsync(id))
            .ReturnsAsync((Quizz?)null);

        // Act
        var result = await _sut.GetByIdAsync(id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ValidDto_CreatesQuizzWithQuestionsAndOptionsAndLogsAction()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var dto = new CreateQuizzDto
        {
            Nom = "New Quizz",
            Active = true,
            Questions = new List<CreateQuestionDto>
            {
                new()
                {
                    Text = "Q1",
                    Position = 1,
                    Options = new List<CreateResponseOptionDto>
                    {
                        new() { Label = "Opt1", Position = 1, TargetedField = "f", Operation = "+", Value = "1" }
                    }
                }
            }
        };

        var createdQuizz = new Quizz { Id = Guid.NewGuid(), Nom = dto.Nom, CreationTime = DateTime.UtcNow };
        _mockFactory.Setup(f => f.Create(dto.Nom)).Returns(createdQuizz);

        var finalQuizz = BuildQuizzWithQuestions(createdQuizz.Id, dto.Nom);
        _mockQuizzRepository.Setup(r => r.GetWithQuestionsAsync(createdQuizz.Id))
            .ReturnsAsync(finalQuizz);

        // Act
        var result = await _sut.CreateAsync(dto, adminId);

        // Assert
        result.Should().NotBeNull();
        _mockQuizzRepository.Verify(r => r.AddAsync(createdQuizz, It.IsAny<CancellationToken>()), Times.Once);
        _mockQuestionRepository.Verify(r => r.AddAsync(
            It.Is<Question>(q => q.Text == "Q1" && q.IdQuizz == createdQuizz.Id), It.IsAny<CancellationToken>()),
            Times.Once);
        _mockOptionRepository.Verify(r => r.AddAsync(
            It.Is<ResponsesOption>(o => o.Label == "Opt1"), It.IsAny<CancellationToken>()),
            Times.Once);
        _mockActionLogger.Verify(l => l.LogCreateAsync(adminId, "Quiz", createdQuizz.Id, It.IsAny<string>()), Times.Once);
        createdQuizz.Active.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_ExistingQuizz_UpdatesFieldsAndLogsAction()
    {
        // Arrange
        var quizz = BuildQuizzWithQuestions();
        var adminId = Guid.NewGuid();
        var dto = new UpdateQuizzDto { Nom = "Renamed", Active = false };

        _mockQuizzRepository.Setup(r => r.FindAsync(quizz.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(quizz);

        // Act
        var result = await _sut.UpdateAsync(quizz.Id, dto, adminId);

        // Assert
        result.Should().NotBeNull();
        quizz.Nom.Should().Be("Renamed");
        quizz.Active.Should().BeFalse();
        _mockQuizzRepository.Verify(r => r.UpdateAsync(quizz, It.IsAny<CancellationToken>()), Times.Once);
        _mockActionLogger.Verify(l => l.LogUpdateAsync(adminId, "Quiz", quizz.Id, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentQuizz_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockQuizzRepository.Setup(r => r.FindAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Quizz?)null);

        // Act
        var result = await _sut.UpdateAsync(id, new UpdateQuizzDto { Nom = "x", Active = true }, Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateFullAsync_ExistingQuizz_ReplacesQuestionsAndOptions()
    {
        // Arrange
        var quizz = BuildQuizzWithQuestions();
        var adminId = Guid.NewGuid();
        var dto = new CreateQuizzDto
        {
            Nom = "Rebuilt",
            Active = true,
            Questions = new List<CreateQuestionDto>
            {
                new() { Text = "New Q", Position = 1, Options = new List<CreateResponseOptionDto>() }
            }
        };

        _mockQuizzRepository.Setup(r => r.GetWithQuestionsAsync(quizz.Id))
            .ReturnsAsync(quizz);

        // Act
        await _sut.UpdateFullAsync(quizz.Id, dto, adminId);

        // Assert: old questions/options deleted
        _mockOptionRepository.Verify(r => r.DeleteAsync(It.IsAny<ResponsesOption>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
        _mockQuestionRepository.Verify(r => r.DeleteAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
        // New question added
        _mockQuestionRepository.Verify(r => r.AddAsync(
            It.Is<Question>(q => q.Text == "New Q"), It.IsAny<CancellationToken>()), Times.Once);
        _mockQuizzRepository.Verify(r => r.UpdateAsync(quizz, It.IsAny<CancellationToken>()), Times.Once);
        _mockActionLogger.Verify(l => l.LogUpdateAsync(adminId, "Quiz", quizz.Id, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UpdateFullAsync_NonExistentQuizz_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockQuizzRepository.Setup(r => r.GetWithQuestionsAsync(id))
            .ReturnsAsync((Quizz?)null);

        // Act
        var result = await _sut.UpdateFullAsync(id, new CreateQuizzDto { Nom = "x" }, Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ExistingQuizz_DeletesQuestionsOptionsAndQuizzAndLogsAction()
    {
        // Arrange
        var quizz = BuildQuizzWithQuestions();
        var adminId = Guid.NewGuid();

        _mockQuizzRepository.Setup(r => r.GetWithQuestionsAsync(quizz.Id))
            .ReturnsAsync(quizz);

        // Act
        var result = await _sut.DeleteAsync(quizz.Id, adminId);

        // Assert
        result.Should().BeTrue();
        _mockOptionRepository.Verify(r => r.DeleteAsync(It.IsAny<ResponsesOption>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
        _mockQuestionRepository.Verify(r => r.DeleteAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
        _mockQuizzRepository.Verify(r => r.DeleteAsync(quizz, It.IsAny<CancellationToken>()), Times.Once);
        _mockActionLogger.Verify(l => l.LogDeleteAsync(adminId, "Quiz", quizz.Id, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentQuizz_ReturnsFalse()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockQuizzRepository.Setup(r => r.GetWithQuestionsAsync(id))
            .ReturnsAsync((Quizz?)null);

        // Act
        var result = await _sut.DeleteAsync(id, Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SetActiveAsync_ExistingQuizz_UpdatesActiveStateAndLogsAction()
    {
        // Arrange
        var quizz = BuildQuizzWithQuestions();
        quizz.Active = false;
        var adminId = Guid.NewGuid();

        _mockQuizzRepository.Setup(r => r.FindAsync(quizz.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(quizz);

        // Act
        var result = await _sut.SetActiveAsync(quizz.Id, true, adminId);

        // Assert
        result.Should().BeTrue();
        quizz.Active.Should().BeTrue();
        _mockQuizzRepository.Verify(r => r.UpdateAsync(quizz, It.IsAny<CancellationToken>()), Times.Once);
        _mockActionLogger.Verify(l => l.LogUpdateAsync(adminId, "Quiz", quizz.Id, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task SetActiveAsync_NonExistentQuizz_ReturnsFalse()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockQuizzRepository.Setup(r => r.FindAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Quizz?)null);

        // Act
        var result = await _sut.SetActiveAsync(id, true, Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }
}
