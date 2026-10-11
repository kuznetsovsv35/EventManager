using EventManager.Application.DataTransferObjects;
using EventManager.Application.Interfaces;
using EventManager.Database;
using EventManager.Domain.ValueObjects;
using EventManager.Infrastructure.Repositories;
using EventManager.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace EventManager.IntegrationTests;

[Collection(nameof(TestContainersCollection))]
public class UserRepositoryTest(TestContainerWrapper<AppDbContext> testContainer) : DatabaseTestBase(testContainer)
{
    [Trait(Category, Category_Repositories)]
    [Fact]
    public async Task AddUser_Success()
    {
        // Given
        await ResetDatabase();
        var hasher = new PasswordHasher();

        User admin = new("admin") 
        {
            Role = UserRole.Admin,
            Password = hasher.Hash("admin")
        };
        var adminInfo = admin.ToInfo();

        User user = new("user") 
        {
            Role = UserRole.User,
            Password = hasher.Hash("user")
        };
        var userInfo = user.ToInfo();

        // When
        await using var workContext = CreateDbContext();
        IUserRepository workRepository = new UserRepository(workContext);
        await workRepository.AddUserAsync(admin, CancellationToken.None);
        await workRepository.AddUserAsync(user, CancellationToken.None);

        // Then
        await using var verifyContext = CreateDbContext();
        IUserRepository verifyRepository = new UserRepository(verifyContext);
        var adminAdded = await verifyRepository.GetUserAsync("admin", CancellationToken.None);
        var userAdded = await verifyRepository.GetUserAsync("user", CancellationToken.None);

        Assert.NotNull(adminAdded);
        Assert.Equal(adminInfo, adminAdded.ToInfo());
        Assert.True(hasher.Verify(adminAdded.Password, "admin"));
        Assert.NotNull(userAdded);
        Assert.Equal(userInfo, userAdded.ToInfo());
        Assert.True(hasher.Verify(userAdded.Password, "user"));
    }

    [Trait(Category, Category_Repositories)]
    [Fact]
    public async Task UpdateUser_Success()
    {
        // Given
        await ResetDatabase();
        var hasher = new PasswordHasher();

        const string login = "test";
        const string newPassword = "123";
        const UserRole newRole = UserRole.Admin;
        
        User user = new("test") { Role = UserRole.User, };

        await using var context = CreateDbContext();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // When
        await using var workContext = CreateDbContext();
        IUserRepository workRepository = new UserRepository(workContext);
        await workRepository.UpdateUserAsync(login, u =>
        {
            u.Role = newRole;
            u.Password = hasher.Hash(newPassword);
        }, CancellationToken.None);

        // Then
        await using var verifyContext = CreateDbContext();
        var userFound = await verifyContext.Users.SingleAsync(u => u.Login == login);

        Assert.NotNull(userFound);
        Assert.Equal(newRole, userFound.Role);
        Assert.True(hasher.Verify(userFound.Password, newPassword));
    }

    [Trait(Category, Category_Repositories)]
    [Fact]
    public async Task DeleteUser_Success()
    {
        await ResetDatabase();
        var hasher = new PasswordHasher();

        const string login = "test";
        
        User user = new("test") { Role = UserRole.User };

        await using var context = CreateDbContext();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // When
        await using var workContext = CreateDbContext();
        IUserRepository workRepository = new UserRepository(workContext);
        await workRepository.DeleteUserAsync(login, CancellationToken.None);

        // Then
        await using var verifyContext = CreateDbContext();
        var dbUser = await verifyContext.Users.SingleAsync(u => u.Login == login);
        IUserRepository verifyRepository = new UserRepository(verifyContext);
        var repoUser = await verifyRepository.GetUserAsync(login, CancellationToken.None);

        Assert.NotNull(dbUser);
        Assert.Null(repoUser);
    }
}