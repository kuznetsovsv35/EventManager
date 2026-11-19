namespace Common.Kafka;

public interface IEventPublisher
{
    Task PublishAsync<T>(string topic, T message, Func<string> keyFactory, CancellationToken cancellation);
    Task PublishAsync<T>(T message, Func<string> keyFactory, CancellationToken cancellation);
}
