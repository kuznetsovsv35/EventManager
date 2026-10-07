using EventManager.Domain.ValueObjects;
using EventManager.Domain.Exceptions;
using EventManager.Common.Interfaces;
using EventManager.Application.Interfaces;
using EventManager.Application.DataTransferObjects;
using EventManager.Application.Authorization;

namespace EventManager.Application.Services;

/// <summary>
/// Сервис управления событиями.
/// </summary>
/// <param name="dbContext"></param>
public class EventService(
    ISyncContextFactory syncContextFactory,
    ICurrentUser currentUser,
    IEventRepository repository,
    IFilter<Event> filter,
    IPaginator<Event> paginator) : AppAuthorizeService<EventService>, IEventService
{
    async Task<EventOutputData> IEventService.CreateEventAsync(EventInputData data, CancellationToken cancellation)
    {
        CheckUserRole(Policies.EventService.CreateEvent, currentUser.ToInfo(), UserRole.Admin);

        if (data == null)
            throw new ArgumentNullException(nameof(data));

        var e = data.ToEvent();
        await repository.AddEventAsync(e, cancellation);
        return e.ToOutputData();
    }

    async Task<EventOutputData> IEventService.DeleteEventAsync(Guid id, CancellationToken cancellation)
    {
        CheckUserRole(Policies.EventService.DeleteEvent, currentUser.ToInfo(), UserRole.Admin);

        if (await repository.DeleteEventAsync(id, cancellation) is Event e)
            return e.ToOutputData();

        throw new EventNotFoundException(id, nameof(id));
    }

    Task<PaginateResult<EventOutputData>> IEventService.GetEvents(FilterParams? filterParams, PageParams pageParams, CancellationToken cancellation)
    {
        CheckUserRole(Policies.EventService.GetEvents, currentUser.ToInfo(), UserRole.User);

        return paginator.PaginateAsync(
            FilterEvents(filterParams),
            pageParams.CurrentPage, pageParams.PageSize,
            e => e.ToOutputData(), cancellation);
    }

    IEnumerable<EventOutputData> IEventService.GetEvents(FilterParams? filterParams)
    {
        CheckUserRole(Policies.EventService.GetEvents, currentUser.ToInfo(), UserRole.User);

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
        CheckUserRole(Policies.EventService.GetEvents, currentUser.ToInfo(), UserRole.User);

        if (await repository.GetEventAsync(id, cancellation) is Event e)
            return e.ToOutputData();

        throw new EventNotFoundException(id, nameof(id));
    }

    async Task<EventOutputData> IEventService.UpdateEventAsync(Guid id, EventInputData data, CancellationToken cancellation)
    {
        CheckUserRole(Policies.EventService.ModifyEvent, currentUser.ToInfo(), UserRole.User);

        var e = await syncContextFactory.CreateContext<Booking>().ExecuteActionAsync(
            async () =>
            {
                if (await repository.GetEventAsync(id, cancellation) is Event e)
                {
                    data.Update(e);
                    await repository.UpdateEventAsync(e, cancellation);
                    return e;
                }
                return null;
            }, cancellation);

        return e is not null
            ? e.ToOutputData()
            : throw new EventNotFoundException(id, nameof(id));
    }
}