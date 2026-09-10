using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;

namespace EventManager.IntegrationTests;

public class DatabaseTestBase : IAsyncLifetime
{
    static readonly string PostgresImage = "postgres:16-alpine";
    static readonly string DatabaseName = "test-db";
    static readonly string UserName = "postgres";
    static readonly string Password = "postgres";

    protected const string Category = "Category";
    protected const string Category_Database = "Database";

    readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder(PostgresImage)
        .WithDatabase(DatabaseName)
        .WithUsername(UserName)
        .WithPassword(Password)
        .Build();

    public Task DisposeAsync()
        => _postgres.DisposeAsync().AsTask();

    public Task InitializeAsync()
        => _postgres.StartAsync();

    protected T CreateDbContext<T>() where T : DbContext
    {
        var options = new DbContextOptionsBuilder<T>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        T context = (T)Activator.CreateInstance(typeof(T), options)!;
        context.Database.EnsureCreated();
        return context;
    }

    protected async Task ResetDatabase<T>() where T : DbContext
    {
        NpgsqlConnection.ClearAllPools();
        await using var context = CreateDbContext<T>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
    }
}