using EventManager.Application.Interfaces;
using EventManager.Infrastructure;
using EventManager.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Application.Services;

public class BookingService(IAppDbContext dbContext) : IBookingService
{
    public async Task<Booking> CreateBookingAsync(Guid eventId)
    {
        if (await dbContext.Events.SingleOrDefaultAsync(x => x.Id == eventId) is Event @event)
        {
            var booking = new Booking()
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                CreatedAt = DateTime.Now,
                Status = BookingStatus.Pending
            };

            await dbContext.AddBookingAsync(booking);

            return booking;
        }

        throw new EventNotFoundException(nameof(eventId), eventId);
    }

    public async Task<Booking> GetBookingByIdAsync(Guid bookingId)
    {
        if (await dbContext.Bookings.SingleOrDefaultAsync(x => x.Id == bookingId) is Booking booking)
        {
            return booking;    
        }
        
        throw new BookingNotFoundException(nameof(bookingId), bookingId);
    }
}