using Database.Postgres.DI;
using UsersService.Application;
using UsersService.Application.Cryptography;
using UsersService.Database;
using UsersService.Database.Repositories;
using UsersService.Services;

namespace UsersService.DI;

public static partial class DependencyInjection
{
    public static IServiceCollection ConfigureInfrastructure(this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.ConfigureDatabase<UsersDbContext>(configuration!, environment.IsDevelopment());
        services.AddScoped<IUserRepository, UserRepository>();
        
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        return services;
    }

    public static Task RunInfrastructure(this IServiceProvider serviceProvider, CancellationToken cancellation)
        => serviceProvider.PrepareDatabase<UsersDbContext>(cancellation, (dbContext, cancellation) =>
        {
            return Task.CompletedTask;
        });    
}