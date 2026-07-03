using EventManager.Application.Interfaces;
using EventManager.Models;

namespace EventManager.Application.Services;

public class BookingService(IAppDbContext dbContext) : IBookingService
{
    public async Task<Booking> CreateBookingAsync(Guid eventId)
    {
        throw new NotImplementedException();
    }

    public async Task<Booking> GetBookingByIdAsync(Guid bookingId)
    {
        throw new NotImplementedException();
    }
}