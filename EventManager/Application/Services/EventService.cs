using EventManager.Application.DataTransfer;
using EventManager.Application.Interfaces;
using EventManager.Data;
using EventManager.Models;

namespace EventManager.Application.Services;

/// <summary>
/// Сервис управления событиями.
/// </summary>
/// <param name="dbContext"></param>
public class EventService(AppDbContext dbContext) : IEventService
{
    public Event CreateEvent(EventInputData data)
    {
        var e = data.ToEvent();
        dbContext?.Events.Add(e);
        dbContext?.SaveChanges();
        return e;
    }

    public Event? DeleteEvent(Guid id)
    {
        if (dbContext.Events.FirstOrDefault(e => e.Id == id) is Event e)
        {
            dbContext.Events.Remove(e);
            dbContext.SaveChanges();
            return e;
        }
        
        return null;
    }

    public IQueryable<Event> GetAllEvents() => dbContext.Events;
    
    public Event? GetEvent(Guid id)
    {
        if (dbContext.Events.FirstOrDefault(e => e.Id == id) is Event e)
            return e;
        
        return null;
    }

    public Event? UpdateEvent(Guid id, EventInputData data)
    {
        if (dbContext.Events.FirstOrDefault(e => e.Id == id) is Event e)
        {
            data.Update(e);
            dbContext.Events.Update(e);
            dbContext.SaveChanges();
            return e;
        }
        return null;
    }
}