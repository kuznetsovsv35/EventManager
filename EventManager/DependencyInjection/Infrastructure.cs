using EventManager.Common.Interfaces;
using EventManager.Common.Services;
using EventManager.Application.Interfaces;
using EventManager.Infrastructure.Services;

namespace EventManager.DependencyInjection;

public static partial class DependencyInjection
{
    public static IServiceCollection ConfigureInfrastructure(this IServiceCollection services, 
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.ConfigureDatabase(configuration!, environment);
        
        services.AddSingleton<ISyncContextFactory, SyncContextFactory>();
        services.AddSingleton<IBookingServiceNotifier, BookingServiceNotifier>();
        
        return services;
    }
    
    public static Task RunInfrastructure(this IServiceProvider serviceProvider, CancellationToken cancellation)
        => serviceProvider.PrepareDatabase(cancellation);
}