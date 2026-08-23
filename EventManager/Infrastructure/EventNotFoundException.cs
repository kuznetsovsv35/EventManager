namespace EventManager.Infrastructure;

public class EventNotFoundException : ObjectNotFoundException<Guid>
{
    internal EventNotFoundException(Guid eventId, string paramName)
        : this(eventId, paramName, null) { }

    internal EventNotFoundException(Guid eventId, string paramName, Exception? innerException)
        : base("Событие не найдено", paramName, eventId, innerException) { }
}