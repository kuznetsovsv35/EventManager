using System.Collections.Concurrent;
using EventManager.Application.Interfaces;

namespace EventManager.Infrastructure;

class SyncDataContext<T> : ISyncContext
{
    static readonly ConcurrentDictionary<int, SemaphoreSlim> _locks = new();
    static int _hashCode = typeof(T).GUID.GetHashCode();
    SemaphoreSlim _lock = _locks.GetOrAdd(_hashCode, (_) => new(1, 1));

    async Task<TResult> ISyncContext.ExecuteActionAsync<TResult>(Func<Task<TResult>> action, CancellationToken cancellation)
    {
        await _lock.WaitAsync(cancellation);
        try
        {
            return await action();
        }
        finally
        {
            _lock.Release();
        }
    }

    async Task ISyncContext.ExecuteActionAsync(Func<Task> action, CancellationToken cancellation)
    {
        await _lock.WaitAsync(cancellation);
        try
        {
            await action();
        }
        finally
        {
            _lock.Release();
        }
    }
}
