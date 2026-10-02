using EventManager.Domain.ValueObjects;

namespace EventManager.Domain.Exceptions;

public class InvalidEventOperationException : UserOperationException
{
    public Guid EventId { get; }

    public InvalidEventOperationException(string message, Event @event, string login, UserRole role)
        : this(message, @event, login, role, null) { }

    public InvalidEventOperationException(string message, Event @event, string login, UserRole role, Exception? innerExceptION)
        : base(message, login, role, innerExceptION)
    {
        EventId = @event.Id;
    }
}