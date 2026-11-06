using Authorization.Domain;

namespace Authorization.Interfaces;

/// <summary>
/// Предоставляет доступ к текущему пользователю.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// Идентификатор пользователя.
    /// </summary>
    Guid Id { get; }
    /// <summary>
    /// Логин пользователя.
    /// </summary>
    string Login { get; }
    /// <summary>
    /// Роль пользователя.
    /// </summary>
    UserRole Role { get; }
    /// <summary>
    /// Признак что пользователь прошел аутентификацию
    /// </summary>
    bool IsAuthenticated { get; }
    /// <summary>
    /// Проверка принадлежности роли.
    /// </summary>
    /// <param name="role"></param>
    /// <returns></returns>
    bool InRole(UserRole role);
}