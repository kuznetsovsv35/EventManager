namespace Common.Kafka;

public interface IEventConsumerConfig
{
    string BootstrapServers { get; }
    string GroupId { get; }
    string Topic { get; }
}

public interface IEventConsumerConfig<T> : IEventConsumerConfig { }
