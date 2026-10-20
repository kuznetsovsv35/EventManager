using EventManager.Domain.ValueObjects;

namespace EventManager.Database;

public static class DbSeeder
{
    static bool EnsureUserCreated(AppDbContext context, string login, UserRole role, Func<string?, string?> hasher)
    {
        if (!context.Users.Any(u => u.Login == login && u.Role == role))
        {
            var user = new User(login)
            {
                Role = role,
                Password = hasher(login)
            };
            context.Users.Add(user);
            return true;
        }
        return false;
    }

    public static async Task SeedAsync(this AppDbContext context, Func<string?, string?> hasher, CancellationToken cancellation)
    {
        var adminCreated = EnsureUserCreated(context, "admin", UserRole.Admin, hasher);
        var userCreated = EnsureUserCreated(context, "user", UserRole.User, hasher);
        if (adminCreated || userCreated)
            await context.SaveChangesAsync(cancellation);
    }
}
