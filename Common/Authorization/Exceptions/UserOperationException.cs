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

    public UserOperationException(string message, ICurrentUser? user) : this(message, user?.Login, user?.Role, null) { }

    public UserOperationException(string message, string? login, UserRole? role, Exception? innerException) : base(message, innerException)
    {
        Login = login ?? Login;
        Role = role.ToString() ?? Role;
    }
    public UserOperationException(string message, ICurrentUser? user, Exception? innerException) : this(message, user?.Login, user?.Role, innerException) { }

    static readonly string DefaultRole = UserRole.User.ToString();
}