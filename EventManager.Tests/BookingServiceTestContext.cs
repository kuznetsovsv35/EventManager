using EventManager.Application.Interfaces;
using EventManager.Application.Services;
using EventManager.Infrastructure;
using EventManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;

namespace EventManager.Tests;

class BookingServiceTestContext
{
    public IAppDbContext DbContext => _dbContext;

    public IAsyncQueue<Booking> BookingQueue { get; }
    
    public IBookingService BookingService => new BookingService(_dbContext.CreateNewInstance(), BookingQueue);

    public IHostedService BackgroundService { get; }

    public async Task<Guid> GetRandomEventId(CancellationToken cancellation)
    {
        int eventCount = await DbContext.Events.CountAsync<Event>(cancellation);
        var eventIndex = Random.Shared.Next(eventCount);
        return (await DbContext.Events.Skip(eventIndex).FirstAsync(cancellation)).Id;        
    }
    readonly TestAppDbContext _dbContext =  new($"Test_{Guid.NewGuid()}");

    internal BookingServiceTestContext()
    {
        BookingQueue = new AsyncQueue<Booking>();        

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(x => x.GetService(typeof(IAppDbContext))!).Returns(_dbContext.CreateNewInstance());

        var mockServiceScope = new Mock<IServiceScope>();
        mockServiceScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);

        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockServiceScope.Object);

        var logFactory = LoggerFactory.Create(builder => builder.AddConsole());

        BackgroundService = new AppBackgroundService(mockScopeFactory.Object, BookingQueue, logFactory.CreateLogger<AppBackgroundService>());
    }
}
