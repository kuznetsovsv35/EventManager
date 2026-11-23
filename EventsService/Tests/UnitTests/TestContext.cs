using System.Linq.Expressions;
using Authorization;
using Common.Data;
using Common.Infrastructure.Services;
using Common.Services;
using Common.Synchronization;
using EventsService.Application;
using EventsService.Application.Services;
using EventsService.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace UnitTests;

/// <summary>
/// Контекст методов теста.
/// </summary>
public class TestContext
{
    internal const int EventCount = 30;

    internal const int EventDuration = 45;

    internal const int MinTotalSeats = 2;

    internal const int MaxTotalSeats = 100;

    readonly IServiceCollection _services;

    public TestContext()
    {
        var currentUser = new Mock<ICurrentUser>();

        _services = new ServiceCollection()
            .AddScoped(_ => new EventRepository(Events).Object)
            .AddScoped<IFilter<Event>, FilterService<Event>>()
            .AddScoped<IPaginator<Event>, PaginateService<Event>>()
            .AddScoped<IEventService, EventService>()
            .AddSingleton(_ => LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<TestContext>())
            .AddSingleton(provider =>
            {
                var mock = new Mock<IServiceScopeFactory>();
                mock.Setup(x => x.CreateScope()).Returns(provider.CreateScope());
                return mock.Object;
            })
            .AddSingleton<ISyncContextFactory, SyncContextFactory>()
            .AddScoped(provider =>
            {
                var mock = new Mock<IAppAuthorizationService>();
                mock.SetupGet(x => x.CurrentUser).Returns(currentUser.Object);
                return mock.Object;
            });
    }

    public IServiceProvider CreateServiceProvider() => _services.BuildServiceProvider();

    public List<Event> Events {get;} = [];

    public Guid GetRandomEventId()
    {
        int eventCount = Events.Count;
        var eventIndex = Random.Shared.Next(eventCount);
        return Events.Skip(eventIndex).First().Id;
    }

    class EventRepository : Mock<IEventRepository>
    {
        readonly List<Event> _events;
        internal EventRepository(List<Event> events) 
        {
            _events = events;

            Setup(x => x.GetEvents(It.IsAny<Expression<Func<Event, bool>>?>()))
                .Returns<Expression<Func<Event, bool>>>(filter =>
                {
                    if (filter is null)
                        return _events.AsQueryable();
                    return _events.AsQueryable().Where(filter);
                });
            
            Setup(x =>x.GetEventAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns<Guid, CancellationToken>((id, _) =>
                {
                    var e = _events.Find(x => x.Id == id);
                    return Task.FromResult(e);
                });
            
            Setup(x => x.AddEventAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()))
                .Returns<Event, CancellationToken>((e, _) =>
                {
                    _events.Add(e);
                    return Task.FromResult(e);
                });
            
            Setup(x => x.UpdateEventAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()))
                .Returns<Event, CancellationToken>((e, _) =>
                {
                    if (_events.Find(x => x.Id == e.Id) is Event dest)
                    {
                        _events.Remove(dest);
                        _events.Add(e);
                        return Task.FromResult<Event?>(e);
                    }
                    return Task.FromResult<Event?>(null);
                });

            Setup(x => x.DeleteEventAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns<Guid, CancellationToken>((id, _) =>
                {
                    if (_events.Find(x => x.Id == id) is Event dest)
                    {
                        _events.Remove(dest);
                        return Task.FromResult<Event?>(dest);
                    }
                    return Task.FromResult<Event?>(null);
                });
            
            //Task ExecuteEventBookingAsync(Guid bookingId, Guid EventId, Func<EventBooking, bool> action, CancellationToken cancellation);
        }
    }
}
