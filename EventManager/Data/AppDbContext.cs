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
        Database.EnsureDeleted();
        Database.Migrate();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    #endregion

    #region  Events
    IQueryable<Event> IObjectRepository<Event, Guid>.GetObjects(Expression<Func<Event, bool>>? filter)
    {
        if (filter is null)
            return Set<Event>().AsNoTracking();
        return Set<Event>().AsNoTracking().Where(filter);
    }

    Task<Event?> IObjectRepository<Event, Guid>.GetObjectAsync(Guid id, CancellationToken cancellation)
        => Set<Event>().AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cancellation);

    async Task<Event> IObjectRepository<Event, Guid>.AddObjectAsync(Event @event, CancellationToken cancellation)
    {
        Set<Event>().Add(@event);
        await SaveChangesAsync(cancellation);
        return @event;
    }
    
    async Task<Event?> IObjectRepository<Event, Guid>.UpdateObjectAsync(Guid id, Action<Event> updater, CancellationToken cancellation)
    {
        if (await Set<Event>().FindAsync(id, cancellation) is Event dest)
        {
            updater(dest);
            await SaveChangesAsync(cancellation);
            return dest;
        }
        return null;
    }

    async Task<Event?> IObjectRepository<Event, Guid>.DeleteObjectAsync(Guid id, CancellationToken cancellation)
    {
        var dbSet = Set<Event>();
        if (await dbSet.FindAsync(id, cancellation) is Event @event)
        {
            dbSet.Remove(@event);
            await SaveChangesAsync(cancellation);
            return @event;
        }
        return null;
    }
    #endregion

    #region  Bookings
    IQueryable<Booking> IObjectRepository<Booking, Guid>.GetObjects(Expression<Func<Booking, bool>>? filter)
    {
        if (filter == null)
            return Set<Booking>().AsNoTracking();

        return Set<Booking>().AsNoTracking().Where(filter);
    }

    Task<Booking?> IObjectRepository<Booking, Guid>.GetObjectAsync(Guid id, CancellationToken cancellation)
        => Set<Booking>().AsNoTracking().Include(b => b.Event).FirstOrDefaultAsync(x => x.Id == id, cancellation);
    
    async Task<Booking> IObjectRepository<Booking, Guid>.AddObjectAsync(Booking booking, CancellationToken cancellation)
    {
        Set<Booking>().Add(booking);
        await SaveChangesAsync(cancellation);
        return booking;
    }

    async Task<Booking?> IObjectRepository<Booking, Guid>.UpdateObjectAsync(Guid id, Action<Booking> updater, CancellationToken cancellation)
    {
        if (await Set<Booking>().FindAsync(id, cancellation) is Booking dest)
        {
            updater(dest);
            await SaveChangesAsync(cancellation);
            return dest;
        }
        return null;
    }

    async Task<Booking?> IObjectRepository<Booking, Guid>.DeleteObjectAsync(Guid id, CancellationToken cancellation)
    {
        var dbSet = Set<Booking>();
        if (await dbSet.FindAsync(id, cancellation) is Booking booking)
        {
            dbSet.Remove(booking);
            await SaveChangesAsync(cancellation);
            return booking;
        }
        return null;
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