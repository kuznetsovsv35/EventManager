using BookingService.Application;

namespace BookingService.DI;

public static partial class DependencyInjection
{
    public static IServiceCollection ConfigureApplication(this IServiceCollection services)
    {
        services.AddScoped<IBookingService, Application.Services.BookingService>();
        return services;
    }
}
