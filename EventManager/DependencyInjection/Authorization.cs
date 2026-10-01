
using EventManager.Application.Authorization;
using EventManager.Presentation.Authorization;

namespace EventManager.DependencyInjection;

public static partial class DependencyInjection
{
    public static IServiceCollection ConfigureAuthorization(this IServiceCollection services)
    {
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IAppAuthorizationService, AppAuthorizationService>();
        return services;
    }
}