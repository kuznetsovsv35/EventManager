using System.Threading.Channels;
using EventManager.Application.Interfaces;
using EventManager.Application.Services;
using EventManager.Data;
using EventManager.Infrastructure;
using EventManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace EventManager.Tests;

/// <summary>
/// Контекст методов теста.
/// </summary>
public class EventManagerTestContext
{
    public IServiceProvider ServiceProvider { get; }

    public async Task<Guid> GetRandomEventId(CancellationToken cancellation)
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        int eventCount = await dbContext.GetEvents().CountAsync<Event>(cancellation);
        var eventIndex = Random.Shared.Next(eventCount);
        return (await dbContext.GetEvents().Skip(eventIndex).FirstAsync(cancellation)).Id;
    }
    readonly IServiceCollection _services;

    public EventManagerTestContext()
    {
        _services = new ServiceCollection()
            .AddSingleton(_ => Channel.CreateBounded<Guid>(new BoundedChannelOptions(1)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleWriter = false,
                SingleReader = true
            }))
            .AddSingleton(_ => new TestAppDbContext($"Test_{Guid.NewGuid()}"))
            .AddScoped(provider => provider.GetRequiredService<TestAppDbContext>().CreateNewInstance())
            .AddScoped<IEventRepository, EventRepository>()
            .AddScoped<IBookingRepository, BookingRepository>()
            .AddScoped<IFilter<Event>, FilterService<Event>>()
            .AddScoped<IPaginator<Event>, PaginateService<Event>>()
            .AddScoped<IEventService, EventService>()
            .AddScoped<IBookingService, BookingService>()
            .AddSingleton(_ => LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<AppBackgroundService>())
            .AddSingleton(provider =>
            {
                var mock = new Mock<IServiceScopeFactory>();
                mock.Setup(x => x.CreateScope()).Returns(provider.CreateScope());
                return mock.Object;
            })
            .AddSingleton<IAppBackgroundService, AppBackgroundService>()
            .AddSingleton<ISyncContextFactory, SyncContextFactory>();
        ServiceProvider = CreateServiceProvider();
    }

    internal IServiceProvider CreateServiceProvider() => _services.BuildServiceProvider();
    internal AsyncServiceScope CreateAsyncScope() => ServiceProvider.CreateAsyncScope();
}
