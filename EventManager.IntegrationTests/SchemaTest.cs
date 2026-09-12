using System.Reflection.Metadata;
using EventManager.Application.DataTransfer;
using EventManager.Data;
using EventManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace EventManager.IntegrationTests;

public class SchemaTest : DatabaseTestBase<AppDbContext>
{
    [Trait(Category, Category_Database)]
    [Fact]
    public async Task DatabaseConnect_Success()
    {
        // Given
        await ResetDatabase();
    
        // When
        await using var context = CreateDbContext();
    
        // Then
        Assert.True(context.Database.CanConnect());
    }

    [Trait(Category, Category_Database)]
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

    [Trait(Category, Category_Database)]
    [Fact]
    public async Task BookingsColumnsTest_Success()
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
        var infoEvent = @event.ToOutputData();

        var booking = new Booking(@event.Id);
        var infoBooking = booking.ToInfo();
    
        // When
        await using var work = CreateDbContext();
        work.Events.Add(@event);
        work.Bookings.Add(booking);
        await work.SaveChangesAsync();

        // Then
        await using var verify = CreateDbContext();
        var bookingFound = await verify
            .Bookings.FromSqlInterpolated($@"
                SELECT b.""Id"", b.""EventId"", b.""Status"", b.""CreatedAt"", b.""ProcessedAt""
                FROM ""Events"" e
                INNER JOIN ""Bookings"" b ON b.""EventId"" = e.""Id""
                WHERE b.""Id"" = {booking.Id}
            ")
            .AsNoTracking()
            .Include(b => b.Event)
            .SingleOrDefaultAsync();

        Assert.NotNull(bookingFound?.Event);
        Assert.Equal(infoEvent.Id, bookingFound.EventId);
        Assert.Equal(infoBooking, bookingFound.ToInfo());
    }

    [Trait(Category, Category_Database)]
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
        
