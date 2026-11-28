using Authorization;
using Authorization.Exceptions;

namespace UsersService.Domain.Exceptions;

/// <summary>
/// Исключение - ошибка регистрации нового пользователя.
/// </summary>
public class UserRegisterException(ICurrentUser user, Exception? innerException) : UserOperationException("Ошибка регистрации пользователя", user, innerException)
{
    public UserRegisterException(ICurrentUser user)
        : this(user, null) { }
}
