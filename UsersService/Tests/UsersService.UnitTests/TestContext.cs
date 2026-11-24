using Authorization;
using Common.Services;
using Common.Synchronization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UsersService.Application;
using UsersService.Application.Cryptography;
using UsersService.Application.Services;
using UsersService.Database.Repositories;

namespace UsersService.UnitTests;

/// <summary>
/// Контекст методов теста.
/// </summary>
public class TestContext
{
    readonly IServiceCollection _services;

    public TestContext()
    {
        var currentUser = new Mock<ICurrentUser>();
        var passwordHasher = new Mock<IPasswordHasher>();

        passwordHasher
            .Setup(x => x.Hash(It.IsAny<string?>()))
            .Returns<string?>(p => p);
        passwordHasher
            .Setup(x => x.Verify(It.IsAny<string?>(), It.IsAny<string?>()))
            .Returns<string?, string>((h, p) => p == h);

        _services = new ServiceCollection()
            .AddSingleton(_ => new TestDbContext($"Test_{Guid.NewGuid()}"))
            .AddScoped(provider => provider.GetRequiredService<TestDbContext>().CreateNewInstance())
            .AddSingleton(provider =>
            {
                var mock = new Mock<IServiceScopeFactory>();
                mock
                    .Setup(x => x.CreateScope())
                    .Returns(provider.CreateScope());
                return mock.Object;
            })
            .AddSingleton<ISyncContextFactory, SyncContextFactory>()
            .AddScoped(provider =>
            {
                var mock = new Mock<IAppAuthorizationService>();
                mock
                    .SetupGet(x => x.CurrentUser)
                    .Returns(currentUser.Object);
                return mock.Object;
            })
            .AddScoped<IUserRepository, UserRepository>()
            .AddScoped<IUserService, UserService>()
            .AddSingleton<IPasswordHasher>(_ => passwordHasher.Object);
    }

    public IServiceProvider CreateServiceProvider() => _services.BuildServiceProvider();    
}
