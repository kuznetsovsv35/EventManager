using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit.Sdk;

namespace EventManager.IntegrationTests;

public class DatabaseTestBase<T> : IAsyncLifetime where T : DbContext
{
    static readonly string PostgresImage = "postgres:16-alpine";
    protected string DatabaseName { get; set; } = "test-db";
    static readonly string UserName = "postgres";
    static readonly string Password = "postgres";

    protected const string Category = "Category";
    protected const string Category_Database = "Database";
    protected const string Category_Repositories = "Repositories";

    PostgreSqlContainer? _postgres;

    public Task DisposeAsync()
        => _postgres?.DisposeAsync().AsTask() ?? Task.CompletedTask;

    public virtual async Task InitializeAsync()
    {
        _postgres = new PostgreSqlBuilder(PostgresImage)
            .WithDatabase(DatabaseName)
            .WithUsername(UserName)
            .WithPassword(Password)
            .Build();       
        
        if (_postgres?.StartAsync() is Task task)
            await task;
    }

    protected T CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<T>()
            .UseNpgsql(_postgres?.GetConnectionString() ?? throw new InvalidOperationException("No container found"))
            .Options;

        T context = (T)Activator.CreateInstance(typeof(T), options)!;
        context.Database.EnsureCreated();
        return context;
    }

    protected async Task ResetDatabase()
    {
        NpgsqlConnection.ClearAllPools();
        await using var context = CreateDbContext();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
    }
}