using BookingService.Application.DTO;

namespace BookingService.Application;

/// <summary>
/// Интерфейс сервиса бронирования.
/// </summary>
public interface IBookingService
{
    /// <summary>
    /// Создать бронь для события.
    /// </summary>
    /// <param name="eventId">Идентификатор события.</param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task<BookingInfo> CreateBookingAsync(Guid eventId, CancellationToken cancellation);
    /// <summary>
    /// Получить бронь по идентификатору.
    /// </summary>
    /// <param name="bookingId"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task<BookingInfo> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellation);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="bookingId"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task<BookingInfo> CancelBookingAsync(Guid bookingId, CancellationToken cancellation);
}