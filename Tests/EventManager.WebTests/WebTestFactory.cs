using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using EventManager.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using EventManager.Application.Cryptography;
using EventManager.Infrastructure.Services;

namespace EventManager.WebTests;

public class WebTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    static readonly string PostgresImage = "postgres:16-alpine";
    static readonly string DatabaseName = "test-db";
    static readonly string UserName = "postgres";
    static readonly string Password = "postgres";
    readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder(PostgresImage)
            .WithDatabase(DatabaseName)
            .WithUsername(UserName)
            .WithPassword(Password)
            .Build();

    static void RemoveService<T>(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(x => x.ServiceType == typeof(T));
        if (descriptor is null)
            return;
        
        services.Remove(descriptor);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Подмена контекста данных.
            RemoveService<DbContextOptions<AppDbContext>>(services);
            // Исключение фонового процесс.
            RemoveService<AppBackgroundService>(services);

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(
                    _postgres.GetConnectionString(),
                    x => x.MigrationsAssembly(typeof(AppDbContext).Assembly));
            });
        });
    }

#region IAsyncLifetime
    async Task IAsyncLifetime.InitializeAsync()
    {
        await _postgres.StartAsync();

        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();

        await context.SeedAsync(Services.GetRequiredService<IPasswordHasher>().Hash, CancellationToken.None);
    }

    Task IAsyncLifetime.DisposeAsync()
        => _postgres.DisposeAsync().AsTask();
#endregion
}
