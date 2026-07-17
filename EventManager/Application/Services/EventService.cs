using EventManager.Application.DataTransfer;
using EventManager.Application.Interfaces;
using EventManager.Infrastructure;
using EventManager.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Application.Services;

/// <summary>
/// Сервис управления событиями.
/// </summary>
/// <param name="dbContext"></param>
public class EventService(
    IAppDbContext dbContext,
    IFilter<Event> filter,
    IPaginator<Event> paginator) : IEventService
{
    public EventOutputData CreateEvent(EventInputData data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        var e = data.ToEvent();
        dbContext.AddEvent(e);
        return e.ToOutputData();
    }

    public EventOutputData DeleteEvent(Guid id)
    {
        if (dbContext.DeleteEvent(id) is Event e)
            return e.ToOutputData();

        throw new EventNotFoundException(nameof(id), id);
    }

    public IEnumerable<EventOutputData> GetAllEvents()
        => dbContext.GetAllEvents().Select(x => x.ToOutputData());

    public PaginateResult<EventOutputData> GetEvents(FilterParams? filterParams, PageParams pageParams)
        => paginator.Paginate(
            FilterEvents(filterParams),
            pageParams.CurrentPage, pageParams.PageSize,
            e => e.ToOutputData());

    public IEnumerable<EventOutputData> GetEvents(FilterParams? filterParams)
        => FilterEvents(filterParams)
            .AsEnumerable()
            .Select(e => e.ToOutputData());

    IQueryable<Event> FilterEvents(FilterParams? filterParams)
    {
        var f = filter.Reset();

        if (filterParams is { Title: string title })
        {
            string titleLowCase = title.ToLower();
            f.AddCondition(e => e.Title.ToLower().Contains(titleLowCase));
        }

        if (filterParams is { From: DateTime from })
        {
            DateTime fromDate = from.Date;
            f.AddCondition(e => e.StartAt >= fromDate);
        }

        if (filterParams is { To: DateTime to })
        {
            DateTime toDate = to.AddDays(1).Date;
            f.AddCondition(e => e.EndAt < toDate);
        }

        return dbContext.GetEvents(f.Expression);
    }

    public EventOutputData GetEvent(Guid id)
    {
        if (dbContext.GetEvent(id) is Event e)
            return e.ToOutputData();

        throw new EventNotFoundException(nameof(id), id);
    }

    public EventOutputData UpdateEvent(Guid id, EventInputData data)
    {
        if (dbContext.GetEvents(e => e.Id == id).FirstOrDefault() is Event e)
        {
            data.Update(e);
            dbContext.UpdateEvent(e);
            return e.ToOutputData();
        }
        throw new EventNotFoundException(nameof(id), id);
    }
}