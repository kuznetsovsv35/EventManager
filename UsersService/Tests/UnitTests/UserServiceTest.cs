using Authorization.Domain;
using Common.Tests;
using Microsoft.Extensions.DependencyInjection;
using UsersService.Application;
using UsersService.Application.DTO;
using UsersService.Domain.Exceptions;

namespace UnitTests;

/// <summary>
/// Тесты сервиса пользователей.
/// </summary>
/// <param name="context"></param>
public class UserServiceTest(TestContext context) : IClassFixture<TestContext>
{
    [Trait(nameof(Category), Category.Users)]
    [Fact]
    public async Task LoginUser_Success()
    {
        // Given
        var serviceProvider = context.CreateServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var expectedInfo = TestContext.GetUserInfo(TestContext.AdminLogin, UserRole.Admin);

        // When
        var loginRequest = new UserRequest
        {
            Login = TestContext.AdminLogin,
            Password = TestContext.AdminPassword
        };

        var userInfo = await userService.LoginUserAsync(loginRequest, CancellationToken.None);

        // Then
        Assert.Equal(expectedInfo, userInfo);
    }

    [Trait(nameof(Category), Category.Users)]
    [Theory]
    [InlineData(["unknown", "password"])]
    [InlineData(["test-admin", "123"])]
    public async Task LoginUser_Fail(string login, string password)
    {
        // Given
        var serviceProvider = context.CreateServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        // When
        var loginRequest = new UserRequest
        {
            Login = login,
            Password = password
        };

        // Then
        await Assert.ThrowsAnyAsync<UserLoginException>(async ()
            => await userService.LoginUserAsync(loginRequest, CancellationToken.None));
    }

    [Trait(nameof(Category), Category.Users)]
    [Fact]
    public async Task RegisterUser_Success()
    {
        // Given
        var serviceProvider = context.CreateServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        const string login = "test";
        const string password = "123";

        UserRequest request = new()
        {
            Login = login,
            Password = password,
        };

        // When
        var userInfo = await userService.RegisterUserAsync(request, CancellationToken.None);

        // Then
        Assert.Equal(login, userInfo.Login);
        Assert.Equal(UserRole.User, userInfo.Role);
    }

    [Trait(nameof(Category), Category.Users)]
    [Fact]
    public async Task RegisterUser_Fail()
    {
        // Given
        var serviceProvider = context.CreateServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        string login = TestContext.UserLogin;
        const string password = "123";

        RegisterUserRequest request = new()
        {
            Login = login,
            Password = password,
            Role = UserRole.User,
        };

        // When
        var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await userService.RegisterUserAsync(request, CancellationToken.None));

        // Then
        Assert.IsType<UserRegisterException>(ex);
    }

    [Trait(nameof(Category), Category.Users)]
    [Fact]
    public async Task ChangePassword_Success()
    {
        // Given
        const string login = "test-1";
        const string password = "123";
        const string newPassword = "321";
        const UserRole role = UserRole.User;
        UserInfo? registered;
        UserInfo? passwordChanged;
        UserInfo? logged;

        var serviceProvider = context.CreateServiceProvider();

        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

            RegisterUserRequest request = new()
            {
                Login = login,
                Role = role,
                Password = password,
            };
            registered = await userService.RegisterUserAsync(request, CancellationToken.None);
        }

        // When
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            UserRequest passwordRequest = new()
            {
                Login = login,
                Password = newPassword,
            };
            passwordChanged = await userService.ChangePasswordAsync(passwordRequest, CancellationToken.None);
        }

        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

            UserRequest loginRequest = new()
            {
                Login = login,
                Password = newPassword
            };
            logged = await userService.LoginUserAsync(loginRequest, CancellationToken.None);
        }

        // Then
        Assert.Equal(registered, passwordChanged);
        Assert.Equal(registered, logged);
    }

    [Trait(nameof(Category), Category.Users)]
    [Fact]
    public async Task ChangeRole_Success()
    {
        // Given
        const string login = "test-2";
        const string password = "123";
        const UserRole role = UserRole.User;
        const UserRole newRole = UserRole.Admin;
        UserInfo? registered;
        UserInfo? roleChanged;
        var serviceProvider = context.CreateServiceProvider();

        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

            RegisterUserRequest request = new()
            {
                Login = login,
                Role = role,
                Password = password,
            };
            registered = await userService.RegisterUserAsync(request, CancellationToken.None);
        }

        // When
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            RegisterUserRequest roleRequest = new()
            {
                Login = login,
                Role = newRole,
            };
            roleChanged = await userService.ChangeRoleAsync(roleRequest, CancellationToken.None);
        }

        // Then
        Assert.Equal(newRole, roleChanged?.Role);
    }
}