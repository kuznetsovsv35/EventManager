using EventManager.Domain.ValueObjects;

namespace EventManager.Domain.Exceptions;

/// <summary>
/// Исключение - бронь на уже начавшееся событие.
/// </summary>
public class StartedEventBookingException : InvalidEventOperationException
{    
    public StartedEventBookingException(Event @event, string login, UserRole role) 
        : this(@event, login, role,  null) { }

    public StartedEventBookingException(Event @event, string login, UserRole role, Exception? innerException)
        : base("Событие уже началось, нельзя создать бронь", @event, login, role, innerException) { }
}