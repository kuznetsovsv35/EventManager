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
    IEventRepository repository,
    IFilter<Event> filter,
    IPaginator<Event> paginator) : IEventService
{
    async Task<EventOutputData> IEventService.CreateEventAsync(EventInputData data, CancellationToken cancellation)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        var e = data.ToEvent();
        await repository.AddObjectAsync(e, cancellation);
        return e.ToOutputData();
    }

    async Task<EventOutputData> IEventService.DeleteEventAsync(Guid id, CancellationToken cancellation)
    {
        if (await repository.DeleteObjectAsync(id, cancellation) is Event e)
            return e.ToOutputData();

        throw new EventNotFoundException(id, nameof(id));
    }

    IEnumerable<EventOutputData> IEventService.GetAllEvents()
        => repository
            .GetObjects()
            .Select(x => x.ToOutputData())
            .ToList();

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
        {
            var s = title.ToLower();
            f.AddCondition(e => e.Title.ToLower().Contains(s));
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

        return repository.GetObjects(f.Expression);
    }

    async Task<EventOutputData> IEventService.GetEventAsync(Guid id, CancellationToken cancellation)
    {
        if (await repository.GetObjectAsync(id, cancellation) is Event e)
            return e.ToOutputData();

        throw new EventNotFoundException(id, nameof(id));
    }

    async Task<EventOutputData> IEventService.UpdateEventAsync(Guid id, EventInputData data, CancellationToken cancellation)
    {        
        var e = await repository.UpdateObjectAsync(id, e => data.Update(e), cancellation);
        
        return e is not null
            ? e.ToOutputData()
            : throw new EventNotFoundException(id, nameof(id));
    }
}