using EventManager.Domain.ValueObjects;

namespace EventManager.Domain.Exceptions;

/// <summary>
/// Исключение - нет свободных мест на событии.
/// </summary>
public class NoAvailableSeatsException : InvalidEventOperationException
{
    public NoAvailableSeatsException(Event @event, string login, UserRole role,  Exception? innerException)
        : base("Нет свободных мест на данном событии", @event, login, role, innerException) { }

    public NoAvailableSeatsException(Event @event, string login, UserRole role) 
        : this(@event, login, role, null) {  }
}