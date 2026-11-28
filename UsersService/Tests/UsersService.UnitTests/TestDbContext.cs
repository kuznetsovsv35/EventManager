using Authorization.Domain;
using Microsoft.EntityFrameworkCore;
using UsersService.Database;
using UsersService.Domain;

namespace UsersService.UnitTests;

/// <summary>
/// Репозиторий с тестовыми данными.
/// </summary>
class TestDbContext : UsersDbContext
{
    internal static readonly string AdminLogin = "admin";
    internal static readonly string AdminPassword = "admin";

    internal static readonly string UserLogin = "user";
    internal static readonly string UserPassword = "user";

    internal string DatabaseName { get; }

    internal TestDbContext(string databaseName) : base(
        new DbContextOptionsBuilder<UsersDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options)
    {
        DatabaseName = databaseName;
        Database.EnsureCreated();
    }

    internal UsersDbContext CreateNewInstance()
        => new UsersDbContext(
            new DbContextOptionsBuilder<UsersDbContext>()
            .UseInMemoryDatabase(DatabaseName)
            .Options);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().HasData([
            new(AdminLogin)
            {
                Role = UserRole.Admin,
                Password = AdminPassword,
            },
            new(UserLogin)
            {
                Role = UserRole.User,
                Password = UserPassword,
            }
        ]);
    }
}
