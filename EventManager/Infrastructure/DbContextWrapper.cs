using EventManager.Application.Interfaces;
using EventManager.Data;
using EventManager.Models;

namespace EventManager.Infrastructure;

/// <summary>
/// Wrapper для DI.
/// </summary>
/// <param name="dbContext"></param>
public class DbContextWrapper(AppDbContext dbContext) : IAppDbContext
{
    public IQueryable<Event> Events => dbContext.Events;

    public void Add(Event @event)
    {
        dbContext.Add(@event);
        dbContext.SaveChanges();
    }

    public void Delete(Event @event)
    {
        dbContext.Events.Remove(@event);
        dbContext.SaveChanges();
    }

    public void Update(Event @event)
    {
        dbContext.Events.Update(@event);
        dbContext.SaveChanges();
    }
}