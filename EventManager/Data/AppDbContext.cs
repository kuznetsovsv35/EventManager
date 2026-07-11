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

    public void Add(Event @event)
    {
        Events.Add(@event);
        SaveChanges();
    }

    public async Task AddBookingAsync(Booking booking)
    {
        await Bookings.AddAsync(booking);
        await SaveChangesAsync();
    }

    public void Delete(Event @event)
    {
        Events.Remove(@event);
        SaveChanges();
    }

    public async Task DeleteBookingAsync(Booking booking)
    {
        Bookings.Remove(booking);
        await SaveChangesAsync();
    }

    public void Update(Event @event)
    {
        Events.Update(@event);
        SaveChanges();
    }

    public async Task UpdateBookingAsync(Booking booking)
    {
        Bookings.Remove(booking);
        await SaveChangesAsync();
    }
}