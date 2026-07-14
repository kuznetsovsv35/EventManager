using EventManager.Application.Interfaces;
using EventManager.Application.Services;
using EventManager.Infrastructure;
using EventManager.Models;
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

    public BookingServiceFixture() : base()
    {
        BookingQueue = new AsyncQueue<Booking>();
        BookingService = new BookingService(this, BookingQueue);

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(x => x.GetService(typeof(IAppDbContext))!).Returns(this);

        var mockServiceScope = new Mock<IServiceScope>();
        mockServiceScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);

        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockServiceScope.Object);

        var logFactory = LoggerFactory.Create(builder => builder.AddConsole());

        BackgroudService = new AppBackgroundService(mockScopeFactory.Object, BookingQueue, logFactory.CreateLogger<AppBackgroundService>());
    }
}