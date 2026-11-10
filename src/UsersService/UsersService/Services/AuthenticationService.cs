using UsersService.Application;
using UsersService.Application.DTO;

namespace UsersService.Services;

public class AuthenticationService(
    IUserService userService,
    ITokenService tokenService) : IAuthenticationService
{
    public async Task<AuthResult> LoginAsync(UserRequest user, CancellationToken cancellation)
        => tokenService.GenerateTokens(await userService.LoginUserAsync(user, cancellation), null);
}
