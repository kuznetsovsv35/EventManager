using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using EventManager.Presentation.Middleware;

namespace EventManager.Presentation;

/// <summary>
/// Внедрение зависимости уровня представления.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddControllers().AddApplicationPart(typeof(DependencyInjection).Assembly);
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        return services;
    }
    public static IApplicationBuilder UseErrorHandler(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<ErrorHandler>();
        return builder;
    }
}