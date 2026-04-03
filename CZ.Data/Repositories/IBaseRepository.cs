using System.Linq.Expressions;

namespace api.CZ.Data.Repositories;

public interface IBaseRepository<TEntity> where TEntity : class
{
    Task<TEntity> AddAsync(TEntity model, CancellationToken cancellationToken = default);
    Task UpdateAsync(TEntity model, CancellationToken cancellationToken = default);
    Task DeleteAsync(TEntity model, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(TEntity model, CancellationToken cancellationToken = default);
    Task<TEntity?> FindAsync<TId>(TId id, CancellationToken cancellationToken = default) where TId : notnull;
    Task<List<TEntity>> ListAsync(CancellationToken cancellationToken = default);
    Task<List<TEntity>> ListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
}