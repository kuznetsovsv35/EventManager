using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Common.Kafka.Services;

public class EventPublisher(IEventPublisherConfig config, ILogger<EventPublisher> logger) : IEventPublisher, IDisposable
{
    readonly IProducer<string, string> _producer = new ProducerBuilder<string, string>(new ProducerConfig
    {
        BootstrapServers = config.BootstrapServers,
        Acks = Acks.All,
    }).Build();
    bool _disposed = false;

    public void Dispose()
    {
        if (_disposed)
            return;

        _producer.Dispose();
        _disposed = true;
    }

    public async Task PublishAsync<T>(string topic, T message, Func<string> keyFactory, CancellationToken cancellation)
    {
        if (_disposed)
            throw new ObjectDisposedException(_producer?.GetType().Name);

        var json = JsonSerializer.Serialize(message);
        var msg = new Message<string, string>
        {
            Key = keyFactory(),
            Value = json,
        };

        var result = await _producer.ProduceAsync(topic, msg, cancellation);

        logger.LogInformation(
            "Event {E} published: partition={P}, offset={O}",
            typeof(T).Name, result.Partition, result.Offset);
    }

    public Task PublishAsync<T>(T message, Func<string> keyFactory, CancellationToken cancellation)
    {
        if (string.IsNullOrWhiteSpace(config.Topic))
            throw new InvalidOperationException("Invalid destination topic");

        return PublishAsync(config.Topic, message, keyFactory, cancellation);
    }
}
