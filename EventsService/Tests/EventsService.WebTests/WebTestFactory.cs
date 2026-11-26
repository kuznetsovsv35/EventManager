using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Microsoft.Extensions.Hosting;
using EventsService.Database;
using Common.Kafka;
using Common.Contracts.Kafka;
using Common.Kafka.Services;
using Common.Tests.AspNetCore;
using Common.Tests.AspNetCore.Services;

namespace EventsService.WebTests;

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
            RemoveService<DbContextOptions<EventsDbContext>>(services);
            // Исключение фонового процесс.
            RemoveService<IEventConsumerConfig<BookingActionMessage>>(services);
            RemoveService<IEventSubscriber<BookingActionMessage>>(services);
            RemoveService<IHostedService>(services, d => d.ImplementationType == typeof(EventConsumer<BookingActionMessage>));

            services.AddDbContext<EventsDbContext>(options =>
            {
                options.UseNpgsql(
                    _postgres.GetConnectionString(),
                    x => x.MigrationsAssembly(typeof(EventsDbContext).Assembly));
            });

            services.AddSingleton<ITokenGenerator, JwtTokenGenerator>();
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