        CheckUniqueConstraint(ex, "Events", "Id");
    }

    [Trait(Category, Category_Database)]
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
            SET ""ReservedCount"" = NULL 
            WHERE ""Id"" = {@event.Id};
            "));

        var exTotalSeats = await Assert.ThrowsAnyAsync<PostgresException>(async() => await work.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""Events""
            SET ""TotalSeats"" = 0
            WHERE ""Id"" = {@event.Id}
            "));

        var exReservedCount = await Assert.ThrowsAnyAsync<PostgresException>(async() => await work.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""Events""
            SET ""ReservedCount"" = -1
            WHERE ""Id"" = {@event.Id}
            "));

        var startAt = DateTime.UtcNow;
        var exStartEndAt = await Assert.ThrowsAnyAsync<PostgresException>(async() => await work.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""Events""
            SET
                ""StartAt"" = {startAt},
                ""EndAt"" = {startAt}
            WHERE ""Id"" = {@event.Id}
            "));
        
        // Then        
        CheckNullConstraint(exIdNull, "Events", "Id");
        CheckNullConstraint(exTitleNull, "Events", "Title");
        CheckNullConstraint(exStartAtNull, "Events", "StartAt");
        CheckNullConstraint(exEndAtNull, "Events", "EndAt");
        CheckNullConstraint(exTotalSeatsNull, "Events", "TotalSeats");
        CheckNullConstraint(exReservedCountNull, "Events", "ReservedCount");

        CheckValueConstraint(exTotalSeats, "Events", "CK_Events_Seats");
        CheckValueConstraint(exReservedCount, "Events", "CK_Events_Seats");
        CheckValueConstraint(exStartEndAt, "Events", "CK_Events_StartEnd");
    }

    [Trait(Category, Category_Database)]
    [Fact]
    public async Task BookingsPrimaryKeyTest()
    {
        // Given
        await ResetDatabase();
        await using var context = CreateDbContext();
        var db = context.Database;
        var eventId = Guid.NewGuid();
        var title = "Simple event";
        var startAt = DateTime.UtcNow;
        var endAt = startAt.AddHours(10);
        var totalSeats = 10;
        var bookingId = Guid.NewGuid();
        var status = BookingStatus.Pending;
        var createdAt = DateTime.UtcNow;
        await db.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO ""Events""(""Id"", ""Title"", ""StartAt"", ""EndAt"", ""TotalSeats"") 
            VALUES({eventId}, {title}, {startAt}, {endAt}, {totalSeats})
        ");

        // When
        await db.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO ""Bookings""(""Id"", ""EventId"", ""Status"", ""CreatedAt"")
            VALUES({bookingId}, {eventId}, {status}, {createdAt})
        ");

        // Then
        var ex = await Assert.ThrowsAnyAsync<PostgresException>(
            async () => await db.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO ""Bookings""(""Id"", ""EventId"", ""Status"", ""CreatedAt"")
                VALUES({bookingId}, {eventId}, {status}, {createdAt})
            "));
        CheckUniqueConstraint(ex, "Bookings", "Id");
    }

    [Trait(Category, Category_Database)]
    [Fact]
    public async Task BookingsConstraintsTest()
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

        var booking = new Booking(@event.Id);
        await using var context = CreateDbContext();
        context.Events.Add(@event);
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();
    
        // When
        await using var work = CreateDbContext();
        var exIdNull = await Assert.ThrowsAnyAsync<PostgresException>(async() => await work.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""Bookings""
            SET ""Id"" = NULL
            WHERE ""Id"" = {booking.Id}
            "));

        var exEventIdNull = await Assert.ThrowsAnyAsync<PostgresException>(async() => await work.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""Bookings""
            SET ""EventId"" = NULL
            WHERE ""Id"" = {booking.Id}
            "));
        
        var exStatusNull = await Assert.ThrowsAnyAsync<PostgresException>(async() => await work.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""Bookings""
            SET ""Status"" = NULL
            WHERE ""Id"" = {booking.Id}
            "));

        var exCreatedAtNull = await Assert.ThrowsAnyAsync<PostgresException>(async() => await work.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""Bookings""
            SET ""CreatedAt"" = NULL
            WHERE ""Id"" = {booking.Id}
            "));

        // Then
        CheckNullConstraint(exIdNull, "Bookings", "Id");
        CheckNullConstraint(exEventIdNull, "Bookings", "EventId");
        CheckNullConstraint(exStatusNull, "Booking", "Status");
        CheckNullConstraint(exCreatedAtNull, "Bookings", "CreatedAt");
    }

    [Trait(Category, Category_Database)]
    [Fact]
    public async Task BookingsForeignKeyTest()
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
        var infoEvent = @event.ToOutputData();

        var booking = new Booking(@event.Id);
        var infoBooking = booking.ToInfo();
        var booking2 = new Booking(Guid.NewGuid());

        await using var context = CreateDbContext();
        context.Events.Add(@event);
        await context.SaveChangesAsync();

        DbUpdateException? exBook2 = null;
    
        // When
        {
            await using var work = CreateDbContext();
            work.Bookings.Add(booking);
            await work.SaveChangesAsync();
        }
        {
            await using var work = CreateDbContext();
            work.Bookings.Add(booking2);
            exBook2 = await Assert.ThrowsAnyAsync<DbUpdateException>(async() => await work.SaveChangesAsync());            
        }

        // Then
        await using var verify = CreateDbContext();
        var bookingFound = await verify.Bookings.AsNoTracking()
            .Where(b => b.Id == booking.Id)
            .Include(b => b.Event)
            .SingleOrDefaultAsync();
        var bookingCount = await verify.Bookings.AsNoTracking().CountAsync();

        Assert.NotNull(bookingFound?.Event);
        Assert.Equal(infoEvent.Id, bookingFound.EventId);
        Assert.Equal(infoBooking, bookingFound.ToInfo());
        Assert.Equal(1, bookingCount);
        CheckForeignKey(exBook2?.InnerException as PostgresException, "Booking", "FK_Bookings_Events_EventId");
    }
    
    void CheckUniqueConstraint(PostgresException? exception, string table, string column)
    {
        Assert.Equal(PostgresErrorCodes.UniqueViolation, exception?.SqlState);
        Assert.Contains(table, exception?.TableName);
        Assert.Equal($"PK_{table}", exception?.ConstraintName);
    }

    void CheckNullConstraint(PostgresException? exception, string table, string column)
    {
        Assert.Equal(PostgresErrorCodes.NotNullViolation, exception?.SqlState);
        Assert.Contains(table, exception?.TableName);
        Assert.Contains(column, exception?.ColumnName);
    }

    void CheckValueConstraint(PostgresException? exception, string table, string name)
    {
        Assert.Equal(PostgresErrorCodes.CheckViolation, exception?.SqlState);
        Assert.Contains(table, exception?.TableName);
        Assert.Equal(name, exception?.ConstraintName);
    }

    void CheckForeignKey(PostgresException? exception, string table, string name)
    {
        Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, exception?.SqlState);
        Assert.Contains(table, exception?.TableName);
        Assert.Equal(name, exception?.ConstraintName);
    }
}
