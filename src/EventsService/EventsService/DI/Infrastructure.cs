using Database.Postgres.DI;
using EventsService.Application;
using EventsService.Database;
using EventsService.Database.Repositories;
using EventsService.Domain;
using EventsService.Services;

namespace EventsService.DI;

public static partial class DependencyInjection
{
    public static IServiceCollection ConfigureInfrastructure(this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.ConfigureDatabase<EventsDbContext>(configuration!, environment.IsDevelopment());

        services.AddScoped<IPaginator<Event>, PaginateService<Event>>();
        services.AddScoped<IEventRepository, EventRepository>();

        return services;
        
    }
    public static Task RunInfrastructure(this IServiceProvider serviceProvider, CancellationToken cancellation)
        => serviceProvider.PrepareDatabase<EventsDbContext>(cancellation, (dbContext, cancellation)
            => Task.CompletedTask);    
}