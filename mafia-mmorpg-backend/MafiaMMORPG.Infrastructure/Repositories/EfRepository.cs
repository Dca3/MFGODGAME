using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using MafiaMMORPG.Application.Repositories;
using MafiaMMORPG.Infrastructure.Data;

namespace MafiaMMORPG.Infrastructure.Repositories;

public class EfRepository<T> : IRepository<T> where T : class
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _dbSet;

    public EfRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, ct);
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, bool asNoTracking = true, CancellationToken ct = default)
    {
        var query = _dbSet.AsQueryable();
        
        if (asNoTracking)
            query = query.AsNoTracking();
            
        return await query.FirstOrDefaultAsync(predicate, ct);
    }

    public async Task<List<T>> ListAsync(Expression<Func<T, bool>>? predicate = null, bool asNoTracking = true, CancellationToken ct = default)
    {
        var query = _dbSet.AsQueryable();
        
        if (asNoTracking)
            query = query.AsNoTracking();
            
        if (predicate != null)
            query = query.Where(predicate);
            
        return await query.ToListAsync(ct);
    }

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
    {
        return await _dbSet.AsNoTracking().ToListAsync(ct);
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default)
    {
        var query = _dbSet.AsQueryable();
        
        if (predicate != null)
            query = query.Where(predicate);
            
        return await query.CountAsync(ct);
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        return await _dbSet.AnyAsync(predicate, ct);
    }

    public async Task AddAsync(T entity, CancellationToken ct = default)
    {
        await _dbSet.AddAsync(entity, ct);
    }

    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
    {
        await _dbSet.AddRangeAsync(entities, ct);
    }

    public Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
    {
        _dbSet.UpdateRange(entities);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(T entity, CancellationToken ct = default)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public Task RemoveRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
    {
        _dbSet.RemoveRange(entities);
        return Task.CompletedTask;
    }
}
