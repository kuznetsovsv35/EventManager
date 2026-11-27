using BookingService.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BookingService.Infrastructure.DI;

public static class DependencyInjection
{
    public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
        => services.AddHostedService<AppBackgroundService>();
}
