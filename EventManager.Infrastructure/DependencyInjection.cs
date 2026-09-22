using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using EventManager.Domain.ValueObjects;
using EventManager.Common.Interfaces;
using EventManager.Common.Services;
using EventManager.Application.Interfaces;
using EventManager.Application.Services;
using EventManager.Infrastructure.Services;
using EventManager.Infrastructure.Repositories;
using EventManager.Database;

namespace EventManager.Infrastructure;

/// <summary>
/// Внедрение зависимости инфраструктуры приложения.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IFilter<Event>, FilterService<Event>>();
        services.AddScoped<IPaginator<Event>, PaginateService<Event>>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();        
        services.AddHostedService<AppBackgroundService>();
        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, 
        IConfiguration configuration,
        bool isDevelopment)
    {
        services.AddDatabase(
            configuration.GetConnectionString("Default")!, 
            isDevelopment);
        
        services.AddSingleton<ISyncContextFactory, SyncContextFactory>();
        services.AddSingleton<IBookingServiceNotifier, BookingServiceNotifier>();
        
        return services;
    }

    public static Task PrepareInfrastructure(this IServiceProvider serviceProvider, CancellationToken cancellation)
        => serviceProvider.PrepareDatabase(cancellation);
}