using Microsoft.Extensions.Configuration;

namespace Common.Kafka.Configuration;

public class EventPublisherConfig(IConfiguration config) : ConfigBase(config), IEventPublisherConfig
{
    public string? Topic { get; set; }
}
