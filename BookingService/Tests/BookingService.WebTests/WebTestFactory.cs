using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Microsoft.Extensions.Hosting;
using BookingService.Database;
using BookingService.Infrastructure.Services;
using Common.Kafka;
using Common.Tests.AspNetCore;
using Common.Tests.AspNetCore.Services;
using Moq;

namespace BookingService.WebTests;

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
            RemoveService<DbContextOptions<BookingsDbContext>>(services);
            // Исключение фонового процесс.
            RemoveService<IHostedService>(services, d => d.ImplementationType == typeof(AppBackgroundService));
            // Kafka
            RemoveService<IEventPublisherConfig>(services);
            RemoveService<IEventPublisher>(services);

            services.AddDbContext<BookingsDbContext>(options =>
            {
                options.UseNpgsql(
                    _postgres.GetConnectionString(),
                    x => x.MigrationsAssembly(typeof(BookingsDbContext).Assembly));
            });

            services.AddSingleton<ITokenGenerator, JwtTokenGenerator>();
            services.AddSingleton(_ => new Mock<IEventPublisher>().Object);
        });
    }

    #region IAsyncLifetime
    Task IAsyncLifetime.InitializeAsync() => _postgres.StartAsync();

    Task IAsyncLifetime.DisposeAsync() => _postgres.DisposeAsync().AsTask();
    #endregion

    #region Token generator
    public ITokenGenerator TokenGenerator => _tokenGenerator
        ??= Services.GetRequiredService<ITokenGenerator>();
    ITokenGenerator? _tokenGenerator;
    #endregion
}
