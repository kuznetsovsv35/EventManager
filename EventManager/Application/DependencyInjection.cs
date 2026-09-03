using EventManager.Application.Interfaces;
using EventManager.Application.Services;
using EventManager.Data;
using EventManager.Infrastructure;
using EventManager.Models;

namespace EventManager.Application;

/// <summary>
/// Внедрение зависимости функциональности приложения.
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
        services.AddSingleton<IAsyncQueue<Guid>, AsyncQueue<Guid>>();
        services.AddHostedService<AppBackgroundService>();
        return services;
    }
}