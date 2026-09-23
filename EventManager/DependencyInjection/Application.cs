using EventManager.Common.Interfaces;
using EventManager.Common.Services;
using EventManager.Domain.ValueObjects;
using EventManager.Application.Interfaces;
using EventManager.Application.Services;
using EventManager.Infrastructure.Services;
using EventManager.Infrastructure.Repositories;

namespace EventManager.DependencyInjection;

public static partial class DependencyInjection
{
    public static IServiceCollection ConfigureApplication(this IServiceCollection services)
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
    
}