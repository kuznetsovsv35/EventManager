using EventManager.Application.Interfaces;
using EventManager.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Data;

/// <summary>
/// Контекст хранения данных события.
/// </summary>
/// <param name="options"></param>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<Event> Events { get; set; }

    public DbSet<Booking> Bookings { get; set; }

    IQueryable<Event> IAppDbContext.Events => Events;

    IQueryable<Booking> IAppDbContext.Bookings => Bookings;

    public void AddEvent(Event @event)
    {
        Events.Add(@event);
        SaveChanges();
    }

    public async Task AddBookingAsync(Booking booking, CancellationToken cancellation)
    {
        await Bookings.AddAsync(booking, cancellation);
        await SaveChangesAsync(cancellation);
    }

    public void DeleteEvent(Event @event)
    {
        Events.Remove(@event);
        SaveChanges();
    }

    public async Task DeleteBookingAsync(Booking booking, CancellationToken cancellation)
    {
        Bookings.Remove(booking);
        await SaveChangesAsync(cancellation);
    }

    public void UpdateEvent(Event @event)
    {
        Events.Update(@event);
        SaveChanges();
    }

    public async Task UpdateBookingAsync(Booking booking, CancellationToken cancellation)
    {
        Bookings.Remove(booking);
        await SaveChangesAsync(cancellation);
    }
}