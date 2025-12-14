namespace api.CZ.Core.ResultPattern;

/// <summary>
/// Represents the result of an operation
/// </summary>
public interface IResult
{
    /// <summary>
    /// Indicates whether the operation succeeded
    /// </summary>
    bool IsSuccess { get; }
    
    /// <summary>
    /// Indicates whether the operation failed
    /// </summary>
    bool IsFailure { get; }
    
    /// <summary>
    /// The error message if the operation failed
    /// </summary>
    string? Error { get; }
    
    /// <summary>
    /// Collection of all error messages
    /// </summary>
    IReadOnlyList<string> Errors { get; }
}

/// <summary>
/// Represents the result of an operation that returns a value
/// </summary>
public interface IResult<out T> : IResult
{
    /// <summary>
    /// The value returned by the operation
    /// </summary>
    T? Value { get; }
}