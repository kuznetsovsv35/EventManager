using Common.Contracts.Kafka;
using Common.Kafka.DI;
using EventsService.Kafka.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventsService.Kafka.DI;

public static class DependencyInjection
{
    public static IServiceCollection ConfigureKafka(this IServiceCollection services, IConfiguration config)
    {
        services.AddEventConsumer<BookingActionMessage, BookingActionSubscriber>(config, config =>
        {
            config.Topic = Topics.BookingAction;
            config.GroupId = "events-service";
        });

        return services;
    }
}
