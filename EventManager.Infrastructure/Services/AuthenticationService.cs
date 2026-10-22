using EventManager.Application.DataTransferObjects;
using EventManager.Application.Interfaces;
using EventManager.Infrastructure.Authorization;

namespace EventManager.Infrastructure.Authentication;

public class AuthenticationService(
    IUserService userService,
    ITokenService tokenService) : IAuthenticationService
{
    public async Task<AuthResult> LoginAsync(UserRequest user, CancellationToken cancellation)
        => tokenService.GenerateTokens(await userService.LoginUserAsync(user, cancellation), null);
}