using System.Runtime.CompilerServices;
using EventManager.Application.Interfaces;
using EventManager.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Data;
/// <summary>
/// Реализация интерфейса репозитория броней.
/// </summary>
/// <param name="dbContext"></param>
public class BookingRepository(AppDbContext dbContext) : IBookingRepository
{
    async Task IBookingRepository.AddBookingAsync(Booking booking, CancellationToken cancellation)
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

    async IAsyncEnumerable<IEnumerable<Booking>> IBookingRepository.GetPendingBookingsAsync(int chunkSize, [EnumeratorCancellation] CancellationToken cancellation)
    {
        if (chunkSize <= 0)
            throw new ArgumentException("Chunk size must be greater than 0", nameof(chunkSize));

        var pendingBookings = dbContext.Bookings
            .AsNoTracking()
            .Where(b => b.Status == BookingStatus.Pending)
            .Include(b => b.Event)
            .OrderBy(b => b.CreatedAt)
            .AsAsyncEnumerable()
            .WithCancellation(cancellation);

        var chunk = new List<Booking>(chunkSize);
        await foreach(var item in pendingBookings)
        {
            chunk.Add(item);
            if (chunk.Count == chunkSize)
            {
                yield return chunk;
                chunk = new(chunkSize);
            }
        }

        if (chunk.Count >0 )
            yield return chunk;
    }
}