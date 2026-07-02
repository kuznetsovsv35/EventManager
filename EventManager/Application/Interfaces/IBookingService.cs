using EventManager.Models;

namespace EventManager.Application.Interfaces;

/// <summary>
/// Интерфейс сервиса бронирования.
/// </summary>
public interface IBookingService
{
    /// <summary>
    /// Создать бронь для события.
    /// </summary>
    /// <param name="eventId">Идентиикатор бронирования.</param>
    /// <returns></returns>
    Task<Booking> CreateBookingAsync(Guid eventId);

    /// <summary>
    /// Получить бронь по идентиикатору.
    /// </summary>
    /// <param name="bookingId"></param>
    /// <returns></returns>
    Task<Booking> GetBookingByIdAsync(Guid bookingId);
}