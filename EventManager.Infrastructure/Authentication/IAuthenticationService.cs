using EventManager.Application.DataTransferObjects;

namespace EventManager.Infrastructure.Authentication;

/// <summary>
/// Сервис аутентификации.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Запускает процесс аутентификации.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task<AuthResult> LoginAsync(UserRequest user, CancellationToken cancellation);
}