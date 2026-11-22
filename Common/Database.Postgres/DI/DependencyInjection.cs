using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Postgres.DI;

public static partial class DependencyInjection
{
    public static IServiceCollection ConfigureDatabase<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        bool useDetailLog) where TContext : DbContext
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Нет строки подключения к БД");

        services.AddDbContext<TContext>(builder =>
        {
            builder.UseNpgsql(connectionString);

            if (useDetailLog)
            {
                builder.LogTo(Console.WriteLine);
                builder.EnableDetailedErrors();
            }
        });
        return services;
    }

    public static async Task PrepareDatabase<TContext>(
        this IServiceProvider serviceProvider, 
        CancellationToken cancellation,
        Func<TContext, CancellationToken, Task> seeder) where TContext : DbContext
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
        await dbContext.Database.MigrateAsync(cancellation);
        await seeder(dbContext, cancellation);
    }
}