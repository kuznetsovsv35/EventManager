using System.Linq.Expressions;
using EventManager.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Data;

/// <summary>
/// Контекст хранения данных события.
/// </summary>
/// <param name="options"></param>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    #region Общие
    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    #endregion

    #region  Events
    public DbSet<Event> Events => Set<Event>();
    public IQueryable<Event> GetEvents(Expression<Func<Event, bool>>? filter = null)
    {
        if (filter == null)
            return Events.AsNoTracking();

        return Events.AsNoTracking().Where(filter);
    }
    public Task<Event?> GetEventAsync(Guid id, CancellationToken cancellation)
        => Events.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellation);
    
    public Task AddEventAsync(Event @event, CancellationToken cancellation)
    {
        Events.Add(@event);
        return SaveChangesAsync(cancellation);
    }
    public async Task<Event?> DeleteEventAsync(Guid id, CancellationToken cancellation)
    {
        Event? @event = await Events.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellation);

        if (@event is not null)
        {
            Events.Remove(@event);
            await SaveChangesAsync(cancellation);
        }

        return @event;
    }
    public async Task<Event?> UpdateEventAsync(Guid id, Action<Event> updater, CancellationToken cancellation)
    {
        if (await Events.FindAsync(id) is Event dest)
        {
            updater(dest);
            await SaveChangesAsync(cancellation);
            return dest;
        }
        return null;
    }
    #endregion

    #region  Bookings
    public DbSet<Booking> Bookings => Set<Booking>();
    public IQueryable<Booking> GetBookings(Expression<Func<Booking, bool>>? filter = null)
    {
        if (filter == null)
            return Bookings.AsNoTracking();

        return Bookings.AsNoTracking().Where(filter);
    }
    public Task<Booking?> GetBookingAsync(Guid id, CancellationToken cancellation)
        => Bookings.AsNoTracking().Include(b => b.Event).FirstOrDefaultAsync(x => x.Id == id, cancellation);

    public Task AddBookingAsync(Booking booking, CancellationToken cancellation)
    {
        Bookings.Add(booking);
        return SaveChangesAsync(cancellation);
    }
    public async Task<Booking?> DeleteBookingAsync(Guid id, CancellationToken cancellation)
    {
        Booking? booking = await Bookings.FindAsync(id, cancellation);
        if (booking is not null)
        {
            Bookings.Remove(booking);
            await SaveChangesAsync(cancellation);
        }
        return booking;
    }
    public async Task UpdateBookingAsync(Guid id, Action<Booking> updater, CancellationToken cancellation)
    {
        if (await Bookings.FindAsync(id, cancellation) is Booking dest)
        {
            updater(dest);
            await SaveChangesAsync(cancellation);
        }
    }
    #endregion
}