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
    IFilter<Event> filter,
    IPaginator<Event> paginator) : IEventService
{
    public EventOutputData CreateEvent(EventInputData data)
    {
        var e = data.ToEvent();
        dbContext?.Events.Add(e);
        dbContext?.SaveChanges();
        return e.ToOutputData();
    }

    public EventOutputData? DeleteEvent(Guid id)
    {
        if (dbContext.Events.FirstOrDefault(e => e.Id == id) is Event e)
        {
            dbContext.Events.Remove(e);
            dbContext.SaveChanges();
            return e.ToOutputData();
        }
        
        return null;
    }

    public PaginateResult<EventOutputData> GetAllEvents()
    {  
        return GetEvents(null, PageParams.Default);
    }
    public PaginateResult<EventOutputData> GetEvents(FilterParams? filterParams, PageParams pageParams)
        => paginator.Paginate(
            FilterEvents(dbContext.Events.AsQueryable<Event>(), filterParams),
            pageParams.CurrentPage, pageParams.PageSize,
            e => e.ToOutputData()); 
        
    IQueryable<Event> FilterEvents(IQueryable<Event> events, FilterParams? filterParams)
    {
        var f = filter.Reset();

        if (filterParams is { Title: string title }) 
            f.AddCondition(e => e.Title.Contains(title));
        if (filterParams is { From: DateTime from }) 
            f.AddCondition(e => e.StartAt >= from);
        if (filterParams is { To: DateTime to })
        {
            to.AddDays(1);
            f.AddCondition(e => e.EndAt < to);
        }

        return f.ApplyFilter(events);
    }

    public EventOutputData? GetEvent(Guid id)
    {
        if (dbContext.Events.FirstOrDefault(e => e.Id == id) is Event e)
            return e.ToOutputData();
        
        return null;
    }

    public EventOutputData? UpdateEvent(Guid id, EventInputData data)
    {
        if (dbContext.Events.FirstOrDefault(e => e.Id == id) is Event e)
        {
            data.Update(e);
            dbContext.Events.Update(e);
            dbContext.SaveChanges();
            return e.ToOutputData();
        }
        return null;
    }
}