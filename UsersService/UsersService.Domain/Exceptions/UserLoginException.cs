namespace UsersService.Domain.Exceptions;

/// <summary>
/// Исключение ошибка входа в приложении.
/// </summary>
public class UserLoginException(Exception? innerException) : ApplicationException("Логин или пароль указан неверно", innerException)
{
    public UserLoginException() : this(null) { }
}
