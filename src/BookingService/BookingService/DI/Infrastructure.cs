using BookingService.Application;
using BookingService.Application.Services;
using BookingService.Database;
using BookingService.Database.Repositories;
using BookingService.Services;
using Common.Services;
using Common.Synchronization;
using Database.Postgres.DI;

namespace BookingService.DI;

public static partial class DependencyInjection
{
    public static IServiceCollection ConfigureInfrastructure(this IServiceCollection services,
        IConfiguration configuration, 
        IWebHostEnvironment environment)
    {
        services.ConfigureDatabase<BookingsDbContext>(configuration!, environment.IsDevelopment());
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddSingleton<ISyncContextFactory, SyncContextFactory>();
        services.AddSingleton<IBookingServiceNotifier, BookingServiceNotifier>();
        services.AddHostedService<AppBackgroundService>();
        return services;        
    }

    public static Task RunInfrastructure(this IServiceProvider serviceProvider, CancellationToken cancellation)
        => serviceProvider.PrepareDatabase<BookingsDbContext>(cancellation, (dbContext, cancellation) 
            => Task.CompletedTask);    
}