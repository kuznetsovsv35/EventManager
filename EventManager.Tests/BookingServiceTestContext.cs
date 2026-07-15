using EventManager.Application.Interfaces;
using EventManager.Application.Services;
using EventManager.Infrastructure;
using EventManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace EventManager.Tests;

class BookingServiceTestContext
{
    public IServiceProvider ServiceProvider { get; }

    public IServiceScope CreateScope() => ServiceProvider.CreateScope();
    public async Task<Guid> GetRandomEventId(CancellationToken cancellation)
    {
        using var scope = ServiceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        int eventCount = await dbContext.Events.CountAsync<Event>(cancellation);
        var eventIndex = Random.Shared.Next(eventCount);
        return (await dbContext.Events.Skip(eventIndex).FirstAsync(cancellation)).Id;
    }
    readonly TestAppDbContext _dbContext = new($"Test_{Guid.NewGuid()}");

    internal BookingServiceTestContext()
    {
        ServiceProvider = new ServiceCollection()
            .AddSingleton<IAsyncQueue<Booking>, AsyncQueue<Booking>>()
            .AddScoped(_ => _dbContext.CreateNewInstance())
            .AddScoped<IBookingService, BookingService>()
            .AddSingleton(_ => LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<AppBackgroundService>())
            .AddSingleton(provider =>
            {
                var mock = new Mock<IServiceScopeFactory>();
                mock.Setup(x => x.CreateScope()).Returns(provider.CreateScope());
                return mock.Object;
            })
            .AddSingleton<IAppBackgroundService, AppBackgroundService>()
            .BuildServiceProvider();
    }
}
