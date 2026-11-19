using BookingService.Domain;

namespace BookingService.Application;

public interface IEventsServiceNotifier
{
    Task BookingConfirmedAsync(Booking booking, CancellationToken cancellation);
}
