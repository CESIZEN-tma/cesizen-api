namespace api.CZ.Core.ResultPattern;

/// <summary>
/// Represents the result of an operation without a return value
/// </summary>
public class Result : IResult
{
    protected Result(bool isSuccess, string? error, IEnumerable<string>? errors = null)
    {
        if (isSuccess && (!string.IsNullOrEmpty(error) || errors?.Any() == true))
            throw new InvalidOperationException("Success result cannot have errors");
        
        if (!isSuccess && string.IsNullOrEmpty(error) && (errors == null || !errors.Any()))
            throw new InvalidOperationException("Failure result must have at least one error");

        IsSuccess = isSuccess;
        Error = error;
        Errors = errors?.ToList().AsReadOnly() ?? (error != null ? new List<string> { error }.AsReadOnly() : new List<string>().AsReadOnly());
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public IReadOnlyList<string> Errors { get; }

    // === Factory Methods ===

    /// <summary>
    /// Creates a success result
    /// </summary>
    public static Result Success() => new(true, null);

    /// <summary>
    /// Creates a failure result with a single error
    /// </summary>
    public static Result Failure(string error) => new(false, error);

    /// <summary>
    /// Creates a failure result with multiple errors
    /// </summary>
    public static Result Failure(IEnumerable<string> errors) => new(false, null, errors);

    /// <summary>
    /// Creates a success result with a value
    /// </summary>
    public static Result<T> Success<T>(T value) => new(value, true, null);

    /// <summary>
    /// Creates a failure result with a value type
    /// </summary>
    public static Result<T> Failure<T>(string error) => new(default, false, error);

    /// <summary>
    /// Creates a failure result with a value type and multiple errors
    /// </summary>
    public static Result<T> Failure<T>(IEnumerable<string> errors) => new(default, false, null, errors);

    // === Utility Methods ===

    /// <summary>
    /// Executes one of two actions based on success or failure
    /// </summary>
    public void Match(Action onSuccess, Action<string> onFailure)
    {
        if (IsSuccess)
            onSuccess();
        else
            onFailure(Error!);
    }

    /// <summary>
    /// Returns a value based on success or failure
    /// </summary>
    public TResult Match<TResult>(Func<TResult> onSuccess, Func<string, TResult> onFailure)
    {
        return IsSuccess ? onSuccess() : onFailure(Error!);
    }

    /// <summary>
    /// Combines multiple results into one
    /// </summary>
    public static Result Combine(params Result[] results)
    {
        var failures = results.Where(r => r.IsFailure).ToList();
        
        if (!failures.Any())
            return Success();

        var allErrors = failures.SelectMany(f => f.Errors);
        return Failure(allErrors);
    }

    /// <summary>
    /// Combines multiple results and returns the first failure or success
    /// </summary>
    public static Result FirstFailureOrSuccess(params Result[] results)
    {
        foreach (var result in results)
        {
            if (result.IsFailure)
                return result;
        }

        return Success();
    }
}

/// <summary>
/// Represents the result of an operation that returns a value
/// </summary>
public class Result<T> : Result, IResult<T>
{
    private readonly T? _value;

    protected internal Result(T? value, bool isSuccess, string? error, IEnumerable<string>? errors = null)
        : base(isSuccess, error, errors)
    {
        _value = value;
    }

    public T? Value
    {
        get
        {
            if (IsFailure)
                throw new InvalidOperationException("Cannot access Value of a failed result");
            
            return _value;
        }
    }

    /// <summary>
    /// Gets the value or returns a default value if failed
    /// </summary>
    public T? GetValueOrDefault(T? defaultValue = default)
    {
        return IsSuccess ? _value : defaultValue;
    }

    // === Utility Methods ===

    /// <summary>
    /// Executes one of two actions based on success or failure
    /// </summary>
    public void Match(Action<T> onSuccess, Action<string> onFailure)
    {
        if (IsSuccess)
            onSuccess(_value!);
        else
            onFailure(Error!);
    }

    /// <summary>
    /// Returns a value based on success or failure
    /// </summary>
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
    {
        return IsSuccess ? onSuccess(_value!) : onFailure(Error!);
    }

    /// <summary>
    /// Maps the value to another type if successful
    /// </summary>
    public Result<TResult> Map<TResult>(Func<T, TResult> mapper)
    {
        return IsSuccess 
            ? Success(mapper(_value!)) 
            : Failure<TResult>(Errors);
    }

    /// <summary>
    /// Binds to another result-returning operation if successful
    /// </summary>
    public Result<TResult> Bind<TResult>(Func<T, Result<TResult>> binder)
    {
        return IsSuccess 
            ? binder(_value!) 
            : Failure<TResult>(Errors);
    }

    /// <summary>
    /// Executes an action if successful and returns the original result
    /// </summary>
    public Result<T> Tap(Action<T> action)
    {
        if (IsSuccess)
            action(_value!);
        
        return this;
    }

    /// <summary>
    /// Executes an async action if successful and returns the original result
    /// </summary>
    public async Task<Result<T>> TapAsync(Func<T, Task> action)
    {
        if (IsSuccess)
            await action(_value!);
        
        return this;
    }

    public static implicit operator Result<T>(T value) => Success(value);
}