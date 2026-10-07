using System.Runtime.CompilerServices;
using EventManager.Application.Authorization;
using EventManager.Application.DataTransferObjects;
using EventManager.Application.Interfaces;
using EventManager.Domain.Exceptions;

namespace EventManager.Application.Services;

public class UserService(
    IAppAuthorizationService appAuthorization,
    IUserRepository users
    ) : AppAuthorizeService<UserService>(appAuthorization), IUserService
{
    public async Task<UserInfo?> ChangePasswordAsync(UserRequest request, string? newPassword, CancellationToken cancellation)
    {
        await AuthorizeAsync(Policies.UserService.ChangePassword, cancellation);
        var user = await users.GetUserAsync(request.Login, cancellation);

        if (user is null)
            throw new UserNotFoundException(request.Login);

        var currentUser = CurrentUser.ToInfo();
                
        if (currentUser.Login != request.Login && currentUser.Role < user.Role)
            throw new ForbiddenException<UserService>(Policies.UserService.ChangePassword, currentUser.Login, currentUser.Role);

        return user.ToInfo();
    }

    public async Task<UserInfo?> ChangeRoleAsync(RegisterUserRequest request, CancellationToken cancellation)
    {
        await AuthorizeAsync(Policies.UserService.ModifyUser, cancellation);
        var user = await users.UpdateUserAsync(request.Login, u => u.Role = request.Role, cancellation);

        if (user is null)
            throw new UserNotFoundException(request.Login);

        return user.ToInfo();
    }

    public async Task<UserInfo> DeleteUserAsync(UserRequest request, CancellationToken cancellation)
    {
        await AuthorizeAsync(Policies.UserService.DeleteUser, cancellation);
        var user = await users.DeleteUserAsync(request.Login, cancellation);

        if (user is null)
            throw new UserNotFoundException(request.Login);

        return user.ToInfo();
    }

    public async Task<UserInfo?> LoginUserAsync(UserRequest request, CancellationToken cancellation)
    {
        var user = await users.GetUserAsync(request.Login, cancellation);
        
        if (user is null || (user.Password is not null && user.Password != request.Password))
            throw new LoginUserException();
        
        return user.ToInfo();
    }

    public async Task<UserInfo> RegisterUserAsync(RegisterUserRequest request, CancellationToken cancellation)
    {
        await AuthorizeAsync(Policies.UserService.RegisterUser, cancellation);
        return (await users.AddUserAsync(request.FromRequest(), cancellation)).ToInfo();
    }
}