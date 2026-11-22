using Microsoft.Extensions.Configuration;

namespace Common.Kafka.Configuration;

public class EventConsumerConfig<T>(IConfiguration config) : ConfigBase(config), IEventConsumerConfig<T>
{
    public string GroupId { get; set; } = "default-group";
    public string Topic { get; set; } = "default-topic";
}
