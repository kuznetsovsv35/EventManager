using EventManager.Application.Authorization;
using EventManager.Domain.ValueObjects;
using EventManager.Infrastructure.Configuration;
using EventManager.Presentation.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace EventManager.DependencyInjection;

public static partial class DependencyInjection
{
    static void ConfigurePolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(Policies.UserService.RegisterUser, builder => 
        {
            builder.RequireAuthenticatedUser();
            builder.RequireRole(UserRole.Admin.ToString());
            builder.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
        });
    }
    public static IServiceCollection ConfigureAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.AddScoped<IAppAuthorizationService, AppAuthorizationService>();
        services.AddAuthorization(options => options.ConfigurePolicies());
        return services;
    }
}