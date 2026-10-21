using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using EventManager.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using EventManager.Application.Cryptography;
using EventManager.Infrastructure.Services;
using Microsoft.Extensions.Hosting;

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

    static void RemoveService<T>(IServiceCollection services, Func<ServiceDescriptor, bool>? predicate = null)
    {
        var descriptor = services
            .SingleOrDefault(x => x.ServiceType == typeof(T) && (predicate is null ? true : predicate(x)));
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
            RemoveService<IHostedService>(services, d => d.ImplementationType == typeof(AppBackgroundService));

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
