using Common.Data;
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
    {
        services.ConfigureDatabase<EventsDbContext>(config, environment.IsDevelopment());

        services.AddScoped<IPaginator<Event>, PaginateService<Event>>();
        services.AddScoped<IEventRepository, EventRepository>();

        services.ConfigureKafka(config);

        return services;

    }
    public static Task RunInfrastructure(this IServiceProvider serviceProvider, CancellationToken cancellation)
        => serviceProvider.PrepareDatabase<EventsDbContext>(cancellation, (dbContext, cancellation)
            => Task.CompletedTask);
}