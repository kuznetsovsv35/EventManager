using EventManager.Database;
using EventManager.Common.Interfaces;
using EventManager.Common.Services;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Infrastructure.Services;

/// <summary>
/// Внедрение зависимости инфраструктуры приложения.
/// </summary>
public static class DependencyInjection
{
    public static IApplicationBuilder UseErrorHandler(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<ErrorHandler>();
        return builder;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, 
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddDatabase(
            configuration.GetConnectionString("Default")!, 
            environment.IsDevelopment());
        
        services.AddSingleton<ISyncContextFactory, SyncContextFactory>();
        return services;
    }

    public static Task PrepareInfrastructure(this IServiceProvider serviceProvider, CancellationToken cancellation)
        => serviceProvider.PrepareDatabase(cancellation);
}