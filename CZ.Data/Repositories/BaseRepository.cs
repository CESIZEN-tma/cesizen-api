using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace api.CZ.Data.Repositories;

public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
{
    private readonly DbContext _context;
    private readonly DbSet<TEntity> _dbSet;

    public BaseRepository(DbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public virtual async Task<TEntity> AddAsync(TEntity model, CancellationToken cancellationToken = default)
    {
        _context.Set<TEntity>().Add(model);

        await SaveChangesAsync(cancellationToken);

        return model;
    }

    public virtual async Task UpdateAsync(TEntity model, CancellationToken cancellationToken = default)
    {
        _context?.Set<TEntity>().Update(model);
        await SaveChangesAsync(cancellationToken);
    }

    public virtual async Task DeleteAsync(TEntity model, CancellationToken cancellationToken = default)
    {
        _context.Set<TEntity>().Remove(model);
        await SaveChangesAsync(cancellationToken);
    }

    public virtual async Task SoftDeleteAsync(TEntity model, CancellationToken cancellationToken = default)
    {
        _context.Set<TEntity>().Update(model);
        await SaveChangesAsync(cancellationToken);
    }

    protected async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task<TEntity?> FindAsync<TId>(TId id, CancellationToken cancellationToken = default) where TId : notnull
    {
        return await _context.FindAsync<TEntity>([id], cancellationToken);
    }

    public virtual async Task<List<TEntity>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<TEntity>().ToListAsync(cancellationToken);
    }

    public virtual async Task<List<TEntity>> ListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.Set<TEntity>().Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.Set<TEntity>().AnyAsync(predicate, cancellationToken);
    }

    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.Set<TEntity>().Where(predicate).CountAsync(cancellationToken);
    }

    public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<TEntity>().CountAsync(cancellationToken);
    }

    public virtual async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.Set<TEntity>().FirstOrDefaultAsync(predicate, cancellationToken);
    }
}