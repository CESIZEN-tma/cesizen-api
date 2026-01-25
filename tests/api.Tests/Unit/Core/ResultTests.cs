using FluentAssertions;
using api.CZ.Core.ResultPattern;

namespace api.Tests.Unit.Core;

public class ResultTests
{
    [Fact]
    public void Success_CreatesSuccessfulResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().BeNull();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Failure_WithSingleError_CreatesFailureResult()
    {
        // Arrange
        const string errorMessage = "Something went wrong";

        // Act
        var result = Result.Failure(errorMessage);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(errorMessage);
        result.Errors.Should().ContainSingle().Which.Should().Be(errorMessage);
    }

    [Fact]
    public void Failure_WithMultipleErrors_CreatesFailureResult()
    {
        // Arrange
        var errors = new[] { "Error 1", "Error 2", "Error 3" };

        // Act
        var result = Result.Failure(errors);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(3);
        result.Errors.Should().BeEquivalentTo(errors);
    }

    // Note: Constructor tests removed as Result constructor is protected and
    // factory methods already ensure proper validation

    [Fact]
    public void Match_OnSuccess_ExecutesOnSuccessAction()
    {
        // Arrange
        var result = Result.Success();
        var successExecuted = false;
        var failureExecuted = false;

        // Act
        result.Match(
            onSuccess: () => successExecuted = true,
            onFailure: _ => failureExecuted = true
        );

        // Assert
        successExecuted.Should().BeTrue();
        failureExecuted.Should().BeFalse();
    }

    [Fact]
    public void Match_OnFailure_ExecutesOnFailureAction()
    {
        // Arrange
        const string errorMessage = "Test error";
        var result = Result.Failure(errorMessage);
        var successExecuted = false;
        string? capturedError = null;

        // Act
        result.Match(
            onSuccess: () => successExecuted = true,
            onFailure: error => capturedError = error
        );

        // Assert
        successExecuted.Should().BeFalse();
        capturedError.Should().Be(errorMessage);
    }

    [Fact]
    public void Match_WithReturnValue_OnSuccess_ReturnsSuccessValue()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var output = result.Match(
            onSuccess: () => "Success!",
            onFailure: _ => "Failure!"
        );

