namespace EventManager.Application.Interfaces;

/// <summary>
/// Интерфейс объекта синхронизации.
/// </summary>
public interface ISyncDataContext
{
    /// <summary>
    /// Атомарно выполнить действие (с результатом).
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="action"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task<TResult> ExecuteActionAsync<TResult>(Func<Task<TResult>> action, CancellationToken cancellation);

    /// <summary>
    /// Атомарно выполнить действие.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task ExecuteActionAsync(Func<Task> action, CancellationToken cancellation);
}