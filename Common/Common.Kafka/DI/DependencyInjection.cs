using Common.Kafka.Configuration;
using Common.Kafka.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Common.Kafka.DI;

public static class DependencyInjection
{
    public static IServiceCollection AddEventConsumer<TContract, TSubscriber>(
        this IServiceCollection services,
        IConfiguration config, 
        Action<EventConsumerConfig<TContract>>? builder = null) where TSubscriber : class, IEventSubscriber<TContract>
    {
        var consumer = new EventConsumerConfig<TContract>(config);
        builder?.Invoke(consumer);

        services.AddSingleton<IEventConsumerConfig<TContract>>(_ => consumer);
        services.AddSingleton<IEventSubscriber<TContract>, TSubscriber>();
        services.AddHostedService<EventConsumer<TContract>>();

        return services;
    }

    public static IServiceCollection AddEventPublisher(
        this IServiceCollection services, 
        IConfiguration config, 
        Action<EventPublisherConfig>? builder = null)
    {
        var publisher = new EventPublisherConfig(config);
        builder?.Invoke(publisher);

        if (string.IsNullOrWhiteSpace(publisher.Topic))
        {
            services.AddSingleton<IEventPublisherConfig>(publisher);
            services.AddSingleton<IEventPublisher, EventPublisher>();
        }
        else
        {
            services.AddKeyedSingleton<IEventPublisherConfig>(publisher.Topic, publisher);
            services.AddKeyedSingleton<IEventPublisher>(publisher.Topic, (provider, topic) =>
            {
                var config = provider.GetRequiredKeyedService<IEventPublisherConfig>(topic);
                var logger = provider.GetRequiredService<ILogger<EventPublisher>>();
                return new EventPublisher(config, logger);
            });
        }
        
        return services;
    }
}
