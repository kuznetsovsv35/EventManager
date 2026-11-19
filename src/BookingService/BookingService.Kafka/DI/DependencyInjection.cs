using BookingService.Application;
using BookingService.Kafka.Services;
using Common.Kafka.DI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookingService.Kafka.DI;

public static class DependencyInjection
{
    public static IServiceCollection ConfigureKafka(this IServiceCollection services, IConfiguration config)
    {
        services.AddEventPublisher(config)
        services.AddSingleton<IEventsServiceNotifier, EventsServiceNotifier>();
        return services;
    }
}
