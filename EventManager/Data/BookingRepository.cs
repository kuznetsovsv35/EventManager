using EventManager.Application.Interfaces;
using EventManager.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Data;

public class BookingRepository(AppDbContext dbContext) : IBookingRepository
{
    async Task IBookingRepository.AddBooking(Booking booking, CancellationToken cancellation)
    {
        dbContext.Bookings.Add(booking);
        await dbContext.SaveChangesAsync(cancellation);
    }
    
    Task<Booking?> IBookingRepository.GetBookingAsync(Guid id, CancellationToken cancellation)
        => dbContext.Bookings.AsNoTracking().Include(b => b.Event).SingleOrDefaultAsync(cancellation);

    async Task IBookingRepository.UpdateBookingStatusAsync(Booking booking, CancellationToken cancellation)
    {
        dbContext.Bookings.Update(booking);
        if (booking.Status == BookingStatus.Rejected && booking.Event is not null)
        {
            booking.Event.ReleaseSeats();
            dbContext.Events.Update(booking.Event);
        }
        await dbContext.SaveChangesAsync(cancellation);
    }
}