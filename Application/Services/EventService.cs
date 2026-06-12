using EventManager.Application.DataTransfer;
using EventManager.Application.Infrastructure;
using EventManager.Data;
using EventManager.Models;

namespace EventManager.Application.Services;

/// <summary>
/// Сервис управления событиями.
/// </summary>
/// <param name="dbContext"></param>
public class EventService(AppDbContext dbContext) : IEventService
{
    public void CreateEvent(EventInputData data)
    {
        var newEvent = data.ToEvent();
        dbContext?.Events.Add(newEvent);
        dbContext?.SaveChanges();
    }

    public EventOutputData? DeleteEvent(Guid id)
    {
        if (dbContext.Events.FirstOrDefault(e => e.Id == id) is Event e)
        {
            dbContext.Events.Remove(e);
            dbContext.SaveChanges();
            return new EventOutputData(e);
        }
        return null;
    }

    public IList<EventOutputData> GetAllEvents()
    {
        return [.. dbContext.Events.Select(e => new EventOutputData(e))];
    }

    public EventOutputData? GetEvent(Guid id)
    {
        if (dbContext.Events.FirstOrDefault(e => e.Id == id) is Event e)
            return new EventOutputData(e);
        return null;
    }

    public EventOutputData? UpdateEvent(Guid id, EventInputData data)
    {
        if (dbContext.Events.FirstOrDefault(e => e.Id == id) is Event e)
        {
            data.Update(e);
            dbContext.Events.Update(e);
            dbContext.SaveChanges();
            return new EventOutputData(e);
        }
        return null;
    }
}