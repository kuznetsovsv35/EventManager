using EventManager.Data;
using EventManager.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Tests;

public class TestAppDbContext : AppDbContext
{
    public TestAppDbContext(DbContextOptions<AppDbContext> options) : base(options) => Database.EnsureCreated();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Event>().HasData(TestData);
    }

    public static readonly DateTime StartAt = DateTime.Now;

    public static readonly int EventCount = 30;

    public static readonly Event[] TestData = CreateTestData();

    static Event[] CreateTestData()
        => [.. Enumerable.Range(1, EventCount)
            .Select(i => new Event()
            {
                Id = Guid.NewGuid(),
                Title = $"Event Title {i}",
                StartAt = StartAt.AddDays(i - 1),
                EndAt = StartAt.AddDays(i - 1).AddMinutes(30),
                Description = $"Event Description {i}"
            })];
}