using EventManager.Application.Interfaces;
using EventManager.Data;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Infrastructure;

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
    
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
           options.UseInMemoryDatabase("EventDb"); 
        });

        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());
        return services;
    }
}