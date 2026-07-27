using System.Data;
using EventManager.Application.Interfaces;
using EventManager.Data;
using EventManager.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Tests;

/// <summary>
/// Репозиторий с тестовыми данными.
/// </summary>
class TestAppDbContext : AppDbContext
{
    internal const int EventCount = 30;

    internal const int EventDuration = 45;

    internal const int MinTotalSeats = 10;

    internal const int MaxTotalSeats = 100;

    internal static readonly DateTime StartAt = new(2026, 6, 28, 10, 0, 0);

    internal static readonly DateTime EndAt = StartAt.AddDays(EventCount);

    internal string DatabaseName { get; }

    internal TestAppDbContext(string databaseName) : base(
        new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options)
    {
        DatabaseName = databaseName;
        Database.EnsureCreated();
    }

    internal IAppDbContext CreateNewInstance()
        => new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(DatabaseName)
            .Options);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Event>().HasData(
            [.. Enumerable.Range(1, EventCount)
            .Select(i => new Event(Random.Shared.Next(MinTotalSeats, MaxTotalSeats))
            {
                Title = $"Event Title {i}",
                StartAt = StartAt.AddDays(i - 1),
                EndAt = StartAt.AddDays(i - 1).AddMinutes(EventDuration),
                Description = $"Event Description {i}",
            })]
        );
    }
}
