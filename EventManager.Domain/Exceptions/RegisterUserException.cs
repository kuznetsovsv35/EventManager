using EventManager.Domain.ValueObjects;

namespace EventManager.Domain.Exceptions;

/// <summary>
/// Исключение - ошибка регистрации нового пользователя.
/// </summary>
public class RegisterUserException : UserOperationException
{
    public RegisterUserException(string login, UserRole role)
        : this(login, role, null) {}

    public RegisterUserException(string login, UserRole role, Exception? innerException)
        : base("Ошибка регистрации пользователя", login, role, innerException) {}
}