namespace EventsService.Domain.Exceptions;

/// <summary>
/// Исключение - неверная операция над событиями.
/// </summary>
public class InvalidEventOperationException(string message, Guid eventId, Exception? innerException)
    : InvalidOperationException($"{message} (Id = {eventId})", innerException)
{
    public InvalidEventOperationException(string message, Guid eventId)
        : this(message, eventId, null) { }
}