using EventManager.Models;

namespace EventManager.Application.Interfaces;

/// <summary>
/// Интерфейс доступа к данным.
/// </summary>
public interface IAppDbContext
{
    /// <summary>
    /// Вовзращает queryable объект набора данных.
    /// </summary>
    IQueryable<Event> Events { get; }

    IQueryable<Booking> Bookings { get; }
    
    /// <summary>
    /// Добавляет событие в набор данных.
    /// </summary>
    /// <param name="event"></param>
    void Add(Event @event);
    
    /// <summary>
    /// Обновляет событие.
    /// </summary>
    /// <param name="event"></param>
    void Update(Event @event);
    
    /// <summary>
    /// Удаляет событие из набора данных.
    /// </summary>
    /// <param name="event"></param>
    void Delete(Event @event);

    /// <summary>
    /// Добавляет бронирование в хранилище.
    /// </summary>
    /// <param name="booking"></param>
    /// <returns></returns>
    Task AddBookingAsync(Booking booking);

    /// <summary>
    /// Обновляет бронирование.
    /// </summary>
    /// <param name="booking"></param>
    /// <returns></returns>
    Task UpdateBookingAsync(Booking booking);

    /// <summary>
    /// Удаляет бронь.
    /// </summary>
    /// <param name="booking"></param>
    /// <returns></returns>
    Task DeleteBookingAsync(Booking booking);
}