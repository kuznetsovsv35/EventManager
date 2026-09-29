namespace EventManager.Domain.ValueObjects;

/// <summary>
/// Роли пользователя.
/// </summary>
public enum UserRole
{
    User,
    Admin
}

/// <summary>
/// Сущность пользователя.
/// </summary>
public class User
{
    public Guid Id { get; private set; }

    public string Login { get; private set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.User;

    public string? Password { get; set; }

    public User(string login)
    {
        Id = Guid.NewGuid();
        Login = login;
    }
}