namespace api.CZ.Core.Factories;

/// <summary>
/// Abstract base class for all factories
/// Provides common implementation for creating instances
/// </summary>
/// <typeparam name="T">The type of object to create</typeparam>
public abstract class BaseFactory<T> : IBaseFactory<T> where T : class
{
    /// <summary>
    /// Creates a default instance of T
    /// </summary>
    public virtual T Create()
    {
        return CreateInstance();
    }

    /// <summary>
    /// Creates an instance of T with the provided parameters
    /// </summary>
    public virtual T Create(params object[] parameters)
    {
        return CreateInstance(parameters);
    }

    /// <summary>
    /// Creates multiple instances of T
    /// </summary>
    public virtual IEnumerable<T> CreateMany(int count)
    {
        if (count <= 0)
            throw new ArgumentException("Count must be greater than zero", nameof(count));

        var instances = new List<T>(count);
        for (int i = 0; i < count; i++)
        {
            instances.Add(Create());
        }
        return instances;
    }

    /// <summary>
    /// Abstract method to be implemented by derived classes
    /// Contains the actual logic for creating instances
    /// </summary>
    /// <param name="parameters">Optional parameters for instance creation</param>
    /// <returns>A new instance of type T</returns>
    protected abstract T CreateInstance(params object[] parameters);
}