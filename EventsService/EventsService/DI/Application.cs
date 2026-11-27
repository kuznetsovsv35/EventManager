using Common.Data;
using Common.Services;
using EventsService.Application;
using EventsService.Application.Services;
using EventsService.Domain;

namespace EventsService.DI;

public static partial class DependencyInjection
{
    public static IServiceCollection ConfigureApplication(this IServiceCollection services)
    {
        services.AddScoped<IFilter<Event>, FilterService<Event>>();
        services.AddScoped<IEventService, EventService>();
        return services;
    }
}
