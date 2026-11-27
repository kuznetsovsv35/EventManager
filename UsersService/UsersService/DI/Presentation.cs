using Authorization.AspNetCore.DI;
using Authorization.Domain;
using Common.Infrastructure.DI;
using Common.Middleware.DI;
using Common.Swagger;
using UsersService.Application.Security;
using UsersService.Infrastructure.DI;
using UsersService.Middleware;

namespace UsersService.DI;

public static partial class DependencyInjection
{
    public static IServiceCollection ConfigurePresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.UseProblemDetailsMapper<UsersProblemDetailsMapper>();
        services.ConfigureUserAuthentication(configuration);
        services.ConfigureJwtAuthorization(configuration, options =>
        {
            options
                .AddPolicy(Policy.RegisterUser, UserRole.Admin)
                .AddPolicy(Policy.ModifyUser, UserRole.Admin)
                .AddPolicy(Policy.ChangePassword, UserRole.Admin, UserRole.User)
                .AddPolicy(Policy.DeleteUser, UserRole.Admin);
        });

        services.AddControllers().ConfigureJsonSerializer();

        services.AddEndpointsApiExplorer();
        services.ConfigureSwagger();
        return services;
    }

    public static IApplicationBuilder ConfigureMiddleware(this IApplicationBuilder application)
    {
        application.RegisterErrorHandler();
        return application;
    }

    public static IApplicationBuilder RunPresentation(this IApplicationBuilder application)
    {
        application.ConfigureMiddleware();

        // Редирект
        application.UseHttpsRedirection();
        // Аутентификация
        application.UseAuthentication();
        // Авторизация
        application.UseAuthorization();
        // Контролеры.
        (application as IEndpointRouteBuilder)?.MapControllers();

        // API в веб-интерфейсе.
        application.RunSwagger();

        return application;
    }
}
