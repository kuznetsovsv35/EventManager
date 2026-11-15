using Common.Middleware.ErrorHandling;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Middleware.DI;

public static class DependencyInjection
{
    public static IServiceCollection UseProblemDetailsMapper<T>(this IServiceCollection services) where T : class, IProblemDetailsMapper
        => services.AddSingleton<IProblemDetailsMapper, T>();

    public static IApplicationBuilder RegisterErrorHandler(this IApplicationBuilder application)
        => application.UseMiddleware<ErrorHandler>();
}
