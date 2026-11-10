namespace UsersService.Application.DTO;

/// <summary>
/// Результат аутентификации.
/// </summary>
/// <param name="AccessToken"></param>
/// <param name="RefreshToken"></param>
/// <param name="ExpirationTime"></param>
public record AuthResult(string AccessToken, string? RefreshToken, int ExpirationTime);