using BookingService.Domain;

namespace BookingService.Application;

/// <summary>
/// Сервис публикации событий бронирования.
/// </summary>
public interface IEventsServiceNotifier
{
    Task BookingConfirmedAsync(Booking booking, Guid? userId, CancellationToken cancellation);
    Task BookingCancelledAsync(Booking booking, Guid? userId, CancellationToken cancellation);
}
