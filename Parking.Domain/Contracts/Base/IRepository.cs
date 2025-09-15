using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace Parking.Domain.Contracts.Base;


public interface IRepository<T> where T : class
{
    // Read
    IQueryable<T> GetAll();
    List<T> ToList();
    Task<List<T>> ToListAsync();

    T? GetById(Guid id);
    Task<T?> GetByIdAsync(Guid id);

    T? GetById(int id);
    Task<T?> GetByIdAsync(int id);

    T? GetById(long id);
    Task<T?> GetByIdAsync(long id);

    T? FirstOrDefault();
    Task<T?> FirstOrDefaultAsync();

    T? FirstOrDefault(Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

    TResult? FirstOrDefault<TResult>(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, TResult>> selector);

    Task<TResult?> FirstOrDefaultAsync<TResult>(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, TResult>> selector);

    IQueryable<T> Find(Expression<Func<T, bool>> predicate);

    // Write
    void Add(T entity);
    Task AddAsync(T entity);

    void Update(T entity);
    void Delete(T entity);

    void ExecuteUpdate(
        Expression<Func<T, bool>> query,
        Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> expression);

    Task ExecuteUpdateAsync(
        Expression<Func<T, bool>> query,
        Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> expression);

    int ExecuteDelete(Expression<Func<T, bool>> filter);
    Task<int> ExecuteDeleteAsync(Expression<Func<T, bool>> filter);

    int Commit();
    Task<int> CommitAsync();
}


