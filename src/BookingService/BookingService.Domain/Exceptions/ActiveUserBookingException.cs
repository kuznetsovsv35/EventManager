using Authorization;
using Authorization.Exceptions;

namespace BookingService.Domain.Exceptions;

/// <summary>
/// Исключение - ошибка превышения максимального числа активных броней.
/// </summary>
public class ActiveUserBookingException : UserOperationException
{
    public ActiveUserBookingException(ICurrentUser user)
        : this(user, null) { }

    public ActiveUserBookingException(ICurrentUser user, Exception? innerException)
        : base("Превышение максимального числа активных броней для одного пользователя", user, innerException) { }
}