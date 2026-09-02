using System.Linq.Expressions;
using EventManager.Application.Interfaces;
using EventManager.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Data;

public class EventRepository(AppDbContext dbContext) : IEventRepository
{
    readonly DbSet<Event> _events = dbContext.Set<Event>();
    
    IQueryable<Event> IObjectRepository<Event, Guid>.GetObjects(Expression<Func<Event, bool>>? filter)
    {
        if (filter is null)
            return _events.AsNoTracking();
        return _events.AsNoTracking().Where(filter);
    }

    Task<Event?> IObjectRepository<Event, Guid>.GetObjectAsync(Guid id, CancellationToken cancellation)
        => _events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cancellation);

    async Task<Event> IObjectRepository<Event, Guid>.AddObjectAsync(Event @event, CancellationToken cancellation)
    {
        _events.Add(@event);
        await dbContext.SaveChangesAsync(cancellation);
        return @event;
    }
    
    async Task<Event?> IObjectRepository<Event, Guid>.UpdateObjectAsync(Guid id, Action<Event> updater, CancellationToken cancellation)
    {
        if (await _events.FindAsync(id, cancellation) is Event dest)
        {
            updater(dest);
            await dbContext.SaveChangesAsync(cancellation);
            return dest;
        }
        return null;
    }

    async Task<Event?> IObjectRepository<Event, Guid>.DeleteObjectAsync(Guid id, CancellationToken cancellation)
    {
        if (await _events.FindAsync(id, cancellation) is Event @event)
        {
            _events.Remove(@event);
            await dbContext.SaveChangesAsync(cancellation);
            return @event;
        }
        return null;
    }

    Task<int> IObjectRepository<Event, Guid>.SaveChangesAsync(CancellationToken cancellation) 
        => dbContext.SaveChangesAsync(cancellation);
}