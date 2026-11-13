using UsersService.Application;
using UsersService.Application.Services;

namespace UsersService.DI;

public static partial class DependencyInjection
{
    public static IServiceCollection ConfigureApplication(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        return services;
    }    
}