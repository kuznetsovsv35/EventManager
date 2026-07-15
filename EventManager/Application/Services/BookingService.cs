using EventManager.Application.DataTransfer;
using EventManager.Application.Interfaces;
using EventManager.Infrastructure;
using EventManager.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Application.Services;

public class BookingService(IAppDbContext dbContext, IAsyncQueue<Booking> bookingQueue) : IBookingService
{
    public async Task<BookingInfo> CreateBookingAsync(Guid eventId, CancellationToken cancellation)
    {
        if (await dbContext.Events.SingleOrDefaultAsync(x => x.Id == eventId, cancellation) is Event @event)
        {
            cancellation.ThrowIfCancellationRequested();
            var booking = new Booking()
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                CreatedAt = DateTime.Now,
                Status = BookingStatus.Pending
            };

            await dbContext.AddBookingAsync(booking, cancellation);
            await bookingQueue.Enqueue(booking);
            return booking.ToInfo();
        }

        throw new EventNotFoundException(nameof(eventId), eventId);
    }

    public async Task<BookingInfo> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellation)
    {
        if (await dbContext.Bookings.SingleOrDefaultAsync(x => x.Id == bookingId, cancellation) is Booking booking)
            return booking.ToInfo();

        throw new BookingNotFoundException(nameof(bookingId), bookingId);
    }
}