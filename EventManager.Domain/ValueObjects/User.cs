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
    /// <summary>
    /// Идентификатор пользователя для связи.
    /// </summary>
    public Guid Id { get; private set; }
    /// <summary>
    /// Логин пользователя.
    /// </summary>
    public string Login { get; private set; } = string.Empty;
    /// <summary>
    /// Роль пользователя в системе.
    /// </summary>
    public UserRole Role { get; set; } = UserRole.User;
    /// <summary>
    /// Хеш пароля.
    /// </summary>
    public string? Password { get; set; }
    /// <summary>
    /// Признак активности записи.
    /// </summary>
    public bool IsActive { get; set; } = true;
    /// <summary>
    /// Дата время создания учетной записи.
    /// </summary>
    public DateTime CreatedAt { get; private set; }
    /// <summary>
    /// Дата время деактивации учетной записи.
    /// </summary>
    public DateTime? DeletedAt { get; private set; }
    /// <summary>
    /// Конструктор по логину.
    /// </summary>
    /// <param name="login"></param>
    public User(string login)
    {
        Id = Guid.NewGuid();
        Login = login;
        CreatedAt = DateTime.UtcNow;
    }
    /// <summary>
    /// Приватный конструктор по умолчанию.
    /// </summary>
    User() { }
    /// <summary>
    /// Метод деактивации учетной записи.
    /// </summary>
    public void Delete()
    {
        if (!IsActive)
            return;

        IsActive = false;
        DeletedAt = DateTime.UtcNow;
    }

    static readonly Guid _adminId = new ("ac28e281-33c8-4f15-b257-f935d8bb8b2c");
    static readonly Guid _userId = new("435f8caf-cfb0-433e-a694-385383dfa3f7");

    static User CreateUser(Guid id, UserRole role)
        => new()
        { 
            Id = id, 
            Login = role.ToString().ToLower(),
            Role = role,
        };

    static readonly User _defaultAdmin = CreateUser(_adminId, UserRole.Admin);

    static readonly User _defaultUser = CreateUser(_userId, UserRole.User);
    
    public static User DefaultAdmin => _defaultAdmin;
    public static User DefaultUser => _defaultUser;
}