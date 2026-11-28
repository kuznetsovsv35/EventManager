using EventsService.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventsService.UnitTests;

public static class Extensions
{
    internal static async Task<Guid> GetRandomEventId(this IServiceProvider provider, CancellationToken cancellation)
    {
        await using var scope = provider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EventsDbContext>();
        int eventCount = await dbContext.Events.AsNoTracking().CountAsync(cancellation);
        var eventIndex = Random.Shared.Next(eventCount);
        return (await dbContext.Events.AsNoTracking().Skip(eventIndex).FirstAsync(cancellation)).Id;
    }
}
