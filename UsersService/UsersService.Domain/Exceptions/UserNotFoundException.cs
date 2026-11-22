namespace UsersService.Domain.Exceptions;

/// <summary>
/// Исключение - пользователь не найден в системе.
/// </summary>
public class UserNotFoundException(string login, Exception? innerException) : ApplicationException("Пользователь не найден", innerException)
{
    public string Login { get; } = login;

    public UserNotFoundException(string login)
        : this(login, null) { }
}
