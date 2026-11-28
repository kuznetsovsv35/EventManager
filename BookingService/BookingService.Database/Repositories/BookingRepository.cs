using System.Runtime.CompilerServices;
using BookingService.Application;
using BookingService.Domain;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Database.Repositories;

/// <summary>
/// Реализация интерфейса репозитория броней.
/// </summary>
/// <param name="dbContext"></param>
public class BookingRepository(BookingsDbContext dbContext) : IBookingRepository
{
    async Task IBookingRepository.AddBookingAsync(Booking booking, CancellationToken cancellation)
    {
        dbContext.Bookings.Add(booking);
        await dbContext.SaveChangesAsync(cancellation);
    }

    Task<Booking?> IBookingRepository.GetBookingAsync(Guid id, CancellationToken cancellation)
        => dbContext.Bookings.AsNoTracking().SingleOrDefaultAsync(b => b.Id == id, cancellation);

    async Task IBookingRepository.UpdateBookingStatusAsync(Booking booking, CancellationToken cancellation)
    {
        dbContext.Bookings.Update(booking);
        if (booking is { Status: BookingStatus.Rejected or BookingStatus.Cancelled })
        {
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
            .OrderBy(b => b.CreatedAt)
            .AsAsyncEnumerable()
            .WithCancellation(cancellation);

        var chunk = new List<Booking>(chunkSize);
        await foreach (var item in pendingBookings)
        {
            chunk.Add(item);
            if (chunk.Count == chunkSize)
            {
                yield return chunk;
                chunk = new(chunkSize);
            }
        }

        if (chunk.Count > 0)
            yield return chunk;
    }

    Task<int> IBookingRepository.GetUserActiveBookingCountAsync(Guid userId, CancellationToken cancellation)
    {
        return dbContext.Bookings
            .Where(b => b.UserId == userId && b.Status == BookingStatus.Pending)
            .CountAsync(cancellation);
    }
}