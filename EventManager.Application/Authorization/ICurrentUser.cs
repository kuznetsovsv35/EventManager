using EventManager.Domain.ValueObjects;

namespace EventManager.Application.Authorization;

/// <summary>
/// Предоставляет доступ к текущему пользователю.
/// </summary>
public interface ICurrentUser
{
    Guid Id { get; }
    string Login { get; }
    UserRole Role { get; }
    bool IsAuthenticated { get; }
    bool InRole(UserRole role);
}