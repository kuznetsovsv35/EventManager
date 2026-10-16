using EventManager.Application.DataTransferObjects;
using EventManager.Database;
using EventManager.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventManager.Tests;

static class TestContextExtensions
{
    internal static async Task<Guid> GetRandomEventId(this IServiceProvider provider, CancellationToken cancellation)
    {
        await using var scope = provider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        int eventCount = await dbContext.Events.AsNoTracking().CountAsync(cancellation);
        var eventIndex = Random.Shared.Next(eventCount);
        return (await dbContext.Events.AsNoTracking().Skip(eventIndex).FirstAsync(cancellation)).Id;
    }
    internal static async Task<UserInfo> GetUserInfo(this IServiceProvider services, string login, UserRole role)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return (await dbContext.Users
            .AsNoTracking()
            .SingleAsync(u => u.Login == login && u.Role == role && u.IsActive))
            .ToInfo();
    }
}