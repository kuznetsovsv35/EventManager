using EventManager.Application.Authorization;
using EventManager.Infrastructure.Configuration;
using EventManager.Presentation.Authorization;

namespace EventManager.DependencyInjection;

public static partial class DependencyInjection
{
    public static IServiceCollection ConfigureAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.AddScoped<IAppAuthorizationService, AppAuthorizationService>();
        services.AddAuthorization();
        return services;
    }
}