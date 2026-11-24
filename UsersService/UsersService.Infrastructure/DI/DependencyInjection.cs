using Authorization.AspNetCore.DI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UsersService.Application;
using UsersService.Infrastructure.Services;

namespace UsersService.Infrastructure.DI;

public static partial class DependencyInjection
{
    public static IServiceCollection ConfigureUserAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.ConfigureJwtAuthentication(configuration);
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        return services;
    }
}
