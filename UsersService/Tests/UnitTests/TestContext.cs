using Authorization;
using Authorization.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using UsersService.Application;
using UsersService.Application.Cryptography;
using UsersService.Application.DTO;
using UsersService.Application.Security;
using UsersService.Application.Services;
using UsersService.Domain;
using UsersService.Domain.Exceptions;

namespace UnitTests;

/// <summary>
/// Контекст методов теста.
/// </summary>
public class TestContext
{
    public static readonly string AdminLogin = "admin";
    public static readonly string AdminPassword = "admin";

    public static readonly string UserLogin = "user";
    public static readonly string UserPassword = "user";

    readonly IServiceCollection _services;

    public TestContext()
    {
        var currentUser = new Mock<ICurrentUser>();

        _services = new ServiceCollection()
            .AddSingleton(_ => LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<TestContext>())
            .AddSingleton(provider =>
            {
                var mock = new Mock<IServiceScopeFactory>();
                mock.Setup(x => x.CreateScope()).Returns(provider.CreateScope());
                return mock.Object;
            })
            .AddScoped(provider =>
            {
                var mock = new Mock<IAppAuthorizationService>();
                mock.SetupGet(x => x.CurrentUser).Returns(currentUser.Object);
                mock.Setup(x => x.AuthorizeAsync(It.IsAny<UserService>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .Returns<UserService, string, CancellationToken>((_, policy, _) =>
                    {
                        if (policy == Policy.RegisterUser)
                            return Task.FromException(new UserRegisterException(currentUser.Object));
                        return Task.CompletedTask;
                    });
                return mock.Object;
            })
            .AddScoped(_ => new UserRepository().Object)
            .AddScoped<IUserService, UserService>()
            .AddSingleton(_ =>
            {
                var hasher = new Mock<IPasswordHasher>();
                hasher.Setup(x => x.Hash(It.IsAny<string?>())).Returns<string?>((password) => password);
                
                hasher
                    .Setup(x => x.Verify(It.IsAny<string?>(), It.IsAny<string?>()))
                    .Returns<string, string>((password, hash) => password == hash);
                return hasher.Object;
            });
    }

    public IServiceProvider CreateServiceProvider() => _services.BuildServiceProvider();

    static readonly List<User> _users = [new(AdminLogin)
            {
                Role = UserRole.Admin,
                Password = AdminPassword,
            },
            new(UserLogin)
            {
                Role = UserRole.User,
                Password = UserPassword,
            }
        ];
    
    public static UserInfo GetUserInfo(string login, UserRole role)
        => _users
            .Single(u => u.Login == login && u.Role == role && u.IsActive)
            .ToInfo();

    class UserRepository : Mock<IUserRepository>
    {
        internal UserRepository()
        {
            Setup(x => x.AddUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .Returns<User, CancellationToken>((user, _) => 
                {
                    _users.Add(user);
                    return Task.FromResult(user);
                });
            
            Setup(x => x.GetUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns<string, CancellationToken>((login, _) =>
                {
                    var u = _users.Find(u => u.Login == login);
                    return Task.FromResult(u);
                });
            
            Setup(x => x.UpdateUserAsync(It.IsAny<string>(), It.IsAny<Action<User>>(), It.IsAny<CancellationToken>()))
                .Returns<string, Action<User>, CancellationToken>((login, action, _) =>
                {
                    var u = _users.Find(u => u.Login == login);
                    if (u is not null)
                    {
                        action(u);
                        return Task.FromResult<User?>(u);
                    }
                    return Task.FromResult<User?>(null);
                });
            
            Setup(x => x.DeleteUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns<string, CancellationToken>((login, _) =>
                {                    
                    var u = _users.Find(u => u.Login == login);
                    if (u is not null)
                    {
                        _users.Remove(u);
                        return Task.FromResult<User?>(u);
                    }
                    return Task.FromResult<User?>(null);
                });
        }
    }
}
