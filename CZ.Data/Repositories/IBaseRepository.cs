using System.Linq.Expressions;

namespace api.CZ.Data.Repositories;

public interface IBaseRepository<TEntity> where TEntity : class
{
    // === Query Operations ===
    
    /// <summary>
    /// Gets an entity by its ID
    /// </summary>
    Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default);
    TEntity? GetById(object id);

    /// <summary>
    /// Gets all entities
    /// </summary>
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    IEnumerable<TEntity> GetAll();

    /// <summary>
    /// Finds entities matching the predicate
    /// </summary>
    Task<IEnumerable<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate, 
        CancellationToken cancellationToken = default);
    IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Gets the first entity matching the predicate or null
    /// </summary>
    Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate, 
        CancellationToken cancellationToken = default);
    TEntity? FirstOrDefault(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Checks if any entity matches the predicate
    /// </summary>
    Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate, 
        CancellationToken cancellationToken = default);
    bool Any(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Counts entities matching the predicate (or all if null)
    /// </summary>
    Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null, 
        CancellationToken cancellationToken = default);
    int Count(Expression<Func<TEntity, bool>>? predicate = null);

    // === Create Operations (auto-save) ===
    
    /// <summary>
    /// Adds a new entity and saves changes
    /// </summary>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    TEntity Add(TEntity entity);

    /// <summary>
    /// Adds multiple entities and saves changes
    /// </summary>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    void AddRange(IEnumerable<TEntity> entities);

    // === Update Operations (auto-save) ===
    
    /// <summary>
    /// Updates an entity and saves changes
    /// </summary>
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Update(TEntity entity);

    /// <summary>
    /// Updates multiple entities and saves changes
    /// </summary>
    Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    void UpdateRange(IEnumerable<TEntity> entities);

    // === Delete Operations (auto-save) ===
    
    /// <summary>
    /// Removes an entity and saves changes
    /// </summary>
    Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Remove(TEntity entity);

    /// <summary>
    /// Removes multiple entities and saves changes
    /// </summary>
    Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    void RemoveRange(IEnumerable<TEntity> entities);

    /// <summary>
    /// Removes an entity by ID and saves changes. Returns true if entity was found and deleted.
    /// </summary>
    Task<bool> RemoveByIdAsync(object id, CancellationToken cancellationToken = default);

    // === Pagination ===
    
    /// <summary>
    /// Gets a page of entities with total count
    /// </summary>
    Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        CancellationToken cancellationToken = default);
}