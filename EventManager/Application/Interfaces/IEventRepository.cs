using System.Linq.Expressions;
using EventManager.Models;

namespace EventManager.Application.Interfaces;

public interface IEventRepository
{
    IQueryable<Event> GetEvents(Expression<Func<Event, bool>>? filter = null);
    
    Task<Event?> GetEventAsync(Guid id, CancellationToken cancellation);

    Task<Event> AddEventAsync(Event @event, CancellationToken cancellation);

    Task<Event?> UpdateEventAsync(Event @event, CancellationToken cancellation);

    Task<Event?> DeleteEventAsync(Guid id, CancellationToken cancellation);
}