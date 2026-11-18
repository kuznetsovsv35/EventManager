using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Common.Kafka.Services;

public class EventPublisher(IEventPublisherConfig config, ILogger<EventPublisher> logger) : IEventPublisher
{
    readonly IProducer<string, string> _producer = new ProducerBuilder<string, string>(new ProducerConfig
    {
        BootstrapServers = config.BootstrapServers,
        Acks = Acks.All,
    }).Build();
    
    public async Task PublishAsync<T>(T message, Func<string> keyFactory, CancellationToken cancellation)
    {
        var json = JsonSerializer.Serialize(message);
        var msg = new Message<string, string>
        {
            Key = keyFactory(),
            Value = json,
        };

        var result = await _producer.ProduceAsync(config.Topic, msg, cancellation);
        
        logger.LogInformation(
            "Event {E} published: partition={P}, offset={O}",
            nameof(T), result.Partition, result.Offset);
    }
}
