using Authorization.Domain;

namespace UsersService.Application.DTO;

/// <summary>
/// DTO запроса на регистрацию пользователя.
/// </summary>
public class RegisterUserRequest : UserRequest
{
    public UserRole Role { get; set; }
}