        // Assert
        output.Should().Be("Success!");
    }

    [Fact]
    public void Match_WithReturnValue_OnFailure_ReturnsFailureValue()
    {
        // Arrange
        var result = Result.Failure("Error");

        // Act
        var output = result.Match(
            onSuccess: () => "Success!",
            onFailure: error => $"Failure: {error}"
        );

        // Assert
        output.Should().Be("Failure: Error");
    }

    [Fact]
    public void Combine_AllSuccess_ReturnsSuccess()
    {
        // Arrange
        var result1 = Result.Success();
        var result2 = Result.Success();
        var result3 = Result.Success();

        // Act
        var combined = Result.Combine(result1, result2, result3);

        // Assert
        combined.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Combine_OneFailure_ReturnsFailureWithAllErrors()
    {
        // Arrange
        var result1 = Result.Success();
        var result2 = Result.Failure("Error 1");
        var result3 = Result.Success();

        // Act
        var combined = Result.Combine(result1, result2, result3);

        // Assert
        combined.IsFailure.Should().BeTrue();
        combined.Errors.Should().Contain("Error 1");
    }

    [Fact]
    public void Combine_MultipleFailures_CombinesAllErrors()
    {
        // Arrange
        var result1 = Result.Failure("Error 1");
        var result2 = Result.Failure("Error 2");
        var result3 = Result.Failure(new[] { "Error 3", "Error 4" });

        // Act
        var combined = Result.Combine(result1, result2, result3);

        // Assert
        combined.IsFailure.Should().BeTrue();
        combined.Errors.Should().HaveCount(4);
        combined.Errors.Should().Contain(new[] { "Error 1", "Error 2", "Error 3", "Error 4" });
    }

    [Fact]
    public void FirstFailureOrSuccess_AllSuccess_ReturnsSuccess()
    {
        // Arrange
        var result1 = Result.Success();
        var result2 = Result.Success();
        var result3 = Result.Success();

        // Act
        var firstFailure = Result.FirstFailureOrSuccess(result1, result2, result3);

        // Assert
        firstFailure.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void FirstFailureOrSuccess_HasFailures_ReturnsFirstFailure()
    {
        // Arrange
        var result1 = Result.Success();
        var result2 = Result.Failure("First error");
        var result3 = Result.Failure("Second error");

        // Act
        var firstFailure = Result.FirstFailureOrSuccess(result1, result2, result3);

        // Assert
        firstFailure.IsFailure.Should().BeTrue();
        firstFailure.Error.Should().Be("First error");
        firstFailure.Errors.Should().ContainSingle();
    }
}

public class ResultWithValueTests
{
    [Fact]
    public void Success_WithValue_CreatesSuccessfulResult()
    {
        // Arrange
        const int expectedValue = 42;

        // Act
        var result = Result.Success(expectedValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(expectedValue);
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_WithValueType_CreatesFailureResult()
    {
        // Arrange
        const string errorMessage = "Failed operation";

        // Act
        var result = Result.Failure<int>(errorMessage);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(errorMessage);
    }

    [Fact]
    public void Value_OnFailure_ThrowsInvalidOperationException()
    {
        // Arrange
        var result = Result.Failure<string>("Error");

        // Act
        var act = () => result.Value;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot access Value of a failed result");
    }

    [Fact]
    public void GetValueOrDefault_OnSuccess_ReturnsValue()
    {
        // Arrange
        const int expectedValue = 100;
        var result = Result.Success(expectedValue);

        // Act
        var value = result.GetValueOrDefault(-1);

        // Assert
        value.Should().Be(expectedValue);
    }

    [Fact]
    public void GetValueOrDefault_OnFailure_ReturnsDefaultValue()
    {
        // Arrange
        const int defaultValue = -1;
        var result = Result.Failure<int>("Error");

        // Act
        var value = result.GetValueOrDefault(defaultValue);

        // Assert
        value.Should().Be(defaultValue);
    }

    [Fact]
    public void Match_WithValue_OnSuccess_ExecutesOnSuccessAction()
    {
        // Arrange
        const string expectedValue = "Test Value";
        var result = Result.Success(expectedValue);
        string? capturedValue = null;
        var failureExecuted = false;

        // Act
        result.Match(
            onSuccess: value => capturedValue = value,
            onFailure: _ => failureExecuted = true
        );

        // Assert
        capturedValue.Should().Be(expectedValue);
        failureExecuted.Should().BeFalse();
    }

    [Fact]
    public void Match_WithValue_OnFailure_ExecutesOnFailureAction()
    {
        // Arrange
        const string errorMessage = "Test error";
        var result = Result.Failure<string>(errorMessage);
        var successExecuted = false;
        string? capturedError = null;

        // Act
        result.Match(
            onSuccess: _ => successExecuted = true,
            onFailure: error => capturedError = error
        );

        // Assert
        successExecuted.Should().BeFalse();
        capturedError.Should().Be(errorMessage);
    }

    [Fact]
    public void Map_OnSuccess_TransformsValue()
    {
        // Arrange
        var result = Result.Success(5);

        // Act
        var mapped = result.Map(x => x * 2);

        // Assert
        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(10);
    }

    [Fact]
    public void Map_OnFailure_PropagatesFailure()
    {
        // Arrange
        const string errorMessage = "Original error";
        var result = Result.Failure<int>(errorMessage);

        // Act
        var mapped = result.Map(x => x * 2);

        // Assert
        mapped.IsFailure.Should().BeTrue();
        mapped.Errors.Should().Contain(errorMessage);
    }

    [Fact]
    public void Bind_OnSuccess_ChainsOperation()
    {
        // Arrange
        var result = Result.Success(10);

        // Act
        var bound = result.Bind(x => Result.Success(x.ToString()));

        // Assert
        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be("10");
    }

    [Fact]
    public void Bind_OnSuccess_CanReturnFailure()
    {
        // Arrange
        var result = Result.Success(0);

        // Act
        var bound = result.Bind(x =>
            x == 0
                ? Result.Failure<string>("Cannot be zero")
                : Result.Success(x.ToString())
        );

        // Assert
        bound.IsFailure.Should().BeTrue();
        bound.Error.Should().Be("Cannot be zero");
    }

    [Fact]
    public void Bind_OnFailure_PropagatesFailure()
    {
        // Arrange
        const string errorMessage = "Original error";
        var result = Result.Failure<int>(errorMessage);

        // Act
        var bound = result.Bind(x => Result.Success(x.ToString()));

        // Assert
        bound.IsFailure.Should().BeTrue();
        bound.Errors.Should().Contain(errorMessage);
    }

    [Fact]
    public void Tap_OnSuccess_ExecutesAction()
    {
        // Arrange
        var result = Result.Success(42);
        var actionExecuted = false;
        var capturedValue = 0;

        // Act
        var tapped = result.Tap(value =>
        {
            actionExecuted = true;
            capturedValue = value;
        });

        // Assert
        tapped.Should().BeSameAs(result);
        actionExecuted.Should().BeTrue();
        capturedValue.Should().Be(42);
    }

    [Fact]
    public void Tap_OnFailure_DoesNotExecuteAction()
    {
        // Arrange
        var result = Result.Failure<int>("Error");
        var actionExecuted = false;

        // Act
        var tapped = result.Tap(_ => actionExecuted = true);

        // Assert
        tapped.Should().BeSameAs(result);
        actionExecuted.Should().BeFalse();
    }

    [Fact]
    public async Task TapAsync_OnSuccess_ExecutesAsyncAction()
    {
        // Arrange
        var result = Result.Success(42);
        var actionExecuted = false;
        var capturedValue = 0;

        // Act
        var tapped = await result.TapAsync(async value =>
        {
            await Task.Delay(1);
            actionExecuted = true;
            capturedValue = value;
        });

        // Assert
        tapped.Should().BeSameAs(result);
        actionExecuted.Should().BeTrue();
        capturedValue.Should().Be(42);
    }

    [Fact]
    public async Task TapAsync_OnFailure_DoesNotExecuteAction()
    {
        // Arrange
        var result = Result.Failure<int>("Error");
        var actionExecuted = false;

        // Act
        var tapped = await result.TapAsync(async _ =>
        {
            await Task.Delay(1);
            actionExecuted = true;
        });

        // Assert
        tapped.Should().BeSameAs(result);
        actionExecuted.Should().BeFalse();
    }

    [Fact]
    public void ImplicitConversion_FromValue_CreatesSuccessResult()
    {
        // Act
        Result<int> result = 42;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Failure_WithMultipleErrors_PreservesAllErrors()
    {
        // Arrange
        var errors = new[] { "Error 1", "Error 2", "Error 3" };

        // Act
        var result = Result.Failure<string>(errors);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(3);
        result.Errors.Should().BeEquivalentTo(errors);
    }

    [Fact]
    public void ComplexChaining_Success_ReturnsExpectedResult()
    {
        // Arrange
        var initialResult = Result.Success(5);

        // Act
        var finalResult = initialResult
            .Map(x => x * 2)                    // 10
            .Tap(x => Console.WriteLine(x))      // Side effect
            .Bind(x => Result.Success(x + 5))    // 15
            .Map(x => x.ToString());             // "15"

        // Assert
        finalResult.IsSuccess.Should().BeTrue();
        finalResult.Value.Should().Be("15");
    }

    [Fact]
    public void ComplexChaining_WithFailure_ShortCircuits()
    {
        // Arrange
        var initialResult = Result.Success(5);
        var mapExecuted = false;

        // Act
        var finalResult = initialResult
            .Map(x => x * 2)                    // 10
            .Bind(x => Result.Failure<int>("Operation failed"))
            .Map(x =>                            // Should not execute
            {
                mapExecuted = true;
                return x.ToString();
            });

        // Assert
        finalResult.IsFailure.Should().BeTrue();
        finalResult.Errors.Should().Contain("Operation failed");
        mapExecuted.Should().BeFalse();
    }
}
