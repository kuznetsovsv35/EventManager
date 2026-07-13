namespace EventManager.Application.Interfaces;

public interface IAsyncQueue<T>
{
    Task Enqueue(T obj);
    
    Task<T> Dequeue(CancellationToken cancellation);

    Task Clear();
}