using Authorization;
using Authorization.Exceptions;

namespace UsersService.Domain.Exceptions;

/// <summary>
/// Исключение - ошибка регистрации нового пользователя.
/// </summary>
public class UserRegisterException : UserOperationException
{
    public UserRegisterException(ICurrentUser user)
        : this(user, null) { }

    public UserRegisterException(ICurrentUser user, Exception? innerException)
        : base("Ошибка регистрации пользователя", user, innerException) { }
}
