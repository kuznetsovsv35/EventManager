using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using EventManager.Application.DataTransfer;
using EventManager.Application.Interfaces;
using EventManager.Data;
using EventManager.Models;

namespace EventManager.Application.Services;

/// <summary>
/// Сервис управления событиями.
/// </summary>
/// <param name="dbContext"></param>
public class EventService(
    AppDbContext dbContext, 
    IEventFilter eventFilter,
    IEventPaginator paginator) : IEventService
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

    public PaginateResult GetAllEvents()
    {  
        return GetEvents(null, PageParams.Default);
    }
    public PaginateResult GetEvents(FilterParams? filterParams, PageParams pageParams)
        => paginator.Paginate(
            FilterEvents(dbContext.Events.AsQueryable<Event>(), filterParams),
            pageParams); 
        
    IQueryable<Event> FilterEvents(IQueryable<Event> events, FilterParams? filter)
    {
        eventFilter.Clear();

        if (filter is { Title: not null })
        {
            eventFilter.TitleContains(filter.Title);
        }

        if (filter is { From: not null })
        {
            eventFilter.UseFromDate(filter.From.Value);
        }

        if (filter is { To: not null })
        {
            eventFilter.UseToDate(filter.To.Value);
        }
        
        if (eventFilter.Expression is Expression<Func<Event, bool>> expression)
            return events.Where(expression);

        return events;
    }

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