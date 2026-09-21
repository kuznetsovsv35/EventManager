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
        services.AddSingleton<ISyncContextFactory, SyncContextFactory>();
        return services;
    }

    public static async Task PrepareInfrastructure(this IServiceProvider serviceProvider, CancellationToken cancellation)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync(cancellation);
    }
}