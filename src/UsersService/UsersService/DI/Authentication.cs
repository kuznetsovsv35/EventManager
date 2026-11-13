using Authorization.AspNetCore.DI;
using UsersService.Application;
using UsersService.Services;

namespace UsersService.DI;

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
