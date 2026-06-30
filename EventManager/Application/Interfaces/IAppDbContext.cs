using EventManager.Models;

namespace EventManager.Application.Interfaces;

/// <summary>
/// Интерфейс доступа к данным.
/// </summary>
public interface IAppDbContext
{
    IQueryable<Event> Events { get; }
    void Add(Event @event);
    void Update(Event @event);
    void Delete(Event @event);
}