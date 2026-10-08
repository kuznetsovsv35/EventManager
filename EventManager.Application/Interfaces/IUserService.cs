using EventManager.Application.DataTransferObjects;

namespace EventManager.Application.Interfaces;

/// <summary>
/// Сервис управления пользователями.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Регистрация нового пользователя.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<UserInfo> RegisterUserAsync(RegisterUserRequest request, CancellationToken cancellation);
    /// <summary>
    /// Вход пользователя в систему.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<UserInfo> LoginUserAsync(UserRequest request, CancellationToken cancellation);
    /// <summary>
    /// Логическое управление пользователя.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<UserInfo> DeleteUserAsync(UserRequest request, CancellationToken cancellation);
    /// <summary>
    /// Изменения роли пользователя.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<UserInfo?> ChangeRoleAsync(RegisterUserRequest request, CancellationToken cancellation);
    /// <summary>
    /// Изменение пароля пользователя.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="newPassword"></param>
    /// <returns></returns>
    Task<UserInfo?> ChangePasswordAsync(UserRequest request, string? newPassword, CancellationToken cancellation);
}