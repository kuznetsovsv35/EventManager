using EventManager.Database;
using EventManager.Common.Interfaces;
using EventManager.Common.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using EventManager.Infrastructure.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using EventManager.Domain.ValueObjects;
using EventManager.Application.Interfaces;
using EventManager.Infrastructure.Services;
using EventManager.Application.Services;
using EventManager.Infrastructure.DataAccess;
using EventManager.Application.DataAccess;
using System.Threading.Channels;

namespace EventManager.Infrastructure;

/// <summary>
/// Внедрение зависимости инфраструктуры приложения.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IFilter<Event>, FilterService<Event>>();
        services.AddScoped<IPaginator<Event>, PaginateService<Event>>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();        
        services.AddHostedService<AppBackgroundService>();
        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, 
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddDatabase(
            configuration.GetConnectionString("Default")!, 
            environment.IsDevelopment());
        
        services.AddSingleton<ISyncContextFactory, SyncContextFactory>();

        services.AddSingleton(_ => Channel.CreateBounded<Guid>(new BoundedChannelOptions(1)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleWriter = false,
            SingleReader = true
        }));
        
        return services;
    }

    public static IApplicationBuilder UseErrorHandler(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<ErrorHandler>();
        return builder;
    }

    public static Task PrepareInfrastructure(this IServiceProvider serviceProvider, CancellationToken cancellation)
        => serviceProvider.PrepareDatabase(cancellation);
}