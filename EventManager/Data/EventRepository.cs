using System.Linq.Expressions;
using EventManager.Application.Interfaces;
using EventManager.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Data;

public class EventRepository(AppDbContext dbContext) : IEventRepository
{    
    IQueryable<Event> IEventRepository.GetEvents(Expression<Func<Event, bool>>? filter)
    {
        if (filter is null)
            return dbContext.Events.AsNoTracking();
        return dbContext.Events.AsNoTracking().Where(filter);
    }

    Task<Event?> IEventRepository.GetEventAsync(Guid id, CancellationToken cancellation)
        => dbContext.Events.AsNoTracking().SingleOrDefaultAsync(e => e.Id == id, cancellation);

    async Task<Event> IEventRepository.AddEventAsync(Event @event, CancellationToken cancellation)
    {
        dbContext.Events.Add(@event);
        await dbContext.SaveChangesAsync(cancellation);
        return @event;
    }
    
    async Task<Event?> IEventRepository.UpdateEventAsync(Event @event, CancellationToken cancellation)
    {
        dbContext.Events.Update(@event);
        return (await dbContext.SaveChangesAsync(cancellation) > 0) ? @event : null;
    }

    async Task<Event?> IEventRepository.DeleteEventAsync(Guid id, CancellationToken cancellation)
    {
        if (await dbContext.Events.FindAsync(id, cancellation) is Event @event)
        {
            dbContext.Events.Remove(@event);
            return (await dbContext.SaveChangesAsync(cancellation) > 0) ? @event : null;
        }
        return null;
    }
}