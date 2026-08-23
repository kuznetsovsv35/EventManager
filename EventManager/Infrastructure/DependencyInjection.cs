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

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, 
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            //options.UseInMemoryDatabase($"{nameof(EventManager)}.db");
            var connectionString = configuration.GetConnectionString("Default") 
                ?? throw new InvalidOperationException("Нет строки подключения к БД");
            var builder = options.UseNpgsql(connectionString);

            if (environment.IsDevelopment())
            {
                builder.LogTo(Console.WriteLine);
                options.EnableDetailedErrors();
            }
        });

        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());
        return services;
    }
}