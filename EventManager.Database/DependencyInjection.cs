using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventManager.Database;

public static class DependencyInjection
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString, bool isDevelopment)
    {
        if (connectionString is null || connectionString == string.Empty)
            throw new ArgumentException("Нет строки подключения к БД", nameof(connectionString));
        
        services.AddDbContext<AppDbContext>(builder =>
        {
            builder.UseNpgsql(connectionString);
            
            if (isDevelopment)
            {
                builder.LogTo(Console.WriteLine);
                builder.EnableDetailedErrors();
            }
        });
        return services;
    }

    public static async Task PrepareDatabase(this IServiceProvider serviceProvider, CancellationToken cancellation)
    {        
        await using var scope = serviceProvider.CreateAsyncScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync(cancellation);
    }
}