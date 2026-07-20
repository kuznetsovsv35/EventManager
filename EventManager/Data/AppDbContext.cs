using System.Linq.Expressions;
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
    #region  Events
    protected DbSet<Event> Events { get; set; }

    IQueryable<Event> IAppDbContext.GetEvents(Expression<Func<Event, bool>>? filter)
    {
        if (filter == null)
            return Events.AsNoTracking();
        
        return Events.AsNoTracking().Where(filter);
    }

    Event? IAppDbContext.GetEvent(Guid id)
        => Events.AsNoTracking().Where(x => x.Id == id).FirstOrDefault();

    Task<Event?> IAppDbContext.GetEventAsync(Guid id, CancellationToken cancellation)
        => Events.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellation);

    void IAppDbContext.AddEvent(Event @event)
    {
        Events.Add(@event);
        SaveChanges();
    }

    Task IAppDbContext.AddEventAsync(Event @event, CancellationToken cancellation)
    {
        Events.Add(@event);
        return SaveChangesAsync(cancellation);
    }

    Event? IAppDbContext.DeleteEvent(Guid id)
    {
        Event? @event = Events.AsNoTracking().FirstOrDefault(x => x.Id == id);
        if (@event is not null)
        {
            Events.Remove(@event);
            SaveChanges();
        }

        return @event;
    }

    async Task<Event?> IAppDbContext.DeleteEventAsync(Guid id, CancellationToken cancellation)
    {
        Event? @event = await Events.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellation);

        if (@event is not null)
        {
            Events.Remove(@event);
            await SaveChangesAsync(cancellation);
        }

        return @event;
    }

    public bool UpdateEvent(Event @event)
    {
        Events.Update(@event);
        return SaveChanges() != 0;
    }

    async Task<bool> IAppDbContext.UpdateEventAsync(Event @event, CancellationToken cancellation)
    {
        Events.Update(@event);
        return (await SaveChangesAsync(cancellation)) != 0;
    }
    #endregion
    
    #region  Bookings
    protected DbSet<Booking> Bookings { get; set; }

    IQueryable<Booking> IAppDbContext.GetBookings(Expression<Func<Booking, bool>>? filter)
    {
        if (filter == null)
            return Bookings.AsNoTracking();

        return Bookings.AsNoTracking().Where(filter);
    }

    Task<Booking?> IAppDbContext.GetBookingAsync(Guid id, CancellationToken cancellation)
        => Bookings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellation);

    async Task IAppDbContext.AddBookingAsync(Booking booking, CancellationToken cancellation)
    {
        Bookings.Add(booking);
        await SaveChangesAsync(cancellation);
    }

    async Task<Booking?> IAppDbContext.DeleteBookingAsync(Guid id, CancellationToken cancellation)
    {
        Booking? booking = await Bookings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellation);
        if (booking is not null)
        {
            Bookings.Remove(booking);
            await SaveChangesAsync(cancellation);
        }
        return booking;
    }

    async Task<bool> IAppDbContext.UpdateBookingAsync(Booking booking, CancellationToken cancellation)
    {
        Bookings.Update(booking);
        return (await SaveChangesAsync(cancellation)) != 0;
    }
    #endregion
}