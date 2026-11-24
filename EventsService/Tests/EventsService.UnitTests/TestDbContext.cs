using EventsService.Database;
using EventsService.Domain;
using Microsoft.EntityFrameworkCore;

namespace EventsService.UnitTests;

/// <summary>
/// Репозиторий с тестовыми данными.
/// </summary>
class TestDbContext : EventsDbContext
{
    internal const int EventCount = 30;

    internal const int EventDuration = 45;

    internal const int MinTotalSeats = 2;

    internal const int MaxTotalSeats = 100;

    internal static readonly DateTime StartAt = DateTime.UtcNow.AddMonths(1);

    internal static readonly DateTime EndAt = StartAt.AddDays(EventCount);

    internal string DatabaseName { get; }

    internal TestDbContext(string databaseName) : base(
        new DbContextOptionsBuilder<EventsDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options)
    {
        DatabaseName = databaseName;
        Database.EnsureCreated();
    }

    internal EventsDbContext CreateNewInstance()
        => new EventsDbContext(
            new DbContextOptionsBuilder<EventsDbContext>()
            .UseInMemoryDatabase(DatabaseName)
            .Options);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Event>().HasData(
            [.. Enumerable.Range(1, EventCount)
            .Select(i => new Event(Random.Shared.Next(MinTotalSeats, MaxTotalSeats + 1))
            {
                Title = $"Event Title {i}",
                StartAt = StartAt.AddDays(i - 1),
                EndAt = StartAt.AddDays(i - 1).AddMinutes(EventDuration),
                Description = $"Event Description {i}",
            })]
        );
    }
}
