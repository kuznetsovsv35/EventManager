namespace EventManager.Application.Interfaces;

/// <summary>
/// Интерфейс асинхронной очереди.
/// </summary>
/// <typeparam name="T">Тип объекта.</typeparam>
public interface IAsyncQueue<T>
{
    /// <summary>
    /// Внесение объекта в очередь.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    Task Enqueue(T obj);

    /// <summary>
    /// Изъятие объекта из очереди, если пуста ждем. Возможная отменя ожидания.
    /// </summary>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task<T> Dequeue(CancellationToken cancellation);

    /// <summary>
    /// Очистка очереди.
    /// </summary>
    /// <returns></returns>
    Task Clear();
}