using EventManager.Application.DataTransfer;
using EventManager.Application.Interfaces;
using EventManager.Infrastructure;
using EventManager.Models;

namespace EventManager.Application.Services;

public class BookingService(
    ISyncContextFactory syncContextFactory,
    IBookingRepository bookings,
    IEventRepository events,
    IAsyncQueue<Guid> bookingQueue) : IBookingService
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
                    await bookings.AddBooking(booking, cancellation);
                    await events.UpdateEventAsync(@event, cancellation);
                    return booking;
                }
                throw new NoAvailableSeatsException(eventId);
            }
            throw new EventNotFoundException(eventId, nameof(eventId));
        }, cancellation);
        
        await bookingQueue.Enqueue(booking.Id, cancellation);
        return booking.ToInfo();
    }

    public async Task<BookingInfo> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellation)
    {
        if (await bookings.GetBookingAsync(bookingId, cancellation) is Booking booking)
            return booking.ToInfo();

        throw new BookingNotFoundException(bookingId, nameof(bookingId));
    }
}