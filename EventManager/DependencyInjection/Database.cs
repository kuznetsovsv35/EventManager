using EventManager.Database;
using Microsoft.EntityFrameworkCore;

namespace EventManager.DependencyInjection;

public static partial class DependencyInjection
{
    public static IServiceCollection ConfigureDatabase(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Нет строки подключения к БД");

        services.AddDbContext<AppDbContext>(builder =>
        {
            builder.UseNpgsql(connectionString);

            if (environment.IsDevelopment())
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