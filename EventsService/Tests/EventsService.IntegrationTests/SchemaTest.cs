using Common.Tests;
using EventsService.Application.DTO;
using EventsService.Domain;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Org.BouncyCastle.Tls.Crypto.Impl;

namespace EventsService.IntegrationTests;

[Collection(nameof(ContainersCollection))]
public class SchemaTest(ContainerWrapper testContainer) : DatabaseTestBase(testContainer)
{
    [Trait(Category.Name, Category.Database)]
    [Fact]
    public Task TestMigrations() => ExecuteTestMigrations(Assert.Equal);

    [Trait(Category.Name, Category.Database)]
    [Fact]
    public Task TestConnection() => ExecuteTestConnection(Assert.True);

    [Trait(Category.Name, Category.Database)]
    [Fact]
    public async Task EventsColumnsTest_Success()
    {
        // Given
        await ResetDatabase();

        Event @event = new(10)
        {
            Title = "Event",
            Description = "Description",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1)
        };
        var info = @event.ToOutputData();

        // When
        await using var work = CreateDbContext();
        work.Events.Add(@event);
        await work.SaveChangesAsync();

        // Then
        await using var verify = CreateDbContext();
        var eventFound = await verify.Events.FromSqlInterpolated($@"
                SELECT *
                FROM ""Events""
                WHERE ""Id"" = {@event.Id}
            ")
            .SingleOrDefaultAsync();

        var infoFound = eventFound?.ToOutputData();

        Assert.NotNull(infoFound);
        Assert.Equal(info, infoFound);
    }

