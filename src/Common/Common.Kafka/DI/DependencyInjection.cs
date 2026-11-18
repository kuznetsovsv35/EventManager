using Common.Kafka.Configuration;
using Common.Kafka.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Kafka.DI;

public static class DependencyInjection
{
    public static IServiceCollection AddEventConsumer<TContract, TSubscriber>(
        this IServiceCollection services,
        IConfiguration config, 
        Action<EventConsumerConfig<TContract>> builder) where TSubscriber : class, IEventSubscriber<TContract>
    {
        var consumerConfig = new EventConsumerConfig<TContract>(config);
        builder(consumerConfig);

        services.AddSingleton<IEventConsumerConfig<TContract>>(_ => consumerConfig);
        services.AddSingleton<IEventSubscriber<TContract>, TSubscriber>();
        services.AddHostedService<EventConsumer<TContract>>();

        return services;
    }

    public static IServiceCollection AddEventPublisher(this IServiceCollection services)
    {
        return services;
    }
}
