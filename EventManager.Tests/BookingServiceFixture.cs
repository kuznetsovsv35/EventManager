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

public class BookingServiceFixture  : TestAppDbContext
{
    public IBookingService BookingService { get; }
    public IAsyncQueue<Booking> BookingQueue { get; }
    public IHostedService BackgroudService { get; }

    public async Task<Guid> GetRandomEventId(CancellationToken cancellation)
    {
        int eventCount = await Events.CountAsync<Event>(cancellation);
        var eventIndex = Random.Shared.Next(eventCount);
        return (await Events.Skip(eventIndex).FirstAsync(cancellation)).Id;        
    }

    public BookingServiceFixture() : base(true)
    {
        BookingQueue = new AsyncQueue<Booking>();
        BookingService = new BookingService(this, BookingQueue);

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(x => x.GetService(typeof(IAppDbContext))!).Returns(new TestAppDbContext(false));

        var mockServiceScope = new Mock<IServiceScope>();
        mockServiceScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);

        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockServiceScope.Object);

        var logFactory = LoggerFactory.Create(builder => builder.AddConsole());

        BackgroudService = new AppBackgroundService(mockScopeFactory.Object, BookingQueue, logFactory.CreateLogger<AppBackgroundService>());
    }
}