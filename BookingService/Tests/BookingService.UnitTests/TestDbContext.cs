using BookingService.Database;
using Microsoft.EntityFrameworkCore;

namespace BookingService.UnitTests;

/// <summary>
/// Репозиторий с тестовыми данными.
/// </summary>
class TestDbContext : BookingsDbContext
{
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
