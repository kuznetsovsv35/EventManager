using EventManager.Models;

namespace EventManager.Application.Interfaces;

/// <summary>
/// Интерфейс доступа к данным.
/// </summary>
public interface IAppDbContext
{
    /// <summary>
    /// Возвращает queryable объект набора данных.
    /// </summary>
    IQueryable<Event> Events { get; }

    IQueryable<Booking> Bookings { get; }
    
    /// <summary>
    /// Добавляет событие в набор данных.
    /// </summary>
    /// <param name="event"></param>
    void AddEvent(Event @event);
    
    /// <summary>
    /// Обновляет событие.
    /// </summary>
    /// <param name="event"></param>
    void UpdateEvent(Event @event);
    
    /// <summary>
    /// Удаляет событие из набора данных.
    /// </summary>
    /// <param name="event"></param>
    void DeleteEvent(Event @event);

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
    /// <param name="booking"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task DeleteBookingAsync(Booking booking, CancellationToken cancellation);
}