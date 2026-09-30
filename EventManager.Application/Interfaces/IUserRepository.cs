using EventManager.Domain.ValueObjects;

namespace EventManager.Application.Interfaces;

/// <summary>
/// Репозиторий пользователей.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Добавляет нового пользователя.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task AddUserAsync(User user, CancellationToken cancellation);
    /// <summary>
    /// Возвращает возвращает пользователя по логину.
    /// </summary>
    /// <param name="login"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task<User?> GetUserAsync(string login, CancellationToken cancellation);
    /// <summary>
    /// Возвращает пользователя по идентификатору.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task<User?> GetUserAsync(Guid id, CancellationToken cancellation);
    /// <summary>
    /// Настраивает свойства пользователя (роль, пароль) по идентификатору.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="action"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task<User?> UpdateUserAsync(Guid id, Action<User> action, CancellationToken cancellation);
    /// <summary>
    /// Настраивает свойства пользователя (роль, пароль) по логину.
    /// </summary>
    /// <param name="login"></param>
    /// <param name="action"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task<User?> UpdateUserAsync(string login, Action<User> action, CancellationToken cancellation);
    /// <summary>
    /// Деактивирует учетную запись пользователя по идентификатору.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task<User?> DeleteUserAsync(Guid id, CancellationToken cancellation);
    /// <summary>
    /// Деактивирует учетную запись пользователя по логину.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task<User?> DeleteUserAsync(string login, CancellationToken cancellation);
}