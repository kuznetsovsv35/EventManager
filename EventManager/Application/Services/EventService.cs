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
    async Task<EventOutputData> IEventService.CreateEventAsync(EventInputData data, CancellationToken cancellation)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        var e = data.ToEvent();
        await dbContext.AddEventAsync(e, cancellation);
        return e.ToOutputData();
    }

    async Task<EventOutputData> IEventService.DeleteEventAsync(Guid id, CancellationToken cancellation)
    {
        if (await dbContext.DeleteEventAsync(id, cancellation) is Event e)
            return e.ToOutputData();

        throw new EventNotFoundException(nameof(id), id);
    }

    IAsyncEnumerable<EventOutputData> IEventService.GetAllEvents()
        => dbContext
            .GetEvents()
            .Select(x => x.ToOutputData())
            .AsAsyncEnumerable();

    Task<PaginateResult<EventOutputData>> IEventService.GetEvents(FilterParams? filterParams, PageParams pageParams, CancellationToken cancellation)
        => paginator.PaginateAsync(
            FilterEvents(filterParams),
            pageParams.CurrentPage, pageParams.PageSize,
            e => e.ToOutputData(), cancellation);

    IAsyncEnumerable<EventOutputData> IEventService.GetEvents(FilterParams? filterParams)
        => FilterEvents(filterParams)
            .Select(e => e.ToOutputData())
            .AsAsyncEnumerable();

    IQueryable<Event> FilterEvents(FilterParams? filterParams)
    {
        var f = filter.Reset();

        if (filterParams is { Title: string title })
            f.AddCondition(e => e.Title.Contains(title, StringComparison.OrdinalIgnoreCase));

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

    async Task<EventOutputData> IEventService.GetEventAsync(Guid id, CancellationToken cancellation)
    {
        if (await dbContext.GetEventAsync(id, cancellation) is Event e)
            return e.ToOutputData();

        throw new EventNotFoundException(nameof(id), id);
    }

    async Task<EventOutputData> IEventService.UpdateEventAsync(Guid id, EventInputData data, CancellationToken cancellation)
    {
        var e = await dbContext.CreateSyncContext<Booking>().ExecuteActionAsync(async () =>
        {
            if (await dbContext.Events.FindAsync(id) is Event @event)
            {
                data.Update(@event);
                return @event;
            }
            return null;
        }, cancellation);

        return e is not null
            ? e.ToOutputData()
            : throw new EventNotFoundException(nameof(id), id);
    }
}