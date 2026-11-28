namespace Common.Kafka;

public interface IEventSubscriber<T>
{
    Task HandleMessage(T message, CancellationToken cancellation);
}
