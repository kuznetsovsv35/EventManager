using Authorization.Domain;
using Common.Tests;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using UsersService.Application.Cryptography;
using UsersService.Application.DTO;
using UsersService.Domain;
using UsersService.Infrastructure.Services;

namespace UsersService.IntegrationTests;

[Collection(nameof(ContainersCollection))]
public class SchemaTest(ContainerWrapper testContainer) : DatabaseTestBase(testContainer)
{
    [Trait(Category.Name, Category.Database)]
    [Fact]
    public Task TestConnection() => ExecuteTestConnection(Assert.True);

    [Trait(Category.Name, Category.Database)]
    [Fact]
    public Task TestMigrations() => ExecuteTestMigrations(Assert.Equal);

    [Trait(Category.Database, Category.Database)]
    [Fact]
    public async Task UsersColumnsTest_Success()
    {
        // Given
        await ResetDatabase();

        const string password = nameof(password);
        IPasswordHasher hasher = new PasswordHasher();

        User user = new("test")
        {
            Role = UserRole.Admin,
            Password = hasher.Hash(password),
        };

        UserInfo info = user.ToInfo();

        // When
        await using var workContext = CreateDbContext();
        workContext.Users.Add(user);
        await workContext.SaveChangesAsync();

        // Then
        await using var verifyContext = CreateDbContext();
        var userFound = await verifyContext.Users
            .FromSqlInterpolated($@"
                SELECT * FROM ""Users""
                WHERE ""Id"" = {user.Id}
                ")
            .AsNoTracking()
            .SingleOrDefaultAsync();

        var infoFound = userFound?.ToInfo();

        Assert.NotNull(infoFound);
        Assert.Equal(info, infoFound);
        Assert.True(userFound is { Password: string p } && hasher.Verify(p, password));
    }

    [Trait(Category.Name, Category.Database)]
    [Fact]
    public async Task UsersPrimaryKeyTest()
    {
        // Given
        await ResetDatabase();

        var id = Guid.NewGuid();
        const string login1 = "test-1";
        const string login2 = "test-2";
        const UserRole role = default;
        var createdAt = DateTime.UtcNow;
        const bool isActive = default;
        await using var context = CreateDbContext();
        var db = context.Database;

        // When
        await db.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO ""Users"" (""Id"", ""Login"", ""Role"", ""IsActive"", ""CreatedAt"")
            VALUES ({id}, {login1}, {role}, {isActive}, {createdAt})
        ");

        // Then
        var ex = await Assert.ThrowsAnyAsync<PostgresException>(async () =>
        {
            await db.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO ""Users"" (""Id"", ""Login"", ""Role"", ""IsActive"", ""CreatedAt"")
                VALUES ({id}, {login2}, {role}, {isActive}, {createdAt})
            ");
        });

        ex.CheckUniqueConstraint("Users", "Id");
    }

    [Trait(Category.Name, Category.Database)]
    [Fact]
    public async Task UsersConstraintsTest()
    {
        // Given
        await ResetDatabase();

        const string password = nameof(password);
        string loginMinLength = string.Empty;
        string loginMaxLength = new('l', 33);
        const string invalidPassword = "123";

        IPasswordHasher hasher = new PasswordHasher();

        User user = new("test")
        {
            Role = UserRole.Admin,
            Password = hasher.Hash(password),
        };
        await using var context = CreateDbContext();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // When
        var verify = CreateDbContext();

        var exIdNull = await Assert.ThrowsAnyAsync<PostgresException>(async () =>
        {
            await verify.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE ""Users"" 
                SET ""Id"" = NULL
                WHERE ""Id"" = {user.Id}
            ");
        });

        var exLoginNull = await Assert.ThrowsAnyAsync<PostgresException>(async () =>
        {
            await verify.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE ""Users""
                SET ""Login"" = NULL
                WHERE ""Id"" = {user.Id}
            ");
        });

        var exLoginMinLength = await Assert.ThrowsAnyAsync<PostgresException>(async () =>
        {
            await verify.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE ""Users""
                SET ""Login"" = {loginMinLength}
                WHERE ""Id"" = {user.Id}
            ");
        });
        var exLoginMaxLength = await Assert.ThrowsAnyAsync<PostgresException>(async () =>
        {
            await verify.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE ""Users""
                SET ""Login"" = {loginMaxLength}
                WHERE ""Id"" = {user.Id}
            ");
        });

        var exPassword = await Assert.ThrowsAnyAsync<PostgresException>(async () =>
        {
            await verify.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE ""Users""
                SET ""Password"" = {invalidPassword}
                WHERE ""Id"" = {user.Id}
            ");
        });

        var exRoleNull = await Assert.ThrowsAnyAsync<PostgresException>(async () =>
        {
            await verify.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE ""Users""
                SET ""Role"" = NULL
                WHERE ""Id"" = {user.Id}
            ");
        });

        var exIsActiveNull = await Assert.ThrowsAnyAsync<PostgresException>(async () =>
        {
            await verify.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE ""Users""
                SET ""IsActive"" = NULL
                WHERE ""Id"" = {user.Id}
            ");
        });

        var exCreatedAtNull = await Assert.ThrowsAnyAsync<PostgresException>(async () =>
        {
            await verify.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE ""Users""
                SET ""CreatedAt"" = NULL
            ");
        });

        // Then
        exIdNull.CheckNullConstraint("Users", "Id");

        exLoginNull.CheckNullConstraint("Users", "Login");
        exLoginMinLength.CheckValueConstraint("Users", "CK_Users_Login");
        exLoginMaxLength.CheckMaxLengthConstraint("Users");

        exPassword.CheckValueConstraint("Users", "CK_Users_Password");

        exRoleNull.CheckNullConstraint("Users", "Role");
        exIsActiveNull.CheckNullConstraint("Users", "IsActive");
        exCreatedAtNull.CheckNullConstraint("Users", "CreatedAt");
    }
}
