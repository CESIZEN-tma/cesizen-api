using System.Security.Claims;
using api.CZ.Features.Quizzes;
using api.CZ.Features.Quizzes.DTOs;
using api.CZ.Features.Quizzes.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace api.Tests.Unit.Features.Quizzes;

public class QuizzControllerTests
{
    private readonly Mock<IQuizzService> _mockService;
    private readonly Mock<ILogger<QuizzController>> _mockLogger;
    private readonly QuizzController _controller;
    private readonly Guid _testAdminId;

    public QuizzControllerTests()
    {
        _mockService = new Mock<IQuizzService>();
        _mockLogger = new Mock<ILogger<QuizzController>>();
        _controller = new QuizzController(_mockService.Object, _mockLogger.Object);
        _testAdminId = Guid.NewGuid();

        // Setup admin claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _testAdminId.ToString()),
            new Claim(ClaimTypes.Role, "Administrator")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_ShouldReturnOkWithQuizzes()
    {
        // Arrange
        var quizzes = new List<GetQuizzDto>
        {
            new GetQuizzDto { Id = Guid.NewGuid(), Nom = "Quiz 1", Active = true, CreationTime = DateTime.UtcNow, QuestionCount = 5 },
            new GetQuizzDto { Id = Guid.NewGuid(), Nom = "Quiz 2", Active = true, CreationTime = DateTime.UtcNow, QuestionCount = 3 }
        };

        _mockService
            .Setup(s => s.GetAllAsync())
            .ReturnsAsync(quizzes);

        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(quizzes);

        _mockService.Verify(s => s.GetAllAsync(), Times.Once);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WhenQuizExists_ShouldReturnOkWithQuiz()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        var quiz = new GetQuizzDetailDto { Id = quizId, Nom = "Test Quiz", Active = true, CreationTime = DateTime.UtcNow };

        _mockService
            .Setup(s => s.GetByIdAsync(quizId))
            .ReturnsAsync(quiz);

        // Act
        var result = await _controller.GetById(quizId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(quiz);

        _mockService.Verify(s => s.GetByIdAsync(quizId), Times.Once);
    }

    [Fact]
    public async Task GetById_WhenQuizDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var quizId = Guid.NewGuid();

        _mockService
            .Setup(s => s.GetByIdAsync(quizId))
            .ReturnsAsync((GetQuizzDetailDto?)null);

        // Act
        var result = await _controller.GetById(quizId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        _mockService.Verify(s => s.GetByIdAsync(quizId), Times.Once);
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_ShouldReturnCreatedWithQuiz()
    {
        // Arrange
        var createDto = new CreateQuizzDto { Nom = "New Quiz", Active = true };
        var createdQuiz = new GetQuizzDetailDto { Id = Guid.NewGuid(), Nom = "New Quiz", Active = true, CreationTime = DateTime.UtcNow };

        _mockService
            .Setup(s => s.CreateAsync(createDto, _testAdminId))
            .ReturnsAsync(createdQuiz);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        createdResult!.Value.Should().BeEquivalentTo(createdQuiz);

        _mockService.Verify(s => s.CreateAsync(createDto, _testAdminId), Times.Once);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WhenSuccessful_ShouldReturnOkWithUpdatedQuiz()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        var updateDto = new UpdateQuizzDto { Nom = "Updated Quiz", Active = true };
        var updatedQuiz = new GetQuizzDto { Id = quizId, Nom = "Updated Quiz", Active = true, CreationTime = DateTime.UtcNow, QuestionCount = 0 };

        _mockService
            .Setup(s => s.UpdateAsync(quizId, updateDto, _testAdminId))
            .ReturnsAsync(updatedQuiz);

        // Act
        var result = await _controller.Update(quizId, updateDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(updatedQuiz);

        _mockService.Verify(s => s.UpdateAsync(quizId, updateDto, _testAdminId), Times.Once);
    }

    [Fact]
    public async Task Update_WhenQuizDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        var updateDto = new UpdateQuizzDto { Nom = "Updated Quiz", Active = true };

        _mockService
            .Setup(s => s.UpdateAsync(quizId, updateDto, _testAdminId))
            .ReturnsAsync((GetQuizzDto?)null);

        // Act
        var result = await _controller.Update(quizId, updateDto);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        _mockService.Verify(s => s.UpdateAsync(quizId, updateDto, _testAdminId), Times.Once);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WhenSuccessful_ShouldReturnOkWithMessage()
    {
        // Arrange
        var quizId = Guid.NewGuid();

        _mockService
            .Setup(s => s.DeleteAsync(quizId, _testAdminId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(quizId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockService.Verify(s => s.DeleteAsync(quizId, _testAdminId), Times.Once);
    }

    [Fact]
    public async Task Delete_WhenQuizDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var quizId = Guid.NewGuid();

        _mockService
            .Setup(s => s.DeleteAsync(quizId, _testAdminId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(quizId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        _mockService.Verify(s => s.DeleteAsync(quizId, _testAdminId), Times.Once);
    }

    #endregion
}
