using EventManager.Common.Interfaces;
using EventManager.Common.Services;
using EventManager.Domain.ValueObjects;
using EventManager.Application.Interfaces;
using EventManager.Application.Services;

namespace EventManager.DependencyInjection;

public static partial class DependencyInjection
{
    public static IServiceCollection ConfigureApplication(this IServiceCollection services)
    {
        services.AddScoped<IFilter<Event>, FilterService<Event>>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IBookingService, BookingService>();
        return services;
    }
    
}