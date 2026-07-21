using System.Linq.Expressions;
using EventManager.Models;

namespace EventManager.Application.Interfaces;

/// <summary>
/// Интерфейс доступа к данным.
/// </summary>
public interface IAppDbContext
{
    /// <summary>
    /// Возвращает запрос отфильтрованного набора.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    IQueryable<Event> GetEvents(Expression<Func<Event, bool>>? filter = null);

    Task<Event?> GetEventAsync(Guid id, CancellationToken cancellation);

    /// <summary>
    /// Добавляет событие в набор данных.
    /// </summary>
    /// <param name="event"></param>
    /// <param name="cancellation"></param>
    Task AddEventAsync(Event @event, CancellationToken cancellation);

    /// <summary>
    /// Обновляет событие.
    /// </summary>
    /// <param name="event"></param>
    /// <param name="cancellation"></param>
    Task UpdateEventAsync(Event @event, CancellationToken cancellation);

    /// <summary>
    /// Удаляет событие из набора данных.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellation"></param>
    Task<Event?> DeleteEventAsync(Guid id, CancellationToken cancellation);

    /// <summary>
    /// Возвращает объект запроса отфильтрованного набора броней.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    IQueryable<Booking> GetBookings(Expression<Func<Booking, bool>>? filter = null);

    /// <summary>
    /// Возвращает бронь по идентификатору.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task<Booking?> GetBookingAsync(Guid id, CancellationToken cancellation);

    /// <summary>
    /// Добавляет бронирование в хранилище.
    /// </summary>
    /// <param name="booking"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task AddBookingAsync(Booking booking, CancellationToken cancellation);

    /// <summary>
    /// Обновляет бронирование.
    /// </summary>
    /// <param name="booking"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task UpdateBookingAsync(Booking booking, CancellationToken cancellation);

    /// <summary>
    /// Удаляет бронь.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task<Booking?> DeleteBookingAsync(Guid id, CancellationToken cancellation);
}