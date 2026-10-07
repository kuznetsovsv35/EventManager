namespace EventManager.Domain.Exceptions;

/// <summary>
/// Исключение - пользователь не найден в системе.
/// </summary>
public class UserNotFoundException : ApplicationException
{
    public string Login { get; }
    public UserNotFoundException(string login)
        : this(login, null) {}

    public UserNotFoundException(string login, Exception? innerException)
        : base("Пользователь не найден", innerException)
    {
        Login = login;
    }
}