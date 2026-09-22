using EventManager.Domain.ValueObjects;
using EventManager.Domain.Exceptions;
using EventManager.Common.Interfaces;
using EventManager.Application.Interfaces;
using EventManager.Application.DataTransferObjects;

namespace EventManager.Application.Services;

public class BookingService(
    ISyncContextFactory syncContextFactory,
    IBookingRepository bookings,
    IEventRepository events,
    IBookingServiceNotifier notifier) : IBookingService
{
    public async Task<BookingInfo> CreateBookingAsync(Guid eventId, CancellationToken cancellation)
    {
        var booking = await syncContextFactory.CreateContext<Booking>().ExecuteActionAsync<Booking>(async() =>
        {
            if (await events.GetEventAsync(eventId, cancellation) is Event @event)
            {
                cancellation.ThrowIfCancellationRequested();

                if (@event.TryReserveSeats())
                {
                    var booking = new Booking(eventId);
                    await bookings.AddBookingAsync(booking, cancellation);
                    await events.UpdateEventAsync(@event, cancellation);
                    return booking;
                }
                throw new NoAvailableSeatsException(eventId);
            }
            throw new EventNotFoundException(eventId, nameof(eventId));
        }, cancellation);
        
        await notifier.BookingCreatedAsync(booking.Id, cancellation);
        return booking.ToInfo();
    }

    public async Task<BookingInfo> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellation)
    {
        if (await bookings.GetBookingAsync(bookingId, cancellation) is Booking booking)
            return booking.ToInfo();

        throw new BookingNotFoundException(bookingId, nameof(bookingId));
    }
}