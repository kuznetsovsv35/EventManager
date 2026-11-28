using Authorization;
using Authorization.Exceptions;

namespace BookingService.Domain.Exceptions;

/// <summary>
/// Исключение - ошибка превышения максимального числа активных броней.
/// </summary>
public class ActiveUserBookingException(ICurrentUser user, Exception? innerException) : UserOperationException("Превышение максимального числа активных броней для одного пользователя", user, innerException)
{
    public ActiveUserBookingException(ICurrentUser user)
        : this(user, null) { }
}