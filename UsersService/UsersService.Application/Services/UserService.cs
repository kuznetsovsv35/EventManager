using Authorization;
using Authorization.Domain;
using Authorization.Exceptions;
using Common.Validation;
using UsersService.Application.Cryptography;
using UsersService.Application.DTO;
using UsersService.Application.Security;
using UsersService.Domain.Exceptions;

namespace UsersService.Application.Services;

public class UserService(
    IAppAuthorizationService appAuthorization,
    IUserRepository users,
    IPasswordHasher hasher
    ) : IUserService
{
    public async Task<UserInfo> ChangePasswordAsync(UserRequest request, CancellationToken cancellation)
    {
        request.Check();

        await appAuthorization.AuthorizeAsync(this, Policy.ChangePassword, cancellation);
        var user = await users.GetUserAsync(request.Login, cancellation);

        if (user is null)
            throw new UserNotFoundException(request.Login);

        if (appAuthorization.CurrentUser.Login != request.Login && user.Role == UserRole.Admin && appAuthorization.CurrentUser.Role != UserRole.Admin)
            throw new ForbiddenException(this, Policy.ChangePassword, appAuthorization.CurrentUser);

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

        await appAuthorization.AuthorizeAsync(this, Policy.ModifyUser, cancellation);
        var user = await users.UpdateUserAsync(request.Login, u => u.Role = request.Role, cancellation);

        if (user is null)
            throw new UserNotFoundException(request.Login);

        return user.ToInfo();
    }

    public async Task<UserInfo> DeleteUserAsync(UserRequest request, CancellationToken cancellation)
    {
        request.Check();

        await appAuthorization.AuthorizeAsync(this, Policy.DeleteUser, cancellation);
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
            throw new UserLoginException();

        return user.ToInfo();
    }

    public async Task<UserInfo> RegisterUserAsync(UserRequest request, CancellationToken cancellation)
    {
        if (request is RegisterUserRequest)
            await appAuthorization.AuthorizeAsync(this, Policy.RegisterUser, cancellation);

        try
        {
            var user = await users.AddUserAsync(request.FromRequest(hasher.Hash), cancellation);
            return user.ToInfo();
        }
        catch (Exception ex)
        {
            throw new UserRegisterException(appAuthorization.CurrentUser, ex);
        }
    }
}