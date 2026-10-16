using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using EventManager.Domain.ValueObjects;
using EventManager.Common.Interfaces;
using EventManager.Common.Services;
using EventManager.Application.Interfaces;
using EventManager.Application.Services;
using EventManager.Infrastructure.Services;
using EventManager.Infrastructure.Repositories;
using EventManager.Infrastructure.Background;
using EventManager.Application.Authorization;
using EventManager.Application.Cryptography;

namespace EventManager.Tests;

/// <summary>
/// Контекст методов теста.
/// </summary>
public class EventManagerTestContext
{
    readonly IServiceCollection _services;

    public EventManagerTestContext()
    {
        var currentUser = new Mock<ICurrentUser>();

        _services = new ServiceCollection()
            .AddSingleton<IBookingServiceNotifier, BookingServiceNotifier>()
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
            .AddSingleton<ISyncContextFactory, SyncContextFactory>()
            .AddScoped(provider =>
            {
                var mock = new Mock<IAppAuthorizationService>();
                mock.SetupGet(x => x.CurrentUser).Returns(currentUser.Object);
                return mock.Object;
            })
            .AddScoped<IUserRepository, UserRepository>()
            .AddScoped<IUserService, UserService>()
            .AddSingleton<IPasswordHasher, PasswordHasher>();            
    }

    public IServiceProvider CreateServiceProvider() => _services.BuildServiceProvider();
}
