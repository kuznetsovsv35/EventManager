using Common.Infrastructure.DI;
using Database.Postgres.DI;
using UsersService.Application;
using UsersService.Application.Cryptography;
using UsersService.Database;
using UsersService.Database.Repositories;
using UsersService.Infrastructure.Services;

namespace UsersService.DI;

public static partial class DependencyInjection
{
    public static IServiceCollection ConfigureInfrastructure(this IServiceCollection services,
        IConfiguration config,
        IWebHostEnvironment env)
        => services
            .ConfigureDatabase<UsersDbContext>(config!, env.IsDevelopment())
            .AddScoped<IUserRepository, UserRepository>()
            .AddSingleton<IPasswordHasher, PasswordHasher>()
            .AddControllers().ConfigureJsonSerializer().Services
            .AddHealthChecks()
            .AddNpgSql(config.GetConnectionString("Default")!).Services;

    public static Task RunInfrastructure(this IServiceProvider serviceProvider, CancellationToken cancellation)
        => serviceProvider.PrepareDatabase<UsersDbContext>(cancellation, async (dbContext, cancellation)
            => await dbContext.SeedAsync(new PasswordHasher().Hash, cancellation));
}