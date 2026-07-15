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

    public IAppDbContext DbContext => _dbContext;

    public T GetService<T>() where T: notnull => ServiceProvider.GetRequiredService<T>();

    public IAppBackgroundService BackgroundService { get; }

    public async Task<Guid> GetRandomEventId(CancellationToken cancellation)
    {
        int eventCount = await DbContext.Events.CountAsync<Event>(cancellation);
        var eventIndex = Random.Shared.Next(eventCount);
        return (await DbContext.Events.Skip(eventIndex).FirstAsync(cancellation)).Id;        
    }
    readonly TestAppDbContext _dbContext =  new($"Test_{Guid.NewGuid()}");

    internal BookingServiceTestContext()
    {
        ServiceProvider = new ServiceCollection()
            .AddSingleton<IAsyncQueue<Booking>, AsyncQueue<Booking>>()
            .AddScoped(_ => _dbContext.CreateNewInstance())
            .AddScoped<IBookingService>(provider =>
            {
                return new BookingService(
                    provider.GetRequiredService<IAppDbContext>(), 
                    provider.GetRequiredService<IAsyncQueue<Booking>>());
            })
            .BuildServiceProvider();

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(x => x.GetService(typeof(IAppDbContext))!).Returns(_dbContext.CreateNewInstance());

        var mockServiceScope = new Mock<IServiceScope>();
        mockServiceScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);

        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockServiceScope.Object);

        var logFactory = LoggerFactory.Create(builder => builder.AddConsole());

        BackgroundService = new AppBackgroundService(
            mockScopeFactory.Object, 
            ServiceProvider.GetRequiredService<IAsyncQueue<Booking>>(), 
            logFactory.CreateLogger<AppBackgroundService>());
    }
}
