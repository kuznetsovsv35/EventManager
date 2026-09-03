using EventManager.Models;

namespace EventManager.Application.Interfaces;

public interface IBookingRepository
{
    Task AddBooking(Booking booking, CancellationToken cancellation);
    
    Task<Booking?> GetBookingAsync(Guid id, CancellationToken cancellation);
    
    Task UpdateBookingStatusAsync(Booking booking, CancellationToken cancellation);
}