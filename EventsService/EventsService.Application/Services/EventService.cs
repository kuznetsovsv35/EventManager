using Authorization;
using Common.Data;
using EventsService.Application.DTO;
using EventsService.Application.Security;
using EventsService.Domain;
using EventsService.Domain.Exceptions;

namespace EventsService.Application.Services;

/// <summary>
/// Сервис управления событиями.
/// </summary>
/// <param name="dbContext"></param>
public class EventService(
    IAppAuthorizationService appAuthorization,
    IEventRepository repository,
    IFilter<Event> filter,
    IPaginator<Event> paginator) : IEventService
{
    async Task<EventOutputData> IEventService.CreateEventAsync(EventInputData data, CancellationToken cancellation)
    {
        await appAuthorization.AuthorizeAsync(this, Policy.CreateEvent, cancellation);

        if (data == null)
            throw new ArgumentNullException(nameof(data));

        var e = data.ToEvent();
        await repository.AddEventAsync(e, cancellation);
        return e.ToOutputData();
    }

    async Task<EventOutputData> IEventService.DeleteEventAsync(Guid id, CancellationToken cancellation)
    {
        await appAuthorization.AuthorizeAsync(this, Policy.DeleteEvent, cancellation);

        if (await repository.DeleteEventAsync(id, cancellation) is Event e)
            return e.ToOutputData();

        throw new EventNotFoundException(id, nameof(id));
    }

    async Task<PaginateResult<EventOutputData>> IEventService.GetEvents(FilterParams? filterParams, PageParams pageParams, CancellationToken cancellation)
    {
        await appAuthorization.AuthorizeAsync(this, Policy.GetEvents, cancellation);

        return await paginator.PaginateAsync(
            FilterEvents(filterParams),
            pageParams.CurrentPage, pageParams.PageSize,
            e => e.ToOutputData(), cancellation);
    }

    async Task<IEnumerable<EventOutputData>> IEventService.GetEvents(FilterParams? filterParams)
    {
        await appAuthorization.AuthorizeAsync(this, Policy.GetEvents, CancellationToken.None);

        return FilterEvents(filterParams)
            .Select(e => e.ToOutputData())
            .AsEnumerable();
    }

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

        return repository.GetEvents(f.Expression);
    }

    async Task<EventOutputData> IEventService.GetEventAsync(Guid id, CancellationToken cancellation)
    {
        await appAuthorization.AuthorizeAsync(this, Policy.GetEvents, cancellation);

        if (await repository.GetEventAsync(id, cancellation) is Event e)
            return e.ToOutputData();

        throw new EventNotFoundException(id, nameof(id));
    }

    async Task<EventOutputData> IEventService.UpdateEventAsync(Guid id, EventInputData data, CancellationToken cancellation)
    {
        await appAuthorization.AuthorizeAsync(this, Policy.ModifyEvent, cancellation);

        if (await repository.GetEventAsync(id, cancellation) is Event e)
        {
            data.Update(e);
            await repository.UpdateEventAsync(e, cancellation);
            return e.ToOutputData();
        }
        throw new EventNotFoundException(id, nameof(id));
    }
}