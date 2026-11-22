using Common.Exceptions;

namespace EventsService.Domain.Exceptions;

/// <summary>
/// Исключение - событие по идентификатору не найдено.
/// </summary>
public class EventNotFoundException(Guid eventId, string paramName, Exception? innerException) 
    : ObjectNotFoundException<Guid>("Событие не найдено", paramName, eventId, innerException)
{
    public EventNotFoundException(Guid eventId, string paramName)
        : this(eventId, paramName, null) { }
}