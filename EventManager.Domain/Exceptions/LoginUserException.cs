namespace EventManager.Domain.Exceptions;

/// <summary>
/// Исключение ошибка входа в приложении.
/// </summary>
public class LoginUserException : ApplicationException
{
    public LoginUserException() : this(null) {}

    public LoginUserException(Exception? innerException)
        : base("Логин или пароль указан неверно", innerException) {}
}