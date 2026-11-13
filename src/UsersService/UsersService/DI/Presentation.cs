using Authorization.AspNetCore.DI;
using Authorization.Domain;
using Common.Swagger;
using UsersService.Application.Security;

namespace UsersService.DI;

public static partial class DependencyInjection
{
    public static IServiceCollection ConfigurePresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.ConfigureUserAuthentication(configuration);
        services.ConfigureJwtAuthorization(configuration, options =>
        {
            options
            .AddPolicy(Policy.RegisterUser, UserRole.Admin)
            .AddPolicy(Policy.ModifyUser, UserRole.Admin)
            .AddPolicy(Policy.ChangePassword, UserRole.Admin, UserRole.User)
            .AddPolicy(Policy.DeleteUser, UserRole.Admin);
        });

        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.ConfigureSwagger();
        return services;
    }

    public static WebApplication ConfigureMiddleware(this WebApplication app)
    {
        // app.UseMiddleware<ErrorHandler>();
        return app;
    }

    public static WebApplication RunPresentation(this WebApplication app)
    {
        app.ConfigureMiddleware();

        // Редирект
        app.UseHttpsRedirection();
        // Аутентификация
        app.UseAuthentication();
        // Авторизация
        app.UseAuthorization();
        // Контролеры.
        app.MapControllers();

        // API в веб-интерфейсе.
        app.RunSwagger();

        return app;
    }
}    
