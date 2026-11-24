using Authorization;
using Common.Data;
using Common.Infrastructure.Services;
using Common.Services;
using Common.Synchronization;
using EventsService.Application;
using EventsService.Application.Services;
using EventsService.Database.Repositories;
using EventsService.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace EventsService.UnitTests;

/// <summary>
/// Контекст методов теста.
/// </summary>
public class TestContext
{
    readonly IServiceCollection _services;

    public TestContext()
    {
        var currentUser = new Mock<ICurrentUser>();

        _services = new ServiceCollection()
            .AddSingleton(_ => new TestDbContext($"Test_{Guid.NewGuid()}"))
            .AddScoped(provider => provider.GetRequiredService<TestDbContext>().CreateNewInstance())
            .AddScoped<IEventRepository, EventRepository>()
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
}
