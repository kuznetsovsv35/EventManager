using EventManager.Application.DataTransferObjects;
using EventManager.Application.Interfaces;
using EventManager.Domain.Exceptions;
using EventManager.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace EventManager.Tests;

/// <summary>
/// Тесты сервиса пользователей.
/// </summary>
/// <param name="context"></param>
public class UserServiceTest(EventManagerTestContext context) : TestObjectBase, IClassFixture<EventManagerTestContext>
{
    [Trait(Category, Category_Users)]
    [Fact]
    public async Task LoginUser_Success()
    {
        // Given
        await using var scope = context.CreateAsyncScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var expectedInfo = await context.GetUserInfo(TestAppDbContext.AdminLogin, UserRole.Admin);
    
        // When
        var loginRequest = new UserRequest
        { 
            Login = TestAppDbContext.AdminLogin, 
            Password = TestAppDbContext.AdminPassword 
        };

        var userInfo = await userService.LoginUserAsync(loginRequest, CancellationToken.None);
    
        // Then
        Assert.Equal(expectedInfo, userInfo);
    }

    [Trait(Category, Category_Users)]
    [Theory]
    [InlineData(["unknown", "password"])]
    [InlineData(["admin", "123"])]
    public async Task LoginUser_Fail(string login, string password)
    {
        // Given
        await using var scope = context.CreateAsyncScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
    
        // When
        var loginRequest = new UserRequest
        { 
            Login = login, 
            Password = password 
        };
    
        // Then
        await Assert.ThrowsAnyAsync<LoginUserException>(async() 
            => await userService.LoginUserAsync(loginRequest, CancellationToken.None));
    }

    [Trait(Category, Category_Users)]
    [Fact]
    public async Task RegisterUser_Success()
    {
        // Given
        await using var scope = context.CreateAsyncScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        const string login = "test";
        const string password = "123";
        const UserRole role = UserRole.User;

        RegisterUserRequest request = new()
        {
            Login = login,
            Role = role,
            Password = password,
        };

        // When
        var userInfo = await userService.RegisterUserAsync(request, CancellationToken.None);
    
        // Then
        Assert.Equal(login, userInfo.Login);
        Assert.Equal(role, userInfo.Role);
    }

    [Trait(Category, Category_Users)]
    [Fact]
    public async Task RegisterUser_Fail()
    {
        // Given
        await using var scope = context.CreateAsyncScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        string login = TestAppDbContext.UserLogin;
        const string password = "123";
        const UserRole role = UserRole.User;

        RegisterUserRequest request = new()
        {
            Login = login,
            Role = role,
            Password = password,
        };

        // When
        var ex = await Assert.ThrowsAnyAsync<Exception>(async() => await userService.RegisterUserAsync(request, CancellationToken.None));
    
        // Then
        Assert.IsType<RegisterUserException>(ex);
    }

    [Trait(Category, Category_Users)]
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

        {
            await using var scope = context.CreateAsyncScope();
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
            await using var scope = context.CreateAsyncScope();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            UserRequest passwordRequest = new() 
            { 
                Login = login,
                Password = password,
            };        
            passwordChanged = await userService.ChangePasswordAsync(passwordRequest, newPassword, CancellationToken.None);
        }

        {
            await using var scope = context.CreateAsyncScope();
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
    [Trait(Category, Category_Users)]
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

        {
            await using var scope = context.CreateAsyncScope();
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
            await using var scope = context.CreateAsyncScope();
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