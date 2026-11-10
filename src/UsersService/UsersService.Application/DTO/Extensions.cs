using Authorization.Domain;
using Authorization.Interfaces;
using Common.Validation;
using UsersService.Domain;

namespace UsersService.Application.DTO;

public static class Extensions
{
    public static User FromRequest(this UserRequest request, Func<string?, string?> hasher)
    {
        request.Check();

        return new(request.Login)
        {
            Role = request is RegisterUserRequest ex ? ex.Role : UserRole.User,
            Password = hasher(request.Password)
        };
    }
    public static UserInfo ToInfo(this User user)
        => new()
        {
            Id = user.Id,
            Login = user.Login,
            Role = user.Role,
        };

    public static UserInfo ToInfo(this ICurrentUser user)
        => new()
        {
            Id = user.Id,
            Login = user.Login,
            Role = user.Role,
        };

    public static RegisterUserRequest SetLogin(this RegisterUserRequest request, string login)
    {
        request.Login = login;
        return request;
    }

    public static RegisterUserRequest ChangeRole(this RegisterUserRequest request, string roleName)
    {
        if (!Enum.TryParse<UserRole>(roleName, out var role))
            throw new ArgumentException("Неверное наименование роли", nameof(roleName));

        request.Role = role;
        return request;
    }

    public static RegisterUserRequest SetPassword(this RegisterUserRequest request, string? password)
    {
        request.Password = password;
        return request;
    }
}