    [Trait(Category.Name, Category.Database)]
    [Fact]
    public async Task EventsPrimaryKeyTest()
    {
        // Given
        await ResetDatabase();
        await using var context = CreateDbContext();
        var db = context.Database;
        var id = Guid.NewGuid();
        var title = "Simple event";
        var startAt = DateTime.UtcNow;
        var endAt = startAt.AddHours(10);
        var totalSeats = 10;

        // When
        await db.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO ""Events""(""Id"", ""Title"", ""StartAt"", ""EndAt"", ""TotalSeats"") 
            VALUES({id}, {title}, {startAt}, {endAt}, {totalSeats})
        ");

        // Then
        var ex = await Assert.ThrowsAnyAsync<PostgresException>(
            async () => await db.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO ""Events""(""Id"", ""Title"", ""StartAt"", ""EndAt"", ""TotalSeats"") 
                VALUES({id}, {title}, {startAt}, {endAt}, {totalSeats})
            "));

        ex.CheckUniqueConstraint("Events", "Id");
    }

    [Trait(Category.Name, Category.Database)]
    [Fact]
    public async Task EventsConstraintsTest()
    {
        // Given
        await ResetDatabase();

        Event @event = new(10)
        {
            Title = "Event",
            Description = "Description",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1)
        };

        await using var context = CreateDbContext();
        context.Events.Add(@event);
        await context.SaveChangesAsync();

        // When
        await using var work = CreateDbContext();

        var exIdNull = await Assert.ThrowsAnyAsync<PostgresException>(async () => await work.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""Events"" 
            SET ""Id"" = NULL 
            WHERE ""Id"" = {@event.Id};
            "));
        var exTitleNull = await Assert.ThrowsAnyAsync<PostgresException>(async () => await work.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""Events"" 
            SET ""Title"" = NULL 
            WHERE ""Id"" = {@event.Id};
            "));

        var exStartAtNull = await Assert.ThrowsAnyAsync<PostgresException>(async () => await work.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""Events"" 
            SET ""StartAt"" = NULL 
            WHERE ""Id"" = {@event.Id};
            "));
        var exEndAtNull = await Assert.ThrowsAnyAsync<PostgresException>(async () => await work.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""Events"" 
            SET ""EndAt"" = NULL 
            WHERE ""Id"" = {@event.Id};
            "));

        var exTotalSeatsNull = await Assert.ThrowsAnyAsync<PostgresException>(async () => await work.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""Events"" 
            SET ""TotalSeats"" = NULL 
            WHERE ""Id"" = {@event.Id};
            "));

        var exReservedCountNull = await Assert.ThrowsAnyAsync<PostgresException>(async () => await work.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""Events"" 
            SET ""ReservedSeats"" = NULL 
            WHERE ""Id"" = {@event.Id};
            "));

        var exTotalSeats = await Assert.ThrowsAnyAsync<PostgresException>(async () => await work.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""Events""
            SET ""TotalSeats"" = 0
            WHERE ""Id"" = {@event.Id}
            "));

        var exReservedCount = await Assert.ThrowsAnyAsync<PostgresException>(async () => await work.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""Events""
            SET ""ReservedSeats"" = -1
            WHERE ""Id"" = {@event.Id}
            "));

        var startAt = DateTime.UtcNow;
        var exStartEndAt = await Assert.ThrowsAnyAsync<PostgresException>(async () => await work.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""Events""
            SET
                ""StartAt"" = {startAt},
                ""EndAt"" = {startAt}
            WHERE ""Id"" = {@event.Id}
            "));

        // Then        
        exIdNull.CheckNullConstraint("Events", "Id");
        exTitleNull.CheckNullConstraint("Events", "Title");
        exStartAtNull.CheckNullConstraint("Events", "StartAt");
        exEndAtNull.CheckNullConstraint("Events", "EndAt");
        exTotalSeatsNull.CheckNullConstraint("Events", "TotalSeats");
        exReservedCountNull.CheckNullConstraint("Events", "ReservedSeats");

        exTotalSeats.CheckValueConstraint("Events", "CK_Events_Seats");
        exReservedCount.CheckValueConstraint("Events", "CK_Events_Seats");
        exStartEndAt.CheckValueConstraint("Events", "CK_Events_StartEnd");
    }

    [Trait(Category.Name, Category.Database)]
    [Fact]
    public async Task EventBookingColumnsTest_Success()
    {
        // Given
        await ResetDatabase();

        Event @event = new(10)
        {
            Title = "Event",
            Description = "Description",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1)
        };

        var seats = 5;
        var eventId = @event.Id;
        var bookingId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var bookedAt = @event.StartAt.AddDays(-1);

        // When
        EventBooking booking = new(@event, bookingId) { UserId = userId };

        booking.Confirm(seats, bookedAt);
        booking.Cancel();

        {
            await using var db = CreateDbContext();
            db.EventBookings.Add(booking);
            await db.SaveChangesAsync();
        }

        // Then
        EventBooking found;
        {
            await using var db = CreateDbContext();
            found = await db.EventBookings.FromSqlInterpolated($@"
                SELECT *
                FROM ""event-bookings""
                WHERE ""BookingId"" = {bookingId}
            ")
            .SingleAsync();
        }

        var cmp = (DateTime? t1, DateTime? t2) =>
        {
            if (t1 is null || t2 is null)
                return false;

            var diff = t1 - t2;
            return diff >= TimeSpan.FromSeconds(-2) && diff <= TimeSpan.FromSeconds(2);
        };
        Assert.NotNull(found);
        Assert.Equal(bookingId, found.BookingId);
        Assert.Equal(eventId, found.EventId);
        Assert.Equal(booking.CancelledAt, found.CancelledAt, cmp);
        Assert.Equal(booking.ConfirmedAt, found.ConfirmedAt, cmp);
    }

    [Trait(Category.Name, Category.Database)]
    [Fact]
    public async Task EventBookingsPrimaryKeyTest()
    {
        // Given
        await ResetDatabase();
        Event @event = new(10)
        {
            Title = "Event",
            Description = "Description",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1)
        };

        var eventId = @event.Id;
        var bookingId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // When
        EventBooking bookingSuccess = new(@event, bookingId) { UserId = userId };
        EventBooking bookingFailed = new(@event.Id, bookingId) { UserId = userId };

        var add = async (EventBooking booking) =>
        {
            try
            {
                await using var db = CreateDbContext();
                db.EventBookings.Add(booking);
                await db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                if (e.InnerException is PostgresException pe)
                    throw pe;
                throw;
            }
        };

        await add(bookingSuccess);

        // Then
        var ex = await Assert.ThrowsAnyAsync<PostgresException>(
            async () => await add(bookingFailed));

        ex.CheckUniqueConstraint("event-bookings", "BookingId");
    }

    [Trait(Category.Name, Category.Database)]
    [Fact]
    public async Task EventBookingsConstraintsTest()
    {
        // Given
        await ResetDatabase();
        Event @event = new(10)
        {
            Title = "Event",
            Description = "Description",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1)
        };

        EventBooking booking = new(@event, Guid.NewGuid()) { UserId = Guid.Empty };

        {
            await using var tmp = CreateDbContext();
            tmp.EventBookings.Add(booking);
            await tmp.SaveChangesAsync();
        }

        // When
        await using var db = CreateDbContext();

        var bookingIdNull = await Assert.ThrowsAnyAsync<PostgresException>(async() 
            => await db.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE ""event-bookings"" SET
                    ""BookingId"" = NULL
                WHERE ""BookingId"" = {booking.BookingId}"));

        var reservedSeatsNull = await Assert.ThrowsAnyAsync<PostgresException>(async() 
            => await db.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE ""event-bookings"" SET
                    ""ReservedSeats"" = NULL
                WHERE ""BookingId"" = {booking.BookingId}"));

        var reservedSeatsCheck = await Assert.ThrowsAnyAsync<PostgresException>(async() 
            => await db.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE ""event-bookings"" SET
                    ""ReservedSeats"" = -1
                WHERE ""BookingId"" = {booking.BookingId}"));

        var eventIdKey = await Assert.ThrowsAnyAsync<PostgresException>(async() 
            => await db.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE ""event-bookings"" SET
                    ""EventId"" = {Guid.Empty}
                WHERE ""BookingId"" = {booking.BookingId}"));

        var eventIdKey = await Assert.throw<PostgresException>(async() 
            => await db.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE ""event-bookings"" SET
                    ""EventId"" = {Guid.Empty}
                WHERE ""BookingId"" = {booking.BookingId}"));
        // Then
        bookingIdNull.CheckNullConstraint("event-bookings", "BookingId");
        reservedSeatsNull.CheckNullConstraint("event-bookings", "ReservedSeats");
        reservedSeatsCheck.CheckValueConstraint("event-bookings", "CK_ReservedSeats");
        eventIdKey.CheckForeignKey("event-bookings", "FK_event-bookings_Events_EventId");
    }
}
