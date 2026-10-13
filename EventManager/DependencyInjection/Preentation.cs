using System.Reflection;
using EventManager.Presentation.Middleware;

namespace EventManager.DependencyInjection;

public static partial class DependencyInjection
{
    public static IServiceCollection ConfigurePresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.ConfigureAuthentication(configuration);
        services.ConfigureAuthorization(configuration);

        services.AddHttpContextAccessor();
        services.AddControllers().AddApplicationPart(Assembly.Load($"{nameof(EventManager)}.{nameof(Presentation)}"));
        services.AddEndpointsApiExplorer();
        services.ConfigureSwagger();
        return services;
    }

    public static WebApplication ConfigureMiddleware(this WebApplication app)
    {
        app.UseMiddleware<ErrorHandler>();
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

        // В разработке работа с API в веб-интерфейсе.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        return app;
    }
}