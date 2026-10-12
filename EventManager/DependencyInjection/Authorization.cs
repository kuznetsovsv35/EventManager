using EventManager.Application.Authorization;
using EventManager.Domain.ValueObjects;
using EventManager.Infrastructure.Configuration;
using EventManager.Presentation.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace EventManager.DependencyInjection;

public static partial class DependencyInjection
{
    static void ConfigurePolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(Policies.UserService.RegisterUser, policy => policy.RequireRole(UserRole.Admin.ToString()));
    }
    public static IServiceCollection ConfigureAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.AddScoped<IAppAuthorizationService, AppAuthorizationService>();
        services.AddAuthorization(options => options.ConfigurePolicies());
        return services;
    }
}