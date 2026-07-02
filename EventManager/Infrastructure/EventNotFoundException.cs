namespace EventManager.Infrastructure;

public class EventNotFoundException : ArgumentException
{
    public Guid EventId { get; }
    
    public EventNotFoundException(string paramName, Guid eventId)
        : this(paramName, eventId, null) {}

    public EventNotFoundException(string paramName, Guid eventId, Exception? innerException)
        : base("Событие не найден", paramName, innerException)
    {
        EventId = eventId;
    }
}