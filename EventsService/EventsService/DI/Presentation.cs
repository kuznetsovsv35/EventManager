using Authorization.AspNetCore.DI;
using Authorization.Domain;
using Common.Middleware.DI;
using Common.Swagger;
using EventsService.Application.Security;
using EventsService.Middleware;

namespace EventsService.DI;

public static partial class DependencyInjection
{
    public static IServiceCollection ConfigurePresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.UseProblemDetailsMapper<EventsProblemDetailsMapper>();
        services.ConfigureJwtAuthentication(configuration);
        services.ConfigureJwtAuthorization(configuration, options =>
        {
            options
                .AddPolicy(Policy.CreateEvent, UserRole.Admin)
                .AddPolicy(Policy.DeleteEvent, UserRole.Admin)
                .AddPolicy(Policy.GetEvents, UserRole.Admin, UserRole.User)
                .AddPolicy(Policy.ModifyEvent, UserRole.Admin);
        });

        services.AddControllers();
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