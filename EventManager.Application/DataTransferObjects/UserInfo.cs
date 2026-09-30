using EventManager.Domain.ValueObjects;

namespace EventManager.Application.DataTransferObjects;

/// <summary>
/// Информации о пользователе
/// </summary>
public class UserInfo
{
    public Guid Id { get; internal init; }
    public string Login { get; internal init; } = string.Empty;
    public UserRole Role { get; internal init; } = UserRole.User;

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
            return true;

        if (obj is UserInfo { Id: Guid id, Login: string login, Role: UserRole role})
            return Id == id && login == Login && role == Role;

        return base.Equals(obj);
    }

    public override int GetHashCode() => HashCode.Combine(Id);
}