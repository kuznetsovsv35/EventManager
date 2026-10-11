using EventManager.Database;
using EventManager.Domain.ValueObjects;
using EventManager.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace EventManager.IntegrationTests.Extensions;

public static class DbContextExtensions
{
    public static async Task<Guid> EnsureCreatedUser(this AppDbContext context, UserRole role)
    {
        var query = context.Users
            .AsNoTracking()
            .Where(u => u.Role == role)
            .Select(u => u.Id);
            
        if (await query.FirstOrDefaultAsync(CancellationToken.None) is Guid userId && userId != Guid.Empty)
            return userId;

        var hasher = new PasswordHasher();
        var login = role.ToString().ToLower();
        User user = new(login)
        {
            Role = role,
            Password = hasher.Hash(login),
        };

        context.Users.Add(user);
        await context.SaveChangesAsync(CancellationToken.None);

        return user.Id;
    }
}