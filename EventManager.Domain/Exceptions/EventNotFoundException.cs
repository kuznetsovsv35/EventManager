namespace EventManager.Domain.Exceptions;

/// <summary>
/// Исключение - событие по идентификатору не найдено.
/// </summary>
public class EventNotFoundException : ObjectNotFoundException<Guid>
{
    public EventNotFoundException(Guid eventId, string paramName)
        : this(eventId, paramName, null) { }

    public EventNotFoundException(Guid eventId, string paramName, Exception? innerException)
        : base("Событие не найдено", paramName, eventId, innerException) { }
}