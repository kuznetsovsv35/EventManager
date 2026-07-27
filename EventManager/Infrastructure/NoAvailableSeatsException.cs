namespace EventManager.Infrastructure;

public class NoAvailableSeatsException : Exception
{
    public Guid EventId { get; }

    internal NoAvailableSeatsException(Guid eventId, Exception? innerException)
        : base("Нет свободных мест на данном событии", innerException)
    {
        EventId = eventId;
    }

    internal NoAvailableSeatsException(Guid eventId)
        : this(eventId, null) { }
}