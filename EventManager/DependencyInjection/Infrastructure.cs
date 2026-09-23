using EventManager.Common.Interfaces;
using EventManager.Common.Services;
using EventManager.Application.Interfaces;
using EventManager.Infrastructure.Services;
using EventManager.Infrastructure.Repositories;
using EventManager.Domain.ValueObjects;

namespace EventManager.DependencyInjection;

public static partial class DependencyInjection
{
    public static IServiceCollection ConfigureInfrastructure(this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.ConfigureDatabase(configuration!, environment);

        services.AddSingleton<ISyncContextFactory, SyncContextFactory>();
        services.AddSingleton<IBookingServiceNotifier, BookingServiceNotifier>();

        services.AddScoped<IPaginator<Event>, PaginateService<Event>>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddHostedService<AppBackgroundService>();

        return services;
    }

    public static Task RunInfrastructure(this IServiceProvider serviceProvider, CancellationToken cancellation)
        => serviceProvider.PrepareDatabase(cancellation);
}