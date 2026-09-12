using EventManager.Data;
using EventManager.Models;

namespace EventManager.IntegrationTests;

public class DataTest : DatabaseTestBase<AppDbContext>
{
    protected const string Category_Filters = "Filters";
    protected const string Category_Paginator = "Paginator";
    
    public override async Task InitializeAsync()
    {
        DatabaseName = "data-test-db";
        await base.InitializeAsync();
        await ResetDatabase();
        await LoadTestData();
    }

    public const int EventCount = 30;
    const int EventDuration = 45;
    const int MinTotalSeats = 10;
    const int MaxTotalSeats = 100;
    static readonly DateTime StartAt = new DateTime(2026, 6, 28, 10, 0, 0).ToUniversalTime();
    static readonly DateTime EndAt = StartAt.AddDays(EventCount);
    async Task LoadTestData()
    {
        await using var db = CreateDbContext();
        db.Events.AddRange(
            [.. Enumerable.Range(1, EventCount)
            .Select(i => new Event(Random.Shared.Next(MinTotalSeats, MaxTotalSeats))
            {
                Title = $"Event Title {i}",
                StartAt = StartAt.AddDays(i - 1),
                EndAt = StartAt.AddDays(i - 1).AddMinutes(EventDuration),
                Description = $"Event Description {i}",
            })]
        );
        await db.SaveChangesAsync();
    }
}