using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Parking.Domain.Contracts.Base;
using Parking.Infrastructure.Context;
using System.Linq.Expressions;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly IDbContextFactory<ApplicationDbContext> _factory;

    public Repository(IDbContextFactory<ApplicationDbContext> factory)
    {
        _factory = factory;
    }

    private ApplicationDbContext Create() => _factory.CreateDbContext();

    public IQueryable<T> GetAll()
    {
        return Create().Set<T>().AsNoTracking();
    }

    public IQueryable<T> Find(Expression<Func<T, bool>> predicate)
    {
        return Create().Set<T>().AsNoTracking().Where(predicate);
    }

    public async Task<List<T>> ToListAsync()
    {
        using var db = Create();
        return await db.Set<T>().AsNoTracking().ToListAsync();
    }

    public List<T> ToList()
    {
        using var db = Create();
        return db.Set<T>().AsNoTracking().ToList();
    }

    public T? GetById(Guid id)
    {
        using var db = Create();
        return db.Set<T>().Find(id);
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        using var db = Create();
        return await db.Set<T>().FindAsync(id);
    }

    public T? GetById(int id)
    {
        using var db = Create();
        return db.Set<T>().Find(id);
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        using var db = Create();
        return await db.Set<T>().FindAsync(id);
    }

    public T? GetById(long id)
    {
        using var db = Create();
        return db.Set<T>().Find(id);
    }

    public async Task<T?> GetByIdAsync(long id)
    {
        using var db = Create();
        return await db.Set<T>().FindAsync(id);
    }

    public T? FirstOrDefault()
    {
        using var db = Create();
        return db.Set<T>().AsNoTracking().FirstOrDefault();
    }

    public async Task<T?> FirstOrDefaultAsync()
    {
        using var db = Create();
        return await db.Set<T>().AsNoTracking().FirstOrDefaultAsync();
    }

    public T? FirstOrDefault(Expression<Func<T, bool>> predicate)
    {
        using var db = Create();
        return db.Set<T>().AsNoTracking().FirstOrDefault(predicate);
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        using var db = Create();
        return await db.Set<T>().AsNoTracking().FirstOrDefaultAsync(predicate);
    }

    public TResult? FirstOrDefault<TResult>(Expression<Func<T, bool>> predicate,
                                            Expression<Func<T, TResult>> selector)
    {
        using var db = Create();
        return db.Set<T>().AsNoTracking().Where(predicate).Select(selector).FirstOrDefault();
    }

    public async Task<TResult?> FirstOrDefaultAsync<TResult>(Expression<Func<T, bool>> predicate,
                                                             Expression<Func<T, TResult>> selector)
    {
        using var db = Create();
        return await db.Set<T>().AsNoTracking().Where(predicate).Select(selector).FirstOrDefaultAsync();
    }


    public void Add(T entity)
    {
        using var db = Create();
        db.Set<T>().Add(entity);
        db.SaveChanges();
    }

    public async Task AddAsync(T entity)
    {
        using var db = Create();
        await db.Set<T>().AddAsync(entity);
        await db.SaveChangesAsync();
    }

    public void Update(T entity)
    {
        using var db = Create();
        db.Set<T>().Update(entity);
        db.SaveChanges();
    }

    public async Task UpdateAsync(T entity)
    {
        using var db = Create();
        db.Set<T>().Update(entity);
        await db.SaveChangesAsync();
    }

    public void Delete(T entity)
    {
        using var db = Create();
        db.Set<T>().Remove(entity);
        db.SaveChanges();
    }

    public async Task<bool> Delete(IEnumerable<T> entities)
    {
        using var db = Create();
        db.Set<T>().RemoveRange(entities);
        await db.SaveChangesAsync();
        return true;
    }

    public int ExecuteDelete(Expression<Func<T, bool>> filter)
    {
        using var db = Create();
        return db.Set<T>().Where(filter).ExecuteDelete();
    }

    public async Task<int> ExecuteDeleteAsync(Expression<Func<T, bool>> filter)
    {
        using var db = Create();
        return await db.Set<T>().Where(filter).ExecuteDeleteAsync();
    }

    public void ExecuteUpdate(Expression<Func<T, bool>> query,
                              Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> expression)
    {
        using var db = Create();
        db.Set<T>().Where(query).ExecuteUpdate(expression);
    }

    public async Task ExecuteUpdateAsync(Expression<Func<T, bool>> query,
                                         Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> expression)
    {
        using var db = Create();
        await db.Set<T>().Where(query).ExecuteUpdateAsync(expression);
    }



    public int Commit()
    {
        using var db = Create();
        return db.SaveChanges();
    }

    public async Task<int> CommitAsync()
    {
        using var db = Create();
        return await db.SaveChangesAsync();
    }
}



