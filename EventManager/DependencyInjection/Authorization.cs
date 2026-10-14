using EventManager.Application.Authorization;
using EventManager.Domain.ValueObjects;
using EventManager.Infrastructure.Configuration;
using EventManager.Presentation.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace EventManager.DependencyInjection;

public static partial class DependencyInjection
{
    static AuthorizationOptions AddPolicy(
        this AuthorizationOptions options,
        string name,
        params UserRole[] roles)
    {
        options.AddPolicy(name, builder =>
        {
            builder.RequireAuthenticatedUser();
            builder.RequireRole(string.Join(",", roles));
            builder.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
        });

        return options;
    }

    static void ConfigurePolicies(this AuthorizationOptions options)
    {
        options
            .AddPolicy(Policies.UserService.RegisterUser, UserRole.Admin);
    }

    public static IServiceCollection ConfigureAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.AddScoped<IAppAuthorizationService, AppAuthorizationService>();
        services.AddAuthorization(options => options.ConfigurePolicies());
        return services;
    }
}