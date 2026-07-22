using FluentAssertions;
using api.CZ.Features.Quizzes.Factories;

namespace api.Tests.Unit.Features.Quizzes;

public class QuizzFactoryTests
{
    private readonly QuizzFactory _sut = new();

    [Fact]
    public void Create_NoParameters_ReturnsActiveQuizzWithNewId()
    {
        // Act
        var quizz = _sut.Create();

        // Assert
        quizz.Id.Should().NotBeEmpty();
        quizz.Active.Should().BeTrue();
    }

    [Fact]
    public void Create_WithNom_SetsNomAndDefaultsActiveToTrue()
    {
        // Act
        var quizz = _sut.Create("Stress Quiz");

        // Assert
        quizz.Nom.Should().Be("Stress Quiz");
        quizz.Active.Should().BeTrue();
    }

    [Fact]
    public void Create_InvalidParameterCount_ThrowsArgumentException()
    {
        // Act
        var act = () => _sut.Create("a", "b");

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
