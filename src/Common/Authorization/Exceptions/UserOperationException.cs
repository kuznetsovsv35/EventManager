using Authorization.Domain;

namespace Authorization.Exceptions;

/// <summary>
/// Базовое исключение для ошибок действий пользователя.
/// </summary>
public class UserOperationException : ApplicationException
{
    public string Login { get; } = string.Empty;
    public string Role { get; } = DefaultRole;

    public UserOperationException(string message, string login, UserRole role) : this(message, login, role, null) { }

    public UserOperationException(string message, string login, UserRole role, Exception? innerException) : base(message, innerException)
    {
        Login = login;
        Role = role.ToString();
    }

    static readonly string DefaultRole = UserRole.User.ToString();
}