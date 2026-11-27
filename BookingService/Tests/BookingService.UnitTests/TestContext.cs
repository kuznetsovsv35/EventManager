using Authorization;
using BookingService.Application;
using BookingService.Application.Services;
using BookingService.Database.Repositories;
using BookingService.Domain;
using BookingService.Infrastructure;
using BookingService.Infrastructure.Services;
using Common.Services;
using Common.Synchronization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace BookingService.UnitTests;

/// <summary>
/// Контекст методов теста.
/// </summary>
public class TestContext
{
    readonly IServiceCollection _services;

    public TestContext()
    {
        var currentUser = new Mock<ICurrentUser>();

        var publisher = new Mock<IEventsServiceNotifier>();

        publisher
            .Setup(x => x.BookingCancelledAsync(It.IsAny<Booking>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(() => Task.CompletedTask);

        _services = new ServiceCollection()
            .AddSingleton<IBookingServiceNotifier, BookingServiceNotifier>()
            .AddSingleton(_ => new TestDbContext($"Test_{Guid.NewGuid()}"))
            .AddScoped(provider => provider.GetRequiredService<TestDbContext>().CreateNewInstance())
            .AddScoped<IBookingRepository, BookingRepository>()
            .AddScoped<IBookingService, Application.Services.BookingService>()
            .AddSingleton(_ => LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<AppBackgroundService>())
            .AddSingleton(provider =>
            {
                var mock = new Mock<IServiceScopeFactory>();
                mock.Setup(x => x.CreateScope()).Returns(provider.CreateScope());
                return mock.Object;
            })
            .AddSingleton<IAppBackgroundService, AppBackgroundService>()
            .AddSingleton<ISyncContextFactory, SyncContextFactory>()
            .AddScoped(provider =>
            {
                var mock = new Mock<IAppAuthorizationService>();
                mock.SetupGet(x => x.CurrentUser).Returns(currentUser.Object);
                return mock.Object;
            })
            .AddSingleton(_ => publisher.Object);
    }

    public IServiceProvider CreateServiceProvider() => _services.BuildServiceProvider();
}
