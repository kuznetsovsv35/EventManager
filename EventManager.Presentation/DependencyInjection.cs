using Microsoft.Extensions.DependencyInjection;

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
}