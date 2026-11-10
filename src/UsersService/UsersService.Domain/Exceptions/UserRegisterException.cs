using Authorization.Domain;
using Authorization.Exceptions;

namespace UsersService.Domain.Exceptions;

/// <summary>
/// Исключение - ошибка регистрации нового пользователя.
/// </summary>
public class UserRegisterException : UserOperationException
{
    public UserRegisterException(string login, UserRole role)
        : this(login, role, null) { }

    public UserRegisterException(string login, UserRole role, Exception? innerException)
        : base("Ошибка регистрации пользователя", login, role, innerException) { }
}
