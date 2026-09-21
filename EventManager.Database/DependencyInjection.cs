using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventManager.Database;

public static class DependencyInjection
{
    public static void AddDatabase(this IServiceCollection services, Action<DbContextOptionsBuilder> configure)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("Default") 
                ?? throw new InvalidOperationException("Нет строки подключения к БД");
            options.UseNpgsql(connectionString);

            if (environment.IsDevelopment())
            {
                options.LogTo(Console.WriteLine);
                options.EnableDetailedErrors();
            }
        });        
    }
}