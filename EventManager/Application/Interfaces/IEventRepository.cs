using System.Linq.Expressions;
using EventManager.Models;

namespace EventManager.Application.Interfaces;

/// <summary>
/// Интерфейс репозитория событий.
/// </summary>
public interface IEventRepository
{
    /// <summary>
    /// Возвращает объект для получения списка объект (опционально фильтр)
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    IQueryable<Event> GetEvents(Expression<Func<Event, bool>>? filter = null);
    /// <summary>
    /// Возвращает событие по идентификатору.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task<Event?> GetEventAsync(Guid id, CancellationToken cancellation);
    /// <summary>
    /// Добавляет новое событие в репозиторий.
    /// </summary>
    /// <param name="event"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task<Event> AddEventAsync(Event @event, CancellationToken cancellation);
    /// <summary>
    /// Обновляет событие в репозитории.
    /// </summary>
    /// <param name="event"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task<Event?> UpdateEventAsync(Event @event, CancellationToken cancellation);
    /// <summary>
    /// Удаляет событие по идентификатору.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task<Event?> DeleteEventAsync(Guid id, CancellationToken cancellation);
}