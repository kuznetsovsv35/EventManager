using EventManager.Domain.ValueObjects;

namespace EventManager.Domain.Exceptions;


public class ActiveUserBookingException : UserOperationException
{
    public ActiveUserBookingException(string login, UserRole role)
        : this(login, role, null) {}

    public ActiveUserBookingException(string login, UserRole role, Exception? innerException)
        : base("Превышение максимального числа активных броней для одного пользователя", login, role, innerException) {}
}