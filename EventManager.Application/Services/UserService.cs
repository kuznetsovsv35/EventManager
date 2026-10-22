using EventManager.Application.Authorization;
using EventManager.Application.Cryptography;
using EventManager.Application.DataTransferObjects;
using EventManager.Application.Interfaces;
using EventManager.Domain.Exceptions;
using EventManager.Domain.ValueObjects;

namespace EventManager.Application.Services;

public class UserService(
    IAppAuthorizationService appAuthorization,
    IUserRepository users,
    IPasswordHasher hasher
    ) : AppAuthorizeService<UserService>(appAuthorization), IUserService
{
    public async Task<UserInfo> ChangePasswordAsync(UserRequest request, CancellationToken cancellation)
    {
        request.Check();

        await AuthorizeAsync(Policies.UserService.ChangePassword, cancellation);
        var user = await users.GetUserAsync(request.Login, cancellation);

        if (user is null)
            throw new UserNotFoundException(request.Login);

        var currentUser = CurrentUser.ToInfo();

        if (currentUser.Login != request.Login && user.Role == UserRole.Admin && currentUser.Role != UserRole.Admin)
            throw new ForbiddenException(this, Policies.UserService.ChangePassword, currentUser.Login, currentUser.Role);

        user = await users.UpdateUserAsync(
            request.Login,
            u => u.Password = request.Password is string p ? hasher.Hash(p) : null,
            cancellation);

        if (user is null)
            throw new UserNotFoundException(request.Login);

        return user.ToInfo();
    }

    public async Task<UserInfo> ChangeRoleAsync(RegisterUserRequest request, CancellationToken cancellation)
    {
        request.Check();

        await AuthorizeAsync(Policies.UserService.ModifyUser, cancellation);
        var user = await users.UpdateUserAsync(request.Login, u => u.Role = request.Role, cancellation);

        if (user is null)
            throw new UserNotFoundException(request.Login);

        return user.ToInfo();
    }

    public async Task<UserInfo> DeleteUserAsync(UserRequest request, CancellationToken cancellation)
    {
        request.Check();

        await AuthorizeAsync(Policies.UserService.DeleteUser, cancellation);
        var user = await users.DeleteUserAsync(request.Login, cancellation);

        if (user is null)
            throw new UserNotFoundException(request.Login);

        return user.ToInfo();
    }

    public async Task<UserInfo> LoginUserAsync(UserRequest request, CancellationToken cancellation)
    {
        request.Check();
        var user = await users.GetUserAsync(request.Login, cancellation);

        if (user is null || !hasher.Verify(user.Password, request.Password))
            throw new LoginUserException();

        return user.ToInfo();
    }

    public async Task<UserInfo> RegisterUserAsync(UserRequest request, CancellationToken cancellation)
    {
        if (request is RegisterUserRequest)
            await AuthorizeAsync(Policies.UserService.RegisterUser, cancellation);

        try
        {
            var user = await users.AddUserAsync(request.FromRequest(hasher.Hash), cancellation);
            return user.ToInfo();
        }
        catch (Exception ex)
        {
            var userInfo = CurrentUser.ToInfo();
            throw new RegisterUserException(userInfo.Login, userInfo.Role, ex);
        }
    }
}