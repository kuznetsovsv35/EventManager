using EventManager.Application.DataTransfer;
using EventManager.Application.Interfaces;
using EventManager.Infrastructure;
using EventManager.Models;

namespace EventManager.Application.Services;

public class BookingService(IAppDbContext dbContext, IAsyncQueue<Booking> bookingQueue) : IBookingService
{
    public async Task<BookingInfo> CreateBookingAsync(Guid eventId, CancellationToken cancellation)
    {
        ISyncDataContext<Event> syncContext = dbContext.CreateSyncContext<Event>();

        await syncContext.ExecuteActionAsync(async(events) =>
        {
            if (await events.FindAsync(eventId, cancellation) is Event @event)
            {
                cancellation.ThrowIfCancellationRequested();
            
                if (!@event.TryReserveSeats())
                {
                }
                return;
            }
            throw new EventNotFoundException(nameof(eventId), eventId);
        }, cancellation);

        var booking = new Booking()
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            CreatedAt = DateTime.Now,
            Status = BookingStatus.Pending
        };

        await Task.WhenAll(
            dbContext.AddBookingAsync(booking, cancellation),
            bookingQueue.Enqueue(booking, cancellation)
        );
        return booking.ToInfo();

        /*
        if (await dbContext.GetEventAsync(eventId, cancellation) is Event @event)
        {
            cancellation.ThrowIfCancellationRequested();
            var booking = new Booking()
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                CreatedAt = DateTime.Now,
                Status = BookingStatus.Pending
            };

            await Task.WhenAll(
                dbContext.AddBookingAsync(booking, cancellation),
                bookingQueue.Enqueue(booking, cancellation)
            );
            return booking.ToInfo();
        }

        throw new EventNotFoundException(nameof(eventId), eventId);
        */
    }

    public async Task<BookingInfo> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellation)
    {
        if (await dbContext.GetBookingAsync(bookingId, cancellation) is Booking booking)
            return booking.ToInfo();

        throw new BookingNotFoundException(nameof(bookingId), bookingId);
    }
}