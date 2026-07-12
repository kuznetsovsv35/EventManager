using EventManager.Application.DataTransfer;

namespace EventManager.Application.Interfaces;

/// <summary>
/// Интерфейс сервиса бронирования.
/// </summary>
public interface IBookingService
{
    /// <summary>
    /// Создать бронь для события.
    /// </summary>
    /// <param name="eventId">Идентификатор бронирования.</param>
    /// <returns></returns>
    Task<BookingInfo> CreateBookingAsync(Guid eventId, CancellationToken cancellation);

    /// <summary>
    /// Получить бронь по идентификатору.
    /// </summary>
    /// <param name="bookingId"></param>
    /// <returns></returns>
    Task<BookingInfo> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellation);
}