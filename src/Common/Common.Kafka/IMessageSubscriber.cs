namespace Common.Kafka;

public interface IMessageSubscriber<T>
{
    Task HandleMessage(T message, CancellationToken cancellation);
}
