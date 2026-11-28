using Microsoft.Extensions.Configuration;

namespace Common.Kafka.Configuration;

public class EventConsumerConfig<T>(IConfiguration config) : ConfigBase(config), IEventConsumerConfig<T>
{
    static readonly string _groupKey = $"{nameof(Kafka)}:{nameof(ConsumerGroup)}"; 

    public string ConsumerGroup { get; } = config[_groupKey] ?? throw new InvalidOperationException($"{_groupKey} is required");
}
