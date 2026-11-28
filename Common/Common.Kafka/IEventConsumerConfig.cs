namespace Common.Kafka;

public interface IEventConsumerConfig
{
    string BootstrapServers { get; }
    string Topic { get; }
    string ConsumerGroup { get;}
}

public interface IEventConsumerConfig<T> : IEventConsumerConfig;
