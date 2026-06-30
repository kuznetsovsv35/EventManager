using System.Data;
using EventManager.Application.Interfaces;
using EventManager.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Tests;

public class TestAppDbContext : DbContext, IAppDbContext
{
    public static readonly int EventCount = 30;

    public static readonly int EventDuration = 45;

    public static readonly DateTime StartAt = new(2026, 6, 28, 10, 0, 0);

    public static readonly DateTime EndAt = StartAt.AddDays(EventCount);
    
    public TestAppDbContext() : base(
        new DbContextOptionsBuilder<TestAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options) => Database.EnsureCreated();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Event>().HasData(
            [.. Enumerable.Range(1, EventCount)
            .Select(i => new Event()
            {
                Id = Guid.NewGuid(),
                Title = $"Event Title {i}",
                StartAt = StartAt.AddDays(i - 1),
                EndAt = StartAt.AddDays(i - 1).AddMinutes(EventDuration),
                Description = $"Event Description {i}"
            })]
        );
    }

    public DbSet<Event> Events { get; set; }
    IQueryable<Event> IAppDbContext.Events => Events;

    static Event[] CreateTestData()
        => [.. Enumerable.Range(1, EventCount)
            .Select(i => new Event()
            {
                Id = Guid.NewGuid(),
                Title = $"Event Title {i}",
                StartAt = StartAt.AddDays(i - 1),
                EndAt = StartAt.AddDays(i - 1).AddMinutes(EventDuration),
                Description = $"Event Description {i}"
            })];

    public void Add(Event @event)
    {
        Events.Add(@event);
        SaveChanges();
    }

    public void Update(Event @event)
    {
        Events.Update(@event);
        SaveChanges();
    }

    public void Delete(Event @event)
    {
        Events.Remove(@event);
        SaveChanges();
    }
}