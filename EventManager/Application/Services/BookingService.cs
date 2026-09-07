using EventManager.Application.DataTransfer;
using EventManager.Application.Interfaces;
using EventManager.Infrastructure;
using EventManager.Models;

namespace EventManager.Application.Services;

public class BookingService(
    IBookingRepository bookings,
    IEventRepository events,
    IAsyncQueue<Guid> bookingQueue) : IBookingService
{
    static readonly SemaphoreSlim _lock = new(1, 1);
    public async Task<BookingInfo> CreateBookingAsync(Guid eventId, CancellationToken cancellation)
    {
        await _lock.WaitAsync(cancellation);
        try
        {
            if (await events.GetEventAsync(eventId, cancellation) is Event @event)
            {
                cancellation.ThrowIfCancellationRequested();

                if (@event.TryReserveSeats())
                {
                    var booking = new Booking(@eventId);
                    await bookings.AddBooking(booking, cancellation);
                    await events.UpdateEventAsync(@event, cancellation);
                    await bookingQueue.Enqueue(booking.Id, cancellation);
                    return booking.ToInfo();
                }
                throw new NoAvailableSeatsException(eventId);
            }
            throw new EventNotFoundException(eventId, nameof(eventId));
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<BookingInfo> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellation)
    {
        if (await bookings.GetBookingAsync(bookingId, cancellation) is Booking booking)
            return booking.ToInfo();

        throw new BookingNotFoundException(bookingId, nameof(bookingId));
    }
}