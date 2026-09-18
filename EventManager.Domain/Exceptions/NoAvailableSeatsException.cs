namespace EventManager.Domain.Exceptions;

/// <summary>
/// Исключение - нет свободных мест на событии.
/// </summary>
public class NoAvailableSeatsException : Exception
{
    public Guid EventId { get; }

    public NoAvailableSeatsException(Guid eventId, Exception? innerException)
        : base("Нет свободных мест на данном событии", innerException) =>  EventId = eventId;

    public NoAvailableSeatsException(Guid eventId) : this(eventId, null) { }
}