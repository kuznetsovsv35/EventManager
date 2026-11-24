using BookingService.Database;
using Microsoft.EntityFrameworkCore;

namespace BookingService.UnitTests;

/// <summary>
/// Репозиторий с тестовыми данными.
/// </summary>
class TestDbContext : BookingsDbContext
{
    /*
    internal const int EventCount = 30;

    internal const int EventDuration = 45;

    internal const int MinTotalSeats = 2;

    internal const int MaxTotalSeats = 100;

    internal static readonly string AdminLogin = "admin";
    internal static readonly string AdminPassword = "admin";

    internal static readonly string UserLogin = "user";
    internal static readonly string UserPassword = "user";

    internal static readonly DateTime StartAt = DateTime.UtcNow.AddMonths(1);

    internal static readonly DateTime EndAt = StartAt.AddDays(EventCount);
    */

    internal string DatabaseName { get; }

    internal TestDbContext(string databaseName) : base(
        new DbContextOptionsBuilder<BookingsDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options)
    {
        DatabaseName = databaseName;
        Database.EnsureCreated();
    }

    internal BookingsDbContext CreateNewInstance()
        => new BookingsDbContext(
            new DbContextOptionsBuilder<BookingsDbContext>()
            .UseInMemoryDatabase(DatabaseName)
            .Options);
}
