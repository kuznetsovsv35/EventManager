using EventManager.Application.DataTransfer;
using EventManager.Application.Interfaces;
using EventManager.Infrastructure;
using EventManager.Models;

namespace EventManager.Application.Services;

public class BookingService(IAppDbContext dbContext, IAsyncQueue<Booking> bookingQueue) : IBookingService
{
    public async Task<BookingInfo> CreateBookingAsync(Guid eventId, CancellationToken cancellation)
    {
        var booking = await dbContext.CreateSyncContext<Event>().ExecuteActionAsync(async() =>
        {
            if (await dbContext.Events.FindAsync(eventId, cancellation) is Event @event)
            {
                cancellation.ThrowIfCancellationRequested();
            
                if (@event.TryReserveSeats())
                {
                    var booking = new Booking(eventId);
                    dbContext.Bookings.Add(booking);
                    return booking;
                }
                throw new NoAvailableSeatsException(eventId);
            }
            throw new EventNotFoundException(nameof(eventId), eventId);
        }, cancellation);

        await bookingQueue.Enqueue(booking, cancellation);
        return booking.ToInfo();
    }

    public async Task<BookingInfo> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellation)
    {
        if (await dbContext.GetBookingAsync(bookingId, cancellation) is Booking booking)
            return booking.ToInfo();

        throw new BookingNotFoundException(nameof(bookingId), bookingId);
    }
}