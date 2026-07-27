namespace EventManager.Infrastructure;

public class EventNotFoundException : ObjectNotFoundException<Guid>
{
    internal EventNotFoundException(string paramName, Guid eventId)
        : this(paramName, eventId, null) { }

    internal EventNotFoundException(string paramName, Guid eventId, Exception? innerException)
        : base("Событие не найдено", paramName, eventId, innerException) { }
}