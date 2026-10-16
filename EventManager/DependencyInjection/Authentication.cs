using EventManager.Infrastructure.Authentication;
using EventManager.Infrastructure.Authorization;
using EventManager.Infrastructure.Configuration;
using EventManager.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace EventManager.DependencyInjection;

public static partial class DependencyInjection
{
    public static IServiceCollection ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(option =>
            {
                JwtSettings settings = configuration.GetSection("Jwt").Get<JwtSettings>() ?? new();
                option.TokenValidationParameters = new TokenValidationParameters().Configure(settings);
            });

        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        return services;
    }
}