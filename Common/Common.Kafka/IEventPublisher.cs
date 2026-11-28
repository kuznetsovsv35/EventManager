namespace Common.Kafka;

public interface IEventPublisher
{
    Task PublishAsync<T>(T message, Func<string> keyFactory, CancellationToken cancellation);
}
