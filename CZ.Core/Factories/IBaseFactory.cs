namespace api.CZ.Core.Factories;

/// <summary>
/// Base interface for all factories
/// Defines the contract for creating instances of type T
/// </summary>
/// <typeparam name="T">The type of object to create</typeparam>
public interface IBaseFactory<T> where T : class
{
    /// <summary>
    /// Creates a new instance of T with default values
    /// </summary>
    /// <returns>A new instance of type T</returns>
    T Create();
    
    /// <summary>
    /// Creates a new instance of T with provided parameters
    /// </summary>
    /// <param name="parameters">Parameters to pass to the constructor or initialization logic</param>
    /// <returns>A new instance of type T</returns>
    T Create(params object[] parameters);
    
    /// <summary>
    /// Creates multiple instances of T
    /// </summary>
    /// <param name="count">Number of instances to create</param>
    /// <returns>A collection of new instances of type T</returns>
    IEnumerable<T> CreateMany(int count);
}