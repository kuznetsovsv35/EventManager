using Authorization.AspNetCore.Extensions;
using Authorization.AspNetCore.Services;
using Authorization.Configuration;
using Authorization.Domain;
using Authorization.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Authorization.AspNetCore.DI;

public static partial class DependencyInjection
{
    public static IServiceCollection ConfigureJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(option =>
            {
                JwtSettings settings = configuration.GetSection("Jwt").Get<JwtSettings>() ?? new();
                option.TokenValidationParameters = new TokenValidationParameters().Configure(settings);
            });

        return services;
    }

    public static AuthorizationOptions AddPolicy(
        this AuthorizationOptions options,
        string name,
        params UserRole[] roles)
    {
        options.AddPolicy(name, builder =>
        {
            builder.RequireAuthenticatedUser();
            builder.RequireRole(roles.Select(x => x.ToString()));
            builder.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
        });

        return options;
    }

    public static IServiceCollection ConfigureJwtAuthorization(
        this IServiceCollection services, 
        IConfiguration configuration,
        Action<AuthorizationOptions> configurePolicies)
    {
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.AddScoped<IAppAuthorizationService, AppAuthorizationService>();
        services.AddAuthorization(options => configurePolicies(options));
        services.AddHttpContextAccessor();

        return services;
    }
}
