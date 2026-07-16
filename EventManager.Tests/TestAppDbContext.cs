using System.Data;
using EventManager.Application.Interfaces;
using EventManager.Data;
using EventManager.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Tests;

public class TestAppDbContext : AppDbContext
{
    public static readonly int EventCount = 30;

    public static readonly int EventDuration = 45;

    public static readonly DateTime StartAt = new(2026, 6, 28, 10, 0, 0);

    public static readonly DateTime EndAt = StartAt.AddDays(EventCount);

    public string DatabaseName { get; }

    public TestAppDbContext(string databaseName) : base(
        new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options)
    {
        DatabaseName = databaseName;
        Database.EnsureCreated();
    }

    public IAppDbContext CreateNewInstance()
        => new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(DatabaseName)
            .Options);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Event>().HasData(
            [.. Enumerable.Range(1, EventCount)
            .Select(i => new Event(0)
            {
                Title = $"Event Title {i}",
                StartAt = StartAt.AddDays(i - 1),
                EndAt = StartAt.AddDays(i - 1).AddMinutes(EventDuration),
                Description = $"Event Description {i}"
            })]
        );
    }
}
