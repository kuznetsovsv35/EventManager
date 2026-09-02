using System.Linq.Expressions;

namespace EventManager.Application.Interfaces;

public interface IObjectRepository<T, TKey> where T : class
{
    IQueryable<T> GetObjects(Expression<Func<T, bool>>? filter = null);

    Task<T?> GetObjectAsync(TKey key, CancellationToken cancellation);

    Task<T> AddObjectAsync(T obj, CancellationToken cancellation);

    Task<T?> UpdateObjectAsync(TKey key, Action<T> update, CancellationToken cancellation);

    Task<T?> DeleteObjectAsync(TKey key, CancellationToken cancellation);

    Task<int> SaveChangesAsync(CancellationToken cancellation);
}