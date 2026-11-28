using Common.Data;
using Common.Infrastructure.DI;
using Common.Infrastructure.Services;
using Database.Postgres.DI;
using EventsService.Application;
using EventsService.Database;
using EventsService.Database.Repositories;
using EventsService.Domain;
using EventsService.Kafka.DI;

namespace EventsService.DI;

public static partial class DependencyInjection
{
    public static IServiceCollection ConfigureInfrastructure(this IServiceCollection services,
        IConfiguration config,
        IWebHostEnvironment environment)
        => services
            .ConfigureDatabase<EventsDbContext>(config, environment.IsDevelopment())
            .AddScoped<IPaginator<Event>, PaginateService<Event>>()
            .AddScoped<IEventRepository, EventRepository>()
            .ConfigureKafka(config)
            .AddControllers().ConfigureJsonSerializer().Services
            .AddHealthChecks()
            .AddNpgSql(config.GetConnectionString("Default")!).Services;

    public static Task RunInfrastructure(this IServiceProvider serviceProvider, CancellationToken cancellation)
        => serviceProvider.PrepareDatabase<EventsDbContext>(cancellation, (dbContext, cancellation)
            => Task.CompletedTask);
}