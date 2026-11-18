namespace Common.Kafka;

public interface IEventPublisherConfig
{
    string BootstrapServers { get; }
    string Topic { get; }
}
