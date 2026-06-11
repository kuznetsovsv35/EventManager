using EventManager.Data;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
           options.UseInMemoryDatabase("EventDb"); 
        });
        return services;
    }
}