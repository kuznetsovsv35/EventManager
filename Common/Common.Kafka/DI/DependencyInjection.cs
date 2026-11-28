using Common.Kafka.Configuration;
using Common.Kafka.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Kafka.DI;

public static class DependencyInjection
{
    public static IServiceCollection AddEventConsumer<TContract, TSubscriber>(
        this IServiceCollection services,
        IConfiguration config) where TSubscriber : class, IEventSubscriber<TContract>
    {
        var consumer = new EventConsumerConfig<TContract>(config);

        services.AddSingleton<IEventConsumerConfig<TContract>>(_ => consumer);
        services.AddSingleton<IEventSubscriber<TContract>, TSubscriber>();
        services.AddHostedService<EventConsumer<TContract>>();

        return services;
    }

    public static IServiceCollection AddEventPublisher(
        this IServiceCollection services,
        IConfiguration config)
    {
        var publisher = new EventPublisherConfig(config);

        return services
            .AddSingleton<IEventPublisherConfig>(publisher)
            .AddSingleton<IEventPublisher, EventPublisher>();
    }
}
