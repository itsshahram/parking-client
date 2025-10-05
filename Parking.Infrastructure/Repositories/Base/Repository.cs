using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Parking.Domain.Contracts.Base;
using Parking.Infrastructure.Context;
using System.Linq.Expressions;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public IQueryable<T> GetAll()
        => _dbSet.AsNoTracking();

    public async Task<List<T>> ToListAsync()
        => await _dbSet.ToListAsync();
    public List<T>? ToList()
    => _dbSet.ToList();

    public T? GetById(Guid id)
    {
        return _dbSet.Find(id);
    }
    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }
    public T? GetById(int id)
    {
        return _dbSet.Find(id);
    }
    public async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }
    public T? GetById(long id)
    {
        return _dbSet.Find(id);
    }
    public async Task<T?> GetByIdAsync(long id)
    {
        return await _dbSet.FindAsync(id);
    }

    public T? FirstOrDefault()
    {
        return _dbSet.FirstOrDefault();
    }

    public async Task<T?> FirstOrDefaultAsync()
        => await _dbSet.FirstOrDefaultAsync();
    public T? FirstOrDefault(Expression<Func<T, bool>> predicate)
    {
        return _dbSet.FirstOrDefault(predicate);
    }
    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }
    public TResult? FirstOrDefault<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector)
    {
        return _dbSet.Where(predicate).Select(selector).FirstOrDefault();
    }
    public async Task<TResult?> FirstOrDefaultAsync<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector)
    {
        return await _dbSet.Where(predicate).Select(selector).FirstOrDefaultAsync();
    }

    public void Add(T entity)
    {
        _dbSet.Add(entity);
        _context.SaveChanges();
        _context.Entry(entity).State = EntityState.Detached;
    }
    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public void Update(T entity)
    {
        _dbSet.Update(entity);
        _context.SaveChanges();
    }

    public async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public void Delete(T entity)
    {
        _dbSet.Remove(entity);
        _context.SaveChanges();
    }

    public void ExecuteUpdate(Expression<Func<T, bool>> query, Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> expression)
    {
        _dbSet.Where(query).ExecuteUpdate(expression);
    }
    public async Task ExecuteUpdateAsync(Expression<Func<T, bool>> query, Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> expression)
    {
        await _dbSet.Where(query).ExecuteUpdateAsync(expression);
    }
    public int ExecuteDelete(Expression<Func<T, bool>> filter)
    {
        return _dbSet.Where(filter).ExecuteDelete();
    }

    public async Task<int> ExecuteDeleteAsync(Expression<Func<T, bool>> filter)
    {
        return await _dbSet.Where(filter).ExecuteDeleteAsync();
    }

    public IQueryable<T> Find(Expression<Func<T, bool>> predicate)
    {
        return _dbSet.AsNoTracking().Where(predicate);
    }

    public async Task<int> CommitAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public int Commit()
    {
        return _context.SaveChanges();
    }

    public async Task<bool> Delete(IEnumerable<T> entities)
    {
        try
        {
            _dbSet.RemoveRange(entities);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            throw;
        }
    }
}



