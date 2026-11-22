using UsersService.Application.DTO;

namespace UsersService.Application;

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