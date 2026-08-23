using System.Collections.Concurrent;
using System.Linq.Expressions;
using EventManager.Application.Interfaces;
using EventManager.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Data;

/// <summary>
/// Контекст хранения данных события.
/// </summary>
/// <param name="options"></param>
public class AppDbContext : DbContext, IAppDbContext
{
    #region Общие
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    #endregion

    #region  Events
    public DbSet<Event> Events { get; private set; }

    IQueryable<Event> IAppDbContext.GetEvents(Expression<Func<Event, bool>>? filter)
    {
        if (filter == null)
            return Events.AsNoTracking();

        return Events.AsNoTracking().Where(filter);
    }

    Task<Event?> IAppDbContext.GetEventAsync(Guid id, CancellationToken cancellation)
        => Events.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellation);

    Task IAppDbContext.AddEventAsync(Event @event, CancellationToken cancellation)
    {
        Events.Add(@event);
        return SaveChangesAsync(cancellation);
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

    async Task<Event?> IAppDbContext.UpdateEventAsync(Guid id, Action<Event> updater, CancellationToken cancellation)
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
    public DbSet<Booking> Bookings { get; private set; }

    IQueryable<Booking> IAppDbContext.GetBookings(Expression<Func<Booking, bool>>? filter)
    {
        if (filter == null)
            return Bookings.AsNoTracking();

        return Bookings.AsNoTracking().Where(filter);
    }

    Task<Booking?> IAppDbContext.GetBookingAsync(Guid id, CancellationToken cancellation)
        => Bookings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellation);

    Task IAppDbContext.AddBookingAsync(Booking booking, CancellationToken cancellation)
    {
        Bookings.Add(booking);
        return SaveChangesAsync(cancellation);
    }

    async Task<Booking?> IAppDbContext.DeleteBookingAsync(Guid id, CancellationToken cancellation)
    {
        Booking? booking = await Bookings.FindAsync(id, cancellation);
        if (booking is not null)
        {
            Bookings.Remove(booking);
            await SaveChangesAsync(cancellation);
        }
        return booking;
    }

    async Task IAppDbContext.UpdateBookingAsync(Guid id, Action<Booking> updater, CancellationToken cancellation)
    {
        if (await Bookings.FindAsync(id, cancellation) is Booking dest)
        {
            updater(dest);
            await SaveChangesAsync(cancellation);
        }
    }
    #endregion

    #region Инфраструктура синхронизации.
    static readonly ConcurrentDictionary<int, SemaphoreSlim> _locks = new();

    class SyncDataContext<T>(AppDbContext dbContext) : ISyncDataContext
    {
        static int _hashCode = typeof(T).GUID.GetHashCode();
        SemaphoreSlim _lock = _locks.GetOrAdd(_hashCode, (_) => new(1, 1));

        async Task<TResult> ISyncDataContext.ExecuteActionAsync<TResult>(Func<Task<TResult>> action, CancellationToken cancellation)
        {
            await _lock.WaitAsync(cancellation);
            try
            {
                var result = await action();
                await dbContext.SaveChangesAsync(cancellation);
                return result;
            }
            catch
            {
                RollbackChanges();
                throw;
            }
            finally
            {
                _lock.Release();
            }
        }

        async Task ISyncDataContext.ExecuteActionAsync(Func<Task> action, CancellationToken cancellation)
        {
            await _lock.WaitAsync(cancellation);
            try
            {
                await action();
                await dbContext.SaveChangesAsync(cancellation);
            }
            catch
            {
                //RollbackChanges();
                throw;
            }
            finally
            {
                _lock.Release();
            }
        }

        void RollbackChanges()
        {
            var entries = dbContext.ChangeTracker.Entries().Where(x => x.State != EntityState.Unchanged).ToList();
            foreach (var entry in entries)
                entry.State = EntityState.Unchanged;
        }
    }

    ISyncDataContext IAppDbContext.CreateSyncContext<T>()
        => new SyncDataContext<T>(this);
    #endregion
}