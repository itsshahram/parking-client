using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Parking.Domain.Contracts.Base;
using Parking.Infrastructure.Context;
using Polly;
using Polly.Retry;
using System.Data.Common;
using System.Linq.Expressions;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    private static readonly AsyncRetryPolicy _retryPolicy = Policy
        .Handle<DbException>()
        .Or<TimeoutException>()
        .WaitAndRetryAsync(
            3,
            attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
            (exception, timespan, retryCount, context) =>
            {
                Console.WriteLine($"Retry {retryCount} after {timespan.TotalSeconds}s due to {exception.Message}");
            });

    public Repository(IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    // ----------------- Helpers -----------------
    private async Task<TResult> ExecuteAsync<TResult>(Func<ApplicationDbContext, Task<TResult>> action)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                await using var context = await _dbContextFactory.CreateDbContextAsync();
                return await action(context);
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Repository operation failed: {ex.Message}");
            return default!;
        }
    }

    private async Task ExecuteAsync(Func<ApplicationDbContext, Task> action)
    {
        try
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                await using var context = await _dbContextFactory.CreateDbContextAsync();
                await action(context);
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Repository operation failed: {ex.Message}");
        }
    }

    // ----------------- Read -----------------
    public IQueryable<T> GetAll()
    {
        var context = _dbContextFactory.CreateDbContext();
        return context.Set<T>().AsNoTracking();
    }

    public List<T> ToList()
    {
        using var context = _dbContextFactory.CreateDbContext();
        return context.Set<T>().AsNoTracking().ToList();
    }

    public Task<List<T>> ToListAsync() =>
        ExecuteAsync(ctx => ctx.Set<T>().AsNoTracking().ToListAsync());

    public T? GetById(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        return context.Set<T>().AsNoTracking().FirstOrDefault(e => EF.Property<Guid>(e, "Id") == id);
    }

    public Task<T?> GetByIdAsync(Guid id) =>
        ExecuteAsync(ctx => ctx.Set<T>().AsNoTracking().FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id));

    public T? GetById(int id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        return context.Set<T>().AsNoTracking().FirstOrDefault(e => EF.Property<int>(e, "Id") == id);
    }

    public Task<T?> GetByIdAsync(int id) =>
        ExecuteAsync(ctx => ctx.Set<T>().AsNoTracking().FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id));

    public T? GetById(long id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        return context.Set<T>().AsNoTracking().FirstOrDefault(e => EF.Property<long>(e, "Id") == id);
    }

    public Task<T?> GetByIdAsync(long id) =>
        ExecuteAsync(ctx => ctx.Set<T>().AsNoTracking().FirstOrDefaultAsync(e => EF.Property<long>(e, "Id") == id));

    public T? FirstOrDefault()
    {
        using var context = _dbContextFactory.CreateDbContext();
        return context.Set<T>().AsNoTracking().FirstOrDefault();
    }

    public Task<T?> FirstOrDefaultAsync() =>
        ExecuteAsync(ctx => ctx.Set<T>().AsNoTracking().FirstOrDefaultAsync());

    public T? FirstOrDefault(Expression<Func<T, bool>> predicate)
    {
        using var context = _dbContextFactory.CreateDbContext();
        return context.Set<T>().AsNoTracking().FirstOrDefault(predicate);
    }

    public Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate) =>
        ExecuteAsync(ctx => ctx.Set<T>().AsNoTracking().FirstOrDefaultAsync(predicate));

    public TResult? FirstOrDefault<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector)
    {
        using var context = _dbContextFactory.CreateDbContext();
        return context.Set<T>().Where(predicate).AsNoTracking().Select(selector).FirstOrDefault();
    }

    public Task<TResult?> FirstOrDefaultAsync<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector) =>
        ExecuteAsync(ctx => ctx.Set<T>().Where(predicate).AsNoTracking().Select(selector).FirstOrDefaultAsync());

    public IQueryable<T> Find(Expression<Func<T, bool>> predicate)
    {
        using var context = _dbContextFactory.CreateDbContext();
        return context.Set<T>().AsNoTracking().Where(predicate);
    }

    // ----------------- Write -----------------
    public void Add(T entity)
    {
        using var context = _dbContextFactory.CreateDbContext();
        context.Set<T>().Add(entity);
        context.SaveChanges();
    }

    public Task AddAsync(T entity) =>
        ExecuteAsync(async ctx =>
        {
            await ctx.Set<T>().AddAsync(entity);
            await ctx.SaveChangesAsync();
        });

    public void Update(T entity)
    {
        using var context = _dbContextFactory.CreateDbContext();
        context.Set<T>().Update(entity);
        context.SaveChanges();
    }

    public void Delete(T entity)
    {
        using var context = _dbContextFactory.CreateDbContext();
        context.Set<T>().Remove(entity);
        context.SaveChanges();
    }

    public void ExecuteUpdate(Expression<Func<T, bool>> query, Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> expression)
    {
        using var context = _dbContextFactory.CreateDbContext();
        context.Set<T>().Where(query).ExecuteUpdate(expression);
    }

    public Task ExecuteUpdateAsync(Expression<Func<T, bool>> query, Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> expression) =>
        ExecuteAsync(ctx => ctx.Set<T>().Where(query).ExecuteUpdateAsync(expression));

    public int ExecuteDelete(Expression<Func<T, bool>> filter)
    {
        using var context = _dbContextFactory.CreateDbContext();
        return context.Set<T>().Where(filter).ExecuteDelete();
    }

    public Task<int> ExecuteDeleteAsync(Expression<Func<T, bool>> filter) =>
        ExecuteAsync(ctx => ctx.Set<T>().Where(filter).ExecuteDeleteAsync());

    public int Commit()
    {
        using var context = _dbContextFactory.CreateDbContext();
        return context.SaveChanges();
    }

    public Task<int> CommitAsync() =>
        ExecuteAsync(ctx => ctx.SaveChangesAsync());
}



