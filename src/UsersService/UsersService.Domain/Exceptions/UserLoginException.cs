namespace UsersService.Domain.Exceptions;

/// <summary>
/// Исключение ошибка входа в приложении.
/// </summary>
public class UserLoginException : ApplicationException
{
    public UserLoginException() : this(null) { }

    public UserLoginException(Exception? innerException)
        : base("Логин или пароль указан неверно", innerException) { }
}
