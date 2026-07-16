using EventManager.Application.Interfaces;

namespace EventManager.Application.Services;

public class AsyncQueue<T> : IAsyncQueue<T>
{
    readonly Queue<T> _queue = new();
    SemaphoreSlim _trigger = new(0);
    readonly SemaphoreSlim _lock = new(1, 1);

    public async Task Clear()
    {
        await _lock.WaitAsync();
        try
        {
            _queue.Clear();
            Interlocked.Exchange(ref _trigger, new(0))?.Dispose();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<T> Dequeue(CancellationToken cancellation)
    {
        while (true)
        {
            await _trigger.WaitAsync(cancellation);
            await _lock.WaitAsync(cancellation);
            try
            {
                if (_queue.TryDequeue(out T? obj) && obj is not null)
                    return obj;
            }
            finally
            {
                _lock.Release();
            }
        }
    }

    public async Task Enqueue(T obj, CancellationToken cancellation)
    {
        await _lock.WaitAsync(cancellation);
        try
        {
            _queue.Enqueue(obj);
        }
        finally
        {
            _lock.Release();
        }
        
        _trigger.Release();
    }
}