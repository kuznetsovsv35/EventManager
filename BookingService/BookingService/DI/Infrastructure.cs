using BookingService.Application;
using BookingService.Application.Services;
using BookingService.Database;
using BookingService.Database.Repositories;
using BookingService.Kafka.DI;
using BookingService.Infrastructure.DI;
using Common.Infrastructure.DI;
using Common.Services;
using Common.Synchronization;
using Database.Postgres.DI;

namespace BookingService.DI;

public static partial class DependencyInjection
{
    public static IServiceCollection ConfigureInfrastructure(this IServiceCollection services,
        IConfiguration config,
        IWebHostEnvironment environment)
        => services
            .ConfigureDatabase<BookingsDbContext>(config, environment.IsDevelopment())
            .AddScoped<IBookingRepository, BookingRepository>()
            .AddSingleton<ISyncContextFactory, SyncContextFactory>()
            .AddSingleton<IBookingServiceNotifier, BookingServiceNotifier>()
            .AddBackgroundServices()
            .ConfigureKafka(config)
            .AddControllers().ConfigureJsonSerializer().Services;

    public static Task RunInfrastructure(this IServiceProvider serviceProvider, CancellationToken cancellation)
        => serviceProvider.PrepareDatabase<BookingsDbContext>(cancellation, (dbContext, cancellation)
            => Task.CompletedTask);
}