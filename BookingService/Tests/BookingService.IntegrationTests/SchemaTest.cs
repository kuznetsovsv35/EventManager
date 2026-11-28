using BookingService.Application.DTO;
using BookingService.Domain;
using Common.Tests;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BookingService.IntegrationTests;

[Collection(nameof(ContainersCollection))]
public class SchemaTest(ContainerWrapper testContainer) : DatabaseTestBase(testContainer)
{
    [Trait(Category.Name, Category.Database)]
    [Fact]
    public Task TestMigration() => ExecuteTestMigrations(Assert.Equal);

    [Trait(Category.Name, Category.Database)]
    [Fact]
    public Task TestConnection() => ExecuteTestConnection(Assert.True);

    [Trait(Category.Name, Category.Database)]
    [Fact]
    public async Task BookingsColumnsTest_Success()
    {
        // Given
        await ResetDatabase();

        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();


        var booking = new Booking(eventId, userId);
        var infoBooking = booking.ToInfo();

        // When
        await using var work = CreateDbContext();
        work.Bookings.Add(booking);
        await work.SaveChangesAsync();

        // Then
        await using var verify = CreateDbContext();
        var bookingFound = await verify
            .Bookings.FromSqlInterpolated($@"
                SELECT ""Id"", ""EventId"", ""UserId"", ""Status"", ""CreatedAt"", ""ProcessedAt""
                FROM ""Bookings""
                WHERE ""Id"" = {booking.Id}
            ")
            .AsNoTracking()
            .SingleAsync();

        Assert.Equal(eventId, bookingFound.EventId);
        Assert.Equal(userId, bookingFound.UserId);
        Assert.Equal(infoBooking, bookingFound.ToInfo());
    }

    [Trait(Category.Name, Category.Database)]
    [Fact]
    public async Task BookingsPrimaryKeyTest()
    {
        // Given
        await ResetDatabase();
        await using var context = CreateDbContext();
        var db = context.Database;
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var startAt = DateTime.UtcNow;
        var endAt = startAt.AddHours(10);
        var bookingId = Guid.NewGuid();
        var status = BookingStatus.Pending;
        var createdAt = DateTime.UtcNow;

        // When
        await db.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO ""Bookings""(""Id"", ""EventId"", ""UserId"", ""Status"", ""CreatedAt"")
            VALUES({bookingId}, {eventId}, {userId}, {status}, {createdAt})
        ");

        // Then
        var ex = await Assert.ThrowsAnyAsync<PostgresException>(
            async () => await db.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO ""Bookings""(""Id"", ""EventId"", ""UserId"", ""Status"", ""CreatedAt"")
                VALUES({bookingId}, {eventId}, {userId}, {status}, {createdAt})
            "));
        ex.CheckUniqueConstraint("Bookings", "Id");
    }

    [Trait(Category.Name, Category.Database)]
    [Fact]
    public async Task BookingsConstraintsTest()
    {
        // Given
        await ResetDatabase();

        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var booking = new Booking(eventId, userId);

        await using var context = CreateDbContext();
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        // When
        await using var work = CreateDbContext();
        var exIdNull = await Assert.ThrowsAnyAsync<PostgresException>(async () => await work.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""Bookings""
            SET ""Id"" = NULL
            WHERE ""Id"" = {booking.Id}
            "));

        var exEventIdNull = await Assert.ThrowsAnyAsync<PostgresException>(async () => await work.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""Bookings""
            SET ""EventId"" = NULL
            WHERE ""Id"" = {booking.Id}
            "));

        var exStatusNull = await Assert.ThrowsAnyAsync<PostgresException>(async () => await work.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""Bookings""
            SET ""Status"" = NULL
            WHERE ""Id"" = {booking.Id}
            "));

        var exCreatedAtNull = await Assert.ThrowsAnyAsync<PostgresException>(async () => await work.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""Bookings""
            SET ""CreatedAt"" = NULL
            WHERE ""Id"" = {booking.Id}
            "));

        // Then
        exIdNull.CheckNullConstraint("Bookings", "Id");
        exEventIdNull.CheckNullConstraint("Bookings", "EventId");
        exStatusNull.CheckNullConstraint("Booking", "Status");
        exCreatedAtNull.CheckNullConstraint("Bookings", "CreatedAt");
    }

}
