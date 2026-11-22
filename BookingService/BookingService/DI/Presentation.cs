using Authorization.AspNetCore.DI;
using Authorization.Domain;
using BookingService.Application.Security;
using BookingService.Middleware;
using Common.Middleware.DI;
using Common.Swagger;

namespace BookingService.DI;

public static partial class DependencyInjection
{
    public static IServiceCollection ConfigurePresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.UseProblemDetailsMapper<BookingsProblemDetailsMapper>();
        services.ConfigureJwtAuthentication(configuration);
        services.ConfigureJwtAuthorization(configuration, options =>
        {
            options
                .AddPolicy(Policy.CreateBooking, UserRole.Admin, UserRole.User)
                .AddPolicy(Policy.GetBooking, UserRole.Admin, UserRole.User)
                .AddPolicy(Policy.CancelBooking, UserRole.Admin, UserRole.User);
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