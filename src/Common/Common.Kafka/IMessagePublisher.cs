namespace Common.Kafka;

public interface IMessagePublisher
{
    Task PublishAsync<T>(string topic, T message, CancellationToken cancellation);
}
