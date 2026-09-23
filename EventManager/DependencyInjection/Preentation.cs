using System.Reflection;
using EventManager.Presentation.Middleware;

namespace EventManager.DependencyInjection;

public static partial class DependencyInjection
{    
    public static IServiceCollection ConfigurePresentation(this IServiceCollection services)
    {
        services.AddControllers().AddApplicationPart(Assembly.Load($"{nameof(EventManager)}.{nameof(Presentation)}"));
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
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

        // В разработке работа с API в веб-интерфейсе.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        
        // Редирект
        app.UseHttpsRedirection();
        // Контролеры.
        app.MapControllers();
        return app;
    }
}