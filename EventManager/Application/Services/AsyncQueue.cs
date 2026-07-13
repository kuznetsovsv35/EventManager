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
        while(true)
        {
            await _lock.WaitAsync();
            try
            {
                if (_queue.TryDequeue(out T? obj) && obj != null)
                    return obj;
            }
            finally
            {
                _lock.Release();
            }

            await _trigger.WaitAsync(cancellation);
        }
    }

    public async Task Enqueue(T obj)
    {
        await _lock.WaitAsync();
        try
        {
            _queue.Enqueue(obj);
            _trigger.Release();
        }
        finally
        {
            _lock.Release();
        }
    }
}